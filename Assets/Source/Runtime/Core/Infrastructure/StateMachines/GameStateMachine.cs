using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine : IGameStateMachine
{
    private readonly Dictionary<Type, IExitState> _states;
    
    private IExitState _currentState;

    public GameStateMachine(Dictionary<Type, IExitState> states)
    {
        _states = states;
    }

    public void Enter<TState>() where TState : class, IEnterState
    {
        var state = ChangeState<TState>();
        state.Enter();
    }

    public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
    {
        var state = ChangeState<TState>();
        state.Enter(payload);
    }

    private TState ChangeState<TState>() where TState : class, IExitState
    {
        _currentState?.Exit();
        var state = GetState<TState>();
        _currentState = state;
        Debug.Log($"[GameStateMachine] Entering {typeof(TState).Name}");
        
        return state;
    }

    private TState GetState<TState>() where TState : class, IExitState =>
        _states[typeof(TState)] as TState ?? throw new InvalidOperationException(typeof(TState).Name);
}
