using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.GetBandPrediction;

public sealed class GetBandPredictionQueryHandler(IBandPredictionRepository predictions, ICurrentUserService currentUser)
    : IQueryHandler<GetBandPredictionQuery, BandPredictionResponse>
{
    public async Task<Result<BandPredictionResponse>> Handle(GetBandPredictionQuery request, CancellationToken cancellationToken)
    {
        var prediction = await predictions
            .GetByIdAsync(BandPredictionId.From(request.BandPredictionId), cancellationToken)
            .ConfigureAwait(false);

        var isOwner = prediction is not null && prediction.UserId == request.UserId;
        var isTeacher = currentUser.Roles.Contains("Teacher");
        if (prediction is null || (!isOwner && !isTeacher))
        {
            return Result.Failure<BandPredictionResponse>(AiErrors.OperationNotFound);
        }

        return Result.Success(ToResponse(prediction));
    }

    internal static BandPredictionResponse ToResponse(BandPrediction prediction) => new(
        prediction.Id.Value, prediction.UserId, prediction.Status.ToString(), prediction.OverallBand, prediction.ReadingBand,
        prediction.ListeningBand, prediction.WritingBand, prediction.SpeakingBand, prediction.Confidence,
        prediction.RecommendMorePractice, prediction.CompletedAtUtc, prediction.ErrorCode, prediction.ErrorMessage);
}
