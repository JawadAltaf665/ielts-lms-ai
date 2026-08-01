using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeBandPredictionRepository : IBandPredictionRepository
{
    private readonly List<BandPrediction> _predictions = [];

    public IReadOnlyList<BandPrediction> Predictions => _predictions;

    public void Seed(BandPrediction prediction) => _predictions.Add(prediction);

    public Task<BandPrediction?> GetByIdAsync(BandPredictionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_predictions.FirstOrDefault(p => p.Id == id));

    public void Add(BandPrediction aggregate) => _predictions.Add(aggregate);

    public void Remove(BandPrediction aggregate) => _predictions.Remove(aggregate);
}
