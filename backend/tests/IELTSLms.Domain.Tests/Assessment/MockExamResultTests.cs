using IELTSLms.Domain.Assessment;

namespace IELTSLms.Domain.Tests.Assessment;

public sealed class MockExamResultTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static MockExamResult CreatePending() =>
        MockExamResult.CreatePending(
            MockExamResultId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), MockExamId.From(Guid.NewGuid()), Now);

    [Fact]
    public void Finalize_fails_while_a_section_band_is_missing()
    {
        var result = CreatePending();
        result.RecordSectionBand(MockExamSectionType.Reading, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Listening, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Writing, 6.5m, Now);
        // Speaking never recorded.

        var finalize = result.Finalize(Now.AddMinutes(1));

        Assert.True(finalize.IsFailure);
        Assert.Equal("BUSINESS_MOCK_EXAM_SECTIONS_INCOMPLETE", finalize.Error.Code);
        Assert.False(result.IsFinalized);
    }

    [Fact]
    public void Finalize_computes_the_overall_band_once_all_four_sections_are_recorded()
    {
        var result = CreatePending();
        result.RecordSectionBand(MockExamSectionType.Reading, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Listening, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Writing, 6.5m, Now);
        result.RecordSectionBand(MockExamSectionType.Speaking, 7.5m, Now);

        var finalize = result.Finalize(Now.AddMinutes(1));

        Assert.True(finalize.IsSuccess);
        Assert.True(result.IsFinalized);
        Assert.Equal(Now.AddMinutes(1), result.FinalizedAtUtc);
        Assert.Equal(BandCalculationService.CalculateOverallBand(7.0m, 7.0m, 6.5m, 7.5m), result.OverallBand);
    }

    [Fact]
    public void Finalize_is_idempotent()
    {
        var result = CreatePending();
        result.RecordSectionBand(MockExamSectionType.Reading, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Listening, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Writing, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Speaking, 7.0m, Now);
        result.Finalize(Now.AddMinutes(1));

        var second = result.Finalize(Now.AddMinutes(2));

        Assert.True(second.IsSuccess);
        Assert.Equal(Now.AddMinutes(1), result.FinalizedAtUtc);
    }

    [Fact]
    public void RecordSectionBand_fails_once_finalized()
    {
        var result = CreatePending();
        result.RecordSectionBand(MockExamSectionType.Reading, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Listening, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Writing, 7.0m, Now);
        result.RecordSectionBand(MockExamSectionType.Speaking, 7.0m, Now);
        result.Finalize(Now.AddMinutes(1));

        var record = result.RecordSectionBand(MockExamSectionType.Reading, 6.0m, Now.AddMinutes(2));

        Assert.True(record.IsFailure);
        Assert.Equal("CONFLICT_MOCK_EXAM_RESULT_ALREADY_FINALIZED", record.Error.Code);
    }
}
