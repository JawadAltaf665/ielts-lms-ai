using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="QuestionVariantDraft"/> (Sprint 8).</summary>
public interface IQuestionVariantDraftRepository : IRepository<QuestionVariantDraft, QuestionVariantDraftId>
{
}
