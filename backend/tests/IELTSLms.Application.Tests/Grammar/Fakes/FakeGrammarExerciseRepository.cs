using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Tests.Grammar.Fakes;

internal sealed class FakeGrammarExerciseRepository : IGrammarExerciseRepository
{
    private readonly List<GrammarExercise> _exercises = [];

    public void Seed(GrammarExercise exercise) => _exercises.Add(exercise);

    public Task<GrammarExercise?> GetByIdAsync(GrammarExerciseId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_exercises.FirstOrDefault(e => e.Id == id));

    public void Add(GrammarExercise aggregate) => _exercises.Add(aggregate);

    public void Remove(GrammarExercise aggregate) => _exercises.Remove(aggregate);

    public Task<GrammarExercise?> GetVisibleByIdAsync(Guid tenantId, GrammarExerciseId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_exercises.FirstOrDefault(e => e.Id == id && (e.TenantId == null || e.TenantId == tenantId)));

    public Task<IReadOnlyList<GrammarExercise>> ListByTopicAsync(Guid tenantId, GrammarTopicId topicId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GrammarExercise> result = _exercises
            .Where(e => e.GrammarTopicId == topicId && (e.TenantId == null || e.TenantId == tenantId))
            .OrderBy(e => e.Order)
            .ToArray();
        return Task.FromResult(result);
    }
}
