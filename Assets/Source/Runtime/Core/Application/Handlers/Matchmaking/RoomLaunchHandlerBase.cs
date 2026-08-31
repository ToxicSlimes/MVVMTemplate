using System.Threading;
using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Configs.Services;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Matchmaking
{
    /// <summary>
    /// Общая часть создания и входа в комнату: проверка ника, выход из лобби и старт сессии.
    /// Лобби закрывается перед стартом — просмотр комнат закончился, держать второе подключение
    /// к облаку незачем. Сцена сессии намеренно не передаётся: её грузит
    /// <see cref="Lobby.GameSceneLoadHandler"/> при переходе в фазу игры.
    /// </summary>
    public abstract class RoomLaunchHandlerBase
    {
        private const string EmptyNicknameStatus = "Enter a nickname";
        private const string MissingConfigStatus = "NetworkConfig is missing";

        private readonly INetworkRunnerController _controller;
        private readonly INetworkLobby _lobby;
        private readonly INetworkSpawner _spawner;
        private readonly IConfigProvider _configProvider;
        private readonly ILobbyModelWriter _model;

        protected RoomLaunchHandlerBase(
            INetworkRunnerController controller,
            INetworkLobby lobby,
            INetworkSpawner spawner,
            IConfigProvider configProvider,
            ILobbyModelWriter model)
        {
            _controller = controller;
            _lobby = lobby;
            _spawner = spawner;
            _configProvider = configProvider;
            _model = model;
        }

        protected void ReportRejected(string status) => _model.SetStatus(status, false);

        protected async UniTask<NetworkStartResult> LaunchAsync(
            NetworkMode mode,
            string sessionName,
            string nickname,
            string busyStatus,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                ReportRejected(EmptyNicknameStatus);
                return NetworkStartResult.Failure(EmptyNicknameStatus, NetworkShutdownReason.InvalidArguments);
            }

            var config = _configProvider.GetConfig<NetworkConfig>();

            if (config == null)
            {
                ReportRejected(MissingConfigStatus);
                return NetworkStartResult.Failure(MissingConfigStatus, NetworkShutdownReason.IncompatibleConfiguration);
            }

            _model.SetStatus(busyStatus, true);

            await _lobby.LeaveAsync(cancellationToken);

            var request = new NetworkStartRequest(
                mode,
                sessionName,
                config.DefaultPlayerCount,
                scenePath: null,
                config.SessionVisible,
                config.SessionOpen);

            var result = await _controller.StartAsync(request, cancellationToken);

            if (!result.IsOk)
            {
                ReportRejected(result.ErrorMessage ?? result.ShutdownReason.ToString());
                return result;
            }

            if (mode == NetworkMode.Host)
            {
                await _spawner.SpawnAsync(NetworkPrefabKeys.GameSession, cancellationToken: cancellationToken);
            }

            return result;
        }
    }
}
