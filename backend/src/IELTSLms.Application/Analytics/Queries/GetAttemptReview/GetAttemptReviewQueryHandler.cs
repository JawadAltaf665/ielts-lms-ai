using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Grammar;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Speaking;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Analytics.Queries.GetAttemptReview;

public sealed class GetAttemptReviewQueryHandler(
    IWritingSubmissionRepository writingSubmissions, IWritingEvaluationRepository writingEvaluations,
    ISpeakingAttemptRepository speakingAttempts, ISpeakingEvaluationRepository speakingEvaluations,
    IReadingAttemptRepository readingAttempts, IListeningAttemptRepository listeningAttempts,
    IGrammarAttemptRepository grammarAttempts, IVocabularyProgressRepository vocabularyProgress,
    IMockExamResultRepository mockExamResults, ICurrentUserService currentUser)
    : IQueryHandler<GetAttemptReviewQuery, AttemptReviewResponse>
{
    public Task<Result<AttemptReviewResponse>> Handle(GetAttemptReviewQuery request, CancellationToken cancellationToken)
    {
        var isTeacher = currentUser.Roles.Contains("Teacher");

        return request.Type.ToLowerInvariant() switch
        {
            "writing" => ReviewWritingAsync(request, isTeacher, cancellationToken),
            "speaking" => ReviewSpeakingAsync(request, isTeacher, cancellationToken),
            "reading" => ReviewReadingAsync(request, isTeacher, cancellationToken),
            "listening" => ReviewListeningAsync(request, isTeacher, cancellationToken),
            "grammar" => ReviewGrammarAsync(request, isTeacher, cancellationToken),
            "vocabulary" => ReviewVocabularyAsync(request, isTeacher, cancellationToken),
            "mock-exam" => ReviewMockExamAsync(request, isTeacher, cancellationToken),
            _ => Task.FromResult(Result.Failure<AttemptReviewResponse>(AnalyticsErrors.InvalidAttemptType)),
        };
    }

    private async Task<Result<AttemptReviewResponse>> ReviewWritingAsync(
        GetAttemptReviewQuery request, bool isTeacher, CancellationToken cancellationToken)
    {
        var submission = await writingSubmissions
            .GetByIdAsync(WritingSubmissionId.From(request.Id), cancellationToken).ConfigureAwait(false);
        if (submission is null || (submission.UserId != request.UserId && !isTeacher))
        {
            return Result.Failure<AttemptReviewResponse>(AnalyticsErrors.AttemptNotFound);
        }

        WritingEvaluation? evaluation = submission.WritingEvaluationId is { } evaluationId
            ? await writingEvaluations.GetByIdAsync(evaluationId, cancellationToken).ConfigureAwait(false)
            : null;

        return Result.Success(new AttemptReviewResponse(
            "writing", submission.Id.Value, submission.CreatedAtUtc, submission.Status.ToString(), evaluation?.EstimatedBand,
            null, null, evaluation?.Confidence, evaluation?.FeedbackText, evaluation?.RequiresHumanReview));
    }

    private async Task<Result<AttemptReviewResponse>> ReviewSpeakingAsync(
        GetAttemptReviewQuery request, bool isTeacher, CancellationToken cancellationToken)
    {
        var attempt = await speakingAttempts
            .GetByIdAsync(SpeakingAttemptId.From(request.Id), cancellationToken).ConfigureAwait(false);
        if (attempt is null || (attempt.UserId != request.UserId && !isTeacher))
        {
            return Result.Failure<AttemptReviewResponse>(AnalyticsErrors.AttemptNotFound);
        }

        SpeakingEvaluation? evaluation = attempt.SpeakingEvaluationId is { } evaluationId
            ? await speakingEvaluations.GetByIdAsync(evaluationId, cancellationToken).ConfigureAwait(false)
            : null;

        return Result.Success(new AttemptReviewResponse(
            "speaking", attempt.Id.Value, attempt.CreatedAtUtc, attempt.Status.ToString(), evaluation?.EstimatedBand,
            null, null, evaluation?.Confidence, evaluation?.FeedbackText, null));
    }

    private async Task<Result<AttemptReviewResponse>> ReviewReadingAsync(
        GetAttemptReviewQuery request, bool isTeacher, CancellationToken cancellationToken)
    {
        var attempt = await readingAttempts
            .GetByIdAsync(ReadingAttemptId.From(request.Id), cancellationToken).ConfigureAwait(false);
        if (attempt is null || (attempt.UserId != request.UserId && !isTeacher))
        {
            return Result.Failure<AttemptReviewResponse>(AnalyticsErrors.AttemptNotFound);
        }

        return Result.Success(new AttemptReviewResponse(
            "reading", attempt.Id.Value, attempt.CreatedAtUtc, attempt.IsSubmitted ? "Submitted" : "InProgress", attempt.Band,
            attempt.RawScore, attempt.MaxScore, null, null, null));
    }

    private async Task<Result<AttemptReviewResponse>> ReviewListeningAsync(
        GetAttemptReviewQuery request, bool isTeacher, CancellationToken cancellationToken)
    {
        var attempt = await listeningAttempts
            .GetByIdAsync(ListeningAttemptId.From(request.Id), cancellationToken).ConfigureAwait(false);
        if (attempt is null || (attempt.UserId != request.UserId && !isTeacher))
        {
            return Result.Failure<AttemptReviewResponse>(AnalyticsErrors.AttemptNotFound);
        }

        return Result.Success(new AttemptReviewResponse(
            "listening", attempt.Id.Value, attempt.CreatedAtUtc, attempt.IsSubmitted ? "Submitted" : "InProgress", attempt.Band,
            attempt.RawScore, attempt.MaxScore, null, null, null));
    }

    private async Task<Result<AttemptReviewResponse>> ReviewGrammarAsync(
        GetAttemptReviewQuery request, bool isTeacher, CancellationToken cancellationToken)
    {
        var attempt = await grammarAttempts
            .GetByIdAsync(GrammarAttemptId.From(request.Id), cancellationToken).ConfigureAwait(false);
        if (attempt is null || (attempt.UserId != request.UserId && !isTeacher))
        {
            return Result.Failure<AttemptReviewResponse>(AnalyticsErrors.AttemptNotFound);
        }

        return Result.Success(new AttemptReviewResponse(
            "grammar", attempt.Id.Value, attempt.CreatedAtUtc, "Scored", null, attempt.RawScore, attempt.MaxScore, null, null, null));
    }

    private async Task<Result<AttemptReviewResponse>> ReviewVocabularyAsync(
        GetAttemptReviewQuery request, bool isTeacher, CancellationToken cancellationToken)
    {
        var progress = await vocabularyProgress
            .GetByIdAsync(VocabularyProgressId.From(request.Id), cancellationToken).ConfigureAwait(false);
        if (progress is null || (progress.UserId != request.UserId && !isTeacher))
        {
            return Result.Failure<AttemptReviewResponse>(AnalyticsErrors.AttemptNotFound);
        }

        return Result.Success(new AttemptReviewResponse(
            "vocabulary", progress.Id.Value, progress.UpdatedAtUtc ?? progress.CreatedAtUtc, "Reviewed", null,
            progress.LastGrade, 5, null, null, null));
    }

    private async Task<Result<AttemptReviewResponse>> ReviewMockExamAsync(
        GetAttemptReviewQuery request, bool isTeacher, CancellationToken cancellationToken)
    {
        var result = await mockExamResults
            .GetByIdAsync(MockExamResultId.From(request.Id), cancellationToken).ConfigureAwait(false);
        if (result is null || (result.UserId != request.UserId && !isTeacher))
        {
            return Result.Failure<AttemptReviewResponse>(AnalyticsErrors.AttemptNotFound);
        }

        return Result.Success(new AttemptReviewResponse(
            "mock-exam", result.Id.Value, result.CreatedAtUtc, result.IsFinalized ? "Finalized" : "InProgress", result.OverallBand,
            null, null, null, null, null));
    }
}
