namespace Core.Mediator.Publishing
{
    using System.Collections.Generic;
    using System.Threading;
    using Core.Mediator.Abstractions;
    using Cysharp.Threading.Tasks;

    public interface INotificationPublisher
    {
        UniTask Publish(
            IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
            INotification notification,
            CancellationToken cancellationToken);
    }
}
