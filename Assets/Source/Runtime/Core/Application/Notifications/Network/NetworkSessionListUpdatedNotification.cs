using System.Collections.Generic;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Types;

namespace Core.Application.Notifications.Network
{
    public readonly struct NetworkSessionListUpdatedNotification : INotification
    {
        public readonly IReadOnlyList<NetworkSessionInfo> Sessions;

        public NetworkSessionListUpdatedNotification(IReadOnlyList<NetworkSessionInfo> sessions)
        {
            Sessions = sessions;
        }
    }
}
