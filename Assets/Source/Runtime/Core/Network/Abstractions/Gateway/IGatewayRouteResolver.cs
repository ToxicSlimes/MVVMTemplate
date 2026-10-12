using System;

namespace Core.Network.Abstractions.Gateway
{
    public interface IGatewayRouteResolver
    {
        bool TryResolve(string route, out IGatewayRoute gatewayRoute);
    }
}
