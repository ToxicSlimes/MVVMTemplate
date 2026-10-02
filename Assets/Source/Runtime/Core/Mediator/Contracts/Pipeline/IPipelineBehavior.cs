namespace Core.Mediator.Pipeline
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Core.Mediator.Abstractions;

    /// <summary>
    /// Pipeline behavior that wraps request handling.
    /// </summary>
    public interface IPipelineBehavior<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        UniTask<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next);
    }
}
