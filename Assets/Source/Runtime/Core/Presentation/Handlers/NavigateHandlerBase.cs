using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using System.Threading;
using Core.Abstractions.Navigation;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Commands.Common;

namespace Core.Presentation.Handlers
{
    public abstract class NavigateHandlerBase<TTarget, TState>
        : IRequestHandler<NavigateCommand<TTarget>, Nothing>
        where TTarget : struct, INavigationTarget
        where TState : class, IEnterState
    {
        private readonly IGameStateMachine _stateMachine;

        protected NavigateHandlerBase(IGameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public UniTask<Nothing> Handle(NavigateCommand<TTarget> command, CancellationToken ct)
        {
            _stateMachine.Enter<TState>();
            return UniTask.FromResult(Nothing.Value);
        }
    }
}
