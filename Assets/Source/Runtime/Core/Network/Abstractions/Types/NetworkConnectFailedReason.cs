namespace Core.Network.Abstractions.Types
{
    public enum NetworkConnectFailedReason
    {
        Unknown = 0,
        Timeout,
        ServerRefused,
        ServerFull
    }
}
