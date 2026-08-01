namespace IELTSLms.Domain.Listening;

/// <summary>
/// Playback mode for a <see cref="ListeningAttempt"/> (Sprint 5, FR-LISTEN-001). Drives both the
/// server-tracked replay cap (<see cref="ListeningReplayPolicy"/>) and transcript visibility on
/// <see cref="ListeningAudio"/>. <see cref="Domain.Reading.ReadingTrack"/> and
/// <see cref="Domain.Reading.ReadingQuestionType"/> are reused as-is for Listening's Academic/General
/// track and question-type set — every Listening item type overlaps Reading's, so no parallel enum.
/// </summary>
public enum ListeningMode
{
    Exam = 1,
    Learning = 2,
}
