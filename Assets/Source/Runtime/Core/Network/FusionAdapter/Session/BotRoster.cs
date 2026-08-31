using System.Threading;
using Core.Network.Abstractions.Services;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Player;
using Cysharp.Threading.Tasks;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class BotRoster
    {
        private const string NicknameFormat = "BOT-{0}";

        private readonly INetworkRunnerController _controller;
        private readonly INetworkSpawner _spawner;
        private readonly NetworkPlayerRegistry _players;
        private readonly ILogger _logger;

        public BotRoster(
            INetworkRunnerController controller,
            INetworkSpawner spawner,
            NetworkPlayerRegistry players,
            ILogger logger)
        {
            _controller = controller;
            _spawner = spawner;
            _players = players;
            _logger = logger;
        }

        public async UniTask AddAsync(CancellationToken cancellationToken)
        {
            if (!_controller.IsStateAuthority)
            {
                return;
            }

            var number = ResolveNextNumber();

            await _spawner.SpawnAsync(
                NetworkPrefabKeys.NetworkPlayer,
                beforeSpawned: entity =>
                {
                    var record = entity.GameObject.GetComponent<NetworkPlayer>();
                    record.BotNumber = number;
                    record.Nickname = string.Format(NicknameFormat, number);
                    record.IsReady = true;
                },
                cancellationToken: cancellationToken);

            _logger.Info("[Bot] BOT-{0}: roster record", number);
        }

        private int ResolveNextNumber()
        {
            var highest = BotIdentity.NoBot;

            for (var i = 0; i < _players.Count; i++)
            {
                if (_players.TryGetAt(i, out var record) && record.BotNumber > highest)
                {
                    highest = record.BotNumber;
                }
            }

            return highest + 1;
        }
    }
}
