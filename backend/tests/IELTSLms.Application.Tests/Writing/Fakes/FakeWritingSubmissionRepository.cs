using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing.Fakes;

internal sealed class FakeWritingSubmissionRepository : IWritingSubmissionRepository
{
    private readonly List<WritingSubmission> _submissions = [];

    public void Seed(WritingSubmission submission) => _submissions.Add(submission);

    public IReadOnlyList<WritingSubmission> Submissions => _submissions;

    public Task<WritingSubmission?> GetByIdAsync(WritingSubmissionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_submissions.FirstOrDefault(s => s.Id == id));

    public void Add(WritingSubmission aggregate) => _submissions.Add(aggregate);

    public void Remove(WritingSubmission aggregate) => _submissions.Remove(aggregate);

    public Task<IReadOnlyList<WritingSubmission>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WritingSubmission>>(_submissions
            .Where(s => s.TenantId == tenantId && s.UserId == userId && s.Status != WritingSubmissionStatus.Draft)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToList());

    public Task<IReadOnlyList<WritingSubmission>> ListSubmittedByTaskAsync(
        Guid tenantId, WritingTaskId taskId, WritingSubmissionId excludingSubmissionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WritingSubmission>>(_submissions
            .Where(s =>
                s.TenantId == tenantId && s.WritingTaskId == taskId && s.Id != excludingSubmissionId
                && s.Status != WritingSubmissionStatus.Draft)
            .ToList());
}
