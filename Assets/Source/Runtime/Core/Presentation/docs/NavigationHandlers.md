# Navigation Handlers — Подробное описание

## Обзор

Navigation Handlers реализуют паттерн CQRS для навигации между экранами. Они обрабатывают `NavigateCommand<TTarget>` и переключают GameState через `IGameStateMachine`.

**Преимущества:**
- Type-safe навигация
- Централизованная точка навигации
- Легко тестировать
- Отделение логики навигации от ViewModels

---

## Архитектура

```
┌─────────────┐     ┌──────────────┐     ┌──────────────────┐     ┌──────────────┐
│  ViewModel  │────►│   Mediator   │────►│NavigationHandler │────►│ StateMachine │
│             │     │              │     │                  │     │              │
│ Send(Nav)   │     │ Route        │     │ Enter<State>()   │     │ Activate     │
└─────────────┘     └──────────────┘     └──────────────────┘     └──────────────┘
```

### Компоненты

| Компонент | Описание |
|-----------|----------|
| `NavigateCommand<TTarget>` | CQRS команда с navigation target |
| `INavigationTarget` | Маркер интерфейс для navigation targets |
| `NavigateHandlerBase` | Базовый класс для всех handlers |
| `NavigationHandlers.cs` | Файл с конкретными handlers |

---

## NavigateCommand

CQRS команда для навигации. Содержит navigation target.

```csharp
public class NavigateCommand<TTarget> : IRequest<Nothing>
    where TTarget : struct, INavigationTarget
{
    public TTarget Target { get; }
    
    public NavigateCommand(TTarget target)
    {
        Target = target;
    }
}
```

### Использование

```csharp
await _mediator.Send(new NavigateCommand<ChatNavigation>(new ChatNavigation()));
```

---

## NavigateHandlerBase

Базовый класс для всех navigation handlers.

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

### Generic параметры

| Параметр | Описание | Constraint |
|----------|----------|------------|
| `TTarget` | Navigation target (маркер) | `struct`, `INavigationTarget` |
| `TState` | State для активации | `class`, `IEnterState` |

### Возвращаемое значение

- `UniTask<Nothing>` — асинхронный результат (Nothing = void в CQRS)

---

## Доступные Handlers

### NavigateToLoginHandler

**Target:** `LoginNavigation`  
**State:** `LoginState`

```csharp
public sealed class NavigateToLoginHandler
    : NavigateHandlerBase<LoginNavigation, LoginState>
{
    public NavigateToLoginHandler(IGameStateMachine sm) : base(sm) { }
}
```

**Когда использовать:**
- Возврат к экрану авторизации
- Logout пользователя

---

### NavigateToMenuSelectionHandler

**Target:** `MenuSelectionNavigation`  
**State:** `MenuSelectionState`

```csharp
public sealed class NavigateToMenuSelectionHandler
    : NavigateHandlerBase<MenuSelectionNavigation, MenuSelectionState>
{
    public NavigateToMenuSelectionHandler(IGameStateMachine sm) : base(sm) { }
}
```

**Когда использовать:**
- После успешной авторизации
- Возврат из под-экранов (Chat, ImageGen, etc.) в главное меню

---

### NavigateToChatHandler

**Target:** `ChatNavigation`  
**State:** `ChatState`

```csharp
public sealed class NavigateToChatHandler
    : NavigateHandlerBase<ChatNavigation, ChatState>
{
    public NavigateToChatHandler(IGameStateMachine sm) : base(sm) { }
}
```

**Когда использовать:**
- Переход в чат с AI из MenuSelection

---

### NavigateToImageGenHandler

**Target:** `ImageGenNavigation`  
**State:** `ImageGenState`

```csharp
public sealed class NavigateToImageGenHandler
    : NavigateHandlerBase<ImageGenNavigation, ImageGenState>
{
    public NavigateToImageGenHandler(IGameStateMachine sm) : base(sm) { }
}
```

**Когда использовать:**
- Переход в генератор изображений из MenuSelection

---

### NavigateToMainHandler

**Target:** `MainNavigation`  
**State:** `MainState`

```csharp
public sealed class NavigateToMainHandler
    : NavigateHandlerBase<MainNavigation, MainState>
{
    public NavigateToMainHandler(IGameStateMachine sm) : base(sm) { }
}
```

**Когда использовать:**
- Переход на главный экран приложения

---

### NavigateToStandaloneWorkflowHandler

**Target:** `StandaloneWorkflowNavigation`  
**State:** `StandaloneWorkflowState`

```csharp
public sealed class NavigateToStandaloneWorkflowHandler
    : NavigateHandlerBase<StandaloneWorkflowNavigation, StandaloneWorkflowState>
{
    public NavigateToStandaloneWorkflowHandler(IGameStateMachine sm) : base(sm) { }
}
```

**Когда использовать:**
- Переход в Standalone Workflow экран из MenuSelection

---

## Navigation Targets

Navigation targets — struct-маркеры, которые обозначают цель навигации.

```csharp
public struct LoginNavigation : INavigationTarget { }
public struct MenuSelectionNavigation : INavigationTarget { }
public struct ChatNavigation : INavigationTarget { }
public struct ImageGenNavigation : INavigationTarget { }
public struct MainNavigation : INavigationTarget { }
public struct StandaloneWorkflowNavigation : INavigationTarget { }
```

**Где определены:** `Core.Abstractions.Navigation`

### Зачем struct?

- Легковесные (не аллоцируются на heap)
- Нет null-проблем
- Ясная семантика "маркера"

---

## Использование

### Из ViewModel

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
            await _mediator.Send(new NavigateCommand<ChatNavigation>(new ChatNavigation()));
        });
    }
}
```

### Из State (редко)

```csharp
public class LoginState : GameStateBase<LoginState>, IEnterState
{
    private readonly IMediator _mediator;
    
    public void OnLoginSuccess()
    {
        // Лучше использовать StateMachine напрямую
        StateMachine.Enter<MenuSelectionState>();
        
        // Или через Mediator, если нужна единообразность
        _mediator.Send(new NavigateCommand<MenuSelectionNavigation>(new MenuSelectionNavigation()));
    }
}
```

**Best Practice:** Из State лучше использовать `StateMachine.Enter<T>()` напрямую, а Mediator — из ViewModels.

---

## Регистрация Handlers

Handlers регистрируются в DI-контейнере как:

```csharp
services.AddScoped<IRequestHandler<NavigateCommand<LoginNavigation>, Nothing>, NavigateToLoginHandler>();
services.AddScoped<IRequestHandler<NavigateCommand<MenuSelectionNavigation>, Nothing>, NavigateToMenuSelectionHandler>();
services.AddScoped<IRequestHandler<NavigateCommand<ChatNavigation>, Nothing>, NavigateToChatHandler>();
// ...
```

**Где:** `CompositionRoot` или `NavigationModule`

---

## Расширение

### Добавить новый Navigation Handler

1. **Создай Navigation Target** в `Abstractions/Navigation`:

```csharp
public struct BattleNavigation : INavigationTarget { }
```

2. **Создай State** в `Presentation/States`:

```csharp
public class BattleState : GameStateBase<BattleState>, IEnterState
{
    public void Enter() { /* ... */ }
    public override void Exit() { }
}
```

3. **Добавь Handler** в `Presentation/Handlers/NavigationHandlers.cs`:

```csharp
public sealed class NavigateToBattleHandler
    : NavigateHandlerBase<BattleNavigation, BattleState>
{
    public NavigateToBattleHandler(IGameStateMachine sm) : base(sm) { }
}
```

4. **Зарегистрируй в DI**:

```csharp
services.AddScoped<IRequestHandler<NavigateCommand<BattleNavigation>, Nothing>, NavigateToBattleHandler>();
```

5. **Используй**:

```csharp
await _mediator.Send(new NavigateCommand<BattleNavigation>(new BattleNavigation()));
```

---

## Паттерны

### Navigation с параметрами

Если нужно передать параметры при навигации, добавь поля в Navigation Target:

```csharp
public struct BattleNavigation : INavigationTarget
{
    public int EnemyId { get; }
    
    public BattleNavigation(int enemyId)
    {
        EnemyId = enemyId;
    }
}
```

Затем в handler передай параметры в State через payload:

```csharp
public sealed class NavigateToBattleHandler
    : IRequestHandler<NavigateCommand<BattleNavigation>, Nothing>
{
    private readonly IGameStateMachine _stateMachine;

    public NavigateToBattleHandler(IGameStateMachine sm)
    {
        _stateMachine = sm;
    }

    public UniTask<Nothing> Handle(NavigateCommand<BattleNavigation> command, CancellationToken ct)
    {
        var payload = new BattlePayload(command.Target.EnemyId);
        _stateMachine.Enter<BattleState, BattlePayload>(payload);
        return UniTask.FromResult(Nothing.Value);
    }
}
```

---

## Flow диаграмма

```
                         ┌──────────────────────────────────┐
                         │     User clicks button           │
                         └──────────┬───────────────────────┘
                                    │
                                    ▼
                         ┌──────────────────────────────────┐
                         │  ViewModel.Command.Execute()     │
                         └──────────┬───────────────────────┘
                                    │
                                    ▼
                         ┌──────────────────────────────────┐
                         │  Mediator.Send(NavigateCommand)  │
                         └──────────┬───────────────────────┘
                                    │
                                    ▼
                         ┌──────────────────────────────────┐
                         │   NavigationHandler.Handle()     │
                         └──────────┬───────────────────────┘
                                    │
                                    ▼
                         ┌──────────────────────────────────┐
                         │  StateMachine.Enter<TState>()    │
                         └──────────┬───────────────────────┘
                                    │
                                    ▼
                         ┌──────────────────────────────────┐
                         │    State.Enter()                 │
                         └──────────┬───────────────────────┘
                                    │
                         ┌──────────┴──────────┐
                         │                     │
                         ▼                     ▼
              ┌─────────────────┐   ┌───────────────────┐
              │ Load Scene      │   │ Show View         │
              │ (if needed)     │   │ (IViewService)    │
              └─────────────────┘   └───────────────────┘
```

---

## Best Practices

### ✅ DO

1. **Используй NavigateCommand** для всех переходов из ViewModels
2. **Создавай отдельный handler** для каждой навигации (не переиспользуй)
3. **Navigation Targets — struct** (легковесные маркеры)
4. **Handlers — stateless** (никакого состояния внутри)

### ❌ DON'T

1. **Не вызывай StateMachine** напрямую из ViewModels (только через Mediator)
2. **Не добавляй логику** в handlers (только переключение State)
3. **Не делай async логику** в handlers (это работа State)
4. **Не храни состояние** в Navigation Targets (только параметры навигации)

---

## Таблица соответствий

| Navigation Target | Handler | State | Scene | View |
|-------------------|---------|-------|-------|------|
| `LoginNavigation` | `NavigateToLoginHandler` | `LoginState` | LoginScene | LoginViewModel |
| `MenuSelectionNavigation` | `NavigateToMenuSelectionHandler` | `MenuSelectionState` | (текущая) | MenuSelectionViewModel |
| `ChatNavigation` | `NavigateToChatHandler` | `ChatState` | ChatScene | ChatViewModel |
| `ImageGenNavigation` | `NavigateToImageGenHandler` | `ImageGenState` | (текущая) | ImageGenViewModel |
| `MainNavigation` | `NavigateToMainHandler` | `MainState` | (текущая) | MainViewModel |
| `StandaloneWorkflowNavigation` | `NavigateToStandaloneWorkflowHandler` | `StandaloneWorkflowState` | (текущая) | StandaloneWorkflowViewModel |

---

## См. также

- [README.md](README.md) — Общая архитектура Presentation слоя
- [States.md](States.md) — Подробнее о States
- [Abstractions/Navigation](../../Abstractions/README.md#navigation) — Navigation targets
