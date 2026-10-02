using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Notifications.Network;
using Core.Mediator.Handlers;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    public sealed class RoomExitedSyncHandler : INotificationHandler<NetworkShutdownNotification>
    {
        private const string LeftStatus = "Left the room";

        private readonly ILobbyModelWriter _lobby;

        public RoomExitedSyncHandler(ILobbyModelWriter lobby)
        {
            _lobby = lobby;
        }

        public UniTask Handle(NetworkShutdownNotification notification, CancellationToken ct)
        {
            _lobby.ClearRoom();
            _lobby.SetStatus(DescribeReason(notification.Reason), false);

            return UniTask.CompletedTask;
        }

        private static string DescribeReason(NetworkShutdownReason reason) => reason switch
        {
            NetworkShutdownReason.Ok => LeftStatus,
            NetworkShutdownReason.GameNotFound => "Room not found",
            NetworkShutdownReason.GameIsFull => "Room is full",
            NetworkShutdownReason.GameClosed => "Room is closed",
            NetworkShutdownReason.GameIdAlreadyExists => "Room code is taken",
            NetworkShutdownReason.ConnectionTimeout => "Connection timed out",
            NetworkShutdownReason.ConnectionRefused => "Connection refused",
            NetworkShutdownReason.OperationCanceled => LeftStatus,
            _ => $"Disconnected: {reason}"
        };
    }
}
