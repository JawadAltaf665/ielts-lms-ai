using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary;

internal static class VocabularyErrors
{
    public static readonly Error WordNotFound = Error.NotFound(
        "NOT_FOUND_VOCABULARY_WORD", "Vocabulary word not found.");

    public static readonly Error WordTaken = Error.Conflict(
        "CONFLICT_VOCABULARY_WORD_TAKEN", "A vocabulary word with this sense already exists.");

    public static readonly Error WordReadOnly = Error.Forbidden(
        "AUTHZ_VOCABULARY_WORD_READ_ONLY", "Global vocabulary words are read-only.");
}
