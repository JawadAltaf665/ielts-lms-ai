using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Abstractions.AI;

/// <summary>
/// Transcribes learner-recorded audio to text for the Speaking Evaluation Workflow. A dedicated port,
/// not routed through <see cref="IAiGateway"/> — that port is shaped for text-in/JSON-out structured
/// calls, unsuited to binary audio input. Implementation lives in Infrastructure/AI (Speaking sprint's
/// own guidance: isolate the STT SDK/HTTP call behind a port, never call it from Application code).
/// </summary>
public interface ISpeechToTextProvider
{
    Task<Result<SpeechToTextResult>> TranscribeAsync(SpeechToTextRequest request, CancellationToken cancellationToken = default);
}

public sealed record SpeechToTextRequest(Stream Audio, string ContentType, string FileName);

public sealed record SpeechToTextResult(string Text, string? Language, int LatencyMs);
