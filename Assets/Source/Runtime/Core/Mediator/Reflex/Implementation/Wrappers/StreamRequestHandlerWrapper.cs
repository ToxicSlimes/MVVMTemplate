namespace Core.Mediator.Implementation.Wrappers
{
    using System.Threading;
    using Abstractions;
    using Exceptions;
    using Handlers;
    using Cysharp.Threading.Tasks;
    using Cysharp.Threading.Tasks.Linq;
    using Reflex.Core;

    public abstract class StreamRequestHandlerWrapper<TResponse>
    {
        public abstract IUniTaskAsyncEnumerable<TResponse> Handle(
            object request,
            Container container,
            CancellationToken cancellationToken);
    }

    public sealed class StreamRequestHandlerWrapperImpl<TRequest, TResponse> : StreamRequestHandlerWrapper<TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        public override IUniTaskAsyncEnumerable<TResponse> Handle(
            object request,
            Container container,
            CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)request;
            var handler = container.Resolve<IStreamRequestHandler<TRequest, TResponse>>();

            if (handler == null)
            {
                throw new HandlerNotFoundException(typeof(TRequest));
            }

            return handler.Handle(typedRequest, cancellationToken);
        }
    }
}
