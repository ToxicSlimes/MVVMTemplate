using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Abstractions;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class LobbyPhaseState : SessionPhaseState
    {
        public LobbyPhaseState(GameSessionMachine session, IMediator mediator, ILogger logger)
            : base(SessionPhase.Lobby, session, mediator, logger)
        {
        }
    }
}
