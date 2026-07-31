using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Reading;

/// <summary>
/// A single question on a <see cref="ReadingPassage"/> (ERD ENT-022). <see cref="AnswerKey"/> is a
/// structured JSON array of acceptable answers (see <see cref="ReadingAnswerGrader"/>), stored as
/// <c>jsonb</c> — never free text. Hidden from learners at the API boundary; that shaping is an
/// Api/Application concern, not Domain's.
/// </summary>
public sealed class ReadingQuestion : AuditableEntity<ReadingQuestionId>
{
    private ReadingQuestion(
        ReadingQuestionId id, Guid? tenantId, ReadingPassageId passageId, ReadingQuestionType type,
        string prompt, string answerKey, int sequence, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        ReadingPassageId = passageId;
        Type = type;
        Prompt = prompt;
        AnswerKey = answerKey;
        Sequence = sequence;
        CreatedAtUtc = nowUtc;
    }

    private ReadingQuestion()
    {
        Prompt = null!;
        AnswerKey = null!;
    }

    public Guid? TenantId { get; private init; }

    public ReadingPassageId ReadingPassageId { get; private init; }

    public ReadingQuestionType Type { get; private set; }

    public string Prompt { get; private set; }

    /// <summary>JSON array of acceptable answers (structured, per <see cref="ReadingAnswerGrader"/>).</summary>
    public string AnswerKey { get; private set; }

    public int Sequence { get; private set; }

    public static Result<ReadingQuestion> Create(
        ReadingQuestionId id, Guid? tenantId, ReadingPassageId passageId, ReadingQuestionType type,
        string prompt, string answerKey, int sequence, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Result.Failure<ReadingQuestion>(Error.Validation(
                "VALIDATION_READING_QUESTION_PROMPT_REQUIRED", "Prompt is required."));
        }

        if (string.IsNullOrWhiteSpace(answerKey))
        {
            return Result.Failure<ReadingQuestion>(Error.Validation(
                "VALIDATION_READING_QUESTION_ANSWER_KEY_REQUIRED", "Answer key is required."));
        }

        if (sequence < 0)
        {
            return Result.Failure<ReadingQuestion>(Error.Validation(
                "VALIDATION_READING_QUESTION_SEQUENCE", "Sequence must be zero or greater."));
        }

        return Result.Success(new ReadingQuestion(id, tenantId, passageId, type, prompt, answerKey, sequence, nowUtc));
    }

    /// <summary>Whether <paramref name="submittedAnswer"/> matches this question's answer key.</summary>
    public bool Grade(string? submittedAnswer) => ReadingAnswerGrader.IsCorrect(Type, AnswerKey, submittedAnswer);
}
