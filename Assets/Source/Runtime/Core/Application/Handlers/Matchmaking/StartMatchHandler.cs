using System.Threading;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Commands.Matchmaking;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Matchmaking
{
    public sealed class StartMatchHandler : IRequestHandler<StartMatchCommand, Nothing>
    {
        private const string NoSessionStatus = "Session is not ready";

        private readonly INetworkSessionControl _session;
        private readonly ILobbyModelWriter _model;

        public StartMatchHandler(INetworkSessionControl session, ILobbyModelWriter model)
        {
            _session = session;
            _model = model;
        }

        public UniTask<Nothing> Handle(StartMatchCommand command, CancellationToken ct)
        {
            if (_session.Exists)
            {
                _session.StartMatch();
            }
            else
            {
                _model.SetStatus(NoSessionStatus, false);
            }

            return UniTask.FromResult(Nothing.Value);
        }
    }
}
