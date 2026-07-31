using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Listening;

/// <summary>
/// A single question on a <see cref="ListeningAudio"/> (ERD ENT-025). Reuses <see cref="ReadingQuestionType"/>
/// and <see cref="ReadingAnswerGrader"/> as-is (Sprint 5 acceptance criteria: "reuses ... applicable
/// scoring strategies — no duplicated logic") since every in-scope Listening item type overlaps
/// Reading's set. <see cref="AnswerKey"/> is hidden from learners at the API boundary, same as
/// <see cref="ReadingQuestion.AnswerKey"/>.
/// </summary>
public sealed class ListeningQuestion : AuditableEntity<ListeningQuestionId>
{
    private ListeningQuestion(
        ListeningQuestionId id, Guid? tenantId, ListeningAudioId audioId, ReadingQuestionType type, string prompt,
        string answerKey, int sequence, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        ListeningAudioId = audioId;
        Type = type;
        Prompt = prompt;
        AnswerKey = answerKey;
        Sequence = sequence;
        CreatedAtUtc = nowUtc;
    }

    private ListeningQuestion()
    {
        Prompt = null!;
        AnswerKey = null!;
    }

    public Guid? TenantId { get; private init; }

    public ListeningAudioId ListeningAudioId { get; private init; }

    public ReadingQuestionType Type { get; private set; }

    public string Prompt { get; private set; }

    /// <summary>JSON array of acceptable answers (<see cref="ReadingAnswerGrader"/>) — never free text.</summary>
    public string AnswerKey { get; private set; }

    public int Sequence { get; private set; }

    public static Result<ListeningQuestion> Create(
        ListeningQuestionId id, Guid? tenantId, ListeningAudioId audioId, ReadingQuestionType type, string prompt,
        string answerKey, int sequence, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Result.Failure<ListeningQuestion>(Error.Validation(
                "VALIDATION_LISTENING_QUESTION_PROMPT_REQUIRED", "Prompt is required."));
        }

        if (string.IsNullOrWhiteSpace(answerKey))
        {
            return Result.Failure<ListeningQuestion>(Error.Validation(
                "VALIDATION_LISTENING_QUESTION_ANSWER_KEY_REQUIRED", "Answer key is required."));
        }

        if (sequence < 0)
        {
            return Result.Failure<ListeningQuestion>(Error.Validation(
                "VALIDATION_LISTENING_QUESTION_SEQUENCE", "Sequence must be zero or greater."));
        }

        return Result.Success(new ListeningQuestion(id, tenantId, audioId, type, prompt, answerKey, sequence, nowUtc));
    }

    /// <summary>Whether <paramref name="submittedAnswer"/> matches this question's answer key (reuses <see cref="ReadingAnswerGrader"/>).</summary>
    public bool Grade(string? submittedAnswer) => ReadingAnswerGrader.IsCorrect(Type, AnswerKey, submittedAnswer);
}
