namespace Core.Domain.Models.Lobby
{
    public readonly struct LobbyPlayerInfo
    {
        public static readonly LobbyPlayerInfo Empty = new(string.Empty, 0, false, false, false, false);

        public string Nickname { get; }
        public int ColorIndex { get; }
        public bool IsReady { get; }
        public bool IsHost { get; }
        public bool IsLocal { get; }
        public bool IsOccupied { get; }

        public LobbyPlayerInfo(string nickname, int colorIndex, bool isReady, bool isHost, bool isLocal, bool isOccupied)
        {
            Nickname = nickname;
            ColorIndex = colorIndex;
            IsReady = isReady;
            IsHost = isHost;
            IsLocal = isLocal;
            IsOccupied = isOccupied;
        }
    }
}
