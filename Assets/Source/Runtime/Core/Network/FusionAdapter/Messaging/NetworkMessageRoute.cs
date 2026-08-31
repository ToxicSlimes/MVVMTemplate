namespace Core.Network.FusionAdapter.Messaging
{
    /// <summary>
    /// Кодируется в <see cref="Fusion.Sockets.ReliableKey"/> и определяет,
    /// должен ли получатель ретранслировать сообщение дальше.
    /// </summary>
    public enum NetworkMessageRoute
    {
        Direct = 0,
        RelayToAll = 1,
        RelayToPlayer = 2
    }
}
