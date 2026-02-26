namespace Core.Abstractions.StateMachines.States
{
    public interface IEnterState : IExitState
    {
        void Enter();
    }
}
