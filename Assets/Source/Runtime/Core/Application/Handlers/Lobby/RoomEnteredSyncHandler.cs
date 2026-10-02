using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Notifications.Network;
using Core.Domain.Enums;
using Core.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class RoomEnteredSyncHandler : INotificationHandler<NetworkConnectedNotification>
    {
        private const string InRoomStatus = "In room";

        private readonly ILobbyModelWriter _lobby;
        private readonly INetworkRunnerController _controller;

        public RoomEnteredSyncHandler(ILobbyModelWriter lobby, INetworkRunnerController controller)
        {
            _lobby = lobby;
            _controller = controller;
        }

        public UniTask Handle(NetworkConnectedNotification notification, CancellationToken ct)
        {
            _lobby.SetRoom(notification.Session.Name, _controller.IsStateAuthority);
            _lobby.SetPhase(SessionPhase.Lobby, 0);
            _lobby.SetStatus(InRoomStatus, false);

            return UniTask.CompletedTask;
        }
    }
}
