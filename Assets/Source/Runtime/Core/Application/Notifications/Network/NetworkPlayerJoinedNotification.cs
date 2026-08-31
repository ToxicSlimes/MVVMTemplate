using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkPlayerJoinedNotification : INotification
    {
        public readonly NetworkPlayerId Player;
        public readonly bool IsLocal;

        public NetworkPlayerJoinedNotification(NetworkPlayerId player, bool isLocal)
        {
            Player = player;
            IsLocal = isLocal;
        }
    }
}
