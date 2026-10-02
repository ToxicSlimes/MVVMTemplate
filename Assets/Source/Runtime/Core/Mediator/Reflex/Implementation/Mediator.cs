namespace Core.Mediator.Implementation
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using Abstractions;
    using Exceptions;
    using Wrappers;
    using Publishing;
    using Cysharp.Threading.Tasks;
    using Reflex.Core;
    using R3;

    public sealed class Mediator : IMediator
    {
        private readonly Container _container;
        private readonly INotificationPublisher _publisher;
        private readonly ConcurrentDictionary<Type, object> _notificationSubjects = new();

        private static readonly ConcurrentDictionary<Type, object> RequestHandlerWrappers = new();
        private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> NotificationHandlerWrappers = new();
        private static readonly ConcurrentDictionary<Type, object> StreamHandlerWrappers = new();

        public Mediator(Container container, INotificationPublisher publisher)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async UniTask<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            cancellationToken.ThrowIfCancellationRequested();

            var requestType = request.GetType();
            var wrapper = GetOrCreateRequestHandlerWrapper<TResponse>(requestType);

            return await wrapper.Handle(request, _container, cancellationToken, true);
        }

        public async UniTask Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            cancellationToken.ThrowIfCancellationRequested();

            var requestType = request.GetType();
            var wrapper = GetOrCreateRequestHandlerWrapper<Nothing>(requestType);

            await wrapper.Handle(request, _container, cancellationToken, true);
        }

        public IUniTaskAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var requestType = request.GetType();
            var wrapper = GetOrCreateStreamHandlerWrapper<TResponse>(requestType);

            return wrapper.Handle(request, _container, cancellationToken);
        }

        public async UniTask Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            cancellationToken.ThrowIfCancellationRequested();

            var notificationType = typeof(TNotification);
            var wrapper = GetOrCreateNotificationHandlerWrapper(notificationType);

            await wrapper.Handle(notification, _container, _publisher, cancellationToken);

            if (_notificationSubjects.TryGetValue(notificationType, out var subjectObj) &&
                subjectObj is Subject<TNotification> subject)
            {
                subject.OnNext(notification);
            }
        }

        public Observable<TNotification> CreateNotificationObservable<TNotification>()
            where TNotification : INotification
        {
            var subject = (Subject<TNotification>)_notificationSubjects.GetOrAdd(
                typeof(TNotification),
                _ => new Subject<TNotification>());

            return subject.AsObservable();
        }

        private static RequestHandlerWrapper<TResponse> GetOrCreateRequestHandlerWrapper<TResponse>(Type requestType)
        {
            return (RequestHandlerWrapper<TResponse>)RequestHandlerWrappers.GetOrAdd(
                requestType,
                static type =>
                {
                    var responseType = ExtractResponseType(type, typeof(IRequest<>));
                    var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(type, responseType);
                    return Activator.CreateInstance(wrapperType)
                           ?? throw new MediatorException($"Failed to create handler wrapper for {type.Name}");
                });
        }

        private static NotificationHandlerWrapper GetOrCreateNotificationHandlerWrapper(Type notificationType)
        {
            return NotificationHandlerWrappers.GetOrAdd(
                notificationType,
                static type =>
                {
                    var wrapperType = typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(type);
                    return (NotificationHandlerWrapper)(Activator.CreateInstance(wrapperType)
                           ?? throw new MediatorException($"Failed to create notification wrapper for {type.Name}"));
                });
        }

        private static StreamRequestHandlerWrapper<TResponse> GetOrCreateStreamHandlerWrapper<TResponse>(Type requestType)
        {
            return (StreamRequestHandlerWrapper<TResponse>)StreamHandlerWrappers.GetOrAdd(
                requestType,
                static type =>
                {
                    var responseType = ExtractResponseType(type, typeof(IStreamRequest<>));
                    var wrapperType = typeof(StreamRequestHandlerWrapperImpl<,>).MakeGenericType(type, responseType);
                    return Activator.CreateInstance(wrapperType)
                           ?? throw new MediatorException($"Failed to create stream wrapper for {type.Name}");
                });
        }

        private static Type ExtractResponseType(Type requestType, Type interfaceDefinition)
        {
            var interfaceType = requestType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceDefinition);

            if (interfaceType == null)
            {
                throw new MediatorException(
                    $"Type {requestType.Name} does not implement {interfaceDefinition.Name}");
            }

            return interfaceType.GetGenericArguments()[0];
        }
    }
}
