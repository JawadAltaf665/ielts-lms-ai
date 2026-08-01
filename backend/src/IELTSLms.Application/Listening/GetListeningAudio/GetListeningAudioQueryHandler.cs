using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.GetListeningAudio;

public sealed class GetListeningAudioQueryHandler(IListeningAudioRepository audios, IListeningQuestionRepository questions)
    : IQueryHandler<GetListeningAudioQuery, ListeningAudioDetailResponse>
{
    public async Task<Result<ListeningAudioDetailResponse>> Handle(GetListeningAudioQuery request, CancellationToken cancellationToken)
    {
        var audioId = ListeningAudioId.From(request.AudioId);
        var audio = await audios.GetVisibleByIdAsync(request.TenantId, audioId, cancellationToken).ConfigureAwait(false);
        if (audio is null)
        {
            return Result.Failure<ListeningAudioDetailResponse>(ListeningErrors.AudioNotFound);
        }

        var audioQuestions = await questions.ListByAudioAsync(audioId, cancellationToken).ConfigureAwait(false);

        // Content-authored answer-key material — exposed only in learning mode (Sprint 5 business
        // rule). The endpoint parses `mode` against a strict enum allowlist defaulting to Exam, so no
        // crafted query value can slip past this check.
        var transcript = request.Mode == ListeningMode.Learning ? audio.TranscriptText : null;

        return Result.Success(new ListeningAudioDetailResponse(
            audio.Id.Value, audio.TenantId, audio.Title, audio.Track.ToString(), audio.CategoryId, audio.DurationSeconds, audio.IsGlobal,
            transcript,
            audioQuestions.Select(q => new ListeningQuestionSummaryResponse(q.Id.Value, q.Type.ToString(), q.Prompt, q.Sequence)).ToArray()));
    }
}
