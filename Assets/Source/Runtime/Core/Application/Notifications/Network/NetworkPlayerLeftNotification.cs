using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkPlayerLeftNotification : INotification
    {
        public readonly NetworkPlayerId Player;

        public NetworkPlayerLeftNotification(NetworkPlayerId player)
        {
            Player = player;
        }
    }
}
