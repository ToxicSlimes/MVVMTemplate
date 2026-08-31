namespace Core.Network.Abstractions.Types
{
    public readonly struct NetworkSessionInfo
    {
        public static readonly NetworkSessionInfo Empty = default;

        public readonly string Name;
        public readonly string Region;
        public readonly int PlayerCount;
        public readonly int MaxPlayers;
        public readonly bool IsOpen;
        public readonly bool IsVisible;
        public readonly bool IsValid;

        public NetworkSessionInfo(
            string name,
            string region,
            int playerCount,
            int maxPlayers,
            bool isOpen,
            bool isVisible)
        {
            Name = name;
            Region = region;
            PlayerCount = playerCount;
            MaxPlayers = maxPlayers;
            IsOpen = isOpen;
            IsVisible = isVisible;
            IsValid = true;
        }

        public override string ToString() =>
            IsValid ? $"Session({Name}, {PlayerCount}/{MaxPlayers})" : "Session(None)";
    }
}
