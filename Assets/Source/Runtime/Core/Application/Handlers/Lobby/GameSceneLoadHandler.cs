using System.Threading;
using Core.Application.Notifications.Matchmaking;
using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class GameSceneLoadHandler : INotificationHandler<SessionPhaseChangedNotification>
    {
        private readonly INetworkRunnerController _controller;
        private readonly INetworkSessionOptionsProvider _optionsProvider;

        public GameSceneLoadHandler(
            INetworkRunnerController controller,
            INetworkSessionOptionsProvider optionsProvider)
        {
            _controller = controller;
            _optionsProvider = optionsProvider;
        }

        public async UniTask Handle(SessionPhaseChangedNotification notification, CancellationToken ct)
        {
            if (notification.Phase != SessionPhase.Playing || !_controller.IsStateAuthority)
            {
                return;
            }

            if (!_optionsProvider.TryGet(out var options) || string.IsNullOrEmpty(options.DefaultScenePath))
            {
                return;
            }

            await _controller.LoadNetworkSceneAsync(options.DefaultScenePath, ct);
        }
    }
}
