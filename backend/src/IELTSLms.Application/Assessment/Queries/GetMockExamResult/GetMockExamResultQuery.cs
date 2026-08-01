using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Assessment.Queries.GetMockExamResult;

/// <summary>
/// Gets the aggregated result of the caller's own mock exam. Sprint 8 API design note: rather than
/// <c>404</c> until finalized, this always returns <c>200</c> with <see cref="MockExamResultResponse.Status"/>
/// ∈ {<c>pending</c>, <c>finalized</c>} — the result resource exists (pending) from the moment the exam
/// starts (<c>StartMockExamCommandHandler</c> creates it alongside the exam), matching this codebase's
/// existing async-operation-status convention (<c>AiOperation</c>) rather than treating "not finalized
/// yet" as "not found."
/// </summary>
public sealed record GetMockExamResultQuery(Guid TenantId, Guid UserId, Guid MockExamId) : IQuery<MockExamResultResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["mock-exam.attempt"];
}

public sealed record MockExamResultResponse(
    Guid Id, Guid MockExamId, string Status, decimal? ReadingBand, decimal? ListeningBand, decimal? WritingBand, decimal? SpeakingBand,
    decimal? OverallBand, DateTimeOffset? FinalizedAtUtc);
