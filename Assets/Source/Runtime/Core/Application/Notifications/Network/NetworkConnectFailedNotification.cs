using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkConnectFailedNotification : INotification
    {
        public readonly NetworkConnectFailedReason Reason;
        public readonly string RemoteAddress;

        public NetworkConnectFailedNotification(NetworkConnectFailedReason reason, string remoteAddress)
        {
            Reason = reason;
            RemoteAddress = remoteAddress;
        }
    }
}
