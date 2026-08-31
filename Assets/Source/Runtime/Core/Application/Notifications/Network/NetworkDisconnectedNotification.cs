using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkDisconnectedNotification : INotification
    {
        public readonly NetworkDisconnectReason Reason;

        public NetworkDisconnectedNotification(NetworkDisconnectReason reason)
        {
            Reason = reason;
        }
    }
}
