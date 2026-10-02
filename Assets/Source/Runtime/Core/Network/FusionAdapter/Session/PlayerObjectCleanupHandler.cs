using System.Threading;
using Core.Application.Notifications.Network;
using Core.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Core.Network.FusionAdapter.Mapping;
using Core.Network.FusionAdapter.Player;
using Core.Network.FusionAdapter.Runner;
using Cysharp.Threading.Tasks;
using Fusion;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class PlayerObjectCleanupHandler : INotificationHandler<NetworkPlayerLeftNotification>
    {
        private readonly INetworkRunnerController _controller;
        private readonly INetworkRunnerAccessor _accessor;
        private readonly NetworkPlayerRegistry _players;
        private readonly PlayerAvatarRegistry _avatars;
        private readonly ILogger _logger;

        public PlayerObjectCleanupHandler(
            INetworkRunnerController controller,
            INetworkRunnerAccessor accessor,
            NetworkPlayerRegistry players,
            PlayerAvatarRegistry avatars,
            ILogger logger)
        {
            _controller = controller;
            _accessor = accessor;
            _players = players;
            _avatars = avatars;
            _logger = logger;
        }

        public UniTask Handle(NetworkPlayerLeftNotification notification, CancellationToken ct)
        {
            if (!_controller.IsStateAuthority || !_accessor.IsRunning)
            {
                return UniTask.CompletedTask;
            }

            var owner = notification.Player.ToFusion();

            DespawnAvatar(owner);
            DespawnRecord(owner);

            return UniTask.CompletedTask;
        }

        private void DespawnAvatar(PlayerRef owner)
        {
            if (!_avatars.TryGet(owner, out var body) || body == null)
            {
                return;
            }

            var networkObject = body.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                return;
            }

            _accessor.Runner.Despawn(networkObject);
        }

        private void DespawnRecord(PlayerRef owner)
        {
            if (!_players.TryGetByOwner(owner, out var player) || player == null)
            {
                return;
            }

            _logger.Info("[Network] Player {0} left, objects cleaned up", player.Nickname.Value);
            _accessor.Runner.Despawn(player.Object);
        }
    }
}
