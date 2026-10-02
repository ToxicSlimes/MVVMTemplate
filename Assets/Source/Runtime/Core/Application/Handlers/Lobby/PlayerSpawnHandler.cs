using System.Threading;
using Core.Application.Notifications.Network;
using Core.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class PlayerSpawnHandler : INotificationHandler<NetworkPlayerJoinedNotification>
    {
        private readonly INetworkRunnerController _controller;
        private readonly INetworkSpawner _spawner;

        public PlayerSpawnHandler(INetworkRunnerController controller, INetworkSpawner spawner)
        {
            _controller = controller;
            _spawner = spawner;
        }

        public async UniTask Handle(NetworkPlayerJoinedNotification notification, CancellationToken ct)
        {
            if (!_controller.IsStateAuthority)
            {
                return;
            }

            await _spawner.SpawnAsync(
                NetworkPrefabKeys.NetworkPlayer,
                inputAuthority: notification.Player,
                cancellationToken: ct);
        }
    }
}
