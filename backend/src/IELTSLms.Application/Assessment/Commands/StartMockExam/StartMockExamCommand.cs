using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Assessment.Commands.StartMockExam;

/// <summary>
/// Starts a new mock exam session (Sprint 8 <c>MockExamFactory</c>): all four section slots exist from
/// birth, none started yet. Enforces "single active session per user" at the command level (a clear
/// 409 <c>BUSINESS_ACTIVE_EXAM_EXISTS</c>) in addition to the DB partial-unique index that backstops it
/// under a genuine race (see <c>StartMockExamCommandHandler</c>).
/// </summary>
public sealed record StartMockExamCommand(Guid TenantId, Guid UserId) : ICommand<MockExamStartResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["mock-exam.attempt"];
}

public sealed record MockExamStartResponse(Guid Id, string State, DateTimeOffset StartedAtUtc);
