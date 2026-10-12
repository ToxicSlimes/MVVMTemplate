using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public sealed class GatewayRouter : IGatewayTerminal, IGatewayRouteResolver
    {
        private readonly IReadOnlyDictionary<string, IGatewayRoute> _routes;

        public GatewayRouter(IEnumerable<IGatewayRoute> routes)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            var routeMap = new Dictionary<string, IGatewayRoute>(StringComparer.Ordinal);
            foreach (var route in routes)
            {
                if (route == null)
                    throw new ArgumentException("Gateway route cannot be null", nameof(routes));
                if (string.IsNullOrWhiteSpace(route.Route))
                    throw new ArgumentException("Gateway route cannot be empty", nameof(routes));
                if (route.RequestType == null)
                    throw new ArgumentException($"Gateway route {route.Route} has no request type", nameof(routes));
                if (route.ResponseType == null)
                    throw new ArgumentException($"Gateway route {route.Route} has no response type", nameof(routes));

                var requestContract = typeof(IGatewayRequest<>).MakeGenericType(route.ResponseType);
                if (requestContract.IsAssignableFrom(route.RequestType) == false)
                {
                    throw new ArgumentException(
                        $"Gateway route {route.Route} request type {route.RequestType.Name} " +
                        $"must implement IGatewayRequest<{route.ResponseType.Name}>",
                        nameof(routes));
                }

                if (routeMap.TryAdd(route.Route, route) == false)
                    throw new ArgumentException($"Gateway route is registered more than once: {route.Route}", nameof(routes));
            }

            _routes = routeMap;
        }

        public bool TryResolve(string route, out IGatewayRoute gatewayRoute)
        {
            gatewayRoute = null;
            return string.IsNullOrWhiteSpace(route) == false &&
                   _routes.TryGetValue(route, out gatewayRoute);
        }

        public async UniTask<object> SendAsync(GatewayContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_routes.TryGetValue(context.Route, out var route) == false)
                throw new GatewayRouteNotFoundException(context.Route);

            if (route.RequestType.IsAssignableFrom(context.RequestType) == false ||
                context.ResponseType.IsAssignableFrom(route.ResponseType) == false)
            {
                throw new GatewayContractMismatchException(
                    context.Route,
                    route.RequestType,
                    context.RequestType,
                    route.ResponseType,
                    context.ResponseType);
            }

            var response = await route.HandleAsync(context, cancellationToken);

            if (response != null && route.ResponseType.IsInstanceOfType(response))
                return response;

            if (response == null &&
                (route.ResponseType.IsValueType == false ||
                 Nullable.GetUnderlyingType(route.ResponseType) != null))
                return null;

            throw new GatewayResponseTypeException(route.ResponseType, response?.GetType());
        }
    }
}
