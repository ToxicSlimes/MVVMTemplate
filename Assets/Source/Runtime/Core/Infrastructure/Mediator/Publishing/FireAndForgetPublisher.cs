namespace Core.Infrastructure.Mediator.Publishing
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Core.Infrastructure.Mediator.Abstractions;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public sealed class FireAndForgetPublisher : INotificationPublisher
    {
        public UniTask Publish(
            IReadOnlyList<NotificationHandlerExecutor> handlerExecutors,
            INotification notification,
            CancellationToken cancellationToken)
        {
            var notificationName = notification.GetType().Name;

            foreach (var executor in handlerExecutors)
            {
                ExecuteAsync(executor, notificationName).Forget();
            }

            return UniTask.CompletedTask;
        }

        private static async UniTaskVoid ExecuteAsync(NotificationHandlerExecutor executor, string notificationName)
        {
            try
            {
                await executor();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Mediator] Fire-and-forget handler for {notificationName} failed: {ex.Message}");
            }
        }
    }
}
