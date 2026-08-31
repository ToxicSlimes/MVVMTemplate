namespace Core.Network.Abstractions.Services
{
    public interface INetworkSessionControl
    {
        bool Exists { get; }

        void StartMatch();

        void EndMatch();
    }
}
