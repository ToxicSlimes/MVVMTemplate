namespace Core.Infrastructure.Mediator.Handlers
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Core.Infrastructure.Mediator.Abstractions;

    /// <summary>
    /// Handler for notifications (can have multiple per notification type).
    /// </summary>
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        UniTask Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
