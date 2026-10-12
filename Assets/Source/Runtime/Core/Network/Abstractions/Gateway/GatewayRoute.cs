using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public sealed class GatewayRoute<TRequest, TResponse> : IGatewayRoute
        where TRequest : IGatewayRequest<TResponse>
    {
        private readonly Func<GatewayContext, TRequest, CancellationToken, UniTask<TResponse>> _handler;

        public string Route { get; }
        public Type RequestType => typeof(TRequest);
        public Type ResponseType => typeof(TResponse);

        public GatewayRoute(
            string route,
            Func<TRequest, CancellationToken, UniTask<TResponse>> handler)
            : this(route, (_, request, cancellationToken) => handler(request, cancellationToken))
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
        }

        public GatewayRoute(
            string route,
            Func<GatewayContext, TRequest, CancellationToken, UniTask<TResponse>> handler)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("Gateway route cannot be empty", nameof(route));

            Route = route;
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public async UniTask<object> HandleAsync(GatewayContext context, CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var response = await _handler(context, (TRequest)context.Request, cancellationToken);
            return response;
        }
    }
}
