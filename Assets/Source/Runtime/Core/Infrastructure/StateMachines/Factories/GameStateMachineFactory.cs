using Core.Abstractions.Factories;
using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using System;
using System.Collections.Generic;

public class GameStateMachineFactory : IGameStateMachineFactory
{
    private readonly IEnumerable<IGameState> _states;

    public GameStateMachineFactory(IEnumerable<IGameState> states)
    {
        _states = states;
    }

    public IGameStateMachine Create()
    {
        var statesDictionary = new Dictionary<Type, IExitState>();
        
        foreach (var state in _states)
            statesDictionary[state.StateType] = state;

        var stateMachine = new GameStateMachine(statesDictionary);
        
        foreach (var state in _states)
            state.SetStateMachine(stateMachine);

        return stateMachine;
    }
}