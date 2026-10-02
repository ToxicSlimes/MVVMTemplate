namespace Core.Mediator.Registration
{
    using System;
    using System.Collections.Generic;

    internal interface IHandlerRegistry
    {
        bool HasHandler(Type requestType);
        Type GetHandlerType(Type requestType);
        IReadOnlyList<Type> GetNotificationHandlerTypes(Type notificationType);
        IReadOnlyList<Type> GetBehaviorTypes();
    }
}
