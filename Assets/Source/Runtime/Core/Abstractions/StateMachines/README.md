# StateMachines Abstractions

Контракты для Game State Machine.

## Структура

```
StateMachines/
├── IGameStateMachine.cs     # State machine controller
├── IStateMachineAware.cs    # State machine injection
└── States/
    ├── IGameState.cs        # Base state (StateType, IExitState)
    ├── IEnterState.cs       # State with Enter()
    ├── IExitState.cs        # State with Exit()
    └── IPayloadState.cs     # State with Enter(T payload)
```

## Interfaces

| Interface | Purpose |
|-----------|---------|
| `IGameStateMachine` | State machine controller |
| `IStateMachineAware` | Inject state machine reference |
| `IGameState` | Base state (`StateType`, extends `IExitState`) |
| `IEnterState` | State with `Enter()` method |
| `IExitState` | State with `Exit()` method |
| `IPayloadState<T>` | State with `Enter(T payload)` method |

## State Machine API

```csharp
public interface IGameStateMachine
{
    void Enter<TState>() where TState : class, IEnterState;
    void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>;
}
```

## State Interfaces

```csharp
// Base state
public interface IGameState : IExitState
{
    Type StateType { get; }
}

// Simple state
public interface IEnterState : IExitState
{
    void Enter();
}

// State with payload
public interface IPayloadState<TPayload> : IExitState
{
    void Enter(TPayload payload);
}

// Exit callback
public interface IExitState
{
    void Exit();
}
```

## Usage

```csharp
// Enter state
_stateMachine.Enter<LoginState>();

// Enter state with payload
_stateMachine.Enter<ChatState, ChatPayload>(new ChatPayload { ChatId = "123" });
```

## Implementing a State

```csharp
public class LoginState : IEnterState
{
    public void Enter()
    {
        // Show login view, etc.
    }

    public void Exit()
    {
        // Cleanup
    }
}
```

## IStateMachineAware

Позволяет state получить ссылку на state machine:

```csharp
public interface IStateMachineAware
{
    void SetStateMachine(IGameStateMachine stateMachine);
}
```

## Implementation

Реализация находится в `Infrastructure/StateMachines/GameStateMachine.cs`.
