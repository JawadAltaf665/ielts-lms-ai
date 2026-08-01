using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.RequestBandPrediction;

/// <summary>
/// Requests a fresh standalone, history-based band prediction (Agentic AI Architecture §13.6). Unlike
/// Writing/Speaking submission, there is no client-supplied input at all — the background job gathers
/// the learner's own recent cross-skill history itself. Returns <c>202 Accepted</c> plus an operation
/// resource; mirrors <c>SubmitWritingCommand</c>'s AI-kickoff shape (entitlement/quota check,
/// <c>AiOperation</c> creation, job enqueue).
/// </summary>
public sealed record RequestBandPredictionCommand(Guid TenantId, Guid UserId) : ICommand<BandPredictionAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["band-prediction.request"];
}

public sealed record BandPredictionAcceptedResponse(Guid BandPredictionId, Guid OperationId, string Status);
