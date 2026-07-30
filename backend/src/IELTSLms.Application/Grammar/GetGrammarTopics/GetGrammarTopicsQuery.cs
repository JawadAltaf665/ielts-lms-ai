using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Grammar.CreateGrammarTopic;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Grammar.GetGrammarTopics;

/// <summary>Lists grammar topics visible to the caller's tenant (global + tenant-authored), filterable by track/difficulty.</summary>
public sealed record GetGrammarTopicsQuery(Guid TenantId, GrammarTrack? Track, GrammarDifficulty? Difficulty, int Page, int PageSize)
    : IQuery<PagedList<GrammarTopicResponse>>;
