using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Assessment.Queries.GetMockExam;

/// <summary>
/// Gets the caller's own mock exam session status and section references. <c>mock-exam.attempt</c>
/// (own) covers the self-service path; cohort-level viewing (<c>analytics.view.cohort</c>) is declared
/// for documentation only — not enforced yet, same as every other fine-grained permission in this
/// codebase (<c>AuthorizationPipelineBehavior</c> only checks <c>RequiredRoles</c> today).
/// </summary>
public sealed record GetMockExamQuery(Guid TenantId, Guid UserId, Guid MockExamId) : IQuery<MockExamResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["mock-exam.attempt"];
}

public sealed record MockExamSectionResponse(
    string Section, Guid? AttemptId, DateTimeOffset? StartedAtUtc, DateTimeOffset? DeadlineAtUtc, DateTimeOffset? CompletedAtUtc);

public sealed record MockExamResponse(
    Guid Id, string State, DateTimeOffset StartedAtUtc, DateTimeOffset? CompletedAtUtc, DateTimeOffset? AbandonedAtUtc,
    IReadOnlyList<MockExamSectionResponse> Sections);
