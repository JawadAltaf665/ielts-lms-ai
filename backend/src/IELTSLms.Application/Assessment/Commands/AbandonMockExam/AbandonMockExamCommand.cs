using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Assessment.Commands.AbandonMockExam;

/// <summary>Abandons an in-progress mock exam session, releasing the single-active-session lock.</summary>
public sealed record AbandonMockExamCommand(Guid TenantId, Guid UserId, Guid MockExamId) : ICommand<MockExamAbandonResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["mock-exam.attempt"];
}

public sealed record MockExamAbandonResponse(Guid Id, string State, DateTimeOffset AbandonedAtUtc);
