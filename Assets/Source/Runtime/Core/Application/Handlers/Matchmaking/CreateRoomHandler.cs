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
    public sealed class CreateRoomHandler : RoomLaunchHandlerBase, IRequestHandler<CreateRoomCommand, Nothing>
    {
        private const string CreatingStatus = "Creating room...";
        private const int CodeCollisionRetries = 3;

        public CreateRoomHandler(
            INetworkRunnerController controller,
            INetworkLobby lobby,
            INetworkSpawner spawner,
            IConfigProvider configProvider,
            ILobbyModelWriter model)
            : base(controller, lobby, spawner, configProvider, model)
        {
        }

        public async UniTask<Nothing> Handle(CreateRoomCommand command, CancellationToken ct)
        {
            for (var attempt = 0; attempt < CodeCollisionRetries; attempt++)
            {
                var result = await LaunchAsync(
                    NetworkMode.Host,
                    RoomCodeGenerator.Generate(),
                    command.Nickname,
                    CreatingStatus,
                    ct);

                if (result.IsOk || result.ShutdownReason != NetworkShutdownReason.GameIdAlreadyExists)
                {
                    break;
                }
            }

            return Nothing.Value;
        }
    }
}
