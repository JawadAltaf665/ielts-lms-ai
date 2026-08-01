using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.GrammarCoach;
using IELTSLms.Application.AI.VocabularyAnalysis;
using IELTSLms.Application.AI.WritingExaminer;
using IELTSLms.Application.Writing;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the Writing Evaluation Workflow for an already-enqueued <see cref="AiOperation"/>
/// (Agentic AI Architecture §13.1), simplified per the sprint's own explicit allowance into a plain
/// async pipeline rather than a separate agent framework: Grammar Agent (reuses Sprint 3's Grammar
/// Coach capability verbatim) and Vocabulary Agent run in parallel, then the Writing Examiner call
/// takes both outputs as context and produces bands, confidence, and feedback in one structured
/// response — folding the workflow's Feedback Composer step into that same call (see this sprint's
/// reconciliation notes for both simplifications). Enqueued via
/// <c>IBackgroundJobService.Enqueue&lt;WritingEvaluationJob&gt;</c> from
/// <see cref="SubmitWriting.SubmitWritingCommandHandler"/>; not a MediatR handler, same reason as
/// <see cref="GrammarCoachingJob"/> (Hangfire needs a plain, container-resolvable class).
/// </summary>
public sealed partial class WritingEvaluationJob(
    IAiOperationRepository operations,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    IWritingSubmissionRepository submissions,
    IWritingTaskRepository tasks,
    IWritingEvaluationRepository evaluations,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<WritingEvaluationJob> logger)
{
    private const int GrammarVocabMaxOutputTokens = 1024;
    private const int ExaminerMaxOutputTokens = 2048;

    /// <summary>Below this examiner-reported confidence, the evaluation is flagged for human review (HITL) rather than silently accepted.</summary>
    private const decimal LowConfidenceThreshold = 0.7m;

    public async Task ExecuteAsync(
        Guid tenantId, Guid operationId, Guid writingSubmissionId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.SetTenant(tenantId);

        var operation = await operations
            .GetByIdAsync(AiOperationId.From(operationId), cancellationToken)
            .ConfigureAwait(false);
        if (operation is null)
        {
            OperationNotFound(logger, operationId);
            return;
        }

        // Idempotency guard: Hangfire's global automatic-retry policy may re-run this job after a
        // mid-flight crash. A terminal operation has already been fully persisted — redoing it would
        // double-charge the quota's intent and could write duplicate AiResponse rows.
        if (operation.Status is AiOperationStatus.Succeeded or AiOperationStatus.Failed)
        {
            return;
        }

        var submission = await submissions
            .GetByIdAsync(WritingSubmissionId.From(writingSubmissionId), cancellationToken)
            .ConfigureAwait(false);
        if (submission is null)
        {
            SubmissionNotFound(logger, writingSubmissionId);
            return;
        }

        var now = clock.UtcNow;
        operation.MarkProcessing(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var grammarPrompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.GrammarCoach, cancellationToken).ConfigureAwait(false);
        var vocabPrompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.VocabularyAnalysis, cancellationToken).ConfigureAwait(false);
        var examinerPrompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.WritingExaminer, cancellationToken).ConfigureAwait(false);

        if (provider is null || grammarPrompt is null || vocabPrompt is null || examinerPrompt is null)
        {
            await FailAsync(operation, submission, AiErrors.PromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var task = await tasks.GetByIdAsync(submission.WritingTaskId, cancellationToken).ConfigureAwait(false);
        if (task is null)
        {
            await FailAsync(operation, submission, WritingErrors.TaskNotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        // Grammar Agent + Vocabulary Agent run in parallel (Agentic AI Architecture §13.1).
        var grammarTask = gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.GrammarCoach, grammarPrompt.PromptVersion, provider.Kind, grammarPrompt.SystemTemplate,
                operation.InputText, GrammarCoachSchema.Name, GrammarCoachSchema.Json, GrammarVocabMaxOutputTokens),
            cancellationToken);
        var vocabTask = gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.VocabularyAnalysis, vocabPrompt.PromptVersion, provider.Kind, vocabPrompt.SystemTemplate,
                operation.InputText, VocabularyAnalysisSchema.Name, VocabularyAnalysisSchema.Json, GrammarVocabMaxOutputTokens),
            cancellationToken);
        await Task.WhenAll(grammarTask, vocabTask).ConfigureAwait(false);

        var grammarInvocation = await grammarTask.ConfigureAwait(false);
        if (grammarInvocation.IsFailure)
        {
            await FailAsync(operation, submission, grammarInvocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var vocabInvocation = await vocabTask.ConfigureAwait(false);
        if (vocabInvocation.IsFailure)
        {
            await FailAsync(operation, submission, vocabInvocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var grammarResult = grammarInvocation.Value;
        var vocabResult = vocabInvocation.Value;
        var inputHash = ComputeHash(operation.InputText);

        var grammarOutput = TryDeserialize<GrammarCoachOutput>(grammarResult.OutputJson);
        var grammarResponseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, grammarPrompt.Id, grammarPrompt.PromptVersion,
            nameof(AiOperation), operationId, inputHash, grammarResult.Model, grammarResult.PromptTokens, grammarResult.CompletionTokens,
            grammarResult.LatencyMs, grammarResult.CostMinorUnits, grammarResult.CostCurrency,
            grammarOutput is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid,
            grammarOutput is not null ? grammarResult.OutputJson : null, grammarResult.ProviderResponseId, now);

        var vocabOutput = TryDeserialize<VocabularyAnalysisOutput>(vocabResult.OutputJson);
        var vocabResponseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, vocabPrompt.Id, vocabPrompt.PromptVersion,
            nameof(AiOperation), operationId, inputHash, vocabResult.Model, vocabResult.PromptTokens, vocabResult.CompletionTokens,
            vocabResult.LatencyMs, vocabResult.CostMinorUnits, vocabResult.CostCurrency,
            vocabOutput is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid,
            vocabOutput is not null ? vocabResult.OutputJson : null, vocabResult.ProviderResponseId, now);

        if (grammarResponseCreate.IsFailure || vocabResponseCreate.IsFailure)
        {
            await FailAsync(operation, submission, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        responses.Add(grammarResponseCreate.Value);
        responses.Add(vocabResponseCreate.Value);

        if (grammarOutput is null || vocabOutput is null)
        {
            await FailAsync(operation, submission, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        // Writing Examiner Agent: scores the four criteria using the grammar/vocabulary findings as
        // supporting context, and (merged simplification) composes the learner-facing feedback in the
        // same call rather than a separate Feedback Composer step.
        var examinerInput = JsonSerializer.Serialize(
            new WritingExaminerInput(task.PromptText, task.TaskType.ToString(), submission.ResponseText, grammarOutput, vocabOutput),
            JsonSerializerOptions.Web);

        var examinerInvocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.WritingExaminer, examinerPrompt.PromptVersion, provider.Kind, examinerPrompt.SystemTemplate,
                examinerInput, WritingExaminerSchema.Name, WritingExaminerSchema.Json, ExaminerMaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (examinerInvocation.IsFailure)
        {
            await FailAsync(operation, submission, examinerInvocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var examinerResult = examinerInvocation.Value;
        var examinerOutput = TryDeserialize<WritingExaminerOutput>(examinerResult.OutputJson);
        var examinerResponseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, examinerPrompt.Id, examinerPrompt.PromptVersion,
            nameof(AiOperation), operationId, ComputeHash(examinerInput), examinerResult.Model, examinerResult.PromptTokens,
            examinerResult.CompletionTokens, examinerResult.LatencyMs, examinerResult.CostMinorUnits, examinerResult.CostCurrency,
            examinerOutput is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid,
            examinerOutput is not null ? examinerResult.OutputJson : null, examinerResult.ProviderResponseId, now);

        if (examinerResponseCreate.IsFailure)
        {
            await FailAsync(operation, submission, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        var examinerResponse = examinerResponseCreate.Value;
        responses.Add(examinerResponse);

        if (examinerOutput is null)
        {
            await FailAsync(operation, submission, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        var evaluationCreate = WritingEvaluation.Create(
            WritingEvaluationId.From(guids.NewGuid()), tenantId, submission.Id, examinerOutput.TaskResponse,
            examinerOutput.CoherenceCohesion, examinerOutput.LexicalResource, examinerOutput.GrammaticalRangeAccuracy,
            examinerOutput.Confidence, examinerOutput.FeedbackText, LowConfidenceThreshold, examinerResponse.Id, provider.Id, now);

        if (evaluationCreate.IsFailure)
        {
            await FailAsync(operation, submission, evaluationCreate.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var evaluation = evaluationCreate.Value;
        evaluations.Add(evaluation);
        submission.AttachEvaluation(evaluation.Id, evaluation.EstimatedBand, now);
        operation.MarkSucceeded(examinerResponse.Id, now);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task FailAsync(AiOperation operation, WritingSubmission submission, Error error, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        operation.MarkFailed(error.Code, error.Message, now);
        submission.MarkEvaluationFailed(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static T? TryDeserialize<T>(string? json) where T : class
    {
        if (json is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ComputeHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private sealed record WritingExaminerInput(
        string TaskPrompt, string TaskType, string ResponseText, GrammarCoachOutput GrammarFindings, VocabularyAnalysisOutput VocabularyFindings);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AI operation {OperationId} not found; the enqueued job has nothing to execute.")]
    private static partial void OperationNotFound(ILogger logger, Guid operationId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Writing submission {WritingSubmissionId} not found; the enqueued job has nothing to execute.")]
    private static partial void SubmissionNotFound(ILogger logger, Guid writingSubmissionId);
}
