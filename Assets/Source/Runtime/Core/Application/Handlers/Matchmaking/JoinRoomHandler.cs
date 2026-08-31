using System.Threading;
using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Commands.Matchmaking;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Matchmaking
{
    public sealed class JoinRoomHandler : RoomLaunchHandlerBase, IRequestHandler<JoinRoomCommand, Nothing>
    {
        private const string MalformedCodeStatus = "Room code must be 6 characters";

        public JoinRoomHandler(
            INetworkRunnerController controller,
            INetworkLobby lobby,
            INetworkSpawner spawner,
            IConfigProvider configProvider,
            ILobbyModelWriter model)
            : base(controller, lobby, spawner, configProvider, model)
        {
        }

        public async UniTask<Nothing> Handle(JoinRoomCommand command, CancellationToken ct)
        {
            var code = RoomCodeGenerator.Normalize(command.RoomCode);

            if (!RoomCodeGenerator.IsValid(code))
            {
                ReportRejected(MalformedCodeStatus);
                return Nothing.Value;
            }

            await LaunchAsync(NetworkMode.Client, code, command.Nickname, $"Joining {code}...", ct);

            return Nothing.Value;
        }
    }
}
