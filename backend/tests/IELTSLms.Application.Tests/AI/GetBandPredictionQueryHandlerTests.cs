using IELTSLms.Application.AI.GetBandPrediction;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI;

public sealed class GetBandPredictionQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Owner_can_read_their_own_prediction()
    {
        var predictions = new FakeBandPredictionRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var prediction = BandPrediction.Enqueue(
            BandPredictionId.From(Guid.NewGuid()), tenantId, userId, AiOperationId.From(Guid.NewGuid()), Now);
        predictions.Seed(prediction);
        var handler = new GetBandPredictionQueryHandler(predictions, new FakeCurrentUserService());

        var result = await handler.Handle(
            new GetBandPredictionQuery(tenantId, userId, prediction.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value.Status);
    }

    [Fact]
    public async Task A_non_owner_who_is_not_a_teacher_gets_not_found()
    {
        var predictions = new FakeBandPredictionRepository();
        var tenantId = Guid.NewGuid();
        var prediction = BandPrediction.Enqueue(
            BandPredictionId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), AiOperationId.From(Guid.NewGuid()), Now);
        predictions.Seed(prediction);
        var handler = new GetBandPredictionQueryHandler(predictions, new FakeCurrentUserService());

        var result = await handler.Handle(
            new GetBandPredictionQuery(tenantId, Guid.NewGuid(), prediction.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_AI_OPERATION", result.Error.Code);
    }

    [Fact]
    public async Task A_teacher_can_read_any_prediction()
    {
        var predictions = new FakeBandPredictionRepository();
        var tenantId = Guid.NewGuid();
        var prediction = BandPrediction.Enqueue(
            BandPredictionId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), AiOperationId.From(Guid.NewGuid()), Now);
        predictions.Seed(prediction);
        var handler = new GetBandPredictionQueryHandler(predictions, new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(
            new GetBandPredictionQuery(tenantId, Guid.NewGuid(), prediction.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
