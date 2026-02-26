namespace Core.Abstractions.StateMachines
{
    public interface IStateMachineAware
    {
        void SetStateMachine(IGameStateMachine stateMachine);
    }
}
