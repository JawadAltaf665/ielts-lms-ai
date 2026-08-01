using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.GrammarCoach;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the actual Grammar Coach gateway call for an already-enqueued <see cref="AiOperation"/>
/// (AI Architecture §9 — non-interactive coaching runs asynchronously). Enqueued via
/// <c>IBackgroundJobService.Enqueue&lt;GrammarCoachingJob&gt;</c> from
/// <see cref="RequestGrammarCoaching.RequestGrammarCoachingCommandHandler"/>; not a MediatR handler
/// because Hangfire needs a plain, container-resolvable class with a directly-callable method. Runs
/// outside any HTTP request scope, so it must establish the tenant context itself before touching any
/// tenant-scoped repository (<see cref="ITenantContextAccessor.SetTenant"/> exists for exactly this).
/// </summary>
public sealed partial class GrammarCoachingJob(
    IAiOperationRepository operations,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<GrammarCoachingJob> logger)
{
    private const int MaxOutputTokens = 1024;

    public async Task ExecuteAsync(Guid tenantId, Guid operationId, CancellationToken cancellationToken = default)
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
        // double-charge the quota's intent and could write a second AiResponse row.
        if (operation.Status is AiOperationStatus.Succeeded or AiOperationStatus.Failed)
        {
            return;
        }

        var now = clock.UtcNow;
        operation.MarkProcessing(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var prompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.GrammarCoach, cancellationToken).ConfigureAwait(false);

        if (provider is null || prompt is null)
        {
            await FailAsync(operation, AiErrors.PromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.GrammarCoach, prompt.PromptVersion, provider.Kind, prompt.SystemTemplate, operation.InputText,
                GrammarCoachSchema.Name, GrammarCoachSchema.Json, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (invocation.IsFailure)
        {
            await FailAsync(operation, invocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var result = invocation.Value;
        GrammarCoachOutput? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<GrammarCoachOutput>(
                result.OutputJson, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            parsed = null;
        }

        var status = parsed is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid;
        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, prompt.Id, prompt.PromptVersion,
            nameof(AiOperation), operationId, ComputeInputHash(operation.InputText), result.Model, result.PromptTokens,
            result.CompletionTokens, result.LatencyMs, result.CostMinorUnits, result.CostCurrency, status,
            status == AiResponseStatus.Succeeded ? result.OutputJson : null, result.ProviderResponseId, now);

        if (responseCreate.IsFailure)
        {
            await FailAsync(operation, responseCreate.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var aiResponse = responseCreate.Value;
        responses.Add(aiResponse);

        if (status == AiResponseStatus.Invalid)
        {
            operation.MarkFailed(AiErrors.InputRejected.Code, "The AI response failed structured-output validation.", now);
        }
        else
        {
            operation.MarkSucceeded(aiResponse.Id, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task FailAsync(AiOperation operation, Error error, CancellationToken cancellationToken)
    {
        operation.MarkFailed(error.Code, error.Message, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ComputeInputHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    [LoggerMessage(Level = LogLevel.Warning, Message = "AI operation {OperationId} not found; the enqueued job has nothing to execute.")]
    private static partial void OperationNotFound(ILogger logger, Guid operationId);
}
