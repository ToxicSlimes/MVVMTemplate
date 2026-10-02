using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Notifications.Matchmaking;
using Core.Domain.Enums;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class MatchCountdownSyncHandler : INotificationHandler<MatchCountdownTickNotification>
    {
        private readonly ILobbyModelWriter _lobby;

        public MatchCountdownSyncHandler(ILobbyModelWriter lobby)
        {
            _lobby = lobby;
        }

        public UniTask Handle(MatchCountdownTickNotification notification, CancellationToken ct)
        {
            _lobby.SetPhase(SessionPhase.Countdown, notification.SecondsLeft);
            return UniTask.CompletedTask;
        }
    }
}
