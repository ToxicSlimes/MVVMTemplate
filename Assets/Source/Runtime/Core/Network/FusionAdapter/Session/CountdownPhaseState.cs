using System;
using Core.Application.Notifications.Matchmaking;
using Core.Domain.Enums;
using Core.Mediator.Abstractions;
using Cysharp.Threading.Tasks;
using Fusion;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    /// <summary>
    /// Отсчёт перед матчем. Таймер живёт на <see cref="GameSessionMachine"/> реплицированным
    /// полем, поэтому оставшееся время одинаково у всех, а не считается каждым локально.
    /// </summary>
    public sealed class CountdownPhaseState : SessionPhaseState
    {
        public const float DurationSeconds = 5f;

        private int _lastPublishedSecond = -1;

        public CountdownPhaseState(GameSessionMachine session, IMediator mediator, ILogger logger)
            : base(SessionPhase.Countdown, session, mediator, logger)
        {
        }

        protected override void OnEnterState()
        {
            base.OnEnterState();

            _lastPublishedSecond = -1;

            if (IsHost)
            {
                Session.CountdownTimer = TickTimer.CreateFromSeconds(Session.Runner, DurationSeconds);
            }
        }

        protected override void OnFixedUpdate()
        {
            PublishRemainingSeconds();

            if (!IsHost)
            {
                return;
            }

            if (Session.CountdownTimer.ExpiredOrNotRunning(Session.Runner))
            {
                GoTo<PlayingPhaseState>();
            }
        }

        protected override void OnExitState()
        {
            if (IsHost)
            {
                Session.CountdownTimer = TickTimer.None;
            }
        }

        private void PublishRemainingSeconds()
        {
            var remaining = Session.CountdownTimer.RemainingTime(Session.Runner);

            if (remaining == null)
            {
                return;
            }

            var seconds = Math.Max(0, (int)Math.Ceiling(remaining.Value));

            if (seconds == _lastPublishedSecond)
            {
                return;
            }

            _lastPublishedSecond = seconds;
            Mediator.Publish(new MatchCountdownTickNotification(seconds)).Forget();
        }
    }
}
