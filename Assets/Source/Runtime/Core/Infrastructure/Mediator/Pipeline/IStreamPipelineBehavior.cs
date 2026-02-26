namespace Core.Infrastructure.Mediator.Pipeline
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Core.Infrastructure.Mediator.Abstractions;

    /// <summary>
    /// Delegate for stream pipeline.
    /// </summary>
    public delegate IUniTaskAsyncEnumerable<TResponse> StreamHandlerDelegate<out TResponse>();

    /// <summary>
    /// Pipeline behavior for stream requests.
    /// </summary>
    public interface IStreamPipelineBehavior<in TRequest, TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        IUniTaskAsyncEnumerable<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            StreamHandlerDelegate<TResponse> next);
    }
}
