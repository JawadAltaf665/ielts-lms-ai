using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Assessment;

/// <summary>
/// The aggregated result of a <see cref="MonthlyMockExam"/> (ERD ENT-037), 1:1 with its exam. Created
/// pending (all bands <see langword="null"/>) alongside the exam at <see cref="MockExamFactory.Start"/>
/// so the result resource always exists once an exam has started — <see cref="IsFinalized"/> is what
/// distinguishes "still in progress" from "done" (GET .../result reports a <c>pending</c> status until
/// then, per Sprint 8 API notes). <see cref="Finalize"/> is the only place <see cref="OverallBand"/> is
/// computed, and only once every section band is recorded (ERD: "Finalized only when all sections evaluated").
/// </summary>
public sealed class MockExamResult : TenantAuditableEntity<MockExamResultId>
{
    private MockExamResult(MockExamResultId id, Guid tenantId, Guid userId, MockExamId mockExamId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        MockExamId = mockExamId;
        CreatedAtUtc = nowUtc;
    }

    private MockExamResult()
    {
    }

    public Guid UserId { get; private init; }

    public MockExamId MockExamId { get; private init; }

    public decimal? ReadingBand { get; private set; }

    public decimal? ListeningBand { get; private set; }

    public decimal? WritingBand { get; private set; }

    public decimal? SpeakingBand { get; private set; }

    public decimal? OverallBand { get; private set; }

    public bool IsFinalized { get; private set; }

    public DateTimeOffset? FinalizedAtUtc { get; private set; }

    public bool HasAllSectionBands =>
        ReadingBand is not null && ListeningBand is not null && WritingBand is not null && SpeakingBand is not null;

    public static MockExamResult CreatePending(MockExamResultId id, Guid tenantId, Guid userId, MockExamId mockExamId, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, mockExamId, nowUtc);

    /// <summary>Records one section's band as it's reported by that skill's own scoring/evaluation. Idempotent: overwrites with the latest reported value.</summary>
    public Result RecordSectionBand(MockExamSectionType section, decimal band, DateTimeOffset nowUtc)
    {
        if (IsFinalized)
        {
            return Result.Failure(MockExamResultDomainErrors.AlreadyFinalized);
        }

        switch (section)
        {
            case MockExamSectionType.Reading:
                ReadingBand = band;
                break;
            case MockExamSectionType.Listening:
                ListeningBand = band;
                break;
            case MockExamSectionType.Writing:
                WritingBand = band;
                break;
            case MockExamSectionType.Speaking:
                SpeakingBand = band;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(section));
        }

        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>Computes and stamps <see cref="OverallBand"/> via <see cref="BandCalculationService"/>. Idempotent once finalized.</summary>
    public Result Finalize(DateTimeOffset nowUtc)
    {
        if (IsFinalized)
        {
            return Result.Success();
        }

        if (!HasAllSectionBands)
        {
            return Result.Failure(MockExamResultDomainErrors.SectionsIncomplete);
        }

        OverallBand = BandCalculationService.CalculateOverallBand(ReadingBand!.Value, ListeningBand!.Value, WritingBand!.Value, SpeakingBand!.Value);
        IsFinalized = true;
        FinalizedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class MockExamResultDomainErrors
{
    public static readonly Error AlreadyFinalized = Error.Conflict(
        "CONFLICT_MOCK_EXAM_RESULT_ALREADY_FINALIZED", "This mock exam result has already been finalized.");

    public static readonly Error SectionsIncomplete = Error.Conflict(
        "BUSINESS_MOCK_EXAM_SECTIONS_INCOMPLETE", "Not all sections have been evaluated yet.");
}
