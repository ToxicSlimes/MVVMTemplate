using System.Collections.Generic;
using Core.Application.Notifications.Matchmaking;
using Core.Domain.Models.Lobby;
using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;
using Fusion;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class NetworkPlayerRegistry : INetworkLocalPlayer
    {
        private readonly IMediator _mediator;
        private readonly List<NetworkPlayer> _players = new();
        private readonly List<LobbyPlayerInfo> _roster = new();

        public NetworkPlayerRegistry(IMediator mediator)
        {
            _mediator = mediator;
        }

        public int Count => _players.Count;

        public bool Exists => TryGetLocal(out _);

        public void SetReady(bool isReady)
        {
            if (TryGetLocal(out var local))
            {
                local.RpcSetReady(isReady);
            }
        }

        public void SetNickname(string nickname)
        {
            if (TryGetLocal(out var local))
            {
                local.RpcSetNickname(nickname);
            }
        }

        public void Add(NetworkPlayer player)
        {
            if (_players.Contains(player))
            {
                return;
            }

            _players.Add(player);
            PublishRoster();
        }

        public void Remove(NetworkPlayer player)
        {
            if (!_players.Remove(player))
            {
                return;
            }

            PublishRoster();
        }

        public void NotifyChanged() => PublishRoster();

        public bool TryGetAt(int index, out NetworkPlayer player)
        {
            if (index < 0 || index >= _players.Count)
            {
                player = null;
                return false;
            }

            player = _players[index];
            return true;
        }

        public bool TryGetByOwner(PlayerRef owner, out NetworkPlayer player)
        {
            for (var i = 0; i < _players.Count; i++)
            {
                var candidate = _players[i];

                if (candidate == null || candidate.Object == null || candidate.Owner != owner)
                {
                    continue;
                }

                player = candidate;
                return true;
            }

            player = null;
            return false;
        }

        private void PublishRoster()
        {
            _roster.Clear();

            for (var i = 0; i < _players.Count; i++)
            {
                var player = _players[i];

                _roster.Add(new LobbyPlayerInfo(
                    player.Nickname.Value,
                    player.ColorIndex,
                    player.IsReady,
                    player.IsHost,
                    player.HasInputAuthority,
                    true));
            }

            _mediator.Publish(new LobbyRosterChangedNotification(new List<LobbyPlayerInfo>(_roster))).Forget();
        }

        private bool TryGetLocal(out NetworkPlayer local)
        {
            for (var i = 0; i < _players.Count; i++)
            {
                if (!_players[i].HasInputAuthority)
                {
                    continue;
                }

                local = _players[i];
                return true;
            }

            local = null;
            return false;
        }
    }
}
