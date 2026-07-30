using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Grammar;

/// <summary>
/// A single question within a <see cref="GrammarExercise"/> (ERD ENT-016, mirrors ENT-022
/// ReadingQuestions). <see cref="AnswerKey"/> is a structured JSON array of acceptable answers
/// (see <see cref="GrammarAnswerGrader"/>), stored as <c>jsonb</c> — never free text. Hidden from
/// learners at the API boundary; that shaping is an Api/Application concern, not Domain's.
/// </summary>
public sealed class GrammarQuestion : AuditableEntity<GrammarQuestionId>
{
    private GrammarQuestion(
        GrammarQuestionId id, Guid? tenantId, GrammarExerciseId exerciseId, GrammarQuestionType type,
        string prompt, string answerKey, int sequence, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        GrammarExerciseId = exerciseId;
        Type = type;
        Prompt = prompt;
        AnswerKey = answerKey;
        Sequence = sequence;
        CreatedAtUtc = nowUtc;
    }

    private GrammarQuestion()
    {
        Prompt = null!;
        AnswerKey = null!;
    }

    public Guid? TenantId { get; private init; }

    public GrammarExerciseId GrammarExerciseId { get; private init; }

    public GrammarQuestionType Type { get; private set; }

    public string Prompt { get; private set; }

    /// <summary>JSON array of acceptable answers (structured, per <see cref="GrammarAnswerGrader"/>).</summary>
    public string AnswerKey { get; private set; }

    public int Sequence { get; private set; }

    public static Result<GrammarQuestion> Create(
        GrammarQuestionId id, Guid? tenantId, GrammarExerciseId exerciseId, GrammarQuestionType type,
        string prompt, string answerKey, int sequence, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Result.Failure<GrammarQuestion>(Error.Validation(
                "VALIDATION_GRAMMAR_QUESTION_PROMPT_REQUIRED", "Prompt is required."));
        }

        if (string.IsNullOrWhiteSpace(answerKey))
        {
            return Result.Failure<GrammarQuestion>(Error.Validation(
                "VALIDATION_GRAMMAR_QUESTION_ANSWER_KEY_REQUIRED", "Answer key is required."));
        }

        if (sequence < 0)
        {
            return Result.Failure<GrammarQuestion>(Error.Validation(
                "VALIDATION_GRAMMAR_QUESTION_SEQUENCE", "Sequence must be zero or greater."));
        }

        return Result.Success(new GrammarQuestion(id, tenantId, exerciseId, type, prompt, answerKey, sequence, nowUtc));
    }

    /// <summary>Whether <paramref name="submittedAnswer"/> matches this question's answer key.</summary>
    public bool Grade(string? submittedAnswer) => GrammarAnswerGrader.IsCorrect(Type, AnswerKey, submittedAnswer);

    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
