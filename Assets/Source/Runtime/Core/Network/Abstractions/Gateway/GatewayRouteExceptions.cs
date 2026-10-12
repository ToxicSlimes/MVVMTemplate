using System;

namespace Core.Network.Abstractions.Gateway
{
    public sealed class GatewayRouteNotFoundException : Exception
    {
        public GatewayRouteNotFoundException(string route)
            : base($"Gateway route is not registered: {route}")
        {
        }
    }

    public sealed class GatewayContractMismatchException : Exception
    {
        public GatewayContractMismatchException(
            string route,
            Type expectedRequest,
            Type actualRequest,
            Type expectedResponse,
            Type actualResponse)
            : base(
                $"Gateway route {route} expects {expectedRequest.Name} -> {expectedResponse.Name}, " +
                $"but received {actualRequest.Name} -> {actualResponse.Name}")
        {
        }
    }
}
