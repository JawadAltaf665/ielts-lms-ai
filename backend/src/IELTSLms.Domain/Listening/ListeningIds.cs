namespace IELTSLms.Domain.Listening;

// Strongly-typed identifiers for the Listening bounded context (BC-11, ERD §6.3).

/// <summary>Strongly-typed identifier for a <see cref="ListeningAudio"/>.</summary>
public readonly record struct ListeningAudioId(Guid Value)
{
    public static ListeningAudioId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="ListeningQuestion"/>.</summary>
public readonly record struct ListeningQuestionId(Guid Value)
{
    public static ListeningQuestionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="ListeningAttempt"/>.</summary>
public readonly record struct ListeningAttemptId(Guid Value)
{
    public static ListeningAttemptId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
