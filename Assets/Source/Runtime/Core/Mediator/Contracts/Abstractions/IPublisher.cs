namespace Core.Infrastructure.Mediator.Abstractions
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using R3;

    public interface IPublisher
    {
        UniTask Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification;

        Observable<TNotification> CreateNotificationObservable<TNotification>()
            where TNotification : INotification;
    }
}
