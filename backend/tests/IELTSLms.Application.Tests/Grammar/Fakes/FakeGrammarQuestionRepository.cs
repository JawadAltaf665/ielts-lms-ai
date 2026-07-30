using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Tests.Grammar.Fakes;

internal sealed class FakeGrammarQuestionRepository : IGrammarQuestionRepository
{
    private readonly List<GrammarQuestion> _questions = [];

    public void Seed(GrammarQuestion question) => _questions.Add(question);

    public Task<GrammarQuestion?> GetByIdAsync(GrammarQuestionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_questions.FirstOrDefault(q => q.Id == id));

    public void Add(GrammarQuestion aggregate) => _questions.Add(aggregate);

    public void Remove(GrammarQuestion aggregate) => _questions.Remove(aggregate);

    public Task<IReadOnlyList<GrammarQuestion>> ListByExerciseAsync(GrammarExerciseId exerciseId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GrammarQuestion> result = _questions
            .Where(q => q.GrammarExerciseId == exerciseId)
            .OrderBy(q => q.Sequence)
            .ToArray();
        return Task.FromResult(result);
    }

    public Task<bool> SequenceExistsAsync(
        GrammarExerciseId exerciseId, int sequence, GrammarQuestionId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_questions.Any(
            q => q.GrammarExerciseId == exerciseId && q.Sequence == sequence && (excludingId is null || q.Id != excludingId.Value)));
}
