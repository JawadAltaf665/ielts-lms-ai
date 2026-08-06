using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Learning;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning.Recommendations.Commands.DismissRecommendation;

public sealed class DismissRecommendationCommandHandler(
    ILearningRecommendationRepository recommendations, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<DismissRecommendationCommand>
{
    public async Task<Result> Handle(DismissRecommendationCommand request, CancellationToken cancellationToken)
    {
        var recommendation = await recommendations
            .GetByIdAsync(LearningRecommendationId.From(request.RecommendationId), cancellationToken)
            .ConfigureAwait(false);
        if (recommendation is null || recommendation.TenantId != request.TenantId || recommendation.UserId != request.UserId)
        {
            return Result.Failure(LearningErrors.RecommendationNotFound);
        }

        var dismissed = recommendation.Dismiss(clock.UtcNow);
        if (dismissed.IsFailure)
        {
            return dismissed;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
