# Factories Abstractions

Контракты фабрик.

## Типы

| Тип | Описание |
|-----|----------|
| `IGameStateMachineFactory` | Фабрика для создания GameStateMachine |

## IGameStateMachineFactory

```csharp
public interface IGameStateMachineFactory
{
    IGameStateMachine Create();
}
```

## Использование

```csharp
public class Bootstrapper
{
    private readonly IGameStateMachineFactory _factory;
    
    public void Initialize()
    {
        var stateMachine = _factory.Create();
        stateMachine.Enter<LoginState>();
    }
}
```

## Реализация

`Infrastructure/Factories/GameStateMachineFactory.cs`
