using IELTSLms.Domain.Writing;

namespace IELTSLms.Abstractions.Persistence.Writing;

/// <summary>Aggregate access for <see cref="WritingSubmission"/> (ERD ENT-028).</summary>
public interface IWritingSubmissionRepository : IRepository<WritingSubmission, WritingSubmissionId>
{
}
