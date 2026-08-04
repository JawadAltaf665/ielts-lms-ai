namespace IELTSLms.Domain.SentenceBuilder;

/// <summary>Difficulty tier of a <see cref="SentenceExercise"/> — same three-tier shape as
/// <see cref="Grammar.GrammarDifficulty"/>, kept as its own enum per bounded context (mirrors
/// Vocabulary's separate <c>VocabularyDifficultyLevel</c>).</summary>
public enum SentenceDifficulty
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
}
