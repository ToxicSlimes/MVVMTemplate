# Game States — Подробное описание

## Обзор

Каждый State отвечает за один экран приложения. State управляет жизненным циклом экрана, загружает сцены и показывает Views.

---

## BootstrapState

**Назначение:** Инициализация приложения при запуске.

**Implements:** `IEnterState`

### Логика

1. Проверяет текущую сцену
2. Если это не `BootstrapScene` — загружает её через `LoadSceneState`
3. После загрузки бутстрап сцены переходит в `LoginState`

### Сцены

- Исходная: `BootstrapScene`
- Следующая: `LoginScene` (через `LoginState`)

### Код

```csharp
public void Enter()
{
    if (SceneManager.GetActiveScene().name != BootstrapSceneName)
    {
        _logger.Info("[BootstrapState] Запуск сцены бутстрапа из {0}", SceneManager.GetActiveScene().name);

        var bootstrapPayload = new SceneLoadPayload(
            BootstrapSceneName,
            SceneLoadType.Regular,
            LoadSceneMode.Single,
            () => StateMachine.Enter<BootstrapState>()
        );

        StateMachine.Enter<LoadSceneState, SceneLoadPayload>(bootstrapPayload);
        return;
    }

    _logger.Info("[BootstrapState] Переход в LoginState");
    StateMachine.Enter<LoginState>();
}
```

---

## DataPreloadState

**Назначение:** Предзагрузка данных перед переходом в игру.

**Implements:** `IEnterState`

### Логика

1. Вызывает `IDataPreloadService.PreloadDataAsync()`
2. После загрузки переходит в `BattleState` через `LoadSceneState`
3. При ошибке — fallback на `BattleState` без загрузки сцены

### Сцены

- Загружает: `LoginScene` (перед переходом в BattleState)

### Зависимости

- `IDataPreloadService` — сервис предзагрузки данных
- `ILogger` — логирование

### Код

```csharp
private async UniTaskVoid LoadDataAndTransition()
{
    try
    {
        await _dataPreloadService.PreloadDataAsync(cancellationToken: CancellationToken.None);
        
        var payload = new SceneLoadPayload(
            SceneName,
            SceneLoadType.Regular,
            UnityEngine.SceneManagement.LoadSceneMode.Single,
            () => StateMachine.Enter<BattleState>()
        );

        StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
    }
    catch (System.Exception ex)
    {
        _logger.Error(ex, "[DataPreloadState] Failed to preload data");
        StateMachine.Enter<BattleState>();
    }
}
```

---

## LoginState

**Назначение:** Экран авторизации пользователя.

**Implements:** `IEnterState`

### Логика

1. Загружает сцену `LoginScene` через `LoadSceneState`
2. После загрузки показывает `LoginViewModel` через `IViewService`

### Сцены

- Загружает: `LoginScene`

### Зависимости

- `IViewService` — показ Views

### Код

```csharp
public void Enter()
{
    var payload = new SceneLoadPayload(
        "LoginScene",
        SceneLoadType.Regular,
        LoadSceneMode.Single,
        ShowLoginView);

    StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
}

private void ShowLoginView()
{
    _viewService.ShowAsync<LoginViewModel>().Forget();
}
```

---

## MenuSelectionState

**Назначение:** Выбор пункта меню после авторизации.

**Implements:** `IEnterState`

### Логика

1. Показывает `MenuSelectionViewModel` через `IViewService`
2. Не загружает новую сцену — работает в текущей

### Зависимости

- `IViewService` — показ Views

### Код

```csharp
public void Enter()
{
    _viewService.ShowAsync<MenuSelectionViewModel>().Forget();
}
```

---

## MainState

**Назначение:** Главный экран приложения.

**Implements:** `IEnterState`

### Логика

1. Показывает `MainViewModel` через `IViewService`
2. Не загружает новую сцену

### Зависимости

- `IViewService` — показ Views

### Код

```csharp
public void Enter()
{
    _viewService.ShowAsync<MainViewModel>().Forget();
}
```

---

## ChatState

**Назначение:** Экран чата с AI.

**Implements:** `IEnterState`

### Логика

1. Загружает сцену `ChatScene` через `LoadSceneState`
2. После загрузки показывает `ChatViewModel`

### Сцены

- Загружает: `ChatScene`

### Зависимости

- `IViewService` — показ Views

### Код

```csharp
public void Enter()
{
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
```

---

## ImageGenState

**Назначение:** Экран генерации изображений.

**Implements:** `IEnterState`

### Логика

1. Показывает `ImageGenViewModel` через `IViewService`
2. Не загружает новую сцену

### Зависимости

- `IViewService` — показ Views

### Код

```csharp
public void Enter()
{
    _viewService.ShowAsync<ImageGenViewModel>().Forget();
}
```

---

## BattleState

**Назначение:** Состояние боя (заглушка).

**Implements:** `IEnterState`, `IStateMachineAware`

### Логика

- Пустая реализация — точка входа для будущей логики боя

### Особенности

- Единственный State, который имплементирует `IStateMachineAware` напрямую
- Используется как fallback после `DataPreloadState`

### Код

```csharp
public class BattleState : GameStateBase<BattleState>, IEnterState, IStateMachineAware
{
    public void Enter()
    {
    }

    public override void Exit()
    {
    }
}
```

---

## NetworkSandboxState

**Назначение:** Тестовый sandbox для работы с сетью.

**Implements:** `IEnterState`

### Логика

1. Загружает сцену `NetworkSandboxScene`
2. После загрузки показывает `NetworkSandboxViewModel`

### Сцены

- Загружает: `NetworkSandboxScene`

### Зависимости

- `IViewService` — показ Views

### Код

```csharp
public void Enter()
{
    var payload = new SceneLoadPayload(
        SceneName,
        SceneLoadType.Regular,
        UnityEngine.SceneManagement.LoadSceneMode.Single,
        ShowNetworkSandboxView
    );

    StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
}

private void ShowNetworkSandboxView()
{
    _viewService.ShowAsync<NetworkSandboxViewModel>().Forget();
}
```

---

## StandaloneWorkflowState

**Назначение:** Экран управления standalone workflows.

**Implements:** `IEnterState`

### Логика

1. Показывает `StandaloneWorkflowViewModel` через `IViewService`
2. Не загружает новую сцену

### Зависимости

- `IViewService` — показ Views

### Документация

См. [StandaloneWorkflow.md](StandaloneWorkflow.md) для подробного описания функционала.

### Код

```csharp
namespace Core.Infrastructure.StateMachines.States
{
    public class StandaloneWorkflowState : GameStateBase<StandaloneWorkflowState>, IEnterState
    {
        private readonly IViewService _viewService;

        public StandaloneWorkflowState(IViewService viewService)
        {
            _viewService = viewService;
        }

        public void Enter()
        {
            _viewService.ShowAsync<StandaloneWorkflowViewModel>().Forget();
        }

        public override void Exit() { }
    }
}
```

---

## LoadSceneState

**Назначение:** Утилитарный State для загрузки Unity сцен.

**Implements:** `IPayloadState<SceneLoadPayload>`

### Логика

1. Получает `SceneLoadPayload` с параметрами загрузки
2. Вызывает `ILoadSceneService.LoadAsync()`
3. Выполняет callback после загрузки (обычно переход в другой State)

### Payload

```csharp
public class SceneLoadPayload
{
    public string SceneName { get; }
    public SceneLoadType LoadType { get; }
    public LoadSceneMode LoadMode { get; }
    public Action OnComplete { get; }
}
```

### Зависимости

- `ILoadSceneService` — сервис загрузки сцен

### Код

```csharp
public void Enter(SceneLoadPayload payload)
{
    LoadAsync(payload).Forget();
}

private async UniTaskVoid LoadAsync(SceneLoadPayload payload)
{
    await _loadSceneService.LoadAsync(payload);
}
```

### Использование

```csharp
var payload = new SceneLoadPayload(
    "LoginScene",
    SceneLoadType.Regular,
    LoadSceneMode.Single,
    () => StateMachine.Enter<LoginState>()
);

StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
```

---

## Паттерны использования

### State с загрузкой сцены

```csharp
public void Enter()
{
    var payload = new SceneLoadPayload(
        "SceneName",
        SceneLoadType.Regular,
        LoadSceneMode.Single,
        ShowView);

    StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
}

private void ShowView()
{
    _viewService.ShowAsync<SomeViewModel>().Forget();
}
```

### State без загрузки сцены

```csharp
public void Enter()
{
    _viewService.ShowAsync<SomeViewModel>().Forget();
}
```

### State с асинхронной логикой

```csharp
public void Enter()
{
    ProcessAsync().Forget();
}

private async UniTaskVoid ProcessAsync()
{
    try
    {
        await SomeAsyncOperation();
        StateMachine.Enter<NextState>();
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Error");
        StateMachine.Enter<FallbackState>();
    }
}
```

---

## Граф переходов

```
┌──────────────┐
│  Bootstrap   │───► LoginState ───► MenuSelectionState ──┬──► MainState
└──────────────┘                                           │
                                                           ├──► ChatState
                                                           │
                                                           ├──► ImageGenState
                                                           │
                                                           ├──► StandaloneWorkflowState
                                                           │
                                                           ├──► NetworkSandboxState
                                                           │
                DataPreloadState ───► BattleState ◄────────┘
```

---

## См. также

- [README.md](README.md) — Общая архитектура Presentation слоя
- [StandaloneWorkflow.md](StandaloneWorkflow.md) — Подробнее о Standalone Workflow
- [Infrastructure/StateMachines](../../Infrastructure/StateMachines/) — Реализация StateMachine
