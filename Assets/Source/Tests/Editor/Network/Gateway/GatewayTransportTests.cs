using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Network.Abstractions.Gateway;
using Core.Network.Abstractions.Gateway.Transport;
using Core.Network.Abstractions.Messaging;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Core.Tests.Network.Gateway
{
    public sealed class GatewayTransportTests
    {
        [Test]
        public async Task ClientTerminal_CorrelatesSuccessfulResponse()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            using var terminal = new GatewayClientTerminal(messenger, codec);
            var request = new EchoRequest("echo", "hello");
            var context = new GatewayContext(request.Route, request, typeof(EchoRequest), typeof(EchoResponse));

            var responseTask = terminal.SendAsync(context, CancellationToken.None).AsTask();
            var sent = messenger.SingleSent<GatewayRequestMessage>();
            messenger.Publish(
                new NetworkPlayerId(10),
                GatewayResponseMessage.Success(
                    sent.Message.RequestId,
                    codec.Serialize(new EchoResponse("world"), typeof(EchoResponse))));

            var response = (EchoResponse)await responseTask;

            Assert.That(sent.Target.Kind, Is.EqualTo(NetworkTargetKind.StateAuthority));
            Assert.That(sent.Message.Route, Is.EqualTo("echo"));
            Assert.That(response.Value, Is.EqualTo("world"));
        }

        [Test]
        public void ClientTerminal_RemoteFailureBecomesGatewayRemoteException()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            using var terminal = new GatewayClientTerminal(messenger, codec);
            var request = new EchoRequest("echo", "hello");
            var context = new GatewayContext(request.Route, request, typeof(EchoRequest), typeof(EchoResponse));

            var responseTask = terminal.SendAsync(context, CancellationToken.None).AsTask();
            var sent = messenger.SingleSent<GatewayRequestMessage>();
            messenger.Publish(
                new NetworkPlayerId(10),
                GatewayResponseMessage.Failure(sent.Message.RequestId, "access_denied", "denied"));

            var exception = Assert.ThrowsAsync<GatewayRemoteException>(async () => await responseTask);
            Assert.That(exception.ErrorCode, Is.EqualTo("access_denied"));
            Assert.That(exception.Message, Does.Contain("denied"));
        }

        [Test]
        public void ClientTerminal_CancellationCancelsPendingRequest()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            using var terminal = new GatewayClientTerminal(messenger, codec);
            using var cancellation = new CancellationTokenSource();
            var request = new EchoRequest("echo", "hello");
            var context = new GatewayContext(request.Route, request, typeof(EchoRequest), typeof(EchoResponse));

            var responseTask = terminal.SendAsync(context, cancellation.Token).AsTask();
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await responseTask);
        }

        [Test]
        public void ClientTerminal_WhenDisposedBeforePendingRegistration_DoesNotTransmit()
        {
            var messenger = new FakeMessenger();
            GatewayClientTerminal terminal = null;
            var codec = new CallbackPayloadCodec(() => terminal.Dispose());
            terminal = new GatewayClientTerminal(messenger, codec);
            var request = new EchoRequest("echo", "hello");
            var context = new GatewayContext(request.Route, request, typeof(EchoRequest), typeof(EchoResponse));

            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await terminal.SendAsync(context, CancellationToken.None));
            Assert.That(messenger.SentMessages, Is.Empty);
        }

        [Test]
        public async Task ServerIngress_RoutesRequestWithRemotePeerFeature()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            var sender = new NetworkPlayerId(27);
            var route = new GatewayRoute<EchoRequest, EchoResponse>(
                "echo",
                (context, request, _) =>
                {
                    Assert.That(context.TryGetFeature<GatewayRemotePeer>(out var peer), Is.True);
                    Assert.That(peer.Player, Is.EqualTo(sender));
                    return UniTask.FromResult(new EchoResponse(request.Value + "!"));
                });
            var router = new GatewayRouter(new IGatewayRoute[] { route });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);
            using var ingress = CreateIngress(messenger, codec, router, pipeline);
            var request = new EchoRequest("echo", "hello");

            messenger.Publish(
                sender,
                new GatewayRequestMessage(
                    "request-1",
                    request.Route,
                    codec.Serialize(request, typeof(EchoRequest))));
            await WaitForAsync(() => messenger.SentMessages.OfType<SentMessage<GatewayResponseMessage>>().Any());

            var sent = messenger.SingleSent<GatewayResponseMessage>();
            var response = (EchoResponse)codec.Deserialize(sent.Message.Payload, typeof(EchoResponse));
            Assert.That(sent.Target.Player, Is.EqualTo(sender));
            Assert.That(sent.Message.IsSuccess, Is.True);
            Assert.That(response.Value, Is.EqualTo("hello!"));
        }

        [Test]
        public async Task ServerIngress_UnknownRouteReturnsFailure()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            var router = new GatewayRouter(Array.Empty<IGatewayRoute>());
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);
            using var ingress = CreateIngress(messenger, codec, router, pipeline);

            messenger.Publish(
                new NetworkPlayerId(4),
                new GatewayRequestMessage("request-2", "missing", "payload"));
            await WaitForAsync(() => messenger.SentMessages.OfType<SentMessage<GatewayResponseMessage>>().Any());

            var response = messenger.SingleSent<GatewayResponseMessage>().Message;
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorCode, Is.EqualTo("route_not_found"));
            Assert.That(response.Error, Is.EqualTo("Gateway route is not available"));
            Assert.That(response.Error, Does.Not.Contain("missing"));
        }

        [Test]
        public async Task ServerIngress_NonAuthorityDoesNotExecuteRequest()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            var executed = false;
            var route = new GatewayRoute<EchoRequest, EchoResponse>(
                "echo",
                (_, request, _) =>
                {
                    executed = true;
                    return UniTask.FromResult(new EchoResponse(request.Value));
                });
            var router = new GatewayRouter(new IGatewayRoute[] { route });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);
            using var ingress = new GatewayServerIngress(
                messenger,
                codec,
                router,
                pipeline,
                new FakeAuthority(false),
                new FakeErrorReporter());
            var request = new EchoRequest("echo", "hello");

            messenger.Publish(
                new NetworkPlayerId(8),
                new GatewayRequestMessage(
                    "request-non-authority",
                    request.Route,
                    codec.Serialize(request, typeof(EchoRequest))));
            await UniTask.Yield();

            Assert.That(executed, Is.False);
            Assert.That(messenger.SentMessages, Is.Empty);
        }

        [Test]
        public async Task ServerIngress_RejectsPayloadRouteThatDiffersFromEnvelope()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            var route = new GatewayRoute<EchoRequest, EchoResponse>(
                "echo",
                (_, request, _) => UniTask.FromResult(new EchoResponse(request.Value)));
            var router = new GatewayRouter(new IGatewayRoute[] { route });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);
            using var ingress = CreateIngress(messenger, codec, router, pipeline);
            var request = new EchoRequest("different-route", "hello");

            messenger.Publish(
                new NetworkPlayerId(9),
                new GatewayRequestMessage(
                    "request-route-mismatch",
                    "echo",
                    codec.Serialize(request, typeof(EchoRequest))));
            await WaitForAsync(() => messenger.SentMessages.OfType<SentMessage<GatewayResponseMessage>>().Any());

            var response = messenger.SingleSent<GatewayResponseMessage>().Message;
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorCode, Is.EqualTo("invalid_request"));
            Assert.That(response.Error, Is.EqualTo("Gateway request is invalid"));
        }

        [Test]
        public async Task ServerIngress_DisposeCancelsInFlightHandlerAndSuppressesReply()
        {
            var messenger = new FakeMessenger();
            var codec = new FakePayloadCodec();
            GatewayServerIngress ingress = null;
            var cancellationObserved = false;
            var route = new GatewayRoute<EchoRequest, EchoResponse>(
                "echo",
                (_, request, cancellationToken) =>
                {
                    ingress.Dispose();
                    cancellationObserved = cancellationToken.IsCancellationRequested;
                    return UniTask.FromResult(new EchoResponse(request.Value));
                });
            var router = new GatewayRouter(new IGatewayRoute[] { route });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);
            ingress = CreateIngress(messenger, codec, router, pipeline);
            var request = new EchoRequest("echo", "hello");

            messenger.Publish(
                new NetworkPlayerId(11),
                new GatewayRequestMessage(
                    "request-dispose",
                    request.Route,
                    codec.Serialize(request, typeof(EchoRequest))));
            await UniTask.Yield();

            Assert.That(cancellationObserved, Is.True);
            Assert.That(messenger.SentMessages, Is.Empty);
        }

        private static GatewayServerIngress CreateIngress(
            FakeMessenger messenger,
            FakePayloadCodec codec,
            GatewayRouter router,
            IGateway pipeline) =>
            new(
                messenger,
                codec,
                router,
                pipeline,
                new FakeAuthority(true),
                new FakeErrorReporter());

        private static async UniTask WaitForAsync(Func<bool> predicate)
        {
            for (var i = 0; i < 100 && predicate() == false; i++)
                await UniTask.Yield();

            Assert.That(predicate(), Is.True, "Timed out waiting for gateway message");
        }

        private sealed class EchoRequest : IGatewayRequest<EchoResponse>
        {
            public string Route { get; }
            public string Value { get; }

            public EchoRequest(string route, string value)
            {
                Route = route;
                Value = value;
            }
        }

        private sealed class EchoResponse
        {
            public string Value { get; }

            public EchoResponse(string value)
            {
                Value = value;
            }
        }

        private sealed class FakePayloadCodec : IGatewayPayloadCodec
        {
            private readonly Dictionary<string, object> _payloads = new();
            private int _nextId;

            public string Serialize(object value, Type type)
            {
                if (value != null && type.IsInstanceOfType(value) == false)
                    throw new InvalidOperationException("Payload type mismatch");

                var key = (++_nextId).ToString();
                _payloads[key] = value;
                return key;
            }

            public object Deserialize(string payload, Type type)
            {
                if (_payloads.TryGetValue(payload, out var value) == false)
                    throw new InvalidOperationException("Payload not found");
                if (value != null && type.IsInstanceOfType(value) == false)
                    throw new InvalidOperationException("Payload type mismatch");

                return value;
            }
        }

        private sealed class CallbackPayloadCodec : IGatewayPayloadCodec
        {
            private readonly Action _onSerialize;

            public CallbackPayloadCodec(Action onSerialize)
            {
                _onSerialize = onSerialize;
            }

            public string Serialize(object value, Type type)
            {
                _onSerialize();
                return "payload";
            }

            public object Deserialize(string payload, Type type) =>
                throw new NotSupportedException();
        }

        private sealed class FakeAuthority : IGatewayAuthority
        {
            public bool CanHandleRequests { get; }

            public FakeAuthority(bool canHandleRequests)
            {
                CanHandleRequests = canHandleRequests;
            }
        }

        private sealed class FakeErrorReporter : IGatewayErrorReporter
        {
            public void Report(Exception exception, string route) { }
        }

        private interface ISentMessage
        {
        }

        private sealed class SentMessage<TMessage> : ISentMessage where TMessage : INetworkMessage
        {
            public TMessage Message { get; }
            public NetworkTarget Target { get; }

            public SentMessage(TMessage message, NetworkTarget target)
            {
                Message = message;
                Target = target;
            }
        }

        private sealed class FakeMessenger : INetworkMessenger
        {
            private readonly Dictionary<Type, List<Delegate>> _handlers = new();

            public List<ISentMessage> SentMessages { get; } = new();

            public void Send<TMessage>(TMessage message, NetworkTarget target) where TMessage : INetworkMessage =>
                SentMessages.Add(new SentMessage<TMessage>(message, target));

            public IDisposable Subscribe<TMessage>(Action<NetworkPlayerId, TMessage> handler)
                where TMessage : INetworkMessage
            {
                if (_handlers.TryGetValue(typeof(TMessage), out var handlers) == false)
                {
                    handlers = new List<Delegate>();
                    _handlers.Add(typeof(TMessage), handlers);
                }

                handlers.Add(handler);
                return new Subscription(handlers, handler);
            }

            public void Publish<TMessage>(NetworkPlayerId sender, TMessage message)
                where TMessage : INetworkMessage
            {
                if (_handlers.TryGetValue(typeof(TMessage), out var handlers) == false)
                    return;

                foreach (var handler in handlers.ToArray())
                    ((Action<NetworkPlayerId, TMessage>)handler)(sender, message);
            }

            public SentMessage<TMessage> SingleSent<TMessage>() where TMessage : INetworkMessage =>
                SentMessages.OfType<SentMessage<TMessage>>().Single();

            private sealed class Subscription : IDisposable
            {
                private readonly List<Delegate> _handlers;
                private readonly Delegate _handler;

                public Subscription(List<Delegate> handlers, Delegate handler)
                {
                    _handlers = handlers;
                    _handler = handler;
                }

                public void Dispose() => _handlers.Remove(_handler);
            }
        }
    }
}
