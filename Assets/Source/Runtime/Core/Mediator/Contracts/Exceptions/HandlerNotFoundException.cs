namespace Core.Infrastructure.Mediator.Exceptions
{
    using System;

    public sealed class HandlerNotFoundException : MediatorException
    {
        public Type RequestType { get; }

        public HandlerNotFoundException(Type requestType)
            : base($"No handler registered for request type {requestType.FullName}")
        {
            RequestType = requestType;
        }

        public HandlerNotFoundException(Type requestType, string message)
            : base(message)
        {
            RequestType = requestType;
        }

        public HandlerNotFoundException(Type requestType, string message, Exception innerException)
            : base(message, innerException)
        {
            RequestType = requestType;
        }
    }
}
