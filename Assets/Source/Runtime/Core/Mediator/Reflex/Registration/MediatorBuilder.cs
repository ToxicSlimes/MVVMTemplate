namespace Core.Mediator.Registration
{
    using System;
    using System.Collections.Generic;
    using Core.Mediator.Abstractions;
    using Core.Mediator.Handlers;
    using Core.Mediator.Implementation;
    using Core.Mediator.Pipeline;
    using Core.Mediator.Publishing;
    using Reflex.Core;

    public sealed class MediatorBuilder
    {
        private readonly ContainerBuilder _containerBuilder;
        private readonly List<HandlerDescriptor> _requestHandlers = new();
        private readonly List<HandlerDescriptor> _notificationHandlers = new();
        private readonly List<HandlerDescriptor> _streamHandlers = new();
        private readonly List<Type> _behaviorTypes = new();
        private readonly List<Type> _preProcessorTypes = new();
        private readonly List<Type> _postProcessorTypes = new();

        private Type _notificationPublisherType = typeof(SequentialPublisher);
        private ServiceLifetime _defaultLifetime = ServiceLifetime.Transient;

        internal MediatorBuilder(ContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder ?? throw new ArgumentNullException(nameof(containerBuilder));
        }

        public MediatorBuilder AddHandler<TRequest, TResponse, THandler>(
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TRequest : IRequest<TResponse>
            where THandler : class, IRequestHandler<TRequest, TResponse>
        {
            _requestHandlers.Add(new HandlerDescriptor(
                typeof(TRequest),
                typeof(TResponse),
                typeof(THandler),
                lifetime));

            return this;
        }

        public MediatorBuilder AddHandler<TRequest, THandler>(
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TRequest : IRequest
            where THandler : class, IRequestHandler<TRequest>
        {
            _requestHandlers.Add(new HandlerDescriptor(
                typeof(TRequest),
                typeof(Nothing),
                typeof(THandler),
                lifetime));

            return this;
        }

        public MediatorBuilder AddNotificationHandler<TNotification, THandler>(
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TNotification : INotification
            where THandler : class, INotificationHandler<TNotification>
        {
            _notificationHandlers.Add(new HandlerDescriptor(
                typeof(TNotification),
                null,
                typeof(THandler),
                lifetime));

            return this;
        }

        public MediatorBuilder AddStreamHandler<TRequest, TResponse, THandler>(
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TRequest : IStreamRequest<TResponse>
            where THandler : class, IStreamRequestHandler<TRequest, TResponse>
        {
            _streamHandlers.Add(new HandlerDescriptor(
                typeof(TRequest),
                typeof(TResponse),
                typeof(THandler),
                lifetime));

            return this;
        }

        public MediatorBuilder AddBehavior<TBehavior>()
            where TBehavior : class
        {
            _behaviorTypes.Add(typeof(TBehavior));
            return this;
        }

        public MediatorBuilder AddPreProcessor<TRequest, TProcessor>()
            where TRequest : notnull
            where TProcessor : class, IRequestPreProcessor<TRequest>
        {
            _preProcessorTypes.Add(typeof(TProcessor));
            return this;
        }

        public MediatorBuilder AddPostProcessor<TRequest, TResponse, TProcessor>()
            where TRequest : notnull
            where TProcessor : class, IRequestPostProcessor<TRequest, TResponse>
        {
            _postProcessorTypes.Add(typeof(TProcessor));
            return this;
        }

        public MediatorBuilder UseNotificationPublisher<TPublisher>()
            where TPublisher : class, INotificationPublisher
        {
            _notificationPublisherType = typeof(TPublisher);
            return this;
        }

        public MediatorBuilder SetDefaultLifetime(ServiceLifetime lifetime)
        {
            _defaultLifetime = lifetime;
            return this;
        }

        internal void Build()
        {
            var registry = new HandlerRegistry(
                _requestHandlers,
                _notificationHandlers,
                _streamHandlers,
                _behaviorTypes,
                _preProcessorTypes,
                _postProcessorTypes);

            _containerBuilder.AddSingleton<IHandlerRegistry>(_ => registry);
            RegisterNotificationPublisher();
            RegisterMediator();
            RegisterHandlers(_requestHandlers);
            RegisterHandlers(_notificationHandlers);
            RegisterHandlers(_streamHandlers);
            RegisterTypes(_behaviorTypes, ServiceLifetime.Transient);
            RegisterTypes(_preProcessorTypes, ServiceLifetime.Transient);
            RegisterTypes(_postProcessorTypes, ServiceLifetime.Transient);
        }

        private void RegisterMediator()
        {
            _containerBuilder.AddSingleton<IMediator>(c =>
                new Mediator(c, c.Resolve<INotificationPublisher>()));

            _containerBuilder.AddSingleton<ISender>(c => c.Resolve<IMediator>());
            _containerBuilder.AddSingleton<IPublisher>(c => c.Resolve<IMediator>());
        }

        private void RegisterNotificationPublisher()
        {
            _containerBuilder.AddSingleton(_notificationPublisherType, typeof(INotificationPublisher));
        }

        private void RegisterHandlers(IReadOnlyList<HandlerDescriptor> handlers)
        {
            foreach (var descriptor in handlers)
            {
                var lifetime = descriptor.Lifetime ?? _defaultLifetime;
                var interfaceType = BuildHandlerInterfaceType(descriptor);
                RegisterHandlerWithInterface(descriptor.HandlerType, interfaceType, lifetime);
            }
        }

        private static Type BuildHandlerInterfaceType(HandlerDescriptor descriptor)
        {
            if (descriptor.ResponseType == null)
            {
                return typeof(INotificationHandler<>).MakeGenericType(descriptor.RequestType);
            }

            if (descriptor.HandlerType.GetInterfaces().Length > 0 &&
                Array.Exists(descriptor.HandlerType.GetInterfaces(),
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>)))
            {
                return typeof(IStreamRequestHandler<,>).MakeGenericType(
                    descriptor.RequestType,
                    descriptor.ResponseType);
            }

            return typeof(IRequestHandler<,>).MakeGenericType(
                descriptor.RequestType,
                descriptor.ResponseType);
        }

        private void RegisterHandlerWithInterface(Type handlerType, Type interfaceType, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    _containerBuilder.AddSingleton(handlerType, interfaceType);
                    break;
                case ServiceLifetime.Scoped:
                case ServiceLifetime.Transient:
                default:
                    _containerBuilder.AddTransient(handlerType, interfaceType);
                    break;
            }
        }

        private void RegisterTypes(IReadOnlyList<Type> types, ServiceLifetime lifetime)
        {
            foreach (var type in types)
            {
                RegisterByLifetime(type, lifetime);
            }
        }

        private void RegisterByLifetime(Type type, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    _containerBuilder.AddSingleton(type);
                    break;
                case ServiceLifetime.Scoped:
                case ServiceLifetime.Transient:
                default:
                    _containerBuilder.AddTransient(type);
                    break;
            }
        }
    }
}
