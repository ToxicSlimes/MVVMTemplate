using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using System;

public abstract class GameStateBase<TSelf> : IGameState where TSelf : GameStateBase<TSelf>
{
    protected IGameStateMachine StateMachine;

    public Type StateType => typeof(TSelf);

    public abstract void Exit();

    public virtual void SetStateMachine(IGameStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }
}
