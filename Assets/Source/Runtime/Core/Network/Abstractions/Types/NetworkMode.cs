namespace Core.Network.Abstractions.Types
{
    public enum NetworkMode
    {
        None = 0,
        Single,
        Shared,
        Host,
        Client,
        Server,
        AutoHostOrClient
    }
}
