using Core.Abstractions.StateMachines;

namespace Core.Abstractions.Factories
{
    public interface IGameStateMachineFactory
    {
        IGameStateMachine Create();
    }
}
