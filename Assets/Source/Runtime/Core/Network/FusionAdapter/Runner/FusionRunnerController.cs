using System;
using System.Collections.Generic;
using System.Threading;
using Core.Application.Notifications.Network;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Callbacks;
using Core.Network.FusionAdapter.Mapping;
using Core.Network.FusionAdapter.Spawning;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Runner
{
    public sealed class FusionRunnerController : FusionCallbacksBase, INetworkRunnerController, INetworkRunnerAccessor
    {
        private const string RunnerObjectName = "NetworkRunner";

        private readonly Container _container;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly List<NetworkPlayerId> _activePlayers = new();
        private readonly List<INetworkRunnerCallbacks> _pendingCallbacks = new();

        private GameObject _runnerObject;
        private NetworkRunner _runner;
        private NetworkSceneManagerDefault _sceneManager;
        private ReflexNetworkObjectProvider _objectProvider;
        private bool _disposed;
        private bool _isStarting;

        public FusionRunnerController(Container container, IMediator mediator, ILogger logger)
        {
            _container = container;
            _mediator = mediator;
            _logger = logger;
        }

        public NetworkRunner Runner => _runner;

        public bool IsRunning => _runner != null && _runner.IsRunning;

        public NetworkMode Mode => IsRunning ? _runner.GameMode.ToCore() : NetworkMode.None;

        public NetworkPlayerId LocalPlayer => IsRunning ? _runner.LocalPlayer.ToCore() : NetworkPlayerId.None;

        public IReadOnlyList<NetworkPlayerId> ActivePlayers => _activePlayers;

        public NetworkSessionInfo Session => IsRunning ? _runner.SessionInfo.ToCore() : NetworkSessionInfo.Empty;

        public bool IsStateAuthority => IsRunning && (_runner.IsServer || _runner.IsSharedModeMasterClient);

        public void RegisterCallbacks(INetworkRunnerCallbacks callbacks)
        {
            if (callbacks == null)
            {
                throw new ArgumentNullException(nameof(callbacks));
            }

            if (_pendingCallbacks.Contains(callbacks))
            {
                return;
            }

            _pendingCallbacks.Add(callbacks);

            if (IsRunning)
            {
                _runner.AddCallbacks(callbacks);
            }
        }

        public void Start(NetworkStartRequest request, Action<NetworkStartResult> onCompleted = null)
        {
            ThrowIfCannotStart();
            StartDetachedAsync(request, onCompleted).Forget();
        }

        public async UniTask<NetworkStartResult> StartAsync(NetworkStartRequest request, CancellationToken cancellationToken = default)
        {
            ThrowIfCannotStart();

            _isStarting = true;

            try
            {
                CreateRunnerObject();

                var args = BuildStartGameArgs(request, cancellationToken);
                var result = (await _runner.StartGame(args)).ToCore();

                if (!result.IsOk)
                {
                    _logger.Error("[Network] Старт сессии провален: {0} ({1})", result.ShutdownReason, result.ErrorMessage);
                    DestroyRunnerObject();
                    return result;
                }

                _logger.Info("[Network] Сессия запущена: режим {0}, сессия '{1}'", Mode, Session.Name);
                RebuildActivePlayers();
                await _mediator.Publish(new NetworkConnectedNotification(Session), cancellationToken);

                return result;
            }
            finally
            {
                _isStarting = false;
            }
        }

        public void Shutdown()
        {
            if (!IsRunning)
            {
                return;
            }

            _runner.Shutdown();
            ReleaseRunnerReferences();
            _activePlayers.Clear();
        }

        public async UniTask ShutdownAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRunning)
            {
                return;
            }

            await _runner.Shutdown().AsUniTask().AttachExternalCancellation(cancellationToken);
            DestroyRunnerObject();
            _activePlayers.Clear();
        }

        public void LoadNetworkScene(string scenePath)
        {
            ValidateSceneLoad(scenePath);
            _runner.LoadScene(scenePath);
        }

        public async UniTask LoadNetworkSceneAsync(string scenePath, CancellationToken cancellationToken = default)
        {
            ValidateSceneLoad(scenePath);
            cancellationToken.ThrowIfCancellationRequested();
            await _runner.LoadScene(scenePath);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DestroyRunnerObject();
            _pendingCallbacks.Clear();
            _activePlayers.Clear();
        }

        protected override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            RebuildActivePlayers();
            var id = player.ToCore();
            _mediator.Publish(new NetworkPlayerJoinedNotification(id, player == runner.LocalPlayer)).Forget();
        }

        protected override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            RebuildActivePlayers();
            _mediator.Publish(new NetworkPlayerLeftNotification(player.ToCore())).Forget();
        }

        protected override void OnConnectedToServer(NetworkRunner runner)
        {
            _mediator.Publish(new NetworkConnectedNotification(runner.SessionInfo.ToCore())).Forget();
        }

        protected override void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            _logger.Warn("[Network] Отключение от сервера: {0}", reason);
            _mediator.Publish(new NetworkDisconnectedNotification(reason.ToCore())).Forget();
        }

        protected override void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            _logger.Error("[Network] Подключение не удалось: {0} ({1})", reason, remoteAddress);
            _mediator.Publish(new NetworkConnectFailedNotification(reason.ToCore(), remoteAddress.ToString())).Forget();
        }

        protected override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _logger.Info("[Network] Сессия остановлена: {0}", shutdownReason);
            _activePlayers.Clear();
            _mediator.Publish(new NetworkShutdownNotification(shutdownReason.ToCore())).Forget();
        }

        protected override void OnSceneLoadDone(NetworkRunner runner)
        {
            _objectProvider.InjectSceneObjects();
            _mediator.Publish(new NetworkSceneLoadDoneNotification()).Forget();
        }

        protected override void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            _logger.Warn("[Network] Миграция хоста");
            _mediator.Publish(new NetworkHostMigrationNotification()).Forget();
        }

        private StartGameArgs BuildStartGameArgs(NetworkStartRequest request, CancellationToken cancellationToken)
        {
            var args = new StartGameArgs
            {
                GameMode = request.Mode.ToFusion(),
                SessionName = request.SessionName,
                SceneManager = _sceneManager,
                ObjectProvider = _objectProvider,
                IsVisible = request.IsVisible,
                IsOpen = request.IsOpen,
                StartGameCancellationToken = cancellationToken
            };

            if (request.PlayerCount > 0)
            {
                args.PlayerCount = request.PlayerCount;
            }

            if (!string.IsNullOrEmpty(request.ScenePath))
            {
                var sceneRef = _sceneManager.GetSceneRef(request.ScenePath);
                if (!sceneRef.IsValid)
                {
                    throw new ArgumentException(
                        $"Сцена '{request.ScenePath}' не найдена в Build Settings", nameof(request));
                }

                var sceneInfo = new NetworkSceneInfo();
                sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
                args.Scene = sceneInfo;
            }

            return args;
        }

        /// <summary>
        /// Источник ввода резолвится здесь, а не лежит зависимостью конструктора: он сам
        /// принимает <see cref="INetworkRunnerAccessor"/>, то есть этот же объект, и пара
        /// замкнулась бы. Резолв обязателен — контейнер создаёт синглтоны лениво, а подписка
        /// на <c>OnInput</c> происходит в конструкторе источника. Без обращения источник
        /// не рождается, Fusion шлёт пустой ввод, и игрок стоит на месте.
        /// </summary>
        private void CreateRunnerObject()
        {
            _runnerObject = new GameObject(RunnerObjectName);
            UnityEngine.Object.DontDestroyOnLoad(_runnerObject);

            _runner = _runnerObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            _sceneManager = _runnerObject.AddComponent<NetworkSceneManagerDefault>();

            _objectProvider = _runnerObject.AddComponent<ReflexNetworkObjectProvider>();
            _objectProvider.SetContainer(_container);

            _container.Resolve<INetworkInputSource>();

            _runner.AddCallbacks(this);
            for (var i = 0; i < _pendingCallbacks.Count; i++)
            {
                _runner.AddCallbacks(_pendingCallbacks[i]);
            }
        }

        private void DestroyRunnerObject()
        {
            if (_runnerObject != null)
            {
                UnityEngine.Object.Destroy(_runnerObject);
            }

            ReleaseRunnerReferences();
        }

        private void ReleaseRunnerReferences()
        {
            _runnerObject = null;
            _runner = null;
            _sceneManager = null;
            _objectProvider = null;
        }

        private void ValidateSceneLoad(string scenePath)
        {
            ThrowIfDisposed();

            if (!IsRunning)
            {
                throw new InvalidOperationException("Сетевая сцена не может быть загружена: сессия не запущена");
            }

            if (!IsStateAuthority)
            {
                throw new InvalidOperationException("Сетевую сцену грузит только state authority");
            }

            if (string.IsNullOrEmpty(scenePath))
            {
                throw new ArgumentException("Путь сцены пуст", nameof(scenePath));
            }
        }

        private async UniTaskVoid StartDetachedAsync(NetworkStartRequest request, Action<NetworkStartResult> onCompleted)
        {
            var result = await StartAsync(request);
            onCompleted?.Invoke(result);
        }

        private void RebuildActivePlayers()
        {
            _activePlayers.Clear();

            if (!IsRunning)
            {
                return;
            }

            foreach (var player in _runner.ActivePlayers)
            {
                _activePlayers.Add(player.ToCore());
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FusionRunnerController));
            }
        }

        /// <remarks>
        /// Одной проверки <see cref="IsRunning"/> мало: между ней и концом <c>StartGame</c> проходят
        /// секунды, и всё это время раннер ещё не запущен. Второй вызов в этом окне — а его даёт
        /// обычный двойной клик по кнопке входа — проходил проверку и создавал второй раннер:
        /// первый оставался жить сам по себе, и в комнату входили два игрока с одного клиента.
        /// </remarks>
        private void ThrowIfCannotStart()
        {
            ThrowIfDisposed();

            if (_isStarting)
            {
                throw new InvalidOperationException(
                    "Сессия уже запускается. Дождись результата предыдущего старта.");
            }

            if (IsRunning)
            {
                throw new InvalidOperationException(
                    $"Сессия уже запущена в режиме {Mode}. Вызови {nameof(ShutdownAsync)} перед повторным стартом.");
            }
        }
    }
}
