using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Network.Abstractions.Gateway;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Core.Tests.Network.Gateway
{
    public sealed class GatewayPipelineTests
    {
        [Test]
        public async Task SendAsync_ExecutesReusablePipesInRegistrationOrder()
        {
            var calls = new List<string>();
            var first = new RecordingPipe("first", calls);
            var second = new RecordingPipe("second", calls);
            var terminal = new RecordingTerminal(calls, new TestResponse(42));
            var pipeline = new GatewayPipeline(new IGatewayPipe[] { first, second }, terminal);

            var response = await pipeline.SendAsync<TestResponse>(new TestRequest("inventory.get"));

            Assert.That(response.Value, Is.EqualTo(42));
            Assert.That(calls, Is.EqualTo(new[]
            {
                "first:before",
                "second:before",
                "terminal:inventory.get",
                "second:after",
                "first:after"
            }));
        }

        [Test]
        public async Task PipeInstance_CanBeReusedByMultiplePipelines()
        {
            var calls = new List<string>();
            var sharedPipe = new RecordingPipe("shared", calls);
            var firstPipeline = new GatewayPipeline(
                new[] { sharedPipe },
                new RecordingTerminal(calls, new TestResponse(1)));
            var secondPipeline = new GatewayPipeline(
                new[] { sharedPipe },
                new RecordingTerminal(calls, new TestResponse(2)));

            var first = await firstPipeline.SendAsync<TestResponse>(new TestRequest("first"));
            var second = await secondPipeline.SendAsync<TestResponse>(new TestRequest("second"));

            Assert.That(first.Value, Is.EqualTo(1));
            Assert.That(second.Value, Is.EqualTo(2));
            Assert.That(sharedPipe.CallCount, Is.EqualTo(2));
        }

        [Test]
        public async Task PipeFeatures_AreScopedToEachSend_WhenPipeInstanceIsReused()
        {
            var sharedPipe = new RouteFeaturePipe();
            var pipeline = new GatewayPipeline(
                new IGatewayPipe[] { sharedPipe },
                new FeatureTerminal());

            var first = await pipeline.SendAsync<string>(new NullRequest("first"));
            var second = await pipeline.SendAsync<string>(new NullRequest("second"));

            Assert.That(first, Is.EqualTo("first"));
            Assert.That(second, Is.EqualTo("second"));
        }

        [Test]
        public async Task PipeFeatures_AreIsolatedAcrossConcurrentSends()
        {
            var pipeline = new GatewayPipeline(
                new IGatewayPipe[] { new YieldingRouteFeaturePipe() },
                new YieldingFeatureTerminal());

            var firstTask = pipeline.SendAsync<string>(new NullRequest("first")).AsTask();
            var secondTask = pipeline.SendAsync<string>(new NullRequest("second")).AsTask();
            var responses = await Task.WhenAll(firstTask, secondTask);

            Assert.That(responses, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public async Task SendAsync_WhenPipeShortCircuits_DoesNotInvokeTerminal()
        {
            var terminal = new CountingTerminal(new TestResponse(1));
            var pipeline = new GatewayPipeline(
                new IGatewayPipe[] { new ShortCircuitPipe(new TestResponse(7)) },
                terminal);

            var response = await pipeline.SendAsync<TestResponse>(new TestRequest("cached"));

            Assert.That(response.Value, Is.EqualTo(7));
            Assert.That(terminal.CallCount, Is.Zero);
        }

        [Test]
        public void SendAsync_WhenTerminalReturnsWrongType_ThrowsGatewayResponseTypeException()
        {
            var pipeline = new GatewayPipeline(
                Array.Empty<IGatewayPipe>(),
                new CountingTerminal("wrong"));

            Assert.ThrowsAsync<GatewayResponseTypeException>(async () =>
                await pipeline.SendAsync<TestResponse>(new TestRequest("wrong.response")));
        }

        [Test]
        public async Task SendAsync_WhenReferenceResponseIsNull_ReturnsNull()
        {
            var pipeline = new GatewayPipeline(
                Array.Empty<IGatewayPipe>(),
                new CountingTerminal(null));

            var response = await pipeline.SendAsync<string>(new NullRequest("nullable"));

            Assert.That(response, Is.Null);
        }

        [Test]
        public void GatewayContext_CanBeCreatedByExternalTransport()
        {
            var request = new TestRequest("server.dispatch");

            var context = new GatewayContext(
                request.Route,
                request,
                typeof(TestRequest),
                typeof(TestResponse));

            Assert.That(context.Request, Is.SameAs(request));
            Assert.That(context.ResponseType, Is.EqualTo(typeof(TestResponse)));
        }

        [Test]
        public async Task SendAsync_ExistingContext_PreservesTransportFeatures()
        {
            var pipeline = new GatewayPipeline(
                Array.Empty<IGatewayPipe>(),
                new FeatureTerminal());
            var request = new NullRequest("server.dispatch");
            var context = new GatewayContext(
                request.Route,
                request,
                typeof(NullRequest),
                typeof(string));
            context.SetFeature(new RouteFeature("authenticated-session"));

            var response = await pipeline.SendAsync(context);

            Assert.That(response, Is.EqualTo("authenticated-session"));
        }

        [Test]
        public async Task Router_DispatchesTypedRequestThroughPipeline()
        {
            var router = new GatewayRouter(new IGatewayRoute[]
            {
                new GatewayRoute<TestRequest, TestResponse>(
                    "inventory.get",
                    (request, _) => UniTask.FromResult(new TestResponse(request.Route.Length)))
            });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);

            var response = await pipeline.SendAsync<TestResponse>(new TestRequest("inventory.get"));

            Assert.That(response.Value, Is.EqualTo("inventory.get".Length));
        }

        [Test]
        public async Task Router_DispatchesRequestDerivedFromRegisteredContract()
        {
            var router = new GatewayRouter(new IGatewayRoute[]
            {
                new GatewayRoute<BaseRequest, TestResponse>(
                    "derived",
                    (_, _) => UniTask.FromResult(new TestResponse(9)))
            });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);

            var response = await pipeline.SendAsync<TestResponse>(new DerivedRequest("derived"));

            Assert.That(response.Value, Is.EqualTo(9));
        }

        [Test]
        public void Router_WhenRouteIsDuplicated_RejectsConfiguration()
        {
            var routes = new IGatewayRoute[]
            {
                new GatewayRoute<TestRequest, TestResponse>(
                    "same",
                    (_, _) => UniTask.FromResult(new TestResponse(1))),
                new GatewayRoute<TestRequest, TestResponse>(
                    "same",
                    (_, _) => UniTask.FromResult(new TestResponse(2)))
            };

            Assert.Throws<ArgumentException>(() => new GatewayRouter(routes));
        }

        [Test]
        public void Router_WhenCustomRouteMetadataIsInvalid_RejectsConfiguration()
        {
            var routes = new IGatewayRoute[]
            {
                new CustomRoute(" ", null, typeof(TestResponse), new TestResponse(0))
            };

            Assert.Throws<ArgumentException>(() => new GatewayRouter(routes));
        }

        [Test]
        public void Router_WhenCustomRouteViolatesResponseContract_RejectsResult()
        {
            var request = new TestRequest("custom");
            var router = new GatewayRouter(new IGatewayRoute[]
            {
                new CustomRoute("custom", typeof(TestRequest), typeof(TestResponse), "wrong")
            });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);
            var context = new GatewayContext(
                request.Route,
                request,
                typeof(TestRequest),
                typeof(object));

            Assert.ThrowsAsync<GatewayResponseTypeException>(async () =>
                await pipeline.SendAsync(context));
        }

        [Test]
        public void Router_WhenCustomRouteReturnsNullForValueResponse_RejectsResult()
        {
            var request = new TestRequest("custom.null");
            var router = new GatewayRouter(new IGatewayRoute[]
            {
                new CustomRoute("custom.null", typeof(TestRequest), typeof(TestResponse), null)
            });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);
            var context = new GatewayContext(
                request.Route,
                request,
                typeof(TestRequest),
                typeof(object));

            Assert.ThrowsAsync<GatewayResponseTypeException>(async () =>
                await pipeline.SendAsync(context));
        }

        [Test]
        public void Router_WhenRouteIsUnknown_ThrowsGatewayRouteNotFoundException()
        {
            var pipeline = new GatewayPipeline(
                Array.Empty<IGatewayPipe>(),
                new GatewayRouter(Array.Empty<IGatewayRoute>()));

            Assert.ThrowsAsync<GatewayRouteNotFoundException>(async () =>
                await pipeline.SendAsync<TestResponse>(new TestRequest("missing")));
        }

        [Test]
        public void Router_WhenRequestTypeDoesNotMatchRoute_ThrowsGatewayContractMismatchException()
        {
            var router = new GatewayRouter(new IGatewayRoute[]
            {
                new GatewayRoute<OtherRequest, TestResponse>(
                    "shared.route",
                    (_, _) => UniTask.FromResult(new TestResponse(1)))
            });
            var pipeline = new GatewayPipeline(Array.Empty<IGatewayPipe>(), router);

            Assert.ThrowsAsync<GatewayContractMismatchException>(async () =>
                await pipeline.SendAsync<TestResponse>(new TestRequest("shared.route")));
        }

        private sealed class TestRequest : IGatewayRequest<TestResponse>
        {
            public string Route { get; }

            public TestRequest(string route)
            {
                Route = route;
            }
        }

        private sealed class OtherRequest : IGatewayRequest<TestResponse>
        {
            public string Route { get; }

            public OtherRequest(string route)
            {
                Route = route;
            }
        }

        private sealed class NullRequest : IGatewayRequest<string>
        {
            public string Route { get; }

            public NullRequest(string route)
            {
                Route = route;
            }
        }

        private class BaseRequest : IGatewayRequest<TestResponse>
        {
            public string Route { get; }

            public BaseRequest(string route)
            {
                Route = route;
            }
        }

        private sealed class DerivedRequest : BaseRequest
        {
            public DerivedRequest(string route) : base(route)
            {
            }
        }

        private readonly struct TestResponse
        {
            public readonly int Value;

            public TestResponse(int value)
            {
                Value = value;
            }
        }

        private sealed class RecordingPipe : IGatewayPipe
        {
            private readonly string _name;
            private readonly List<string> _calls;

            public int CallCount { get; private set; }

            public RecordingPipe(string name, List<string> calls)
            {
                _name = name;
                _calls = calls;
            }

            public async UniTask<object> InvokeAsync(
                GatewayContext context,
                GatewayPipeDelegate next,
                CancellationToken cancellationToken)
            {
                CallCount++;
                _calls.Add($"{_name}:before");
                var response = await next(context, cancellationToken);
                _calls.Add($"{_name}:after");
                return response;
            }
        }

        private sealed class ShortCircuitPipe : IGatewayPipe
        {
            private readonly object _response;

            public ShortCircuitPipe(object response)
            {
                _response = response;
            }

            public UniTask<object> InvokeAsync(
                GatewayContext context,
                GatewayPipeDelegate next,
                CancellationToken cancellationToken)
                => UniTask.FromResult(_response);
        }

        private sealed class RouteFeaturePipe : IGatewayPipe
        {
            public UniTask<object> InvokeAsync(
                GatewayContext context,
                GatewayPipeDelegate next,
                CancellationToken cancellationToken)
            {
                context.SetFeature(new RouteFeature(context.Route));
                return next(context, cancellationToken);
            }
        }

        private sealed class FeatureTerminal : IGatewayTerminal
        {
            public UniTask<object> SendAsync(GatewayContext context, CancellationToken cancellationToken)
            {
                Assert.That(context.TryGetFeature<RouteFeature>(out var feature), Is.True);
                return UniTask.FromResult<object>(feature.Route);
            }
        }

        private sealed class YieldingRouteFeaturePipe : IGatewayPipe
        {
            public async UniTask<object> InvokeAsync(
                GatewayContext context,
                GatewayPipeDelegate next,
                CancellationToken cancellationToken)
            {
                context.SetFeature(new RouteFeature(context.Route));
                await UniTask.Yield(cancellationToken);
                return await next(context, cancellationToken);
            }
        }

        private sealed class YieldingFeatureTerminal : IGatewayTerminal
        {
            public async UniTask<object> SendAsync(GatewayContext context, CancellationToken cancellationToken)
            {
                await UniTask.Yield(cancellationToken);
                Assert.That(context.TryGetFeature<RouteFeature>(out var feature), Is.True);
                return feature.Route;
            }
        }

        private readonly struct RouteFeature
        {
            public readonly string Route;

            public RouteFeature(string route)
            {
                Route = route;
            }
        }

        private sealed class CustomRoute : IGatewayRoute
        {
            private readonly object _response;

            public string Route { get; }
            public Type RequestType { get; }
            public Type ResponseType { get; }

            public CustomRoute(string route, Type requestType, Type responseType, object response)
            {
                Route = route;
                RequestType = requestType;
                ResponseType = responseType;
                _response = response;
            }

            public UniTask<object> HandleAsync(GatewayContext context, CancellationToken cancellationToken) =>
                UniTask.FromResult(_response);
        }

        private sealed class RecordingTerminal : IGatewayTerminal
        {
            private readonly List<string> _calls;
            private readonly object _response;

            public RecordingTerminal(List<string> calls, object response)
            {
                _calls = calls;
                _response = response;
            }

            public UniTask<object> SendAsync(GatewayContext context, CancellationToken cancellationToken)
            {
                _calls.Add($"terminal:{context.Route}");
                return UniTask.FromResult(_response);
            }
        }

        private sealed class CountingTerminal : IGatewayTerminal
        {
            private readonly object _response;

            public int CallCount { get; private set; }

            public CountingTerminal(object response)
            {
                _response = response;
            }

            public UniTask<object> SendAsync(GatewayContext context, CancellationToken cancellationToken)
            {
                CallCount++;
                return UniTask.FromResult(_response);
            }
        }
    }
}
