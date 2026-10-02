using System.Threading;
using Core.Application.Commands.Matchmaking;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Matchmaking
{
    public sealed class LeaveRoomHandler : IRequestHandler<LeaveRoomCommand, Nothing>
    {
        private readonly INetworkRunnerController _controller;

        public LeaveRoomHandler(INetworkRunnerController controller)
        {
            _controller = controller;
        }

        public async UniTask<Nothing> Handle(LeaveRoomCommand command, CancellationToken ct)
        {
            await _controller.ShutdownAsync(ct);
            return Nothing.Value;
        }
    }
}
