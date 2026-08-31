using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Notifications.Matchmaking;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class LobbyRosterSyncHandler : INotificationHandler<LobbyRosterChangedNotification>
    {
        private readonly ILobbyModelWriter _lobby;

        public LobbyRosterSyncHandler(ILobbyModelWriter lobby)
        {
            _lobby = lobby;
        }

        public UniTask Handle(LobbyRosterChangedNotification notification, CancellationToken ct)
        {
            _lobby.SetRoster(notification.Roster);
            return UniTask.CompletedTask;
        }
    }
}
