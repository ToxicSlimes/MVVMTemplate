namespace Core.Infrastructure.Mediator.Implementation.Wrappers
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Reflex.Core;

    public abstract class RequestHandlerWrapper
    {
        public abstract UniTask<object> Handle(
            object request,
            Container container,
            CancellationToken cancellationToken);
    }

    public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerWrapper
    {
        public abstract UniTask<TResponse> Handle(
            object request,
            Container container,
            CancellationToken cancellationToken,
            bool usePipeline);
    }
}
