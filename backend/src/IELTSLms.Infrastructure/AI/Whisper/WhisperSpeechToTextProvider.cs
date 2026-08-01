using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.AI.Whisper;

/// <summary>
/// <see cref="ISpeechToTextProvider"/> implementation over OpenAI's Whisper transcription API — a
/// hand-rolled typed <see cref="HttpClient"/> call rather than a vendor SDK, same "no new dependency for
/// one endpoint" reasoning as <see cref="Claude.ClaudeChatProvider"/>. Any non-success response or
/// transport failure surfaces as <see cref="ErrorType.ServiceUnavailable"/> (503) rather than an empty
/// transcript, so a provider outage fails the evaluation loudly instead of silently.
/// </summary>
public sealed class WhisperSpeechToTextProvider(HttpClient httpClient, IOptions<WhisperOptions> options) : ISpeechToTextProvider
{
    private const string TranscriptionsPath = "/v1/audio/transcriptions";

    private static readonly Error NotConfigured = Error.ServiceUnavailable(
        "SERVICE_UNAVAILABLE_STT_PROVIDER", "The speech-to-text provider is not configured.");

    private static readonly Error TranscriptionFailed = Error.ServiceUnavailable(
        "SERVICE_UNAVAILABLE_STT_PROVIDER", "The speech-to-text provider failed to transcribe this recording.");

    public async Task<Result<SpeechToTextResult>> TranscribeAsync(
        SpeechToTextRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Result.Failure<SpeechToTextResult>(NotConfigured);
        }

        using var content = new MultipartFormDataContent
        {
            { new StringContent(options.Value.Model), "model" },
            { new StringContent("verbose_json"), "response_format" },
        };
        using var audioContent = new StreamContent(request.Audio);
        audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
        content.Add(audioContent, "file", request.FileName);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, TranscriptionsPath) { Content = content };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<SpeechToTextResult>(TranscriptionFailed);
        }

        using (httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Failure<SpeechToTextResult>(TranscriptionFailed);
            }

            var payload = await httpResponse.Content
                .ReadFromJsonAsync<JsonElement>(cancellationToken)
                .ConfigureAwait(false);

            var text = payload.TryGetProperty("text", out var textElement) ? textElement.GetString() : null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return Result.Failure<SpeechToTextResult>(TranscriptionFailed);
            }

            var language = payload.TryGetProperty("language", out var languageElement) ? languageElement.GetString() : null;

            return Result.Success(new SpeechToTextResult(text, language, (int)stopwatch.ElapsedMilliseconds));
        }
    }
}
