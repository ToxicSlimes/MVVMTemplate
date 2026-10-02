using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Notifications.Matchmaking;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class SessionPhaseSyncHandler : INotificationHandler<SessionPhaseChangedNotification>
    {
        private readonly ILobbyModelWriter _lobby;

        public SessionPhaseSyncHandler(ILobbyModelWriter lobby)
        {
            _lobby = lobby;
        }

        public UniTask Handle(SessionPhaseChangedNotification notification, CancellationToken ct)
        {
            _lobby.SetPhase(notification.Phase, 0);
            return UniTask.CompletedTask;
        }
    }
}
