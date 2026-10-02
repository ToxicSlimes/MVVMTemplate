using System.Collections.Generic;
using Core.Domain.Models.Lobby;
using Core.Mediator.Abstractions;

namespace Core.Application.Notifications.Matchmaking
{
    public readonly struct LobbyRosterChangedNotification : INotification
    {
        public readonly IReadOnlyList<LobbyPlayerInfo> Roster;

        public LobbyRosterChangedNotification(IReadOnlyList<LobbyPlayerInfo> roster)
        {
            Roster = roster;
        }
    }
}
