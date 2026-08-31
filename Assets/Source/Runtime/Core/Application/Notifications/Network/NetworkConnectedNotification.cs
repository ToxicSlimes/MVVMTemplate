using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkConnectedNotification : INotification
    {
        public readonly NetworkSessionInfo Session;

        public NetworkConnectedNotification(NetworkSessionInfo session)
        {
            Session = session;
        }
    }
}
