namespace IELTSLms.Api.Contracts;

public sealed record GenerateQuestionVariantRequest(Guid SourceQuestionId, int DifficultyDelta);
