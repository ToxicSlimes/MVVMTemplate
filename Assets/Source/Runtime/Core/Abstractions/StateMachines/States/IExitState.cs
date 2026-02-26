using Core.Abstractions.StateMachines;

namespace Core.Abstractions.StateMachines.States
{
    public interface IExitState : IStateMachineAware
    {
        void Exit();
    }
}
