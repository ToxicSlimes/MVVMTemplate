using Core.Domain.Enums;
using Core.Mediator.Abstractions;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    public sealed class PlayingPhaseState : SessionPhaseState
    {
        public PlayingPhaseState(
            GameSessionMachine session,
            IMediator mediator,
            ILogger logger)
            : base(SessionPhase.Playing, session, mediator, logger)
        {
        }

        protected override void OnEnterState()
        {
            if (IsHost)
            {
                Session.ResetMatchState();
            }

            base.OnEnterState();
        }
    }
}
