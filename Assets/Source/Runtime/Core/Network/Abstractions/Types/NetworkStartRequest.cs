namespace Core.Network.Abstractions.Types
{
    public readonly struct NetworkStartRequest
    {
        public readonly NetworkMode Mode;
        public readonly string SessionName;
        public readonly int PlayerCount;
        public readonly string ScenePath;
        public readonly bool IsVisible;
        public readonly bool IsOpen;

        public NetworkStartRequest(
            NetworkMode mode,
            string sessionName = null,
            int playerCount = 0,
            string scenePath = null,
            bool isVisible = true,
            bool isOpen = true)
        {
            Mode = mode;
            SessionName = sessionName;
            PlayerCount = playerCount;
            ScenePath = scenePath;
            IsVisible = isVisible;
            IsOpen = isOpen;
        }

        public override string ToString() => $"StartRequest({Mode}, {SessionName ?? "auto"})";
    }
}
