using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Application.Tests.SentenceBuilder.Fakes;

internal sealed class FakeSentenceExerciseRepository : ISentenceExerciseRepository
{
    private readonly List<SentenceExercise> _exercises = [];

    public void Seed(SentenceExercise exercise) => _exercises.Add(exercise);

    public Task<SentenceExercise?> GetByIdAsync(SentenceExerciseId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_exercises.FirstOrDefault(e => e.Id == id));

    public void Add(SentenceExercise aggregate) => _exercises.Add(aggregate);

    public void Remove(SentenceExercise aggregate) => _exercises.Remove(aggregate);

    public Task<SentenceExercise?> GetVisibleByIdAsync(Guid tenantId, SentenceExerciseId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_exercises.FirstOrDefault(e => e.Id == id && (e.TenantId == null || e.TenantId == tenantId)));

    public Task<IReadOnlyList<SentenceExercise>> ListVisibleAsync(
        Guid tenantId, SentenceDifficulty? difficulty, int skip, int take, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SentenceExercise> result = Visible(tenantId, difficulty)
            .OrderBy(e => e.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToArray();
        return Task.FromResult(result);
    }

    public Task<long> CountVisibleAsync(Guid tenantId, SentenceDifficulty? difficulty, CancellationToken cancellationToken = default) =>
        Task.FromResult((long)Visible(tenantId, difficulty).Count());

    private IEnumerable<SentenceExercise> Visible(Guid tenantId, SentenceDifficulty? difficulty)
    {
        var query = _exercises.Where(e => e.TenantId == null || e.TenantId == tenantId);
        return difficulty is { } value ? query.Where(e => e.Difficulty == value) : query;
    }
}
