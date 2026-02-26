using Core.Abstractions.StateMachines.States;

namespace Core.Abstractions.StateMachines
{
    public interface IGameStateMachine
    {
        void Enter<TState>() where TState : class, IEnterState;
        void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>;
    }
}
