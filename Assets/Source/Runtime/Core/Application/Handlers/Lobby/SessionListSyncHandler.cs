using System.Collections.Generic;
using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Notifications.Network;
using Core.Domain.Models.Lobby;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Lobby
{
    /// <summary>
    /// Единственное место, где сетевое описание сессии превращается в доменное.
    /// Ниже по течению о сети уже никто не знает.
    /// </summary>
    public sealed class SessionListSyncHandler : INotificationHandler<NetworkSessionListUpdatedNotification>
    {
        private readonly ILobbyModelWriter _lobby;

        public SessionListSyncHandler(ILobbyModelWriter lobby)
        {
            _lobby = lobby;
        }

        public UniTask Handle(NetworkSessionListUpdatedNotification notification, CancellationToken ct)
        {
            var source = notification.Sessions;
            var summaries = new List<SessionSummary>(source?.Count ?? 0);

            if (source != null)
            {
                for (var i = 0; i < source.Count; i++)
                {
                    summaries.Add(new SessionSummary(source[i].Name, source[i].PlayerCount, source[i].MaxPlayers));
                }
            }

            _lobby.SetSessions(summaries);

            return UniTask.CompletedTask;
        }
    }
}
