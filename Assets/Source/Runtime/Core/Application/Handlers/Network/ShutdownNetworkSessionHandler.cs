using System.Threading;
using Core.Application.Commands.Network;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Network
{
    public sealed class ShutdownNetworkSessionHandler : IRequestHandler<ShutdownNetworkSessionCommand, Nothing>
    {
        private readonly INetworkRunnerController _controller;

        public ShutdownNetworkSessionHandler(INetworkRunnerController controller)
        {
            _controller = controller;
        }

        public async UniTask<Nothing> Handle(ShutdownNetworkSessionCommand command, CancellationToken ct)
        {
            await _controller.ShutdownAsync(ct);
            return Nothing.Value;
        }
    }
}
