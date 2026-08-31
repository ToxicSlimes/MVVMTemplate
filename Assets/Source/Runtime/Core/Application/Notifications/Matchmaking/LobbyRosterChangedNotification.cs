using System.Collections.Generic;
using Core.Data.Domain.Models.Lobby;
using Core.Infrastructure.Mediator.Abstractions;

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
