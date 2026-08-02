using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// Backs the generic async operation-resource endpoint (API Design Standards §14.2) for AI
/// capabilities: created <see cref="AiOperationStatus.Queued"/> when a command enqueues a gateway call,
/// then driven through <see cref="MarkProcessing"/>/<see cref="MarkSucceeded"/>/<see cref="MarkFailed"/>
/// by the background job that actually invokes the gateway. Scoped to AI (not a cross-domain
/// "Operations" framework) — every future AI capability (Sprints 6, 7, 9) reuses this same row shape.
/// </summary>
public sealed class AiOperation : TenantAuditableEntity<AiOperationId>
{
    private AiOperation(AiOperationId id, Guid tenantId, Guid userId, string kind, string inputText, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Kind = kind;
        InputText = inputText;
        Status = AiOperationStatus.Queued;
        CreatedAtUtc = nowUtc;
    }

    private AiOperation()
    {
        Kind = null!;
        InputText = null!;
    }

    public Guid UserId { get; private init; }

    /// <summary>Capability tag, e.g. <c>"grammar-coach"</c>.</summary>
    public string Kind { get; private init; }

    /// <summary>The learner-submitted text this operation acts on, kept so the background job can load
    /// it by operation id alone rather than threading raw user text through Hangfire's job-args store.</summary>
    public string InputText { get; private init; }

    public AiOperationStatus Status { get; private set; }

    public AiResponseId? AiResponseId { get; private set; }

    public string? ErrorCode { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static AiOperation Enqueue(AiOperationId id, Guid tenantId, Guid userId, string kind, string inputText, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, kind, inputText, nowUtc);

    public void MarkProcessing(DateTimeOffset nowUtc)
    {
        if (Status != AiOperationStatus.Queued)
        {
            return;
        }

        Status = AiOperationStatus.Processing;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary><paramref name="aiResponseId"/> is <see langword="null"/> only for a turn that never
    /// reached the gateway (e.g. AI Tutor's scope guard rejecting the question before any provider call
    /// is made) — there is no <c>AiResponse</c> row to link in that case.</summary>
    public void MarkSucceeded(AiResponseId? aiResponseId, DateTimeOffset nowUtc)
    {
        Status = AiOperationStatus.Succeeded;
        AiResponseId = aiResponseId;
        CompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkFailed(string errorCode, string errorMessage, DateTimeOffset nowUtc)
    {
        Status = AiOperationStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        CompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
