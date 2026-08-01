using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Domain.Speaking;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Speaking;

internal sealed class SpeakingTaskRepository(ApplicationDbContext dbContext)
    : RepositoryBase<SpeakingTask, SpeakingTaskId>(dbContext), ISpeakingTaskRepository
{
    public async Task<SpeakingTask?> GetVisibleByIdAsync(Guid tenantId, SpeakingTaskId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.Id == id && (t.TenantId == null || t.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<SpeakingTask>> ListVisibleAsync(
        Guid tenantId, SpeakingPart? part, int skip, int take, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, part)
            .OrderBy(t => t.Title)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<long> CountVisibleAsync(Guid tenantId, SpeakingPart? part, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, part).LongCountAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, SpeakingTaskId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            t => t.TenantId == tenantId && t.Title == title && (!excludingId.HasValue || t.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);

    private IQueryable<SpeakingTask> Visible(Guid tenantId, SpeakingPart? part)
    {
        var query = Set.Where(t => t.TenantId == null || t.TenantId == tenantId);

        if (part is { } partValue)
        {
            query = query.Where(t => t.Part == partValue);
        }

        return query;
    }
}
