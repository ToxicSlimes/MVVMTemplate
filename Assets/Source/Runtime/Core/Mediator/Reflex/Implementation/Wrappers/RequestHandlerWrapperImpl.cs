namespace Core.Mediator.Implementation.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Abstractions;
    using Exceptions;
    using Handlers;
    using Pipeline;
    using Cysharp.Threading.Tasks;
    using Reflex.Core;

    public sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        private static readonly IReadOnlyList<IPipelineBehavior<TRequest, TResponse>> EmptyBehaviors =
            Array.Empty<IPipelineBehavior<TRequest, TResponse>>();

        public override async UniTask<object> Handle(
            object request,
            Container container,
            CancellationToken cancellationToken)
        {
            return await Handle(request, container, cancellationToken, true);
        }

        public override async UniTask<TResponse> Handle(
            object request,
            Container container,
            CancellationToken cancellationToken,
            bool usePipeline)
        {
            var typedRequest = (TRequest)request;
            var handler = ResolveHandler(container);

            if (!usePipeline)
            {
                return await handler.Handle(typedRequest, cancellationToken);
            }

            var behaviors = ResolveBehaviors(container);

            if (behaviors.Count == 0)
            {
                return await handler.Handle(typedRequest, cancellationToken);
            }

            return await ExecutePipeline(typedRequest, handler, behaviors, cancellationToken);
        }

        private static IRequestHandler<TRequest, TResponse> ResolveHandler(Container container)
        {
            try
            {
                return container.Resolve<IRequestHandler<TRequest, TResponse>>();
            }
            catch (Reflex.Exceptions.UnknownContractException)
            {
                throw new HandlerNotFoundException(typeof(TRequest));
            }
        }

        private static IReadOnlyList<IPipelineBehavior<TRequest, TResponse>> ResolveBehaviors(Container container)
        {
            var enumerable = container.All<IPipelineBehavior<TRequest, TResponse>>();

            if (enumerable is IReadOnlyList<IPipelineBehavior<TRequest, TResponse>> readOnlyList)
                return readOnlyList.Count > 0 ? readOnlyList : EmptyBehaviors;

            if (enumerable is ICollection<IPipelineBehavior<TRequest, TResponse>> { Count: 0 })
                return EmptyBehaviors;

            var list = enumerable as List<IPipelineBehavior<TRequest, TResponse>> ?? enumerable.ToList();
            return list.Count > 0 ? list : EmptyBehaviors;
        }

        private static async UniTask<TResponse> ExecutePipeline(
            TRequest request,
            IRequestHandler<TRequest, TResponse> handler,
            IReadOnlyList<IPipelineBehavior<TRequest, TResponse>> behaviors,
            CancellationToken cancellationToken)
        {
            RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(request, cancellationToken);

            for (var i = behaviors.Count - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var next = pipeline;
                pipeline = () => behavior.Handle(request, cancellationToken, next);
            }

            return await pipeline();
        }
    }
}
