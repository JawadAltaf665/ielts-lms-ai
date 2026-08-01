using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.Commands.SubmitWriting;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing;

public sealed class SubmitWritingCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (Guid TenantId, WritingTask Task) SeedTask(FakeWritingTaskRepository tasks, int minWordCount = 3, int maxWordCount = 10)
    {
        var tenantId = Guid.NewGuid();
        var task = WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), tenantId, "Task 2", WritingTaskType.Task2, ReadingTrack.Academic, "Discuss.", null,
            minWordCount, maxWordCount, null, Now).Value;
        tasks.Seed(task);
        return (tenantId, task);
    }

    private static SubmitWritingCommandHandler CreateHandler(
        FakeWritingTaskRepository tasks, FakeWritingSubmissionRepository submissions, FakeAiOperationRepository operations,
        FakeUnitOfWork unitOfWork, FakeEntitlementService entitlements, FakeQuotaService quotas, FakeBackgroundJobService backgroundJobs) =>
        new(tasks, submissions, operations, unitOfWork, entitlements, quotas, backgroundJobs, new FakeClock(Now), new FakeGuidProvider());

    [Fact]
    public async Task Server_recomputes_the_word_count_and_ignores_any_client_notion_of_it()
    {
        var tasks = new FakeWritingTaskRepository();
        var (tenantId, task) = SeedTask(tasks, minWordCount: 3, maxWordCount: 10);
        var submissions = new FakeWritingSubmissionRepository();
        var operations = new FakeAiOperationRepository();
        var handler = CreateHandler(
            tasks, submissions, operations, new FakeUnitOfWork(), new FakeEntitlementService(), new FakeQuotaService(),
            new FakeBackgroundJobService());

        var result = await handler.Handle(
            new SubmitWritingCommand(tenantId, Guid.NewGuid(), task.Id.Value, "one two three four five"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var submission = Assert.Single(submissions.Submissions);
        Assert.Equal(5, submission.WordCount);
        Assert.Equal(WritingSubmissionStatus.Submitted, submission.Status);
    }

    [Fact]
    public async Task Word_count_below_the_tasks_minimum_is_rejected_without_creating_a_submission()
    {
        var tasks = new FakeWritingTaskRepository();
        var (tenantId, task) = SeedTask(tasks, minWordCount: 10, maxWordCount: 20);
        var submissions = new FakeWritingSubmissionRepository();
        var operations = new FakeAiOperationRepository();
        var handler = CreateHandler(
            tasks, submissions, operations, new FakeUnitOfWork(), new FakeEntitlementService(), new FakeQuotaService(),
            new FakeBackgroundJobService());

        var result = await handler.Handle(
            new SubmitWritingCommand(tenantId, Guid.NewGuid(), task.Id.Value, "too short"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_WORD_COUNT_OUT_OF_BOUNDS", result.Error.Code);
        Assert.Empty(submissions.Submissions);
        Assert.Empty(operations.Operations);
    }

    [Fact]
    public async Task Word_count_above_the_tasks_maximum_is_rejected()
    {
        var tasks = new FakeWritingTaskRepository();
        var (tenantId, task) = SeedTask(tasks, minWordCount: 1, maxWordCount: 3);
        var submissions = new FakeWritingSubmissionRepository();
        var handler = CreateHandler(
            tasks, submissions, new FakeAiOperationRepository(), new FakeUnitOfWork(), new FakeEntitlementService(), new FakeQuotaService(),
            new FakeBackgroundJobService());

        var result = await handler.Handle(
            new SubmitWritingCommand(tenantId, Guid.NewGuid(), task.Id.Value, "one two three four five"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_WORD_COUNT_OUT_OF_BOUNDS", result.Error.Code);
    }

    [Fact]
    public async Task Unknown_task_is_rejected()
    {
        var tasks = new FakeWritingTaskRepository();
        var handler = CreateHandler(
            tasks, new FakeWritingSubmissionRepository(), new FakeAiOperationRepository(), new FakeUnitOfWork(),
            new FakeEntitlementService(), new FakeQuotaService(), new FakeBackgroundJobService());

        var result = await handler.Handle(
            new SubmitWritingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "one two three"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_WRITING_TASK", result.Error.Code);
    }

    [Fact]
    public async Task Not_entitled_is_rejected_without_creating_a_submission_or_operation()
    {
        var tasks = new FakeWritingTaskRepository();
        var (tenantId, task) = SeedTask(tasks);
        var submissions = new FakeWritingSubmissionRepository();
        var operations = new FakeAiOperationRepository();
        var handler = CreateHandler(
            tasks, submissions, operations, new FakeUnitOfWork(), new FakeEntitlementService { HasFeature = false }, new FakeQuotaService(),
            new FakeBackgroundJobService());

        var result = await handler.Handle(
            new SubmitWritingCommand(tenantId, Guid.NewGuid(), task.Id.Value, "one two three four"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
        Assert.Empty(submissions.Submissions);
        Assert.Empty(operations.Operations);
    }

    [Fact]
    public async Task Quota_exceeded_short_circuits_before_any_job_is_enqueued()
    {
        var tasks = new FakeWritingTaskRepository();
        var (tenantId, task) = SeedTask(tasks);
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            tasks, new FakeWritingSubmissionRepository(), new FakeAiOperationRepository(), new FakeUnitOfWork(),
            new FakeEntitlementService(), new FakeQuotaService { Allowed = false }, backgroundJobs);

        var result = await handler.Handle(
            new SubmitWritingCommand(tenantId, Guid.NewGuid(), task.Id.Value, "one two three four"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RATE_LIMITED_AI_QUOTA_EXCEEDED", result.Error.Code);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Successful_submission_creates_a_queued_operation_and_enqueues_the_evaluation_job()
    {
        var tasks = new FakeWritingTaskRepository();
        var (tenantId, task) = SeedTask(tasks);
        var operations = new FakeAiOperationRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(
            tasks, new FakeWritingSubmissionRepository(), operations, unitOfWork, new FakeEntitlementService(), new FakeQuotaService(),
            backgroundJobs);

        var result = await handler.Handle(
            new SubmitWritingCommand(tenantId, Guid.NewGuid(), task.Id.Value, "one two three four"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Equal(1, backgroundJobs.EnqueueCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        var operation = Assert.Single(operations.Operations);
        Assert.Equal(result.Value.OperationId, operation.Id.Value);
        Assert.Equal("writing-evaluation", operation.Kind);
        Assert.NotEqual(Guid.Empty, result.Value.SubmissionId);
    }
}
