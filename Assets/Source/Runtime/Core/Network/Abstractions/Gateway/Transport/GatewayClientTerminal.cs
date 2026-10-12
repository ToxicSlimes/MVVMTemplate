using System;
using System.Collections.Concurrent;
using System.Threading;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway.Transport
{
    public sealed class GatewayClientTerminal : IGatewayTerminal, IDisposable
    {
        private readonly INetworkMessenger _messenger;
        private readonly IGatewayPayloadCodec _codec;
        private readonly ConcurrentDictionary<string, PendingRequest> _pending = new(StringComparer.Ordinal);
        private readonly IDisposable _responseSubscription;
        private readonly object _lifecycleSync = new();
        private int _disposed;

        public GatewayClientTerminal(INetworkMessenger messenger, IGatewayPayloadCodec codec)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _responseSubscription = _messenger.Subscribe<GatewayResponseMessage>(OnResponse);
        }

        public async UniTask<object> SendAsync(
            GatewayContext context,
            CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            cancellationToken.ThrowIfCancellationRequested();
            ValidateRoute(context.Route);

            var payload = _codec.Serialize(context.Request, context.RequestType);
            ValidatePayload(payload);

            var requestId = Guid.NewGuid().ToString("N");
            var pending = new PendingRequest(context.ResponseType);
            lock (_lifecycleSync)
            {
                if (_disposed != 0)
                    throw new ObjectDisposedException(nameof(GatewayClientTerminal));
                if (_pending.TryAdd(requestId, pending) == false)
                    throw new InvalidOperationException("Failed to allocate a unique gateway request id");

                pending.SetCancellationRegistration(cancellationToken.Register(
                    () => CancelPending(requestId, cancellationToken)));

                if (pending.IsCompleted == false)
                {
                    try
                    {
                        _messenger.Send(
                            new GatewayRequestMessage(requestId, context.Route, payload),
                            NetworkTarget.StateAuthority);
                    }
                    catch (Exception exception)
                    {
                        if (_pending.TryRemove(requestId, out var removed))
                            removed.TrySetException(exception);
                    }
                }
            }

            return await pending.Task;
        }

        public void Dispose()
        {
            lock (_lifecycleSync)
            {
                if (_disposed != 0)
                    return;

                _disposed = 1;
            }

            _responseSubscription.Dispose();
            foreach (var pair in _pending)
            {
                if (_pending.TryRemove(pair.Key, out var pending))
                    pending.TrySetException(new ObjectDisposedException(nameof(GatewayClientTerminal)));
            }
        }

        private void OnResponse(NetworkPlayerId sender, GatewayResponseMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.RequestId))
                return;
            if (_pending.TryRemove(message.RequestId, out var pending) == false)
                return;

            if (message.Version != GatewayProtocol.CurrentVersion)
            {
                pending.TrySetException(new GatewayProtocolException(
                    $"Unsupported gateway response protocol version {message.Version}"));
                return;
            }

            if (message.IsSuccess == false)
            {
                pending.TrySetException(new GatewayRemoteException(message.ErrorCode, message.Error));
                return;
            }

            try
            {
                ValidatePayload(message.Payload);
                var response = _codec.Deserialize(message.Payload, pending.ResponseType);
                pending.TrySetResult(response);
            }
            catch (Exception exception)
            {
                pending.TrySetException(exception);
            }
        }

        private void CancelPending(string requestId, CancellationToken cancellationToken)
        {
            if (_pending.TryRemove(requestId, out var pending))
                pending.TrySetCanceled(cancellationToken);
        }

        private static void ValidateRoute(string route)
        {
            if (string.IsNullOrWhiteSpace(route) || route.Length > GatewayProtocol.MaxRouteLength)
                throw new GatewayProtocolException("Gateway route is missing or too long");
        }

        private static void ValidatePayload(string payload)
        {
            if (payload == null || payload.Length > GatewayProtocol.MaxPayloadLength)
                throw new GatewayProtocolException("Gateway payload is missing or too large");
        }

        private sealed class PendingRequest
        {
            private readonly object _sync = new();
            private readonly UniTaskCompletionSource<object> _source = new();
            private CancellationTokenRegistration _cancellationRegistration;
            private bool _completed;

            public Type ResponseType { get; }
            public UniTask<object> Task => _source.Task;

            public bool IsCompleted
            {
                get
                {
                    lock (_sync)
                        return _completed;
                }
            }

            public PendingRequest(Type responseType)
            {
                ResponseType = responseType ?? throw new ArgumentNullException(nameof(responseType));
            }

            public void SetCancellationRegistration(CancellationTokenRegistration registration)
            {
                lock (_sync)
                {
                    if (_completed)
                    {
                        registration.Dispose();
                        return;
                    }

                    _cancellationRegistration = registration;
                }
            }

            public void TrySetResult(object response) => Complete(() => _source.TrySetResult(response));

            public void TrySetException(Exception exception) =>
                Complete(() => _source.TrySetException(exception));

            public void TrySetCanceled(CancellationToken cancellationToken) =>
                Complete(() => _source.TrySetCanceled(cancellationToken));

            private void Complete(Action completion)
            {
                CancellationTokenRegistration registration;
                lock (_sync)
                {
                    if (_completed)
                        return;

                    _completed = true;
                    registration = _cancellationRegistration;
                }

                registration.Dispose();
                completion();
            }
        }
    }
}
