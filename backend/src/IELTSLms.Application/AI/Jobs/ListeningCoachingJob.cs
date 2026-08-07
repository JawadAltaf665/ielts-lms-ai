using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.ListeningCoach;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the actual Listening Coach gateway call for an already-enqueued <see cref="AiOperation"/>.
/// Mirrors <see cref="ReadingCoachingJob"/> exactly — same enqueue/idempotency/tenant-context shape,
/// different capability tag and output schema.
/// </summary>
public sealed partial class ListeningCoachingJob(
    IAiOperationRepository operations,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    IPromptCompositionService promptComposition,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<ListeningCoachingJob> logger)
{
    private const int MaxOutputTokens = 1024;
    private static readonly IReadOnlyDictionary<string, string> NoSystemVariables =
        new Dictionary<string, string>();

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
        // mid-flight crash. A terminal operation has already been fully persisted.
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
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.ListeningCoach, cancellationToken).ConfigureAwait(false);

        if (provider is null || prompt is null)
        {
            await FailAsync(operation, AiErrors.PromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var composed = promptComposition.Compose(prompt, NoSystemVariables, operation.InputText);
        if (composed.IsFailure)
        {
            await FailAsync(operation, composed.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.ListeningCoach, prompt.PromptVersion, provider.Kind, composed.Value.SystemPrompt,
                composed.Value.UserContent, ListeningCoachSchema.Name, ListeningCoachSchema.Json, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (invocation.IsFailure)
        {
            await FailAsync(operation, invocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var result = invocation.Value;
        ListeningCoachOutput? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<ListeningCoachOutput>(result.OutputJson, JsonSerializerOptions.Web);
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
