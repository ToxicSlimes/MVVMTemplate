using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Domain.Enums;
using Core.Domain.Models.Lobby;

namespace Core.Presentation.MVVM.Models.Lobby
{
    /// <summary>
    /// Состояние лобби. Ничего не знает о сети: наполняется извне через
    /// <see cref="ILobbyModelWriter"/>, вьюмодели только читают.
    /// </summary>
    public class LobbyModel : ILobbyModel, ILobbyModelWriter
    {
        private const string IdleStatus = "Not connected";
        private const string NicknamePrefix = "Alien-";
        private const int MinimumPlayersToStart = 2;

        private static readonly Random NicknameSuffix = new();

        private readonly List<LobbyPlayerInfo> _roster = new();
        private readonly List<SessionSummary> _sessions = new();

        public event Action RosterChanged;
        public event Action SessionsChanged;
        public event Action PhaseChanged;
        public event Action StatusChanged;

        /// <summary>
        /// Ник заполнен с самого начала: пустое поле уводило игрока в тупик, где кнопка входа
        /// выглядит рабочей, а отказ виден только мелким статусом внизу экрана.
        /// </summary>
        public string Nickname { get; private set; } = NicknamePrefix + NicknameSuffix.Next(1000, 10000);
        public string RoomCode { get; private set; } = string.Empty;
        public SessionPhase Phase { get; private set; } = SessionPhase.None;
        public int CountdownSeconds { get; private set; }
        public IReadOnlyList<LobbyPlayerInfo> Roster => _roster;
        public IReadOnlyList<SessionSummary> Sessions => _sessions;
        public bool IsHost { get; private set; }
        public bool IsLocalReady { get; private set; }
        public string StatusText { get; private set; } = IdleStatus;
        public bool IsBusy { get; private set; }
        public bool IsValid => true;

        public bool CanStart
        {
            get
            {
                if (!IsHost)
                {
                    return false;
                }

                var occupied = 0;

                for (var i = 0; i < _roster.Count; i++)
                {
                    if (!_roster[i].IsOccupied)
                    {
                        continue;
                    }

                    if (!_roster[i].IsReady)
                    {
                        return false;
                    }

                    occupied++;
                }

                return occupied >= MinimumPlayersToStart;
            }
        }

        public void SetNickname(string nickname)
        {
            if (Nickname == nickname || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            Nickname = nickname;
            RosterChanged?.Invoke();
        }

        public void SetSessions(IReadOnlyList<SessionSummary> sessions)
        {
            _sessions.Clear();

            if (sessions != null)
            {
                _sessions.AddRange(sessions);
            }

            SessionsChanged?.Invoke();
        }

        /// <summary>
        /// Готовность и роль хоста берутся из своей записи в ростере, а не хранятся отдельно:
        /// источник истины — реплицированное состояние, локальная копия разошлась бы с ним.
        /// </summary>
        public void SetRoster(IReadOnlyList<LobbyPlayerInfo> roster)
        {
            _roster.Clear();

            if (roster != null)
            {
                _roster.AddRange(roster);
            }

            for (var i = 0; i < _roster.Count; i++)
            {
                if (!_roster[i].IsLocal)
                {
                    continue;
                }

                IsLocalReady = _roster[i].IsReady;
                IsHost = _roster[i].IsHost;
                break;
            }

            RosterChanged?.Invoke();
        }

        public void SetRoom(string roomCode, bool isHost)
        {
            if (RoomCode == roomCode && IsHost == isHost)
            {
                return;
            }

            RoomCode = roomCode ?? string.Empty;
            IsHost = isHost;
            RosterChanged?.Invoke();
        }

        public void ClearRoom()
        {
            RoomCode = string.Empty;
            IsHost = false;
            IsLocalReady = false;
            _roster.Clear();

            SetPhase(SessionPhase.None, 0);
            RosterChanged?.Invoke();
        }

        public void SetPhase(SessionPhase phase, int countdownSeconds)
        {
            if (Phase == phase && CountdownSeconds == countdownSeconds)
            {
                return;
            }

            Phase = phase;
            CountdownSeconds = countdownSeconds;
            PhaseChanged?.Invoke();
        }

        public void SetStatus(string statusText, bool isBusy)
        {
            if (StatusText == statusText && IsBusy == isBusy)
            {
                return;
            }

            StatusText = statusText ?? string.Empty;
            IsBusy = isBusy;
            StatusChanged?.Invoke();
        }

        public void Reset()
        {
            ClearRoom();
            _sessions.Clear();

            SetStatus(IdleStatus, false);
            SessionsChanged?.Invoke();
        }

        public void Dispose()
        {
            RosterChanged = null;
            SessionsChanged = null;
            PhaseChanged = null;
            StatusChanged = null;
        }
    }
}
