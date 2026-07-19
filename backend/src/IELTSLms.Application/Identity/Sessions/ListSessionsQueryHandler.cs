using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Identity.Sessions;

/// <summary>Handles <see cref="ListSessionsQuery"/> by projecting the user's active sessions to summaries.</summary>
public sealed class ListSessionsQueryHandler(IUserSessionRepository sessions, IClock clock)
    : IQueryHandler<ListSessionsQuery, IReadOnlyList<SessionSummary>>
{
    public async Task<Result<IReadOnlyList<SessionSummary>>> Handle(
        ListSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var active = await sessions
            .GetActiveByUserAsync(UserId.From(request.UserId), now, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<SessionSummary> summaries = active
            .Select(s => new SessionSummary(
                s.Id.Value,
                s.DeviceLabel,
                s.IpAddress,
                s.LastSeenAtUtc,
                s.CreatedAtUtc,
                s.ExpiresAtUtc))
            .ToArray();

        return Result.Success(summaries);
    }
}
