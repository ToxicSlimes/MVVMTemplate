using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models;
using Core.Data.Domain.Enums;
using Core.Data.Domain.Models.Lobby;

namespace Core.Abstractions.MVVM.Models.Lobby
{
    public interface ILobbyModel : IModel, IDisposable
    {
        event Action RosterChanged;
        event Action SessionsChanged;
        event Action PhaseChanged;
        event Action StatusChanged;

        string Nickname { get; }
        string RoomCode { get; }
        SessionPhase Phase { get; }
        int CountdownSeconds { get; }
        IReadOnlyList<LobbyPlayerInfo> Roster { get; }
        IReadOnlyList<SessionSummary> Sessions { get; }
        bool IsHost { get; }
        bool IsLocalReady { get; }
        bool CanStart { get; }
        string StatusText { get; }
        bool IsBusy { get; }

        void SetNickname(string nickname);
    }
}
