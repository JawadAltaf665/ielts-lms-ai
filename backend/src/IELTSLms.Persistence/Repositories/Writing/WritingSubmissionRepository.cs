using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Persistence.Repositories.Writing;

internal sealed class WritingSubmissionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<WritingSubmission, WritingSubmissionId>(dbContext), IWritingSubmissionRepository
{
}
