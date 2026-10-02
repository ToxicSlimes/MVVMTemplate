namespace Core.Mediator.Handlers
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Core.Mediator.Abstractions;

    /// <summary>
    /// Handler that returns a stream of responses.
    /// </summary>
    public interface IStreamRequestHandler<in TRequest, out TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        IUniTaskAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
