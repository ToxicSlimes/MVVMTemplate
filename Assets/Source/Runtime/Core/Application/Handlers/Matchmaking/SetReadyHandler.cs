using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Commands.Matchmaking;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Matchmaking
{
    public sealed class SetReadyHandler : IRequestHandler<SetReadyCommand, Nothing>
    {
        private readonly INetworkLocalPlayer _localPlayer;
        private readonly ILobbyModelWriter _model;

        public SetReadyHandler(INetworkLocalPlayer localPlayer, ILobbyModelWriter model)
        {
            _localPlayer = localPlayer;
            _model = model;
        }

        public UniTask<Nothing> Handle(SetReadyCommand command, CancellationToken ct)
        {
            if (_localPlayer.Exists)
            {
                _localPlayer.SetReady(command.IsReady);
            }
            else
            {
                _model.SetStatus("Not in a room", false);
            }

            return UniTask.FromResult(Nothing.Value);
        }
    }
}
