using System;
using System.Collections.Generic;
using System.Threading;
using Core.Application.Notifications.Network;
using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Callbacks;
using Core.Network.FusionAdapter.Mapping;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Runner
{
    /// <summary>
    /// Раннер, который только слушает список сессий. Не симулирует, не спавнит и не отдаёт ввод,
    /// поэтому обходится без сцен-менеджера и провайдера объектов.
    /// </summary>
    public sealed class FusionLobbyController : FusionCallbacksBase, INetworkLobby, IDisposable
    {
        private const string LobbyRunnerObjectName = "NetworkLobbyRunner";

        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly List<NetworkSessionInfo> _sessions = new();

        private GameObject _runnerObject;
        private NetworkRunner _runner;
        private bool _disposed;

        public FusionLobbyController(IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public bool IsInLobby => _runner != null && _runner.LobbyInfo.IsValid;

        public IReadOnlyList<NetworkSessionInfo> Sessions => _sessions;

        public async UniTask<NetworkStartResult> JoinAsync(
            string lobbyName = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (IsInLobby)
            {
                return NetworkStartResult.Success;
            }

            CreateRunnerObject();

            var result = (await _runner.JoinSessionLobby(
                SessionLobby.ClientServer,
                lobbyName,
                cancellationToken: cancellationToken)).ToCore();

            if (!result.IsOk)
            {
                _logger.Error("[Lobby] Вход в лобби провален: {0} ({1})", result.ShutdownReason, result.ErrorMessage);
                DestroyRunnerObject();
                return result;
            }

            _logger.Info("[Lobby] В лобби '{0}', регион {1}", _runner.LobbyInfo.Name, _runner.LobbyInfo.Region);

            return result;
        }

        public async UniTask LeaveAsync(CancellationToken cancellationToken = default)
        {
            if (_runner == null)
            {
                return;
            }

            await _runner.Shutdown().AsUniTask().AttachExternalCancellation(cancellationToken);
            DestroyRunnerObject();
            _sessions.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DestroyRunnerObject();
            _sessions.Clear();
        }

        protected override void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _sessions.Clear();

            for (var i = 0; i < sessionList.Count; i++)
            {
                _sessions.Add(sessionList[i].ToCore());
            }

            _mediator.Publish(new NetworkSessionListUpdatedNotification(new List<NetworkSessionInfo>(_sessions))).Forget();
        }

        protected override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _logger.Info("[Lobby] Лобби закрыто: {0}", shutdownReason);
            _sessions.Clear();
        }

        private void CreateRunnerObject()
        {
            if (_runnerObject != null && _runner != null && !_runner.IsShutdown)
            {
                return;
            }

            DestroyRunnerObject();

            _runnerObject = new GameObject(LobbyRunnerObjectName);
            UnityEngine.Object.DontDestroyOnLoad(_runnerObject);

            _runner = _runnerObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = false;
            _runner.AddCallbacks(this);
        }

        private void DestroyRunnerObject()
        {
            if (_runnerObject != null)
            {
                UnityEngine.Object.Destroy(_runnerObject);
            }

            _runnerObject = null;
            _runner = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FusionLobbyController));
            }
        }
    }
}
