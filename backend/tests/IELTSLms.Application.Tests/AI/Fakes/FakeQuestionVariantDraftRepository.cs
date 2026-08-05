using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeQuestionVariantDraftRepository : IQuestionVariantDraftRepository
{
    private readonly List<QuestionVariantDraft> _drafts = [];

    public void Seed(QuestionVariantDraft draft) => _drafts.Add(draft);

    public IReadOnlyList<QuestionVariantDraft> Drafts => _drafts;

    public Task<QuestionVariantDraft?> GetByIdAsync(QuestionVariantDraftId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_drafts.FirstOrDefault(d => d.Id == id));

    public void Add(QuestionVariantDraft aggregate) => _drafts.Add(aggregate);

    public void Remove(QuestionVariantDraft aggregate) => _drafts.Remove(aggregate);
}
