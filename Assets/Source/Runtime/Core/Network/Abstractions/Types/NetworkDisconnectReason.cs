namespace Core.Network.Abstractions.Types
{
    public enum NetworkDisconnectReason
    {
        Unknown = 0,
        Requested,
        ByRemote,
        Timeout,
        ProtocolError,
        SendWindowFull,
        SequenceOutOfBounds
    }
}
