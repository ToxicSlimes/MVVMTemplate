using Core.Domain.Enums;
using Core.Mediator.Abstractions;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class EndedPhaseState : SessionPhaseState
    {
        public EndedPhaseState(GameSessionMachine session, IMediator mediator, ILogger logger)
            : base(SessionPhase.Ended, session, mediator, logger)
        {
        }
    }
}
