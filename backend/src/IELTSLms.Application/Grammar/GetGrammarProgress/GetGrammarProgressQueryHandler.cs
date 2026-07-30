using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.GetGrammarProgress;

public sealed class GetGrammarProgressQueryHandler(IGrammarProgressRepository progress)
    : IQueryHandler<GetGrammarProgressQuery, IReadOnlyList<GrammarProgressResponse>>
{
    private static readonly Error NotFound = Error.NotFound(
        "NOT_FOUND_GRAMMAR_PROGRESS", "Grammar progress not found.");

    public async Task<Result<IReadOnlyList<GrammarProgressResponse>>> Handle(
        GetGrammarProgressQuery request, CancellationToken cancellationToken)
    {
        if (request.CallerUserId != request.RequestedUserId)
        {
            return Result.Failure<IReadOnlyList<GrammarProgressResponse>>(NotFound);
        }

        var rows = await progress.ListByUserAsync(request.TenantId, request.RequestedUserId, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<GrammarProgressResponse> response = rows
            .Select(p => new GrammarProgressResponse(p.GrammarTopicId.Value, p.MasteryLevel.ToString(), p.AttemptCount, p.LastAttemptedAtUtc))
            .ToArray();

        return Result.Success(response);
    }
}
