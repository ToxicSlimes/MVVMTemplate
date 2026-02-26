# Presentation Layer — Архитектура

## Обзор

Presentation Layer — слой навигации и управления состояниями UI.  
Отвечает за переходы между экранами и управление жизненным циклом Views.

**Содержит:**
- **States** — GameState'ы для каждого экрана
- **Handlers** — Navigation handlers для CQRS навигации

---

## Структура

```
Presentation/
├── States/             # GameState'ы (экраны приложения)
│   ├── GameStateBase.cs       # Базовый класс
│   ├── BootstrapState.cs      # Инициализация
│   ├── DataPreloadState.cs    # Предзагрузка данных
│   ├── LoadSceneState.cs      # Загрузка сцен
│   ├── LoginState.cs          # Авторизация
│   ├── MenuSelectionState.cs  # Выбор меню
│   ├── MainState.cs           # Главный экран
│   ├── ChatState.cs           # Чат
│   ├── ImageGenState.cs       # Генерация изображений
│   ├── BattleState.cs         # Бой
│   ├── NetworkSandboxState.cs # Тестовый sandbox
│   └── StandaloneWorkflowState.cs # Standalone Workflow
│
├── Handlers/           # Navigation Handlers
│   ├── NavigateHandlerBase.cs # Базовый handler
│   └── NavigationHandlers.cs  # Все navigation handlers
│
└── docs/               # Документация
    ├── README.md              # Обзор Presentation слоя (этот файл)
    ├── States.md              # Подробное описание каждого State
    ├── NavigationHandlers.md  # Подробное описание Navigation Handlers
    └── StandaloneWorkflow.md  # Standalone Workflow экран
```

---

## Правила зависимостей

```
Presentation МОЖЕТ зависеть от:
├── Abstractions (IViewService, IGameStateMachine, INavigationTarget)
├── Application (через Mediator)
├── Infrastructure.Mediator (IRequestHandler)
└── MVVM (ViewModels для показа Views)

Presentation НЕ МОЖЕТ зависеть от:
├── Infrastructure/Services (конкретные реализации)
└── Data (DTOs, Models напрямую)
```

---

## States

### GameStateBase

Базовый класс для всех состояний.

```csharp
public abstract class GameStateBase<TSelf> : IGameState
    where TSelf : GameStateBase<TSelf>
{
    protected IGameStateMachine StateMachine;
    
    public Type StateType => typeof(TSelf);
    
    public virtual void SetStateMachine(IGameStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }
    
    public abstract void Exit();
}
```

### Пример State

```csharp
public class ChatState : GameStateBase<ChatState>, IEnterState
{
    private readonly IViewService _viewService;

    public ChatState(IViewService viewService)
    {
        _viewService = viewService;
    }

    public void Enter()
    {
        // Загружаем сцену, затем показываем View
        var payload = new SceneLoadPayload(
            "ChatScene",
            SceneLoadType.Regular,
            LoadSceneMode.Single,
            ShowChatView);

        StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
    }

    private void ShowChatView()
    {
        _viewService.ShowAsync<ChatViewModel>().Forget();
    }

    public override void Exit() { }
}
```

### Доступные States

| State | Описание | Implements |
|-------|----------|------------|
| `BootstrapState` | Инициализация приложения | IEnterState |
| `DataPreloadState` | Предзагрузка данных | IEnterState |
| `LoadSceneState` | Загрузка сцен (payload) | IPayloadState<SceneLoadPayload> |
| `LoginState` | Экран авторизации | IEnterState |
| `MenuSelectionState` | Выбор пункта меню | IEnterState |
| `MainState` | Главный экран | IEnterState |
| `ChatState` | Экран чата | IEnterState |
| `ImageGenState` | Генерация изображений | IEnterState |
| `BattleState` | Бой | IEnterState, IStateMachineAware |
| `NetworkSandboxState` | Тестовый sandbox | IEnterState |
| `StandaloneWorkflowState` | Standalone Workflow | IEnterState |

---

## Navigation Handlers

Type-safe навигация через CQRS. Handlers обрабатывают navigation requests.

### NavigateHandlerBase

```csharp
public abstract class NavigateHandlerBase<TTarget, TState>
    : IRequestHandler<NavigateCommand<TTarget>, Nothing>
    where TTarget : struct, INavigationTarget
    where TState : class, IEnterState
{
    private readonly IGameStateMachine _stateMachine;

    protected NavigateHandlerBase(IGameStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public UniTask<Nothing> Handle(NavigateCommand<TTarget> command, CancellationToken ct)
    {
        _stateMachine.Enter<TState>();
        return UniTask.FromResult(Nothing.Value);
    }
}
```

### Конкретные Handlers

```csharp
// Все handlers наследуют NavigateHandlerBase
// В NavigationHandlers.cs определены все navigation handlers

public sealed class NavigateToChatHandler
    : NavigateHandlerBase<ChatNavigation, ChatState>
{
    public NavigateToChatHandler(IGameStateMachine sm) : base(sm) { }
}

public sealed class NavigateToMainHandler
    : NavigateHandlerBase<MainNavigation, MainState>
{
    public NavigateToMainHandler(IGameStateMachine sm) : base(sm) { }
}

public sealed class NavigateToLoginHandler
    : NavigateHandlerBase<LoginNavigation, LoginState>
{
    public NavigateToLoginHandler(IGameStateMachine sm) : base(sm) { }
}
```

### Доступные Navigation Handlers

| Handler | Navigation Target | State |
|---------|-------------------|-------|
| `NavigateToLoginHandler` | `LoginNavigation` | `LoginState` |
| `NavigateToMenuSelectionHandler` | `MenuSelectionNavigation` | `MenuSelectionState` |
| `NavigateToChatHandler` | `ChatNavigation` | `ChatState` |
| `NavigateToImageGenHandler` | `ImageGenNavigation` | `ImageGenState` |
| `NavigateToMainHandler` | `MainNavigation` | `MainState` |
| `NavigateToStandaloneWorkflowHandler` | `StandaloneWorkflowNavigation` | `StandaloneWorkflowState` |

---

## Использование

### Навигация из ViewModel

```csharp
public class MenuSelectionViewModel : DisposableObject
{
    private readonly IMediator _mediator;
    
    public ReactiveCommand<Unit> GoToChatCommand { get; }
    
    public MenuSelectionViewModel(IMediator mediator)
    {
        _mediator = mediator;
        
        GoToChatCommand = new ReactiveCommand<Unit>(async _ =>
        {
            // Type-safe навигация через Mediator
            await _mediator.Send(new NavigateCommand<ChatNavigation>(new ChatNavigation()));
        });
    }
}
```

### Навигация из State

```csharp
public class LoginState : GameStateBase<LoginState>, IEnterState
{
    public void Enter()
    {
        // Показать View, затем перейти на MenuSelection
        _viewService.ShowAsync<LoginViewModel>().Forget();
    }
    
    public void OnLoginSuccess()
    {
        StateMachine.Enter<MenuSelectionState>();
    }
}
```

---

## Flow

```
┌─────────────┐      ┌─────────────┐      ┌─────────────────┐      ┌─────────────┐
│  ViewModel  │─────►│   Mediator  │─────►│NavigationHandler│─────►│ StateMachine│
│   (MVVM)    │      │             │      │ (Presentation)  │      │             │
│             │      │             │      │                 │      │             │
│ Send(Nav)   │      │ Route       │      │ Enter<State>()  │      │ State.Enter │
└─────────────┘      └─────────────┘      └─────────────────┘      └──────┬──────┘
                                                                          │
                                                                          ▼
                                                                   ┌─────────────┐
                                                                   │    State    │
                                                                   │             │
                                                                   │ ShowView()  │
                                                                   └──────┬──────┘
                                                                          │
                                                                          ▼
                                                                   ┌─────────────┐
                                                                   │    View     │
                                                                   │   (MVVM)    │
                                                                   └─────────────┘
```

---

## Best Practices

### ✅ DO

1. **Один State — один экран**
2. **States показывают Views** через IViewService
3. **Используй NavigateRequest** для type-safe навигации
4. **States чистят за собой** в Exit()

### ❌ DON'T

1. **Не вызывай Services напрямую** — через Mediator/Application
2. **Не храни состояние в Handlers** — они stateless
3. **Не пропускай Exit()** — очистка ресурсов важна

---

## См. также

- [States.md](States.md) — Подробное описание каждого State
- [NavigationHandlers.md](NavigationHandlers.md) — Подробное описание Navigation Handlers
- [StandaloneWorkflow.md](StandaloneWorkflow.md) — Standalone Workflow экран
- [Architecture.md](../../Documentation/Architecture.md) — Общая архитектура
- [Navigation в Abstractions](../../Abstractions/README.md#navigation) — Navigation targets
- [StateMachines в Infrastructure](../../Infrastructure/StateMachines/) — StateMachine реализация
