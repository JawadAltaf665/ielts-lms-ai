namespace IELTSLms.Api.Contracts;

/// <summary><see cref="ContentId"/> is the passage/audio id for Reading/Listening (required); ignored for Writing/Speaking.</summary>
public sealed record StartMockExamSectionRequest(Guid? ContentId);
