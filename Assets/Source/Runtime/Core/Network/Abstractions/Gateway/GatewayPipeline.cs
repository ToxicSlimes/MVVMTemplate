using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Gateway
{
    public sealed class GatewayPipeline : IGateway
    {
        private readonly GatewayPipeDelegate _pipeline;

        public GatewayPipeline(IEnumerable<IGatewayPipe> pipes, IGatewayTerminal terminal)
        {
            if (pipes == null)
                throw new ArgumentNullException(nameof(pipes));
            if (terminal == null)
                throw new ArgumentNullException(nameof(terminal));

            var pipeList = pipes as IReadOnlyList<IGatewayPipe> ?? new List<IGatewayPipe>(pipes);
            GatewayPipeDelegate pipeline = terminal.SendAsync;

            for (var i = pipeList.Count - 1; i >= 0; i--)
            {
                var pipe = pipeList[i] ?? throw new ArgumentException("Gateway pipe cannot be null", nameof(pipes));
                var next = pipeline;
                pipeline = (context, cancellationToken) => pipe.InvokeAsync(context, next, cancellationToken);
            }

            _pipeline = pipeline;
        }

        public async UniTask<TResponse> SendAsync<TResponse>(
            IGatewayRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Route))
                throw new ArgumentException("Gateway route cannot be empty", nameof(request));

            cancellationToken.ThrowIfCancellationRequested();

            var context = new GatewayContext(
                request.Route,
                request,
                request.GetType(),
                typeof(TResponse));
            var response = await SendAsync(context, cancellationToken);

            return (TResponse)response;
        }

        public async UniTask<object> SendAsync(
            GatewayContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            cancellationToken.ThrowIfCancellationRequested();

            var response = await _pipeline(context, cancellationToken);

            if (response != null && context.ResponseType.IsInstanceOfType(response))
                return response;

            if (response == null &&
                (context.ResponseType.IsValueType == false ||
                 Nullable.GetUnderlyingType(context.ResponseType) != null))
                return null;

            throw new GatewayResponseTypeException(context.ResponseType, response?.GetType());
        }
    }
}
