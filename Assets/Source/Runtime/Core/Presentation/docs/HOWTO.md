# Presentation — HOWTO (Рецепты)

*Обновлено: 2026-02-11*

---

## 1. Как добавить новый State

### Простой state (без загрузки сцены)

```csharp
// Presentation/States/MyFeatureState.cs
public class MyFeatureState : GameStateBase<MyFeatureState>, IEnterState
{
    private readonly IViewService _viewService;

    public MyFeatureState(IViewService viewService)
    {
        _viewService = viewService;
    }

    public void Enter()
    {
        _viewService.ShowAsync<MyFeatureViewModel>().Forget();
    }

    public override void Exit() { }
}
```

### State с загрузкой сцены

```csharp
public class MyFeatureState : GameStateBase<MyFeatureState>, IEnterState
{
    private readonly IViewService _viewService;

    public MyFeatureState(IViewService viewService)
    {
        _viewService = viewService;
    }

    public void Enter()
    {
        var payload = new SceneLoadPayload(
            "MyFeatureScene",
            SceneLoadType.Regular,
            LoadSceneMode.Single,
            ShowView);   // callback после загрузки

        StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
    }

    private void ShowView()
    {
        _viewService.ShowAsync<MyFeatureViewModel>().Forget();
    }

    public override void Exit() { }
}
```

### State с payload (параметрами)

```csharp
public class DetailState : GameStateBase<DetailState>, IPayloadState<DetailPayload>
{
    public void Enter(DetailPayload payload)
    {
        // payload.ItemId, payload.Context и т.д.
    }

    public override void Exit() { }
}

// Вызов:
StateMachine.Enter<DetailState, DetailPayload>(new DetailPayload { ItemId = "123" });
```

### Регистрация
Зарегистрируй state в DI и добавь в `IGameStateMachine`.

---

## 2. Как добавить NavigationHandler

### Шаг 1: Navigation target (маркер)

```csharp
// Core/Abstractions/Navigation/
public struct MyFeatureNavigation : INavigationTarget { }
```

### Шаг 2: Handler

```csharp
// Presentation/Handlers/NavigationHandlers.cs (добавить в существующий файл)
public sealed class NavigateToMyFeatureHandler
    : NavigateHandlerBase<MyFeatureNavigation, MyFeatureState>
{
    public NavigateToMyFeatureHandler(IGameStateMachine sm) : base(sm) { }
}
```

### Шаг 3: Вызов из ViewModel

```csharp
await _mediator.Send(new NavigateCommand<MyFeatureNavigation>());
```

**Как это работает:**
1. `NavigateCommand<MyFeatureNavigation>` попадает в Mediator
2. Mediator находит `NavigateToMyFeatureHandler` по типу `MyFeatureNavigation`
3. Handler вызывает `StateMachine.Enter<MyFeatureState>()`
4. State загружает сцену (если нужно) и показывает View

---

## 3. State Machine Flow

### Паттерн перехода

```
ViewModel                    Mediator                  Handler                 StateMachine
   │                           │                         │                        │
   │ NavigateCommand<Target>   │                         │                        │
   ├──────────────────────────>│                         │                        │
   │                           │ Handle(command, ct)     │                        │
   │                           ├────────────────────────>│                        │
   │                           │                         │ Enter<TState>()        │
   │                           │                         ├───────────────────────>│
   │                           │                         │                        │ currentState.Exit()
   │                           │                         │                        │ newState.Enter()
```

### Загрузка сцены (промежуточный state)

```
MyState.Enter()
   │
   ├─ Создаёт SceneLoadPayload(sceneName, callback)
   │
   └─ StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload)
        │
        ├─ LoadSceneState.Enter(payload)
        │   └─ await loadSceneService.LoadAsync(payload)
        │       └─ payload.OnLoaded() → callback
        │           └─ viewService.ShowAsync<VM>()
        │
        └─ (LoadSceneState остаётся текущим до следующего перехода)
```
