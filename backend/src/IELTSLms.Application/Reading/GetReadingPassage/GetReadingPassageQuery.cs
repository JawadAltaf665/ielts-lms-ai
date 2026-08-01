using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Reading.GetReadingPassage;

/// <summary>Gets a passage and its questions. Answer keys are never included (hidden from learners at the API boundary).</summary>
public sealed record GetReadingPassageQuery(Guid TenantId, Guid PassageId) : IQuery<ReadingPassageDetailResponse>;

public sealed record ReadingPassageDetailResponse(
    Guid Id, Guid? TenantId, string Title, string PassageText, string Track, string Difficulty, Guid? CategoryId,
    int DurationSeconds, bool IsGlobal, IReadOnlyList<ReadingQuestionSummaryResponse> Questions);

public sealed record ReadingQuestionSummaryResponse(Guid Id, string Type, string Prompt, int Sequence);
