using System;
using System.Threading;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway.Transport
{
    public sealed class GatewayServerIngress : IDisposable
    {
        private readonly INetworkMessenger _messenger;
        private readonly IGatewayPayloadCodec _codec;
        private readonly IGatewayRouteResolver _routes;
        private readonly IGateway _serverGateway;
        private readonly IGatewayAuthority _authority;
        private readonly IGatewayErrorReporter _errorReporter;
        private readonly IDisposable _requestSubscription;
        private readonly CancellationTokenSource _lifetime = new();
        private readonly object _lifecycleSync = new();
        private int _disposed;

        public GatewayServerIngress(
            INetworkMessenger messenger,
            IGatewayPayloadCodec codec,
            IGatewayRouteResolver routes,
            IGateway serverGateway,
            IGatewayAuthority authority,
            IGatewayErrorReporter errorReporter)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _routes = routes ?? throw new ArgumentNullException(nameof(routes));
            _serverGateway = serverGateway ?? throw new ArgumentNullException(nameof(serverGateway));
            _authority = authority ?? throw new ArgumentNullException(nameof(authority));
            _errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
            _requestSubscription = _messenger.Subscribe<GatewayRequestMessage>(OnRequest);
        }

        public void Dispose()
        {
            lock (_lifecycleSync)
            {
                if (_disposed != 0)
                    return;

                _disposed = 1;
                _requestSubscription.Dispose();
                _lifetime.Cancel();
            }
        }

        private void OnRequest(NetworkPlayerId sender, GatewayRequestMessage message)
        {
            if (Volatile.Read(ref _disposed) != 0 || _authority.CanHandleRequests == false)
                return;

            HandleAsync(sender, message, _lifetime.Token).Forget();
        }

        private async UniTaskVoid HandleAsync(
            NetworkPlayerId sender,
            GatewayRequestMessage message,
            CancellationToken cancellationToken)
        {
            var requestId = message?.RequestId;
            try
            {
                ValidateMessage(message);
                cancellationToken.ThrowIfCancellationRequested();

                if (_routes.TryResolve(message.Route, out var route) == false)
                    throw new GatewayRouteNotFoundException(message.Route);

                var request = _codec.Deserialize(message.Payload, route.RequestType);
                if (request == null || route.RequestType.IsInstanceOfType(request) == false)
                    throw new GatewayProtocolException($"Gateway payload does not contain {route.RequestType.Name}");
                if (request is not IGatewayRequest gatewayRequest ||
                    string.Equals(gatewayRequest.Route, message.Route, StringComparison.Ordinal) == false)
                    throw new GatewayProtocolException("Gateway payload route does not match its envelope");

                ThrowIfCannotContinue(cancellationToken);

                var context = new GatewayContext(route.Route, request, route.RequestType, route.ResponseType);
                context.SetFeature(new GatewayRemotePeer(sender));

                var response = await _serverGateway.SendAsync(context, cancellationToken);
                var payload = _codec.Serialize(response, route.ResponseType);
                if (payload == null || payload.Length > GatewayProtocol.MaxPayloadLength)
                    throw new GatewayProtocolException("Gateway response payload is missing or too large");

                TrySend(
                    GatewayResponseMessage.Success(message.RequestId, payload),
                    sender,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _errorReporter.Report(exception, message?.Route);

                if (string.IsNullOrWhiteSpace(requestId) ||
                    requestId.Length > GatewayProtocol.MaxRequestIdLength)
                    return;

                var error = ToPublicError(exception);
                try
                {
                    TrySend(
                        GatewayResponseMessage.Failure(requestId, error.Code, error.Message),
                        sender,
                        cancellationToken);
                }
                catch (Exception sendException)
                {
                    _errorReporter.Report(sendException, message?.Route);
                }
            }
        }

        private void ThrowIfCannotContinue(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_authority.CanHandleRequests == false)
                throw new OperationCanceledException("Gateway authority was lost", cancellationToken);
        }

        private void TrySend(
            GatewayResponseMessage response,
            NetworkPlayerId target,
            CancellationToken cancellationToken)
        {
            lock (_lifecycleSync)
            {
                if (_disposed != 0 ||
                    cancellationToken.IsCancellationRequested ||
                    _authority.CanHandleRequests == false)
                    return;

                _messenger.Send(response, NetworkTarget.ToPlayer(target));
            }
        }

        private static void ValidateMessage(GatewayRequestMessage message)
        {
            if (message == null)
                throw new GatewayProtocolException("Gateway request is missing");
            if (message.Version != GatewayProtocol.CurrentVersion)
                throw new GatewayProtocolException($"Unsupported gateway request protocol version {message.Version}");
            if (string.IsNullOrWhiteSpace(message.RequestId) ||
                message.RequestId.Length > GatewayProtocol.MaxRequestIdLength)
                throw new GatewayProtocolException("Gateway request id is missing or too long");
            if (string.IsNullOrWhiteSpace(message.Route) ||
                message.Route.Length > GatewayProtocol.MaxRouteLength)
                throw new GatewayProtocolException("Gateway route is missing or too long");
            if (message.Payload == null || message.Payload.Length > GatewayProtocol.MaxPayloadLength)
                throw new GatewayProtocolException("Gateway request payload is missing or too large");
        }

        private static PublicError ToPublicError(Exception exception)
        {
            return exception switch
            {
                GatewayRouteNotFoundException => new PublicError("route_not_found", "Gateway route is not available"),
                GatewayProtocolException => new PublicError("invalid_request", "Gateway request is invalid"),
                OperationCanceledException => new PublicError("request_cancelled", "Gateway request was cancelled"),
                _ => new PublicError("internal_error", "Gateway request failed")
            };
        }

        private readonly struct PublicError
        {
            public string Code { get; }
            public string Message { get; }

            public PublicError(string code, string message)
            {
                Code = code;
                Message = message;
            }
        }
    }
}