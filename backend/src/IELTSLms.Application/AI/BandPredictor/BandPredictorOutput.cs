namespace IELTSLms.Application.AI.BandPredictor;

/// <summary>Structured Band Predictor output (Agentic AI Architecture §13.6): predicted overall/section
/// bands plus confidence. Per-section bands are nullable — the model omits a section it has no history
/// for rather than guessing.</summary>
public sealed record BandPredictorOutput(
    decimal? OverallBand, decimal? ReadingBand, decimal? ListeningBand, decimal? WritingBand, decimal? SpeakingBand, decimal Confidence);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained (AI Architecture §8).</summary>
internal static class BandPredictorSchema
{
    public const string Name = "record_band_prediction";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "overallBand": { "type": ["number", "null"], "description": "Predicted overall IELTS band, 0-9 in 0.5 steps, or null if the history is too sparse to estimate." },
            "readingBand": { "type": ["number", "null"], "description": "Predicted Reading band, or null if no Reading history exists." },
            "listeningBand": { "type": ["number", "null"], "description": "Predicted Listening band, or null if no Listening history exists." },
            "writingBand": { "type": ["number", "null"], "description": "Predicted Writing band, or null if no Writing history exists." },
            "speakingBand": { "type": ["number", "null"], "description": "Predicted Speaking band, or null if no Speaking history exists." },
            "confidence": { "type": "number", "description": "Confidence in this prediction, 0 to 1. Use a low value for sparse or inconsistent history." }
          },
          "required": ["overallBand", "readingBand", "listeningBand", "writingBand", "speakingBand", "confidence"]
        }
        """;
}
