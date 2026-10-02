using System.Collections.Generic;
using Core.Domain.Enums;
using Core.Domain.Models.Lobby;

namespace Core.Abstractions.MVVM.Models.Lobby
{
    /// <summary>
    /// Запись в модель лобби. Отделена от <see cref="ILobbyModel"/>, чтобы вьюмодели читали
    /// состояние, но не подменяли его: наполняет модель только сетевой слой.
    /// </summary>
    public interface ILobbyModelWriter
    {
        void SetSessions(IReadOnlyList<SessionSummary> sessions);

        void SetRoster(IReadOnlyList<LobbyPlayerInfo> roster);

        void SetRoom(string roomCode, bool isHost);

        void ClearRoom();

        void SetPhase(SessionPhase phase, int countdownSeconds);

        void SetStatus(string statusText, bool isBusy);
    }
}
