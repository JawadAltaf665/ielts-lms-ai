using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="BandPrediction"/> (ERD ENT-053).</summary>
public interface IBandPredictionRepository : IRepository<BandPrediction, BandPredictionId>
{
}
