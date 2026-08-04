using MediatR;

namespace IELTSLms.Application.Tests.AI.Fakes;

/// <summary>Minimal <see cref="ISender"/> fake for handlers that delegate to another command/query via
/// <c>ISender.Send</c> (mirrors <c>StartMockExamSectionCommandHandler</c>'s cross-module delegation
/// shape) — captures the last request and returns a preconfigured response, no real MediatR pipeline.</summary>
internal sealed class FakeSender : ISender
{
    public object? LastRequest { get; private set; }

    public object? NextResponse { get; set; }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult((TResponse)NextResponse!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        LastRequest = request;
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not used by these tests.");

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not used by these tests.");

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not used by these tests.");
}
