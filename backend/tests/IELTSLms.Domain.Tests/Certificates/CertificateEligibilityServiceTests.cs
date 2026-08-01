using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Certificates;

namespace IELTSLms.Domain.Tests.Certificates;

public sealed class CertificateEligibilityServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static MockExamResult FinalizedWith(decimal reading, decimal listening, decimal writing, decimal speaking)
    {
        var result = MockExamResult.CreatePending(
            MockExamResultId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), MockExamId.From(Guid.NewGuid()), Now);
        result.RecordSectionBand(MockExamSectionType.Reading, reading, Now);
        result.RecordSectionBand(MockExamSectionType.Listening, listening, Now);
        result.RecordSectionBand(MockExamSectionType.Writing, writing, Now);
        result.RecordSectionBand(MockExamSectionType.Speaking, speaking, Now);
        result.Finalize(Now);
        return result;
    }

    [Fact]
    public void IsEligible_is_true_for_a_finalized_result_at_or_above_the_qualifying_band()
    {
        var result = FinalizedWith(6.0m, 6.0m, 5.0m, 5.0m);

        Assert.True(result.IsFinalized);
        Assert.True(CertificateEligibilityService.IsEligible(result));
    }

    [Fact]
    public void IsEligible_is_false_below_the_qualifying_band()
    {
        var result = FinalizedWith(3.0m, 3.0m, 3.0m, 3.0m);

        Assert.False(CertificateEligibilityService.IsEligible(result));
    }

    [Fact]
    public void IsEligible_is_false_for_a_result_that_is_not_finalized()
    {
        var result = MockExamResult.CreatePending(
            MockExamResultId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), MockExamId.From(Guid.NewGuid()), Now);

        Assert.False(CertificateEligibilityService.IsEligible(result));
    }
}
