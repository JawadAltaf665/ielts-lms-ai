using System.Net;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Caching;
using IELTSLms.Domain.AI;
using IELTSLms.Infrastructure.AI;
using IELTSLms.Infrastructure.AI.Claude;
using IELTSLms.Infrastructure.AI.OpenAi;
using Microsoft.Extensions.Options;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// Proves the two things Phase 7 Sprint 1 hardens: (1) both <see cref="IAiChatProvider"/> adapters,
/// called through the same port with identical logical input, produce schema-conformant canonical
/// results — the actual regression test for "is the provider abstraction real"; (2) the response cache
/// key is capability-parameterized, so two capabilities sharing identical input text never collide.
/// </summary>
public sealed class AiGatewayTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private const string SchemaJson = """{"type":"object","properties":{"corrections":{"type":"array"}},"required":["corrections"]}""";

    [Fact]
    public async Task Claude_and_OpenAi_adapters_produce_schema_conformant_canonical_results_for_identical_input()
    {
        var claudeHandler = new StubHttpMessageHandler(_ => JsonResponse("""
            {
              "id": "msg_claude_1",
              "model": "claude-3-5-haiku-20241022",
              "content": [{"type":"tool_use","name":"record_grammar_corrections","input":{"corrections":[]}}],
              "usage": {"input_tokens": 10, "output_tokens": 5}
            }
            """));
        var claude = new ClaudeChatProvider(
            new HttpClient(claudeHandler) { BaseAddress = new Uri("https://api.anthropic.com") },
            Options.Create(new ClaudeOptions { ApiKey = "test-key" }));

        var openAiHandler = new StubHttpMessageHandler(_ => JsonResponse("""
            {
              "id": "chatcmpl_openai_1",
              "model": "gpt-4o-mini",
              "choices": [{"message":{"tool_calls":[{"function":{"name":"record_grammar_corrections","arguments":"{\"corrections\":[]}"}}]}}],
              "usage": {"prompt_tokens": 10, "completion_tokens": 5}
            }
            """));
        var openAi = new OpenAiChatProvider(
            new HttpClient(openAiHandler) { BaseAddress = new Uri("https://api.openai.com") },
            Options.Create(new OpenAiOptions { ApiKey = "test-key" }));

        var request = new AiChatRequest(
            SystemPrompt: "You are an IELTS grammar coach.",
            UserContent: "He go to school.",
            ResponseSchemaName: "record_grammar_corrections",
            ResponseSchemaJson: SchemaJson,
            MaxOutputTokens: 512);

        var claudeResult = await claude.CompleteAsync(request);
        var openAiResult = await openAi.CompleteAsync(request);

        foreach (var result in new[] { claudeResult, openAiResult })
        {
            using var document = JsonDocument.Parse(result.StructuredJson);
            Assert.True(document.RootElement.TryGetProperty("corrections", out _));
            Assert.False(string.IsNullOrWhiteSpace(result.Model));
            Assert.True(result.PromptTokens >= 0);
            Assert.True(result.CompletionTokens >= 0);
            Assert.False(string.IsNullOrWhiteSpace(result.ProviderResponseId));
        }

        Assert.Equal(AiProviderKind.Claude, claude.Kind);
        Assert.Equal(AiProviderKind.OpenAi, openAi.Kind);
    }

    [Fact]
    public async Task Two_capabilities_with_identical_input_text_never_share_a_cache_entry()
    {
        var cache = new FakeCacheService();
        var provider = new CountingFakeChatProvider(AiProviderKind.Claude);
        var gateway = new AiGateway([provider], cache);

        var baseRequest = new AiGatewayRequest(
            TenantId, Feature: "grammar-coach", PromptVersion: 1, AiProviderKind.Claude,
            SystemPrompt: "system", UserInput: "identical input text", ResponseSchemaName: "schema",
            ResponseSchemaJson: SchemaJson, MaxOutputTokens: 256);

        var first = await gateway.InvokeAsync(baseRequest);
        var second = await gateway.InvokeAsync(baseRequest with { Feature = "writing-examiner" });

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, provider.InvokeCallCount);
        Assert.Equal(2, cache.StoredKeys.Count);
        Assert.NotEqual(cache.StoredKeys[0], cache.StoredKeys[1]);

        var repeat = await gateway.InvokeAsync(baseRequest);
        Assert.True(repeat.Value.FromCache);
        Assert.Equal(2, provider.InvokeCallCount);
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private sealed class CountingFakeChatProvider(AiProviderKind kind) : IAiChatProvider
    {
        public int InvokeCallCount { get; private set; }

        public AiProviderKind Kind => kind;

        public Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            InvokeCallCount++;
            return Task.FromResult(new AiChatResult(
                StructuredJson: """{"corrections":[]}""", Model: "fake-model", PromptTokens: 1, CompletionTokens: 1,
                CostMinorUnits: 0, CostCurrency: "USD", ProviderResponseId: $"fake_{InvokeCallCount}"));
        }
    }

    private sealed class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _store = [];
        public List<string> StoredKeys { get; } = [];

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class =>
            Task.FromResult(_store.TryGetValue(key, out var value) ? (T)value : null);

        public Task SetAsync<T>(
            string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
            where T : class
        {
            _store[key] = value;
            StoredKeys.Add(key);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key, Func<CancellationToken, Task<T>> factory, CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (cached is not null)
            {
                return cached;
            }

            var created = await factory(cancellationToken).ConfigureAwait(false);
            await SetAsync(key, created, options, cancellationToken).ConfigureAwait(false);
            return created;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _store.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
