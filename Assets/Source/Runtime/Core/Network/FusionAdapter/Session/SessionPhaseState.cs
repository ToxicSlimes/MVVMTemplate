using Core.Application.Notifications.Matchmaking;
using Core.Data.Domain.Enums;
using Core.Infrastructure.Mediator.Abstractions;
using Cysharp.Threading.Tasks;
using Fusion.Addons.FSM;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Network.FusionAdapter.Session
{
    /// <summary>
    /// Фаза сессии. Обычный класс, не компонент: зависимости приходят конструктором из
    /// <see cref="GameSessionMachine"/>, которому их выдал Reflex.
    /// </summary>
    public abstract class SessionPhaseState : State<SessionPhaseState>
    {
        private readonly ILogger _logger;

        protected SessionPhaseState(SessionPhase phase, GameSessionMachine session, IMediator mediator, ILogger logger)
            : base(phase.ToString())
        {
            Phase = phase;
            Session = session;
            Mediator = mediator;
            _logger = logger;
        }

        public SessionPhase Phase { get; }

        protected GameSessionMachine Session { get; }

        protected IMediator Mediator { get; }

        protected bool IsHost => Session.HasStateAuthority;

        /// <summary>
        /// Обобщённый <c>TryActivateState&lt;T&gt;</c> — расширение над <c>IStateMachine</c>, и на
        /// типизированной машине он перекрывается инстансным методом с <c>int</c>. Идём через
        /// идентификатор, чтобы перегрузка не зависела от того, как выведен тип.
        /// </summary>
        protected bool GoTo<TPhase>() where TPhase : SessionPhaseState
        {
            return Machine.TryActivateState(Machine.GetState<TPhase>().StateId);
        }

        protected override void OnEnterState()
        {
            _logger.Info("[Session] Фаза: {0}", Phase);
            Mediator.Publish(new SessionPhaseChangedNotification(Phase)).Forget();
        }
    }
}
