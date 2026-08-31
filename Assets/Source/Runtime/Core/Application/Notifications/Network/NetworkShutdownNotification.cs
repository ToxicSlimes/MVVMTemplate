using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkShutdownNotification : INotification
    {
        public readonly NetworkShutdownReason Reason;

        public NetworkShutdownNotification(NetworkShutdownReason reason)
        {
            Reason = reason;
        }
    }
}
