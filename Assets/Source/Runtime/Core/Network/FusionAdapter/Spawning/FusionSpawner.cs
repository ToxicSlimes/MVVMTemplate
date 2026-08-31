using System;
using System.Threading;
using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Mapping;
using Core.Network.FusionAdapter.Runner;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace Core.Network.FusionAdapter.Spawning
{
    public sealed class FusionSpawner : INetworkSpawner
    {
        private readonly INetworkRunnerAccessor _accessor;

        public FusionSpawner(INetworkRunnerAccessor accessor)
        {
            _accessor = accessor;
        }

        public INetworkEntity Spawn(
            NetworkPrefabKey prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            NetworkPlayerId? inputAuthority = null,
            Action<INetworkEntity> beforeSpawned = null)
        {
            ValidateSpawn(prefab);

            var spawned = _accessor.Runner.Spawn(
                prefab.Value,
                position,
                rotation,
                inputAuthority?.ToFusion(),
                WrapBeforeSpawned(beforeSpawned));

            return RequireSpawned(spawned, prefab);
        }

        public async UniTask<INetworkEntity> SpawnAsync(
            NetworkPrefabKey prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            NetworkPlayerId? inputAuthority = null,
            Action<INetworkEntity> beforeSpawned = null,
            CancellationToken cancellationToken = default)
        {
            ValidateSpawn(prefab);
            cancellationToken.ThrowIfCancellationRequested();

            var spawned = await _accessor.Runner.SpawnAsync(
                prefab.Value,
                position,
                rotation,
                inputAuthority?.ToFusion(),
                WrapBeforeSpawned(beforeSpawned));

            cancellationToken.ThrowIfCancellationRequested();
            return RequireSpawned(spawned, prefab);
        }

        public void Despawn(INetworkEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!_accessor.IsRunning)
            {
                throw new InvalidOperationException("Despawn невозможен: сетевая сессия не запущена");
            }

            var networkObject = NetworkEntityAdapter.Unwrap(entity);
            if (networkObject == null)
            {
                throw new ArgumentException($"Сущность {entity.Id} не является Fusion-объектом", nameof(entity));
            }

            _accessor.Runner.Despawn(networkObject);
        }

        public bool TryGetEntity(NetworkEntityId id, out INetworkEntity entity)
        {
            entity = null;

            if (!_accessor.IsRunning || !id.IsValid)
            {
                return false;
            }

            if (!_accessor.Runner.TryFindObject(id.ToFusion(), out var networkObject))
            {
                return false;
            }

            entity = NetworkEntityAdapter.Wrap(networkObject);
            return entity != null;
        }

        private void ValidateSpawn(NetworkPrefabKey prefab)
        {
            if (!prefab.IsValid)
            {
                throw new ArgumentException("Ключ префаба пуст", nameof(prefab));
            }

            if (!_accessor.IsRunning)
            {
                throw new InvalidOperationException($"Спавн '{prefab}' невозможен: сетевая сессия не запущена");
            }
        }

        private static NetworkRunner.OnBeforeSpawned WrapBeforeSpawned(Action<INetworkEntity> beforeSpawned)
        {
            if (beforeSpawned == null)
            {
                return null;
            }

            return (_, spawnedObject) => beforeSpawned(NetworkEntityAdapter.Wrap(spawnedObject));
        }

        private static INetworkEntity RequireSpawned(NetworkObject spawned, NetworkPrefabKey prefab)
        {
            if (spawned == null)
            {
                throw new InvalidOperationException(
                    $"Fusion не смог заспавнить '{prefab}'. Проверь, что префаб зарегистрирован в NetworkProjectConfig");
            }

            return NetworkEntityAdapter.Wrap(spawned);
        }
    }
}
