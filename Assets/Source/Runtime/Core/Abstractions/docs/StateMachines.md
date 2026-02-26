# StateMachines — Машина состояний

> Finite State Machine для управления состояниями приложения.

## 📋 Обзор интерфейсов

| Интерфейс | Назначение | Путь |
|-----------|------------|------|
| `IGameStateMachine` | Главная машина состояний | `StateMachines/` |
| `IGameState` | Базовое состояние | `StateMachines/States/` |
| `IEnterState` | Состояние с Enter | `StateMachines/States/` |
| `IExitState` | Состояние с Exit | `StateMachines/States/` |
| `IPayloadState<T>` | Состояние с payload | `StateMachines/States/` |
| `IStateMachineAware` | Доступ к машине | `StateMachines/` |

---

## IGameStateMachine

**Путь:** `StateMachines/IGameStateMachine.cs`

```csharp
public interface IGameStateMachine
{
    void Enter<TState>() where TState : class, IEnterState;
    void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>;
}
```

### Назначение

Главная машина состояний приложения. Управляет переходами между экранами/режимами.

### Реализация

**Путь:** `Core/Infrastructure/StateMachines/GameStateMachine.cs`

### Пример использования

```csharp
public class Bootstrap : MonoBehaviour
{
    [Inject] private IGameStateMachine _stateMachine;
    
    void Start()
    {
        // Переход без payload
        _stateMachine.Enter<BootstrapState>();
    }
}

public class BootstrapState : IEnterState
{
    private readonly IGameStateMachine _stateMachine;
    
    public BootstrapState(IGameStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    public void Enter()
    {
        // После загрузки — переход на логин
        _stateMachine.Enter<LoginState>();
    }
    
    public void Exit() { }
}

// С payload
_stateMachine.Enter<GameplayState, LevelData>(new LevelData { LevelId = 1 });
```

---

## IGameState

**Путь:** `StateMachines/States/IGameState.cs`

```csharp
public interface IGameState : IExitState
{
    Type StateType { get; }
}
```

### Назначение

Базовый маркер для всех состояний. Содержит тип для рефлексии.

---

## IEnterState

**Путь:** `StateMachines/States/IEnterState.cs`

```csharp
public interface IEnterState : IExitState
{
    void Enter();
}
```

### Назначение

Состояние с простым входом без параметров.

### Пример

```csharp
public class LoginState : IEnterState
{
    private readonly IViewService _viewService;
    private IGameStateMachine _stateMachine;
    
    public LoginState(IViewService viewService)
    {
        _viewService = viewService;
    }
    
    public void SetStateMachine(IGameStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    public async void Enter()
    {
        await _viewService.ShowAsync<LoginViewModel>();
    }
    
    public void Exit()
    {
        _viewService.HideCurrentAsync().Forget();
    }
}
```

---

## IExitState

**Путь:** `StateMachines/States/IExitState.cs`

```csharp
public interface IExitState : IStateMachineAware
{
    void Exit();
}
```

### Назначение

Гарантирует наличие метода Exit() для cleanup при выходе из состояния.

---

## IPayloadState<T>

**Путь:** `StateMachines/States/IPayloadState.cs`

```csharp
public interface IPayloadState<in TPayload> : IExitState
{
    void Enter(TPayload payload);
}
```

### Назначение

Состояние с параметризованным входом.

### Пример

```csharp
public class ChatState : IPayloadState<ChatStatePayload>
{
    private readonly IViewService _viewService;
    private readonly IChatService _chatService;
    
    public async void Enter(ChatStatePayload payload)
    {
        var chat = await _chatService.GetChatHistoryAsync(payload.ChatId);
        await _viewService.ShowAsync<ChatViewModel>();
    }
    
    public void Exit()
    {
        _viewService.HideCurrentAsync().Forget();
    }
}

public class ChatStatePayload
{
    public string ChatId { get; set; }
    public string CharacterId { get; set; }
}

// Использование
_stateMachine.Enter<ChatState, ChatStatePayload>(new ChatStatePayload
{
    ChatId = chatId,
    CharacterId = characterId
});
```

---

## IStateMachineAware

**Путь:** `StateMachines/IStateMachineAware.cs`

```csharp
public interface IStateMachineAware
{
    void SetStateMachine(IGameStateMachine stateMachine);
}
```

### Назначение

Позволяет состоянию получить ссылку на машину состояний для переходов.

---

## 🗺️ Диаграмма состояний

```
┌──────────────────────────────────────────────────────────────────┐
│                         GAME STATES                               │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│   ┌─────────────────┐                                             │
│   │  BootstrapState │──────────────────────┐                      │
│   └─────────────────┘                      │                      │
│           │                                │                      │
│           ▼                                │                      │
│   ┌─────────────────┐     ┌────────────────▼───────────────────┐ │
│   │   LoginState    │────▶│        MenuSelectionState          │ │
│   └─────────────────┘     └────────────────────────────────────┘ │
│           │                        │                              │
│           │                        ├───────────────┐              │
│           │                        │               │              │
│           │                        ▼               ▼              │
│           │               ┌────────────┐   ┌────────────────┐    │
│           │               │ ChatState  │   │ ImageGenState  │    │
│           │               └────────────┘   └────────────────┘    │
│           │                        │               │              │
│           │                        ▼               ▼              │
│           │               ┌───────────────────────────────────┐  │
│           │               │    StandaloneWorkflowState        │  │
│           │               └───────────────────────────────────┘  │
│           │                                │                      │
│           │                                ▼                      │
│           │                        ┌────────────┐                 │
│           └───────────────────────▶│ MainState  │                 │
│                                    └────────────┘                 │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

---

## 📋 Существующие состояния

| Состояние | Тип | Описание |
|-----------|-----|----------|
| `BootstrapState` | `IEnterState` | Начальная загрузка |
| `LoginState` | `IEnterState` | Экран авторизации |
| `MenuSelectionState` | `IEnterState` | Выбор режима |
| `ChatState` | `IEnterState` | Чат с персонажем |
| `ImageGenState` | `IEnterState` | Генерация изображений |
| `StandaloneWorkflowState` | `IEnterState` | Standalone workflows |
| `MainState` | `IEnterState` | Главное меню игры |
| `NetworkSandboxState` | `IEnterState` | Тесты сети |

### Путь к реализациям

`Core/Infrastructure/StateMachines/States/`

---

## 🏗️ Создание нового состояния

1. **Создать класс состояния:**

```csharp
public class InventoryState : IEnterState
{
    private readonly IViewService _viewService;
    private IGameStateMachine _stateMachine;
    
    public InventoryState(IViewService viewService)
    {
        _viewService = viewService;
    }
    
    public void SetStateMachine(IGameStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    public async void Enter()
    {
        await _viewService.ShowAsync<InventoryViewModel>();
    }
    
    public void Exit()
    {
        _viewService.HideCurrentAsync().Forget();
    }
}
```

2. **Зарегистрировать в DI:**

```csharp
// В InfrastructureBindings или аналоге
builder.AddTransient<InventoryState>();
```

3. **Использовать:**

```csharp
_stateMachine.Enter<InventoryState>();
```

---

## 🔄 Lifecycle

```
Previous State
    │
    └── Exit()
          │
          ▼
    ┌───────────────────────────┐
    │ GameStateMachine          │
    │ - Resolve new state       │
    │ - SetStateMachine(this)   │
    └───────────────────────────┘
          │
          ▼
New State
    │
    └── Enter() / Enter(payload)
```

---

## 🎯 Best Practices

1. **Один экран = одно состояние**
2. **State не хранит UI state** — это делает ViewModel
3. **Exit() должен cleanup** — hide views, cancel operations
4. **Не держать ссылки** на предыдущие состояния
5. **Async в Enter()** — но сам метод void, используй `.Forget()`
