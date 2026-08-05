using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Caching;
using IELTSLms.Domain.AI;
using IELTSLms.Infrastructure.AI;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// Sprint 8's cache-hit acceptance criterion: two learners submitting the same wrong answer to the same
/// passage/question produce one provider call and two served responses. The composed grounding text
/// (passage + question + wrong answer, see <c>GetReadingAttemptCoachingQueryHandler</c>) is identical
/// regardless of which learner it came from, so it resolves to the same gateway cache key — proven here
/// directly at <see cref="AiGateway"/> without needing a live provider or database.
/// </summary>
public sealed class ReadingListeningCoachCacheTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private const string SchemaJson =
        """{"type":"object","properties":{"explanation":{"type":"string"}},"required":["explanation"]}""";

    [Fact]
    public async Task Identical_wrong_answer_input_from_two_learners_hits_the_cache_on_the_second_call()
    {
        var cache = new FakeCacheService();
        var provider = new CountingFakeChatProvider();
        var gateway = new AiGateway([provider], cache);

        var groundedText =
            "Passage: Ocean Currents\nOceans move heat around the globe.\n\n" +
            "Question: What drives ocean currents?\nSubmitted answer: wind\nCorrect answer: heat";

        var request = new AiGatewayRequest(
            TenantId, Feature: "reading-coach", PromptVersion: 1, AiProviderKind.Claude, SystemPrompt: "system",
            UserInput: groundedText, ResponseSchemaName: "record_reading_coaching", ResponseSchemaJson: SchemaJson,
            MaxOutputTokens: 256);

        // Learner A asks first.
        var learnerAResult = await gateway.InvokeAsync(request);
        // Learner B gives the identical wrong answer to the identical question — same request shape.
        var learnerBResult = await gateway.InvokeAsync(request);

        Assert.True(learnerAResult.IsSuccess);
        Assert.True(learnerBResult.IsSuccess);
        Assert.False(learnerAResult.Value.FromCache);
        Assert.True(learnerBResult.Value.FromCache);
        Assert.Equal(1, provider.InvokeCallCount);
        Assert.Equal(learnerAResult.Value.OutputJson, learnerBResult.Value.OutputJson);
    }

    private sealed class CountingFakeChatProvider : IAiChatProvider
    {
        public int InvokeCallCount { get; private set; }

        public AiProviderKind Kind => AiProviderKind.Claude;

        public Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            InvokeCallCount++;
            return Task.FromResult(new AiChatResult(
                StructuredJson: """{"explanation":"Ocean currents are driven by heat, not wind.","strategyTips":[]}""",
                Model: "fake-model", PromptTokens: 1, CompletionTokens: 1, CostMinorUnits: 0, CostCurrency: "USD",
                ProviderResponseId: $"fake_{InvokeCallCount}"));
        }
    }

    private sealed class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _store = [];

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class =>
            Task.FromResult(_store.TryGetValue(key, out var value) ? (T)value : null);

        public Task SetAsync<T>(
            string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
            where T : class
        {
            _store[key] = value;
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
