using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class QuestionVariantDraftRepository(ApplicationDbContext dbContext)
    : RepositoryBase<QuestionVariantDraft, QuestionVariantDraftId>(dbContext), IQuestionVariantDraftRepository
{
}
