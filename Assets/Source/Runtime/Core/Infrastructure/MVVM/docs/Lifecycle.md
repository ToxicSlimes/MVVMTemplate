# Lifecycle — Жизненный цикл MVVM

Полное описание жизненного цикла View и ViewModel от создания до уничтожения.

---

## Общая схема

```mermaid
flowchart TB
    subgraph Creation["1. CREATION"]
        A[ViewService.ShowAsync] --> B[ViewFactory.CreateAsync]
        B --> C[Addressables.Load]
        C --> D[Object.Instantiate]
        D --> E[DI Injection]
        E --> F[Resolve ViewModel]
        F --> G[BindTo]
    end

    subgraph Show["2. SHOW"]
        G --> H[AssignSortingOrder]
        H --> I[AnimateShowAsync]
        I --> J[ShowAsync]
        J --> K[ViewShown event]
    end

    subgraph Active["3. ACTIVE"]
        K --> L[User Interaction]
        L --> M[ViewModel Updates]
        M --> N[View Reacts]
        N --> L
    end

    subgraph Hide["4. HIDE"]
        L --> O[HideAsync called]
        O --> P[AnimateHideAsync]
        P --> Q[HideAsync on View]
        Q --> R[ViewHidden event]
    end

    subgraph Destroy["5. DESTROY"]
        R --> S{IsPersistent?}
        S -->|Yes| T[Keep in cache]
        S -->|No| U[Unbind]
        U --> V[Dispose ViewModel]
        V --> W[ReleaseInstance]
        W --> X[Destroyed]
        T --> L
    end
```

---

## Фаза 1: Создание (Creation)

### 1.1 ViewService.ShowAsync

```csharp
// Точка входа
await _viewService.ShowAsync<MyView, MyViewModel>(showParams);
```

```mermaid
sequenceDiagram
    participant C as Caller
    participant VS as ViewService
    participant VF as ViewFactory
    participant AP as AssetProvider
    participant DI as Container
    participant V as View
    participant VM as ViewModel

    C->>VS: ShowAsync<MyView, MyViewModel>()
    
    Note over VS: Check cache
    alt View in cache
        VS->>VS: Use cached view
    else Not in cache
        VS->>VF: CreateAsync()
        VF->>AP: LoadAsync(address)
        AP-->>VF: prefab
        VF->>VF: Instantiate(prefab)
        VF->>DI: InjectRecursive(gameObject)
        VF->>DI: Resolve<TViewModel>()
        DI-->>VF: viewModel
        VF->>V: BindTo(viewModel)
        V->>V: OnBind(ref disposables)
        VF-->>VS: view
        VS->>VS: Add to cache
    end
```

### 1.2 ViewFactory.CreateAsync

Порядок операций:

| Шаг | Операция | Описание |
|-----|----------|----------|
| 1 | GetViewAddress | Получить Addressables адрес из ViewConfig |
| 2 | ResolveParent | Определить родительский Transform |
| 3 | InstantiateAsync | Загрузить и создать prefab |
| 4 | InjectRecursive | DI инъекция во все компоненты |
| 5 | Resolve ViewModel | Получить ViewModel из контейнера |
| 6 | BindTo | Привязать ViewModel к View |

### 1.3 BindTo и OnBind

```csharp
public IDisposable BindTo(TViewModel viewModel)
{
    // 1. Очистить предыдущие подписки
    _disposables.Dispose();
    _disposables = new DisposableBag();

    // 2. Установить ViewModel
    _viewModel.Value = viewModel;

    // 3. Настроить подписки (пользовательский код)
    OnBind(ref _disposables);

    return _disposables;
}
```

---

## Фаза 2: Показ (Show)

### 2.1 Sorting Order

```mermaid
graph LR
    subgraph Before
        V1[View1: order 1]
        V2[View2: order 2]
    end
    
    subgraph After
        V1a[View1: order 1]
        V2a[View2: order 2]
        V3[NewView: order 3]
    end
    
    Before --> After
```

```csharp
private void AssignSortingOrder(IView view)
{
    var canvas = GetOrAddCanvas(view);
    _currentMaxSortingOrder++;
    canvas.overrideSorting = true;
    canvas.sortingOrder = _currentMaxSortingOrder;
}
```

### 2.2 AnimateShowAsync

```mermaid
sequenceDiagram
    participant VS as ViewService
    participant VA as ViewAnimator
    participant V as View
    participant CG as CanvasGroup

    VS->>VA: AnimateShowAsync(view, Fade|Scale)
    
    par Parallel animations
        VA->>CG: alpha 0 → 1
    and
        VA->>V: scale 0 → 1
    end
    
    VA-->>VS: Complete
```

### 2.3 ShowAsync на View

```csharp
public virtual async UniTask ShowAsync()
{
    gameObject.SetActive(true);      // 1
    IsVisible = true;                 // 2
    await OnBeforeShow();             // 3 - hook
    await PlayShowAnimation();        // 4 - fallback
    await OnAfterShow();              // 5 - hook
}
```

---

## Фаза 3: Активное состояние

### 3.1 Реактивный поток данных

```mermaid
flowchart LR
    subgraph ViewModel
        RP[ReactiveProperty]
        RC[ReactiveCommand]
    end

    subgraph View
        UI[UI Elements]
        Handler[Event Handlers]
    end

    RP -->|Subscribe| UI
    UI -->|OnClick| Handler
    Handler -->|Execute| RC
    RC -->|Update| RP
```

### 3.2 Пример потока

```csharp
// ViewModel
public ReactiveProperty<int> Score { get; } = new(0);
public ReactiveCommand<Unit> IncrementCommand { get; }

IncrementCommand = new ReactiveCommand<Unit>();
IncrementCommand.Subscribe(_ => Score.Value++);

// View
ViewModel.Score
    .Subscribe(_scoreText, static (s, t) => t.text = s.ToString())
    .AddTo(ref disposables);

_button.OnClickAsObservable()
    .Subscribe(ViewModel.IncrementCommand, 
        static (_, cmd) => cmd.Execute(Unit.Default))
    .AddTo(ref disposables);
```

---

## Фаза 4: Скрытие (Hide)

### 4.1 HideAsync flow

```mermaid
sequenceDiagram
    participant C as Caller
    participant VS as ViewService
    participant VA as ViewAnimator
    participant V as View

    C->>VS: HideAsync(view)
    
    Note over VS: Get ViewShowParams
    
    alt Has params
        VS->>VS: OnHidden callback
        VS->>VA: AnimateHideAsync
        VA-->>VS: Complete
    end
    
    VS->>V: HideAsync()
    V->>V: OnBeforeHide()
    V->>V: PlayHideAnimation()
    V->>V: OnAfterHide()
    V->>V: IsVisible = false
    V->>V: gameObject.SetActive(false)
    
    VS->>VS: ReleaseSortingOrder
    VS->>VS: ViewHidden event
```

### 4.2 HideAsync на View

```csharp
public virtual async UniTask HideAsync()
{
    await OnBeforeHide();              // 1 - hook
    await PlayHideAnimation();         // 2 - fallback
    await OnAfterHide();               // 3 - hook
    IsVisible = false;                 // 4
    gameObject.SetActive(false);       // 5
}
```

---

## Фаза 5: Уничтожение (Destroy)

### 5.1 Persistent vs Non-Persistent

```mermaid
flowchart TD
    A[HideAsync complete] --> B{IsPersistent?}
    
    B -->|true| C[Keep in cache]
    C --> D[gameObject inactive]
    D --> E[Ready for re-show]
    
    B -->|false| F[Remove from cache]
    F --> G[Unbind]
    G --> H[ViewModel.Dispose]
    H --> I[ReleaseInstance]
    I --> J[GameObject destroyed]
```

### 5.2 Unbind

```csharp
public virtual void Unbind()
{
    var currentViewModel = _viewModel.Value;
    _viewModel.Value = null;
    
    // ⚠️ ViewModel уничтожается!
    currentViewModel?.Dispose();
    
    _disposables.Dispose();
    _disposables = new DisposableBag();
}
```

### 5.3 ViewModel.Dispose

```csharp
public virtual void Dispose()
{
    if (IsDisposed) return;
    IsDisposed = true;

    // Отмена async операций
    SafeCancelAndDisposeRequestCts();
    
    // Очистка всех подписок
    _disposables.Dispose();
}
```

### 5.4 ReleaseInstance

```csharp
// ViewFactory
public void Destroy(IView view)
{
    if (view is IUnbindable unbindable)
        unbindable.Unbind();

    _assetProvider.ReleaseInstance(component.gameObject);
}
```

---

## Lifecycle Hooks Reference

### BindableView hooks

| Hook | Когда вызывается | Использование |
|------|------------------|---------------|
| `OnBind` | После BindTo | Настройка подписок |
| `OnBeforeShow` | Перед анимацией показа | Загрузка данных |
| `OnAfterShow` | После анимации показа | Запуск анимаций UI |
| `OnBeforeHide` | Перед анимацией скрытия | Сохранение данных |
| `OnAfterHide` | После анимации скрытия | Очистка UI |
| `OnDestroy` | При уничтожении | Автоматический Unbind |

### BindableElement hooks

| Hook | Когда вызывается | Использование |
|------|------------------|---------------|
| `OnBind` | Первая привязка | Установка UI |
| `OnRefresh` | Обновление данных | Оптимизированное обновление |
| `OnUnbind` | Отвязка данных | Сброс состояния |
| `OnSelectionChanged` | Изменение выделения | Визуальный отклик |
| `OnDispose` | Уничтожение | Очистка ресурсов |

---

## Диаграмма состояний View

```mermaid
stateDiagram-v2
    [*] --> NotExists

    NotExists --> Created: CreateAsync
    
    Created --> Bound: BindTo
    
    state Bound {
        [*] --> Hidden
        Hidden --> Showing: ShowAsync
        Showing --> Visible: Animation done
        Visible --> Hiding: HideAsync
        Hiding --> Hidden: Animation done
    }
    
    Bound --> Unbound: Unbind
    
    state Unbound {
        [*] --> Disposing
        Disposing --> Disposed: Dispose complete
    }
    
    Unbound --> Bound: Re-BindTo (rare)
    Disposed --> [*]
```

---

## Диаграмма состояний ViewModel

```mermaid
stateDiagram-v2
    [*] --> Created: new / DI Resolve

    Created --> Active: Properties initialized
    
    state Active {
        [*] --> Ready
        Ready --> Processing: Async operation
        Processing --> Ready: Operation complete
        Processing --> Cancelled: CTS cancelled
        Cancelled --> Ready: Reset
    }
    
    Active --> Disposing: Dispose called
    
    Disposing --> Disposed: IsDisposed = true
    Disposed --> [*]
```

---

## Timing и производительность

### Типичное время операций

| Операция | Время | Заметки |
|----------|-------|---------|
| Addressables.LoadAsync | 10-100ms | Зависит от размера |
| Instantiate | 1-5ms | Сложность prefab |
| DI Injection | <1ms | Количество полей |
| BindTo + OnBind | 1-10ms | Количество подписок |
| AnimateShow | 200-300ms | Настраиваемо |
| AnimateHide | 150-250ms | Настраиваемо |

### Оптимизации

1. **Preload Addressables** для критичных View
2. **Cache View** через IsPersistent
3. **Static lambdas** для zero-allocation подписок
4. **Batch Reconcile** вместо поэлементного обновления

---

## Thread Safety

```mermaid
flowchart TD
    subgraph MainThread["Main Thread Only"]
        A[Unity API calls]
        B[GameObject operations]
        C[UI updates]
    end

    subgraph ThreadSafe["Thread Safe"]
        D[ViewService operations]
        E[Cache access]
        F[History management]
    end

    subgraph Background["Can be Background"]
        G[Addressables loading]
        H[Data processing]
    end
```

ViewService использует:
- `ConcurrentDictionary` для кеша
- `lock(_sync)` для стеков
- `Interlocked.Exchange` для currentView

---

## Debugging Lifecycle

### События для отладки

```csharp
// В ViewService
_viewService.ViewShown += view => 
    Debug.Log($"[MVVM] Shown: {view.GetType().Name}");

_viewService.ViewHidden += view => 
    Debug.Log($"[MVVM] Hidden: {view.GetType().Name}");
```

### Проверка состояния

```csharp
// В любом месте
Debug.Log($"Current: {_viewService.CurrentView?.GetType().Name}");
Debug.Log($"Active: {_viewService.ActiveViews.Count}");
Debug.Log($"History: {_viewService.ViewHistory.Count}");
Debug.Log($"IsActive<T>: {_viewService.IsViewActive<MyView>()}");
```

---

## Частые ошибки

### ❌ Забыли AddTo

```csharp
// BAD: утечка памяти
ViewModel.Name.Subscribe(n => _text.text = n);

// GOOD
ViewModel.Name.Subscribe(...).AddTo(ref disposables);
```

### ❌ Использование после Dispose

```csharp
// BAD: может крашнуть
public async UniTask LoadAsync()
{
    var data = await _service.GetAsync();
    Name.Value = data.Name; // ViewModel может быть disposed!
}

// GOOD
public async UniTask LoadAsync()
{
    var data = await _service.GetAsync();
    if (IsDisposed) return;
    Name.Value = data.Name;
}
```

### ❌ Забыли проверить IsDisposed

```csharp
// BAD
public void DoSomething() => SomeProperty.Value = "value";

// GOOD
public void DoSomething()
{
    if (IsDisposed) return;
    SomeProperty.Value = "value";
}
```

---

## См. также

- [BindableView.md](BindableView.md) — детали View
- [ViewModel.md](ViewModel.md) — детали ViewModel
- [ViewService.md](ViewService.md) — API сервиса
- [ViewAnimator.md](ViewAnimator.md) — анимации
