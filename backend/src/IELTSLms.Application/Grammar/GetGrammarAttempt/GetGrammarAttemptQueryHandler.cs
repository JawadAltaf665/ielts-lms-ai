using System.Text.Json;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Grammar.SubmitGrammarAttempt;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.GetGrammarAttempt;

public sealed class GetGrammarAttemptQueryHandler(IGrammarAttemptRepository attempts)
    : IQueryHandler<GetGrammarAttemptQuery, GrammarAttemptResponse>
{
    public async Task<Result<GrammarAttemptResponse>> Handle(GetGrammarAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(GrammarAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<GrammarAttemptResponse>(GrammarErrors.AttemptNotFound);
        }

        var answers = JsonSerializer.Deserialize<GrammarAttemptAnswerResponse[]>(attempt.AnswersJson) ?? [];

        return Result.Success(new GrammarAttemptResponse(
            attempt.Id.Value, attempt.GrammarExerciseId.Value, attempt.RawScore, attempt.MaxScore, answers, attempt.CreatedAtUtc));
    }
}
