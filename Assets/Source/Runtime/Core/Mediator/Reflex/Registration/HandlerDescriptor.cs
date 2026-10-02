namespace Core.Mediator.Registration
{
    using System;

    public sealed class HandlerDescriptor
    {
        public Type RequestType { get; }
        public Type ResponseType { get; }
        public Type HandlerType { get; }
        public ServiceLifetime? Lifetime { get; }

        public HandlerDescriptor(
            Type requestType,
            Type responseType,
            Type handlerType,
            ServiceLifetime? lifetime = null)
        {
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
            ResponseType = responseType;
            Lifetime = lifetime;
        }
    }
}
