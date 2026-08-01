namespace IELTSLms.Application.Exceptions;

/// <summary>
/// Translated from a PostgreSQL unique-violation (23505) by <c>ApplicationDbContext.SaveChangesAsync</c>
/// — same translation pattern as <see cref="ConcurrencyConflictException"/>, so a handler racing another
/// request for the same partial-unique row (e.g. the single-active-mock-exam-per-user index) can catch
/// this and return a clean business <c>Result.Failure</c> instead of a raw 500.
/// </summary>
public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string entityName, string? constraintName = null)
        : base($"A unique constraint was violated on {entityName}{(constraintName is not null ? $" ('{constraintName}')" : "")}.")
    {
        EntityName = entityName;
        ConstraintName = constraintName;
    }

    public string EntityName { get; }

    public string? ConstraintName { get; }
}
