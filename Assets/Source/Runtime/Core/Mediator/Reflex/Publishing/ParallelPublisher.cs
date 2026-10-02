namespace Core.Mediator.Publishing
{
    using System.Collections.Generic;
    using System.Threading;
    using Core.Mediator.Abstractions;
    using Cysharp.Threading.Tasks;

    public sealed class ParallelPublisher : INotificationPublisher
    {
        public async UniTask Publish(
            IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
            INotification notification,
            CancellationToken cancellationToken)
        {
            if (handlerExecutors.Count == 0)
                return;

            var tasks = new UniTask[handlerExecutors.Count];
            for (var i = 0; i < handlerExecutors.Count; i++)
            {
                tasks[i] = handlerExecutors[i]();
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
