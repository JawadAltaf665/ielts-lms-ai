using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using IELTSLms.Abstractions.AI;
using IELTSLms.Domain.AI;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.AI.Claude;

/// <summary>
/// <see cref="IAiChatProvider"/> implementation over Anthropic's Messages API — a hand-rolled typed
/// <see cref="HttpClient"/> call rather than a vendor SDK (no Anthropic/OpenAI package existed in this
/// solution; a single JSON POST is smaller and more auditable than a new dependency for one endpoint).
/// Structured output is requested via a forced tool-choice: the caller's JSON Schema becomes a single
/// tool the model is required to call, and the tool's <c>input</c> is the structured payload — this is
/// the anti-corruption-layer translation (DDD §6.2) that keeps <see cref="IAiChatProvider"/> itself
/// vendor-agnostic.
/// </summary>
public sealed class ClaudeChatProvider(HttpClient httpClient, IOptions<ClaudeOptions> options) : IAiChatProvider
{
    private const string MessagesPath = "/v1/messages";

    public AiProviderKind Kind => AiProviderKind.Claude;

    public async Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Ai:Claude:ApiKey is not configured. Set it via configuration/environment before using AI features.");
        }

        var schema = JsonNode.Parse(request.ResponseSchemaJson)
            ?? throw new InvalidOperationException("ResponseSchemaJson must be valid JSON.");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, MessagesPath);
        httpRequest.Headers.Add("x-api-key", apiKey);
        httpRequest.Headers.Add("anthropic-version", options.Value.AnthropicVersion);
        httpRequest.Content = JsonContent.Create(new
        {
            model = options.Value.Model,
            max_tokens = request.MaxOutputTokens,
            system = request.SystemPrompt,
            messages = new[] { new { role = "user", content = request.UserContent } },
            tools = new[]
            {
                new
                {
                    name = request.ResponseSchemaName,
                    description = "Record the structured result for this request.",
                    input_schema = schema,
                },
            },
            tool_choice = new { type = "tool", name = request.ResponseSchemaName },
        });

        using var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        httpResponse.EnsureSuccessStatusCode();

        var payload = await httpResponse.Content
            .ReadFromJsonAsync<JsonElement>(cancellationToken)
            .ConfigureAwait(false);

        var toolUse = payload.GetProperty("content")
            .EnumerateArray()
            .First(block => block.GetProperty("type").GetString() == "tool_use");

        var usage = payload.GetProperty("usage");
        var promptTokens = usage.GetProperty("input_tokens").GetInt32();
        var completionTokens = usage.GetProperty("output_tokens").GetInt32();

        return new AiChatResult(
            StructuredJson: toolUse.GetProperty("input").GetRawText(),
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
