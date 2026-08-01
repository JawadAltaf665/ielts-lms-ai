using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;

namespace IELTSLms.Application.Assessment.Commands.StartMockExamSection;

/// <summary>
/// Starts one section within an in-progress mock exam session. For Reading/Listening (which already
/// support a decoupled server-timed Start-then-Submit flow) this delegates to that skill's own
/// <c>Start&lt;Skill&gt;Attempt</c> command — <paramref name="ContentId"/> is that section's
/// passage/audio id and is required. Writing/Speaking have no such phase in their own modules (their
/// <c>Create</c>/<c>Submit</c> is atomic); for those two this only stamps the server timer, and
/// <paramref name="ContentId"/> is ignored — the candidate submits via the skill's own standalone
/// endpoint within the deadline, and the resulting submission is linked back automatically (see
/// <c>Application/Assessment/EventHandlers</c>).
/// </summary>
public sealed record StartMockExamSectionCommand(Guid TenantId, Guid UserId, Guid MockExamId, MockExamSectionType Section, Guid? ContentId)
    : ICommand<MockExamSectionStartResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["mock-exam.attempt"];
}

public sealed record MockExamSectionStartResponse(
    Guid MockExamId, string Section, Guid? AttemptId, DateTimeOffset StartedAtUtc, DateTimeOffset DeadlineAtUtc);
