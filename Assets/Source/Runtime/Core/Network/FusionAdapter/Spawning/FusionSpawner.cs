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
    /// <summary>Retains the original runner and object when stale-spawn cleanup fails.</summary>
    public sealed class FusionSpawnCleanupException : InvalidOperationException
    {
        public NetworkRunner Origin { get; }
        public NetworkObject SpawnedObject { get; }

        public FusionSpawnCleanupException(NetworkRunner origin, NetworkObject spawnedObject, Exception cause)
            : base("Fusion spawn cleanup failed; the original runner and object are retained for diagnosis or recovery", cause)
        {
            Origin = origin;
            SpawnedObject = spawnedObject;
        }
    }

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

            var origin = _accessor.Runner;
            var callbackActive = true;
            NetworkObject spawned;
            try
            {
                spawned = await origin.SpawnAsync(
                    prefab.Value,
                    position,
                    rotation,
                    inputAuthority?.ToFusion(),
                    beforeSpawned == null ? null : (runner, networkObject) =>
                    {
                        if (callbackActive && runner == origin && _accessor.IsRunning && _accessor.Runner == origin && origin.IsRunning)
                        {
                            beforeSpawned(NetworkEntityAdapter.Wrap(networkObject));
                        }
                    });
            }
            finally
            {
                callbackActive = false;
            }

            if (!_accessor.IsRunning || _accessor.Runner != origin || origin == null || !origin.IsRunning)
            {
                if (spawned != null)
                {
                    try
                    {
                        if (origin == null || !origin.IsRunning || spawned.Runner != origin || !spawned.HasStateAuthority)
                        {
                            throw new InvalidOperationException("Fusion spawn cleanup невозможен: исходный runner недоступен, не владеет объектом или отсутствует state authority");
                        }

                        origin.Despawn(spawned);
                    }
                    catch (Exception error)
                    {
                        throw new FusionSpawnCleanupException(origin, spawned, error);
                    }
                }

                throw new OperationCanceledException("Сетевая сессия сменилась во время spawn", cancellationToken);
            }

            return RequireSpawned(spawned, prefab);
        }

        public void Despawn(INetworkEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var networkObject = NetworkEntityAdapter.Unwrap(entity);
            if (networkObject == null)
            {
                throw new ArgumentException("Сущность не является действительным Fusion-объектом", nameof(entity));
            }

            var owner = networkObject.Runner;
            if (owner == null || !owner.IsRunning || !networkObject.IsValid || !owner.Exists(networkObject) || !networkObject.HasStateAuthority)
            {
                throw new InvalidOperationException("Despawn невозможен: объект недействителен, не принадлежит активному runner или отсутствует state authority");
            }

            owner.Despawn(networkObject);
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
