namespace Core.Infrastructure.Mediator.Implementation.Wrappers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Abstractions;
    using Handlers;
    using Publishing;
    using Cysharp.Threading.Tasks;
    using Reflex.Core;

    public abstract class NotificationHandlerWrapper
    {
        public abstract UniTask Handle(
            INotification notification,
            Container container,
            INotificationPublisher publisher,
            CancellationToken cancellationToken);
    }

    public sealed class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        public override async UniTask Handle(
            INotification notification,
            Container container,
            INotificationPublisher publisher,
            CancellationToken cancellationToken)
        {
            var typedNotification = (TNotification)notification;
            var enumerable = container.All<INotificationHandler<TNotification>>();

            var handlers = ResolveHandlers(enumerable);
            if (handlers.Count == 0)
                return;

            var executors = CreateExecutors(handlers, typedNotification, cancellationToken);
            await publisher.Publish(executors, notification, cancellationToken);
        }

        private static IReadOnlyList<INotificationHandler<TNotification>> ResolveHandlers(
            IEnumerable<INotificationHandler<TNotification>> enumerable)
        {
            if (enumerable is IReadOnlyList<INotificationHandler<TNotification>> readOnlyList)
                return readOnlyList;

            if (enumerable is ICollection<INotificationHandler<TNotification>> { Count: 0 })
                return System.Array.Empty<INotificationHandler<TNotification>>();

            return enumerable as List<INotificationHandler<TNotification>> ?? enumerable.ToList();
        }

        private static IReadOnlyList<NotificationHandlerExecutor> CreateExecutors(
            IReadOnlyList<INotificationHandler<TNotification>> handlers,
            TNotification notification,
            CancellationToken cancellationToken)
        {
            var executors = new NotificationHandlerExecutor[handlers.Count];

            for (var i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                executors[i] = () => handler.Handle(notification, cancellationToken);
            }

            return executors;
        }
    }
}
