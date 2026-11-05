using System;
using System.Collections.Generic;
using System.Threading;
using Core.Application.Handlers.Matchmaking;
using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Core.Presentation.MVVM.Models.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Core.Tests.Network
{
    public sealed class RoomLaunchHandlerTests
    {
        [TestCase("leave", false)]
        [TestCase("start", false)]
        [TestCase("spawn", false)]
        [TestCase("leave", true)]
        [TestCase("start", true)]
        [TestCase("spawn", true)]
        public void FailureOrCancellationClearsBusyAndPropagatesOriginal(string stage, bool cancelled)
        {
            var model = new LobbyModel();
            var error = cancelled ? (Exception)new OperationCanceledException("cancelled") : new InvalidOperationException("network failed");
            var network = new FakeNetwork { FailureStage = stage, Error = error };
            var handler = new TestHandler(network, model);

            var caught = Assert.Catch(() => handler.Run(NetworkMode.Host).GetAwaiter().GetResult());

            if (cancelled)
                Assert.That(caught, Is.InstanceOf<OperationCanceledException>());
            else
                Assert.That(caught, Is.SameAs(error));
            Assert.That(model.IsBusy, Is.False);
        }

        [Test]
        public void StatusSubscriberFailureDoesNotReplaceNetworkException()
        {
            var model = new LobbyModel();
            var networkError = new InvalidOperationException("network failed");
            var subscriberError = new InvalidOperationException("subscriber failed");
            var network = new FakeNetwork { FailureStage = "leave", Error = networkError };
            model.StatusChanged += () => { if (!model.IsBusy) throw subscriberError; };

            var caught = Assert.Catch(() => new TestHandler(network, model).Run(NetworkMode.Host).GetAwaiter().GetResult());

            Assert.That(caught, Is.SameAs(networkError));
            Assert.That(caught.Data["RoomLaunchStatusResetException"], Is.SameAs(subscriberError));
            Assert.That(model.IsBusy, Is.False);
        }

        [TestCase(NetworkMode.Host)]
        [TestCase(NetworkMode.Client)]
        public void SuccessKeepsBusyForNetworkNotification(NetworkMode mode)
        {
            var model = new LobbyModel();
            var network = new FakeNetwork();
            var result = new TestHandler(network, model).Run(mode).GetAwaiter().GetResult();
            Assert.That(result.IsOk, Is.True);
            Assert.That(model.IsBusy, Is.True);
            Assert.That(network.Spawned, Is.EqualTo(mode == NetworkMode.Host));
        }

        private sealed class TestHandler : RoomLaunchHandlerBase
        {
            public TestHandler(FakeNetwork network, LobbyModel model) : base(network, network, network, network, model) { }
            public UniTask<NetworkStartResult> Run(NetworkMode mode) => LaunchAsync(mode, "ABC123", "Player", "Connecting", CancellationToken.None);
        }

        private sealed class FakeNetwork : INetworkLobby, INetworkRunnerController, INetworkSpawner, INetworkSessionOptionsProvider
        {
            public string FailureStage;
            public Exception Error;
            public bool Spawned;
            public bool IsInLobby => true;
            public IReadOnlyList<NetworkSessionInfo> Sessions => Array.Empty<NetworkSessionInfo>();
            public bool IsRunning => true;
            public NetworkMode Mode => NetworkMode.Host;
            public NetworkPlayerId LocalPlayer => default;
            public IReadOnlyList<NetworkPlayerId> ActivePlayers => Array.Empty<NetworkPlayerId>();
            public NetworkSessionInfo Session => default;
            public bool IsStateAuthority => true;
            public bool TryGet(out NetworkSessionOptions options) { options = new NetworkSessionOptions(4, true, true, null); return true; }
            public UniTask LeaveAsync(CancellationToken cancellationToken = default) => FailureStage == "leave" ? UniTask.FromException(Error) : UniTask.CompletedTask;
            public UniTask<NetworkStartResult> StartAsync(NetworkStartRequest request, CancellationToken cancellationToken = default) => FailureStage == "start" ? UniTask.FromException<NetworkStartResult>(Error) : UniTask.FromResult(NetworkStartResult.Success);
            public UniTask<INetworkEntity> SpawnAsync(NetworkPrefabKey prefab, Vector3? position = null, Quaternion? rotation = null, NetworkPlayerId? inputAuthority = null, Action<INetworkEntity> beforeSpawned = null, CancellationToken cancellationToken = default)
            {
                Spawned = true;
                return FailureStage == "spawn" ? UniTask.FromException<INetworkEntity>(Error) : UniTask.FromResult<INetworkEntity>(null);
            }
            public UniTask<NetworkStartResult> JoinAsync(string lobbyName = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public void Start(NetworkStartRequest request, Action<NetworkStartResult> onCompleted = null) => throw new NotSupportedException();
            public void Shutdown() => throw new NotSupportedException();
            public UniTask ShutdownAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public void LoadNetworkScene(string scenePath) => throw new NotSupportedException();
            public UniTask LoadNetworkSceneAsync(string scenePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public INetworkEntity Spawn(NetworkPrefabKey prefab, Vector3? position = null, Quaternion? rotation = null, NetworkPlayerId? inputAuthority = null, Action<INetworkEntity> beforeSpawned = null) => throw new NotSupportedException();
            public void Despawn(INetworkEntity entity) => throw new NotSupportedException();
            public bool TryGetEntity(NetworkEntityId id, out INetworkEntity entity) => throw new NotSupportedException();
            public void Dispose() { }
        }
    }
}
