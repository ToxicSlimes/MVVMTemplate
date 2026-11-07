using System;
using System.Collections.Generic;
using System.Threading;
using Core.Application.Handlers.Lobby;
using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Core.Tests.Network
{
    public sealed class PlayerAvatarSpawnerTests
    {
        private static readonly NetworkPlayerId Player = new(1);

        [TestCase(false)]
        [TestCase(true)]
        public void FailedOrCancelledSpawnCanBeRetried(bool cancelled)
        {
            var network = new FakeNetwork();
            network.Failure = cancelled ? (Exception)new OperationCanceledException() : new InvalidOperationException();
            var subject = new PlayerAvatarSpawner(network, network, network);

            Assert.Catch(() => subject.SpawnAsync(Player, default).GetAwaiter().GetResult());
            network.Failure = null;
            subject.SpawnAsync(Player, default).GetAwaiter().GetResult();
            subject.SpawnAsync(Player, default).GetAwaiter().GetResult();

            Assert.That(network.Calls, Is.EqualTo(2));
        }

        [Test]
        public void ConcurrentSpawnIsNotDuplicated()
        {
            var network = new FakeNetwork { Pending = new UniTaskCompletionSource<INetworkEntity>() };
            var subject = new PlayerAvatarSpawner(network, network, network);
            var first = subject.SpawnAsync(Player, default);
            subject.SpawnAsync(Player, default).GetAwaiter().GetResult();
            Assert.That(network.Calls, Is.EqualTo(1));
            network.Pending.TrySetResult(null);
            first.GetAwaiter().GetResult();
            subject.SpawnAsync(Player, default).GetAwaiter().GetResult();
            Assert.That(network.Calls, Is.EqualTo(1));
        }

        [Test]
        public void OldFailureAfterResetDoesNotClearNewSpawn()
        {
            var pending = new UniTaskCompletionSource<INetworkEntity>();
            var network = new FakeNetwork { Pending = pending };
            var subject = new PlayerAvatarSpawner(network, network, network);
            var old = subject.SpawnAsync(Player, default);
            subject.Reset();
            network.Pending = null;
            subject.SpawnAsync(Player, default).GetAwaiter().GetResult();
            pending.TrySetException(new InvalidOperationException());
            Assert.Catch(() => old.GetAwaiter().GetResult());
            subject.SpawnAsync(Player, default).GetAwaiter().GetResult();
            Assert.That(network.Calls, Is.EqualTo(2));
        }

        private sealed class FakeNetwork : INetworkRunnerController, INetworkSpawner, INetworkSpawnPoints
        {
            public int Calls;
            public Exception Failure;
            public UniTaskCompletionSource<INetworkEntity> Pending;
            public bool IsRunning => true;
            public long SessionGeneration { get; set; }
            public NetworkMode Mode => NetworkMode.Host;
            public NetworkPlayerId LocalPlayer => Player;
            public IReadOnlyList<NetworkPlayerId> ActivePlayers => new[] { Player };
            public NetworkSessionInfo Session => default;
            public bool IsStateAuthority => true;
            public int Count => 0;
            public bool TryGet(int index, out Vector3 position, out Quaternion rotation) { position = default; rotation = default; return false; }
            public UniTask<INetworkEntity> SpawnAsync(NetworkPrefabKey prefab, Vector3? position = null, Quaternion? rotation = null, NetworkPlayerId? inputAuthority = null, Action<INetworkEntity> beforeSpawned = null, CancellationToken cancellationToken = default)
            {
                Calls++;
                if (Failure != null) return UniTask.FromException<INetworkEntity>(Failure);
                return Pending != null ? Pending.Task : UniTask.FromResult<INetworkEntity>(null);
            }
            public INetworkEntity Spawn(NetworkPrefabKey prefab, Vector3? position = null, Quaternion? rotation = null, NetworkPlayerId? inputAuthority = null, Action<INetworkEntity> beforeSpawned = null) => throw new NotSupportedException();
            public void Despawn(INetworkEntity entity) => throw new NotSupportedException();
            public bool TryGetEntity(NetworkEntityId id, out INetworkEntity entity) => throw new NotSupportedException();
            public void Start(NetworkStartRequest request, Action<NetworkStartResult> onCompleted = null) => throw new NotSupportedException();
            public UniTask<NetworkStartResult> StartAsync(NetworkStartRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public void Shutdown() => throw new NotSupportedException();
            public UniTask ShutdownAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public void LoadNetworkScene(string scenePath) => throw new NotSupportedException();
            public UniTask LoadNetworkSceneAsync(string scenePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public void Dispose() { }
        }
    }
}
