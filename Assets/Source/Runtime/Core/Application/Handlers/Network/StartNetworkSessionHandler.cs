using System.Threading;
using Core.Application.Commands.Network;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Network
{
    public sealed class StartNetworkSessionHandler : IRequestHandler<StartNetworkSessionCommand, NetworkStartResult>
    {
        private readonly INetworkRunnerController _controller;

        public StartNetworkSessionHandler(INetworkRunnerController controller)
        {
            _controller = controller;
        }

        public UniTask<NetworkStartResult> Handle(StartNetworkSessionCommand command, CancellationToken ct) =>
            _controller.StartAsync(command.Request, ct);
    }
}
