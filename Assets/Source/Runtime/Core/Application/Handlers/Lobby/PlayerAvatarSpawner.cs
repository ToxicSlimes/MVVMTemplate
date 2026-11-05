using System.Collections.Generic;
using System.Threading;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Application.Handlers.Lobby
{
    public sealed class PlayerAvatarSpawner
    {
        private readonly INetworkRunnerController _controller;
        private readonly INetworkSpawner _spawner;
        private readonly INetworkSpawnPoints _spawnPoints;
        private HashSet<int> _spawned = new();

        public PlayerAvatarSpawner(
            INetworkRunnerController controller,
            INetworkSpawner spawner,
            INetworkSpawnPoints spawnPoints)
        {
            _controller = controller;
            _spawner = spawner;
            _spawnPoints = spawnPoints;
        }

        public void Reset() => _spawned = new HashSet<int>();

        public async UniTask SpawnAllAsync(CancellationToken cancellationToken)
        {
            var players = _controller.ActivePlayers;

            for (var i = 0; i < players.Count; i++)
            {
                await SpawnAsync(players[i], cancellationToken);
            }
        }

        public async UniTask SpawnAsync(NetworkPlayerId player, CancellationToken cancellationToken)
        {
            var spawned = _spawned;
            if (!_controller.IsStateAuthority || !player.IsValid || !spawned.Add(player.Raw))
            {
                return;
            }

            try
            {
                ResolvePose(player, out var position, out var rotation);

                await _spawner.SpawnAsync(
                    NetworkPrefabKeys.PlayerAvatar,
                    position,
                    rotation,
                    player,
                    cancellationToken: cancellationToken);
            }
            catch
            {
                spawned.Remove(player.Raw);
                throw;
            }
        }

        private void ResolvePose(NetworkPlayerId player, out Vector3 position, out Quaternion rotation)
        {
            var count = _spawnPoints.Count;

            if (count > 0 && _spawnPoints.TryGet(Mathf.Abs(player.Raw) % count, out position, out rotation))
            {
                return;
            }

            position = Vector3.zero;
            rotation = Quaternion.identity;
        }
    }
}
