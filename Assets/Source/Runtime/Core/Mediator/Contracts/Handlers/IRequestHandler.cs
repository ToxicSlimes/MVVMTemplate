namespace Core.Infrastructure.Mediator.Handlers
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Core.Infrastructure.Mediator.Abstractions;

    /// <summary>
    /// Handler for requests with response.
    /// </summary>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        UniTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Handler for requests without response.
    /// </summary>
    public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Nothing>
        where TRequest : IRequest<Nothing>
    {
    }
}
