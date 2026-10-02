using System.Threading;
using Core.Application.Commands.Matchmaking;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class SpawnBotHandler : IRequestHandler<SpawnBotCommand, Nothing>
    {
        private readonly BotRoster _bots;

        public SpawnBotHandler(BotRoster bots)
        {
            _bots = bots;
        }

        public async UniTask<Nothing> Handle(SpawnBotCommand command, CancellationToken ct)
        {
            await _bots.AddAsync(ct);

            return Nothing.Value;
        }
    }
}
