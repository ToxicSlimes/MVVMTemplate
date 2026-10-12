using Core.Network.Abstractions.Types;

namespace Core.Network.Abstractions.Gateway.Transport
{
    public readonly struct GatewayRemotePeer
    {
        public NetworkPlayerId Player { get; }

        public GatewayRemotePeer(NetworkPlayerId player)
        {
            Player = player;
        }
    }
}
