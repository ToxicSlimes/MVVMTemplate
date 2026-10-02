using System.Collections.Generic;
using Core.Application.Notifications.Matchmaking;
using Core.Domain.Enums;
using Core.Mediator.Abstractions;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Addons.FSM;
using Reflex.Attributes;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    [RequireComponent(typeof(StateMachineController))]
    public sealed class GameSessionMachine : NetworkBehaviour, IStateMachineOwner
    {
        private const string MachineName = "GameSession";

        [Networked] public TickTimer CountdownTimer { get; set; }

        [Networked] public MatchOutcome Outcome { get; set; }

        [Inject] private IMediator _mediator;
        [Inject] private ILogger _logger;
        [Inject] private GameSessionRegistry _registry;

        private enum PhaseRequest
        {
            None = 0,
            Countdown,
            Ended
        }

        private StateMachine<SessionPhaseState> _machine;
        private PhaseRequest _pending;
        private SessionPhase _observedPhase = SessionPhase.None;
        private MatchOutcome _observedOutcome = MatchOutcome.None;

        public SessionPhase Phase => _machine?.ActiveState?.Phase ?? SessionPhase.None;

        public void CollectStateMachines(List<IStateMachine> stateMachines)
        {
            _machine = new StateMachine<SessionPhaseState>(
                MachineName,
                new LobbyPhaseState(this, _mediator, _logger),
                new CountdownPhaseState(this, _mediator, _logger),
                new PlayingPhaseState(this, _mediator, _logger),
                new EndedPhaseState(this, _mediator, _logger));

            stateMachines.Add(_machine);
        }

        public override void Spawned()
        {
            Runner.MakeDontDestroyOnLoad(gameObject);
            _registry.Set(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Clear(this);
        }

        public void StartMatch() => Request(PhaseRequest.Countdown);

        public void EndMatch() => Request(PhaseRequest.Ended);

        public void FinishMatch(MatchOutcome outcome)
        {
            if (!HasStateAuthority || Outcome != MatchOutcome.None)
            {
                return;
            }

            Outcome = outcome;
            _logger.Info("[Session] Outcome: {0}", outcome);

            EndMatch();
        }

        public void ResetMatchState()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            Outcome = MatchOutcome.None;
        }

        public override void FixedUpdateNetwork()
        {
            if (_pending == PhaseRequest.None)
            {
                return;
            }

            var request = _pending;
            _pending = PhaseRequest.None;

            switch (request)
            {
                case PhaseRequest.Countdown:
                    GoTo<CountdownPhaseState>();
                    break;

                case PhaseRequest.Ended:
                    GoTo<EndedPhaseState>();
                    break;
            }
        }

        public override void Render()
        {
            PublishOutcome();

            if (HasStateAuthority)
            {
                return;
            }

            var phase = Phase;

            if (phase == _observedPhase)
            {
                return;
            }

            _observedPhase = phase;
            _logger.Info("[Session] Phase: {0}", phase);
            _mediator.Publish(new SessionPhaseChangedNotification(phase)).Forget();
        }

        private void PublishOutcome()
        {
            var outcome = Outcome;

            if (outcome == _observedOutcome)
            {
                return;
            }

            _observedOutcome = outcome;
            _mediator.Publish(new MatchOutcomeChangedNotification(outcome)).Forget();
        }

        private void Request(PhaseRequest request)
        {
            if (!HasStateAuthority)
            {
                return;
            }

            _pending = request;
        }

        private void GoTo<TPhase>() where TPhase : SessionPhaseState
        {
            _machine.TryActivateState(_machine.GetState<TPhase>().StateId);
        }
    }
}
