using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkPlayerJoinedNotification : INotification
    {
        public readonly NetworkPlayerId Player;
        public readonly bool IsLocal;
        public readonly long SessionGeneration;

        public NetworkPlayerJoinedNotification(NetworkPlayerId player, bool isLocal) : this(player, isLocal, 0) { }

        public NetworkPlayerJoinedNotification(NetworkPlayerId player, bool isLocal, long sessionGeneration)
        {
            Player = player;
            IsLocal = isLocal;
            SessionGeneration = sessionGeneration;
        }
    }
}
