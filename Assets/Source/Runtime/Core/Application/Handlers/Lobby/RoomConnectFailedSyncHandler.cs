using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Notifications.Network;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class RoomConnectFailedSyncHandler : INotificationHandler<NetworkConnectFailedNotification>
    {
        private readonly ILobbyModelWriter _lobby;

        public RoomConnectFailedSyncHandler(ILobbyModelWriter lobby)
        {
            _lobby = lobby;
        }

        public UniTask Handle(NetworkConnectFailedNotification notification, CancellationToken ct)
        {
            _lobby.ClearRoom();
            _lobby.SetStatus($"Connection failed: {notification.Reason}", false);

            return UniTask.CompletedTask;
        }
    }
}
