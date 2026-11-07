using System;
using System.Collections.Generic;
using System.Threading;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Services.Network
{
    public sealed class NullNetworkRunnerController : INetworkRunnerController
    {
        private const string NetworkDisabled = "Сеть выключена: включи её флагом Network Enabled на ProjectInstaller";

        public bool IsRunning => false;
        public long SessionGeneration => 0;
        public NetworkMode Mode => NetworkMode.None;
        public NetworkPlayerId LocalPlayer => NetworkPlayerId.None;
        public IReadOnlyList<NetworkPlayerId> ActivePlayers => Array.Empty<NetworkPlayerId>();
        public NetworkSessionInfo Session => NetworkSessionInfo.Empty;
        public bool IsStateAuthority => false;

        public void Start(NetworkStartRequest request, Action<NetworkStartResult> onCompleted = null) =>
            throw new InvalidOperationException(NetworkDisabled);

        public UniTask<NetworkStartResult> StartAsync(NetworkStartRequest request, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(NetworkDisabled);

        public void Shutdown() { }

        public UniTask ShutdownAsync(CancellationToken cancellationToken = default) => UniTask.CompletedTask;

        public void LoadNetworkScene(string scenePath) =>
            throw new InvalidOperationException(NetworkDisabled);

        public UniTask LoadNetworkSceneAsync(string scenePath, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(NetworkDisabled);

        public void Dispose() { }
    }
}
