using IELTSLms.Abstractions.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeSpeechToTextProvider : ISpeechToTextProvider
{
    public int TranscribeCallCount { get; private set; }

    public Result<SpeechToTextResult>? NextResult { get; set; }

    public Task<Result<SpeechToTextResult>> TranscribeAsync(SpeechToTextRequest request, CancellationToken cancellationToken = default)
    {
        TranscribeCallCount++;
        return Task.FromResult(NextResult ?? Result.Success(new SpeechToTextResult("This is what the learner said.", "en", 500)));
    }
}
