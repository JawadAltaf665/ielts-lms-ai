using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using IELTSLms.Abstractions.AI;
using IELTSLms.Domain.AI;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.AI.OpenAi;

/// <summary>
/// <see cref="IAiChatProvider"/> implementation over OpenAI's Chat Completions API — the second adapter
/// behind the port (AI Architecture §3), proving the abstraction now that a capability could route to
/// either vendor. Hand-rolled <see cref="HttpClient"/> call, mirroring
/// <see cref="Claude.ClaudeChatProvider"/>'s approach rather than adding a vendor SDK. Structured output
/// is requested via a forced tool call (OpenAI's <c>tool_choice</c>), the same anti-corruption-layer
/// translation Claude's tool-use forcing performs, so <see cref="IAiChatProvider"/> itself stays
/// vendor-agnostic.
/// </summary>
public sealed class OpenAiChatProvider(HttpClient httpClient, IOptions<OpenAiOptions> options) : IAiChatProvider
{
    private const string ChatCompletionsPath = "/v1/chat/completions";

    public AiProviderKind Kind => AiProviderKind.OpenAi;

    public async Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Ai:OpenAi:ApiKey is not configured. Set it via configuration/environment before using AI features.");
        }

        var schema = JsonNode.Parse(request.ResponseSchemaJson)
            ?? throw new InvalidOperationException("ResponseSchemaJson must be valid JSON.");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsPath);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = JsonContent.Create(new
        {
            model = options.Value.Model,
            max_tokens = request.MaxOutputTokens,
            messages = new[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserContent },
            },
            tools = new[]
            {
                new
                {
                    type = "function",
                    function = new
                    {
                        name = request.ResponseSchemaName,
                        description = "Record the structured result for this request.",
                        parameters = schema,
                    },
                },
            },
            tool_choice = new { type = "function", function = new { name = request.ResponseSchemaName } },
        });

        using var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        httpResponse.EnsureSuccessStatusCode();

        var payload = await httpResponse.Content
            .ReadFromJsonAsync<JsonElement>(cancellationToken)
            .ConfigureAwait(false);

        var toolCall = payload.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("tool_calls")[0];

        var usage = payload.GetProperty("usage");
        var promptTokens = usage.GetProperty("prompt_tokens").GetInt32();
        var completionTokens = usage.GetProperty("completion_tokens").GetInt32();

        return new AiChatResult(
            StructuredJson: toolCall.GetProperty("function").GetProperty("arguments").GetString() ?? "{}",
            Model: payload.GetProperty("model").GetString() ?? options.Value.Model,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens,
            CostMinorUnits: ComputeCostMinorUnits(promptTokens, completionTokens),
            CostCurrency: "USD",
            ProviderResponseId: payload.TryGetProperty("id", out var id) ? id.GetString() : null);
    }

    private long ComputeCostMinorUnits(int promptTokens, int completionTokens)
    {
        var inputCost = promptTokens / 1_000_000m * options.Value.InputCostPerMillionTokensUsd;
        var outputCost = completionTokens / 1_000_000m * options.Value.OutputCostPerMillionTokensUsd;
        return (long)Math.Round((inputCost + outputCost) * 100m, MidpointRounding.AwayFromZero);
    }
}
