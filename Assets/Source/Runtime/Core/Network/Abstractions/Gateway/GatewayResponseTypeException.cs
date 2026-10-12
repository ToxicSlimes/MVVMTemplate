using System;

namespace Core.Network.Abstractions.Gateway
{
    public sealed class GatewayResponseTypeException : Exception
    {
        public GatewayResponseTypeException(Type expectedType, Type actualType)
            : base($"Gateway expected response {expectedType.Name}, but received {actualType?.Name ?? "null"}")
        {
        }
    }
}
