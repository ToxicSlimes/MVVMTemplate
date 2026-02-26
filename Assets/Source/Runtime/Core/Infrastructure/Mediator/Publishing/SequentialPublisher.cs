namespace Core.Infrastructure.Mediator.Publishing
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Core.Infrastructure.Mediator.Abstractions;
    using Cysharp.Threading.Tasks;

    public sealed class SequentialPublisher : INotificationPublisher
    {
        public async UniTask Publish(
            IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
            INotification notification,
            CancellationToken cancellationToken)
        {
            List<Exception> exceptions = null;

            foreach (var executor in handlerExecutors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await executor();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            if (exceptions is { Count: > 0 })
            {
                throw new AggregateException(
                    $"One or more notification handlers for {notification.GetType().Name} failed",
                    exceptions);
            }
        }
    }
}
