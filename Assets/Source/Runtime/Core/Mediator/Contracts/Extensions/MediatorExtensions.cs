namespace Core.Mediator.Extensions
{
    using System;
    using System.Threading;
    using Core.Mediator.Abstractions;
    using Core.Mediator.Exceptions;
    using Cysharp.Threading.Tasks;

    public static class MediatorExtensions
    {
        public static async UniTask<TResponse> SendWithTimeout<TResponse>(
            this IMediator mediator,
            IRequest<TResponse> request,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutCts.Token);

            try
            {
                return await mediator.Send(request, linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Request {request.GetType().Name} timed out after {timeout.TotalSeconds}s");
            }
        }

        public static async UniTask SendWithTimeout(
            this IMediator mediator,
            IRequest request,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutCts.Token);

            try
            {
                await mediator.Send(request, linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Request {request.GetType().Name} timed out after {timeout.TotalSeconds}s");
            }
        }

        public static UniTask PublishFireAndForget<TNotification>(
            this IMediator mediator,
            TNotification notification)
            where TNotification : INotification
        {
            mediator.Publish(notification, CancellationToken.None).Forget();
            return UniTask.CompletedTask;
        }

        public static async UniTask<TResponse> SendOrDefault<TResponse>(
            this IMediator mediator,
            IRequest<TResponse> request,
            TResponse defaultValue = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await mediator.Send(request, cancellationToken);
            }
            catch (HandlerNotFoundException)
            {
                return defaultValue;
            }
        }
    }
}
