namespace Core.Domain.Models.Lobby
{
    public readonly struct SessionSummary
    {
        public string RoomCode { get; }
        public int PlayerCount { get; }
        public int MaxPlayers { get; }

        public SessionSummary(string roomCode, int playerCount, int maxPlayers)
        {
            RoomCode = roomCode;
            PlayerCount = playerCount;
            MaxPlayers = maxPlayers;
        }

        public bool IsFull => PlayerCount >= MaxPlayers;
    }
}
