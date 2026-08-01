using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class BandPredictionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<BandPrediction, BandPredictionId>(dbContext), IBandPredictionRepository
{
}
