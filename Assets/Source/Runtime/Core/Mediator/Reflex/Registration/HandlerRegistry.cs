namespace Core.Mediator.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class HandlerRegistry : IHandlerRegistry
    {
        private static readonly IReadOnlyList<Type> EmptyTypeList = Array.Empty<Type>();

        private readonly Dictionary<Type, HandlerDescriptor> _requestHandlers;
        private readonly Dictionary<Type, List<HandlerDescriptor>> _notificationHandlers;
        private readonly Dictionary<Type, HandlerDescriptor> _streamHandlers;
        private readonly List<Type> _behaviorTypes;
        private readonly List<Type> _preProcessorTypes;
        private readonly List<Type> _postProcessorTypes;

        public HandlerRegistry(
            IReadOnlyList<HandlerDescriptor> requestHandlers,
            IReadOnlyList<HandlerDescriptor> notificationHandlers,
            IReadOnlyList<HandlerDescriptor> streamHandlers,
            IReadOnlyList<Type> behaviorTypes,
            IReadOnlyList<Type> preProcessorTypes,
            IReadOnlyList<Type> postProcessorTypes)
        {
            _requestHandlers = new Dictionary<Type, HandlerDescriptor>();
            _notificationHandlers = new Dictionary<Type, List<HandlerDescriptor>>();
            _streamHandlers = new Dictionary<Type, HandlerDescriptor>();
            _behaviorTypes = behaviorTypes?.ToList() ?? new List<Type>();
            _preProcessorTypes = preProcessorTypes?.ToList() ?? new List<Type>();
            _postProcessorTypes = postProcessorTypes?.ToList() ?? new List<Type>();

            if (requestHandlers != null)
            {
                foreach (var descriptor in requestHandlers)
                {
                    _requestHandlers[descriptor.RequestType] = descriptor;
                }
            }

            if (notificationHandlers != null)
            {
                foreach (var descriptor in notificationHandlers)
                {
                    if (!_notificationHandlers.TryGetValue(descriptor.RequestType, out var handlers))
                    {
                        handlers = new List<HandlerDescriptor>();
                        _notificationHandlers[descriptor.RequestType] = handlers;
                    }
                    handlers.Add(descriptor);
                }
            }

            if (streamHandlers != null)
            {
                foreach (var descriptor in streamHandlers)
                {
                    _streamHandlers[descriptor.RequestType] = descriptor;
                }
            }
        }

        public bool HasHandler(Type requestType)
        {
            return _requestHandlers.ContainsKey(requestType) 
                   || _streamHandlers.ContainsKey(requestType);
        }

        public Type GetHandlerType(Type requestType)
        {
            if (_requestHandlers.TryGetValue(requestType, out var descriptor))
                return descriptor.HandlerType;

            if (_streamHandlers.TryGetValue(requestType, out var streamDescriptor))
                return streamDescriptor.HandlerType;

            throw new InvalidOperationException(
                $"No handler registered for request type {requestType.FullName}");
        }

        public IReadOnlyList<Type> GetNotificationHandlerTypes(Type notificationType)
        {
            if (_notificationHandlers.TryGetValue(notificationType, out var handlers))
                return handlers.Select(h => h.HandlerType).ToList();

            return EmptyTypeList;
        }

        public IReadOnlyList<Type> GetBehaviorTypes() => _behaviorTypes;

        internal IReadOnlyList<Type> GetPreProcessorTypes() => _preProcessorTypes;

        internal IReadOnlyList<Type> GetPostProcessorTypes() => _postProcessorTypes;

        internal HandlerDescriptor GetRequestDescriptor(Type requestType)
        {
            return _requestHandlers.TryGetValue(requestType, out var descriptor) 
                ? descriptor 
                : null;
        }

        internal HandlerDescriptor GetStreamDescriptor(Type requestType)
        {
            return _streamHandlers.TryGetValue(requestType, out var descriptor) 
                ? descriptor 
                : null;
        }
    }
}
