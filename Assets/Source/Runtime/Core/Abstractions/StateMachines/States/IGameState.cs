using System;

namespace Core.Abstractions.StateMachines.States
{
    public interface IGameState : IExitState
    {
        Type StateType { get; }
    }
}
