namespace Core.Mediator.Abstractions
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Sends requests to a single handler and returns response.
    /// </summary>
    public interface ISender
    {
        /// <summary>
        /// Send a request to a single handler.
        /// </summary>
        UniTask<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a request without response (command).
        /// </summary>
        UniTask Send(
            IRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a stream of responses.
        /// </summary>
        IUniTaskAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default);
    }
}
