using System;
using System.Collections.Generic;
using System.Threading;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Network.Abstractions.Services
{
    public interface INetworkRunnerController : IDisposable
    {
        bool IsRunning { get; }
        NetworkMode Mode { get; }
        NetworkPlayerId LocalPlayer { get; }
        IReadOnlyList<NetworkPlayerId> ActivePlayers { get; }
        NetworkSessionInfo Session { get; }
        bool IsStateAuthority { get; }

        /// <summary>
        /// Запускает сессию и возвращает управление сразу. Результат приходит в колбэк.
        /// Блокирующего варианта нет: подключение требует сетевого обмена с Photon Cloud.
        /// </summary>
        void Start(NetworkStartRequest request, Action<NetworkStartResult> onCompleted = null);

        UniTask<NetworkStartResult> StartAsync(NetworkStartRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Инициирует остановку и возвращает управление сразу.
        /// Объект раннера уничтожает сам Fusion.
        /// </summary>
        void Shutdown();

        UniTask ShutdownAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Запускает сетевую загрузку сцены и возвращает управление сразу.
        /// Завершение отслеживай по <c>NetworkSceneLoadDoneNotification</c>.
        /// </summary>
        void LoadNetworkScene(string scenePath);

        UniTask LoadNetworkSceneAsync(string scenePath, CancellationToken cancellationToken = default);
    }
}
