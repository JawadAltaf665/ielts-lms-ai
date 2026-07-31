using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;

namespace IELTSLms.Application.Listening.GetListeningAudio;

/// <summary>Gets an audio and its questions. Answer keys are never included; transcript is included only in <see cref="ListeningMode.Learning"/>.</summary>
public sealed record GetListeningAudioQuery(Guid TenantId, Guid AudioId, ListeningMode Mode) : IQuery<ListeningAudioDetailResponse>;

public sealed record ListeningAudioDetailResponse(
    Guid Id, Guid? TenantId, string Title, string Track, Guid? CategoryId, int DurationSeconds, bool IsGlobal,
    string? TranscriptText, IReadOnlyList<ListeningQuestionSummaryResponse> Questions);

public sealed record ListeningQuestionSummaryResponse(Guid Id, string Type, string Prompt, int Sequence);
