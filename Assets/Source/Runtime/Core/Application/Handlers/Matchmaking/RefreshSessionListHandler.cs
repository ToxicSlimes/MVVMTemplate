using System.Threading;
using Core.Application.Commands.Matchmaking;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Matchmaking
{
    public sealed class RefreshSessionListHandler : IRequestHandler<RefreshSessionListCommand, Nothing>
    {
        private readonly INetworkLobby _lobby;

        public RefreshSessionListHandler(INetworkLobby lobby)
        {
            _lobby = lobby;
        }

        public async UniTask<Nothing> Handle(RefreshSessionListCommand command, CancellationToken ct)
        {
            await _lobby.JoinAsync(cancellationToken: ct);
            return Nothing.Value;
        }
    }
}
