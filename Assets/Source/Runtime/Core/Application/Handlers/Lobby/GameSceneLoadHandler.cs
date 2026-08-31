using System.Threading;
using Core.Abstractions.Config;
using Core.Application.Notifications.Matchmaking;
using Core.Configs.Services;
using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class GameSceneLoadHandler : INotificationHandler<SessionPhaseChangedNotification>
    {
        private readonly INetworkRunnerController _controller;
        private readonly IConfigProvider _configProvider;

        public GameSceneLoadHandler(INetworkRunnerController controller, IConfigProvider configProvider)
        {
            _controller = controller;
            _configProvider = configProvider;
        }

        public async UniTask Handle(SessionPhaseChangedNotification notification, CancellationToken ct)
        {
            if (notification.Phase != SessionPhase.Playing || !_controller.IsStateAuthority)
            {
                return;
            }

            var config = _configProvider.GetConfig<NetworkConfig>();

            if (config == null || string.IsNullOrEmpty(config.DefaultScenePath))
            {
                return;
            }

            await _controller.LoadNetworkSceneAsync(config.DefaultScenePath, ct);
        }
    }
}
