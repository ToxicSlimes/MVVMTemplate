using System;
using System.Collections.Generic;
using System.Threading;
using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Input;
using Core.Network.Abstractions.Messaging;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Network.FusionAdapter
{
    /// <summary>
    /// Единая точка доступа к сетевому слою. Делегирует вызовы сегрегированным сервисам
    /// и не содержит собственной логики.
    /// </summary>
    public sealed class NetworkService : INetworkService
    {
        private readonly INetworkRunnerController _controller;
        private readonly INetworkSpawner _spawner;
        private readonly INetworkMessenger _messenger;
        private readonly INetworkInputSource _inputSource;

        public NetworkService(
            INetworkRunnerController controller,
            INetworkSpawner spawner,
            INetworkMessenger messenger,
            INetworkInputSource inputSource)
        {
            _controller = controller;
            _spawner = spawner;
            _messenger = messenger;
            _inputSource = inputSource;
        }

        public bool IsRunning => _controller.IsRunning;
        public long SessionGeneration => _controller.SessionGeneration;

        public NetworkMode Mode => _controller.Mode;

        public NetworkPlayerId LocalPlayer => _controller.LocalPlayer;

        public IReadOnlyList<NetworkPlayerId> ActivePlayers => _controller.ActivePlayers;

        public NetworkSessionInfo Session => _controller.Session;

        public bool IsStateAuthority => _controller.IsStateAuthority;

        public void Start(NetworkStartRequest request, Action<NetworkStartResult> onCompleted = null) =>
            _controller.Start(request, onCompleted);

        public UniTask<NetworkStartResult> StartAsync(NetworkStartRequest request, CancellationToken cancellationToken = default) =>
            _controller.StartAsync(request, cancellationToken);

        public void Shutdown() => _controller.Shutdown();

        public UniTask ShutdownAsync(CancellationToken cancellationToken = default) =>
            _controller.ShutdownAsync(cancellationToken);

        public void LoadNetworkScene(string scenePath) => _controller.LoadNetworkScene(scenePath);

        public UniTask LoadNetworkSceneAsync(string scenePath, CancellationToken cancellationToken = default) =>
            _controller.LoadNetworkSceneAsync(scenePath, cancellationToken);

        public INetworkEntity Spawn(
            NetworkPrefabKey prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            NetworkPlayerId? inputAuthority = null,
            Action<INetworkEntity> beforeSpawned = null) =>
            _spawner.Spawn(prefab, position, rotation, inputAuthority, beforeSpawned);

        public UniTask<INetworkEntity> SpawnAsync(
            NetworkPrefabKey prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            NetworkPlayerId? inputAuthority = null,
            Action<INetworkEntity> beforeSpawned = null,
            CancellationToken cancellationToken = default) =>
            _spawner.SpawnAsync(prefab, position, rotation, inputAuthority, beforeSpawned, cancellationToken);

        public void Despawn(INetworkEntity entity) => _spawner.Despawn(entity);

        public bool TryGetEntity(NetworkEntityId id, out INetworkEntity entity) =>
            _spawner.TryGetEntity(id, out entity);

        public void Send<TMessage>(TMessage message, NetworkTarget target) where TMessage : INetworkMessage =>
            _messenger.Send(message, target);

        public IDisposable Subscribe<TMessage>(Action<NetworkPlayerId, TMessage> handler) where TMessage : INetworkMessage =>
            _messenger.Subscribe(handler);

        public bool RepeatLastInputOnMissing
        {
            get => _inputSource.RepeatLastInputOnMissing;
            set => _inputSource.RepeatLastInputOnMissing = value;
        }

        public void Unregister<TInput>() where TInput : struct => _inputSource.Unregister<TInput>();

        public void Dispose() => _controller.Dispose();
    }
}
