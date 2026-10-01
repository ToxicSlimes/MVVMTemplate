namespace Core.Network.Abstractions.Services
{
    public readonly struct NetworkSessionOptions
    {
        public int DefaultPlayerCount { get; }
        public bool SessionVisible { get; }
        public bool SessionOpen { get; }
        public string DefaultScenePath { get; }

        public NetworkSessionOptions(
            int defaultPlayerCount,
            bool sessionVisible,
            bool sessionOpen,
            string defaultScenePath)
        {
            DefaultPlayerCount = defaultPlayerCount;
            SessionVisible = sessionVisible;
            SessionOpen = sessionOpen;
            DefaultScenePath = defaultScenePath;
        }
    }

    public interface INetworkSessionOptionsProvider
    {
        bool TryGet(out NetworkSessionOptions options);
    }
}
