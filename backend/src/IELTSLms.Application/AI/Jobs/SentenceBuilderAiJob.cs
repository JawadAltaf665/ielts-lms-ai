using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.SentenceBuilderAi;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.SentenceBuilder;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the AI scaffolding/validation call for an already-submitted <see cref="SentenceAttempt"/>
/// and attaches the result directly to it (no <see cref="AiOperation"/> row — unlike Grammar/Vocabulary
/// Coach, there is no client-facing 202 operation to poll here; the caller polls the attempt resource
/// itself, per Sprint 7's endpoint table). Enqueued from
/// <see cref="SentenceBuilder.EventHandlers.SentenceAttemptSubmittedHandler"/>. Runs outside any HTTP
/// request scope, so it establishes the tenant context itself (mirrors <see cref="GrammarCoachingJob"/>).
/// </summary>
public sealed partial class SentenceBuilderAiJob(
    ISentenceAttemptRepository attempts,
    ISentenceExerciseRepository exercises,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    IPromptCompositionService promptComposition,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<SentenceBuilderAiJob> logger)
{
    private const int MaxOutputTokens = 1024;
    private static readonly IReadOnlyDictionary<string, string> NoSystemVariables =
        new Dictionary<string, string>();

    public async Task ExecuteAsync(Guid tenantId, Guid attemptId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.SetTenant(tenantId);

        var attempt = await attempts
            .GetByIdAsync(SentenceAttemptId.From(attemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null)
        {
            AttemptNotFound(logger, attemptId);
            return;
        }

        // Idempotency guard: Hangfire's global automatic-retry policy may re-run this job after a
        // mid-flight crash. Feedback is attached exactly once (see SentenceAttempt.AttachAiFeedback).
        if (attempt.AiResponseId is not null)
        {
            return;
        }

        var exercise = await exercises
            .GetVisibleByIdAsync(tenantId, attempt.SentenceExerciseId, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            ExerciseNotFound(logger, attempt.SentenceExerciseId.Value);
            return;
        }

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var prompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.SentenceBuilderAi, cancellationToken).ConfigureAwait(false);

        if (provider is null || prompt is null)
        {
            // No AiOperation to fail here — the attempt simply keeps its deterministic score with no
            // AI feedback (additive, not gating, per business rules).
            PromptNotConfigured(logger, attemptId);
            return;
        }

        var userContent = ComposeUserContent(exercise.Prompt, exercise.TargetPatternJson, attempt.SubmittedText);
        var composed = promptComposition.Compose(prompt, NoSystemVariables, userContent);
        if (composed.IsFailure)
        {
            CompositionFailed(logger, attemptId, composed.Error.Code);
            return;
        }

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.SentenceBuilderAi, prompt.PromptVersion, provider.Kind, composed.Value.SystemPrompt,
                composed.Value.UserContent, SentenceBuilderAiSchema.Name, SentenceBuilderAiSchema.Json, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (invocation.IsFailure)
        {
            GatewayCallFailed(logger, attemptId, invocation.Error.Code);
            return;
        }

        var result = invocation.Value;
        SentenceBuilderAiOutput? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<SentenceBuilderAiOutput>(result.OutputJson, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            parsed = null;
        }

        var status = parsed is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid;
        var now = clock.UtcNow;
        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, prompt.Id, prompt.PromptVersion,
            nameof(SentenceAttempt), attemptId, ComputeInputHash(userContent), result.Model, result.PromptTokens,
            result.CompletionTokens, result.LatencyMs, result.CostMinorUnits, result.CostCurrency, status,
            status == AiResponseStatus.Succeeded ? result.OutputJson : null, result.ProviderResponseId, now);

        if (responseCreate.IsFailure)
        {
            return;
        }

        // Persist the AiResponse row regardless of parse success — it is a write-once cost/audit log
        // of the provider call (mirrors GrammarCoachingJob), independent of whether the attempt itself
        // gets feedback attached.
        var aiResponse = responseCreate.Value;
        responses.Add(aiResponse);

        if (parsed is not null)
        {
            var feedback = parsed.SuggestedRevision is { Length: > 0 }
                ? $"{parsed.Feedback} Suggested revision: {parsed.SuggestedRevision}"
                : parsed.Feedback;
            attempt.AttachAiFeedback(aiResponse.Id, feedback, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ComposeUserContent(string exercisePrompt, string targetPatternJson, string submittedText) =>
        $"Exercise prompt: {exercisePrompt}\nTarget pattern: {targetPatternJson}\nLearner's sentence: {submittedText}";

    private static string ComputeInputHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sentence attempt {AttemptId} not found; the enqueued AI job has nothing to execute.")]
    private static partial void AttemptNotFound(ILogger logger, Guid attemptId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sentence exercise {ExerciseId} not found for AI scaffolding.")]
    private static partial void ExerciseNotFound(ILogger logger, Guid exerciseId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sentence Builder AI prompt not configured; skipping enrichment for attempt {AttemptId}.")]
    private static partial void PromptNotConfigured(ILogger logger, Guid attemptId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Prompt composition failed ({ErrorCode}) for sentence attempt {AttemptId}.")]
    private static partial void CompositionFailed(ILogger logger, Guid attemptId, string errorCode);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AI gateway call failed ({ErrorCode}) for sentence attempt {AttemptId}.")]
    private static partial void GatewayCallFailed(ILogger logger, Guid attemptId, string errorCode);
}
