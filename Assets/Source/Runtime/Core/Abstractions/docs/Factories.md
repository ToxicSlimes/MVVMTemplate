# Factories — Фабрики

> Фабрики для создания сложных объектов.

## 📋 Обзор интерфейсов

| Интерфейс | Назначение | Путь | Реализация |
|-----------|------------|------|------------|
| `IGameStateMachineFactory` | Создание state machine | `Factories/` | `GameStateMachineFactory` |
| `IViewFactory` | Создание View | `MVVM/Factory/` | `ViewFactory` |
| `IUIElementFactory` | Создание UI элементов | `MVVM/Factory/` | `UIElementFactory` |
| `IPopupPoolFactory` | Создание popup pools | `Popup/` | `PopupPoolFactory` |
| `IUiComponentFactory` | Создание UI компонентов | `Services/ProceduralUI/` | `UiComponentFactory` |

---

## IGameStateMachineFactory

**Путь:** `Factories/IGameStateMachineFactory.cs`

```csharp
public interface IGameStateMachineFactory
{
    IGameStateMachine Create();
}
```

### Назначение

Создаёт и настраивает `IGameStateMachine` со всеми зарегистрированными состояниями.

### Реализация

**Путь:** `Core/Infrastructure/StateMachines/Factories/GameStateMachineFactory.cs`

### Пример использования

```csharp
public class GameBootstrapper : MonoBehaviour
{
    [Inject] private IGameStateMachineFactory _factory;
    
    private IGameStateMachine _stateMachine;
    
    void Start()
    {
        _stateMachine = _factory.Create();
        _stateMachine.Enter<BootstrapState>();
    }
}
```

### Типичная реализация

```csharp
public class GameStateMachineFactory : IGameStateMachineFactory
{
    private readonly Container _container;
    
    public GameStateMachineFactory(Container container)
    {
        _container = container;
    }
    
    public IGameStateMachine Create()
    {
        var stateMachine = new GameStateMachine();
        
        // Регистрация всех состояний
        stateMachine.RegisterState(_container.Resolve<BootstrapState>());
        stateMachine.RegisterState(_container.Resolve<LoginState>());
        stateMachine.RegisterState(_container.Resolve<MenuSelectionState>());
        stateMachine.RegisterState(_container.Resolve<ChatState>());
        stateMachine.RegisterState(_container.Resolve<ImageGenState>());
        stateMachine.RegisterState(_container.Resolve<MainState>());
        
        return stateMachine;
    }
}
```

---

## IViewFactory

**Путь:** `MVVM/Factory/IViewFactory.cs`

```csharp
public interface IViewFactory
{
    UniTask<IView> CreateAsync(Type viewModelType, ViewShowParams showParams = null);
    
    UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null) 
        where TView : BindableView<TViewModel> 
        where TViewModel : DisposableObject;
    
    UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null) 
        where TView : BindableView<TViewModel> 
        where TViewModel : DisposableObject;
    
    UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams) 
        where TView : BindableView<TViewModel> 
        where TViewModel : DisposableObject;
    
    void Destroy(IView view);
}
```

### Назначение

Создаёт View через Addressables и связывает с ViewModel. Управляет:
- Загрузкой View prefab через IAssetProvider
- Инстанциацией в нужный Canvas
- Cleanup при уничтожении

### Реализация

**Путь:** `Core/Infrastructure/MVVM/Factory/ViewFactory.cs`

### Пример использования

```csharp
public class ViewService : IViewService
{
    private readonly IViewFactory _viewFactory;
    private readonly ICanvasProvider _canvasProvider;
    
    public async UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null)
    {
        // Factory автоматически найдёт нужный prefab через IViewConfig
        var view = await _viewFactory.CreateAsync(typeof(TViewModel), showParams);
        
        // Bind ViewModel
        var viewModel = _container.Resolve<TViewModel>();
        view.BindTo(viewModel);
        
        // Show
        await view.ShowAsync();
        
        return view;
    }
}
```

---

## IUIElementFactory

**Путь:** `MVVM/Factory/IUIElementFactory.cs`

```csharp
public interface IUIElementFactory
{
    GameObject Instantiate(GameObject prefab, Transform parent);
    T Instantiate<T>(T prefab, Transform parent) where T : Component;
}
```

### Назначение

Простая фабрика для инстанциации UI элементов (cards, list items, buttons).
Может включать pooling для переиспользования.

### Реализация

**Путь:** `Core/Infrastructure/MVVM/Factory/UIElementFactory.cs`

### Пример использования

```csharp
public class HeroCardListView : MonoBehaviour
{
    [SerializeField] private Transform _cardContainer;
    [SerializeField] private HeroCardElement _cardPrefab;
    [Inject] private IUIElementFactory _elementFactory;
    
    private List<HeroCardElement> _cards = new();
    
    public void ShowHeroes(List<HeroCardData> heroes)
    {
        // Clear existing
        ClearCards();
        
        // Create new
        foreach (var hero in heroes)
        {
            var card = _elementFactory.Instantiate(_cardPrefab, _cardContainer);
            card.Bind(hero);
            _cards.Add(card);
        }
    }
    
    private void ClearCards()
    {
        foreach (var card in _cards)
        {
            Destroy(card.gameObject);
        }
        _cards.Clear();
    }
}
```

---

## IPopupPoolFactory

**Путь:** `Popup/IPopupPoolFactory.cs`

```csharp
public interface IPopupPoolFactory
{
    IPopupPool CreatePool(GameObject prefab, Transform poolContainer);
}

public interface IPopupPool
{
    IPopup Get();
    void Return(IPopup popup);
    void Clear();
    int CountActive { get; }
    int CountInactive { get; }
}
```

### Назначение

Создаёт пулы для popup prefabs. Каждый тип popup имеет свой пул.

### Реализация

**Путь:** `Core/Infrastructure/Services/UI/Popup/PopupPoolFactory.cs`

### Пример использования

```csharp
public class PopupService : IPopupService
{
    private readonly IPopupPoolFactory _poolFactory;
    private readonly Dictionary<Type, IPopupPool> _pools = new();
    
    public async UniTask<TPopup> ShowAsync<TPopup>(...)
    {
        if (!_pools.TryGetValue(typeof(TPopup), out var pool))
        {
            var prefab = GetPrefabFor<TPopup>();
            pool = _poolFactory.CreatePool(prefab, _poolContainer);
            _pools[typeof(TPopup)] = pool;
        }
        
        var popup = pool.Get() as TPopup;
        // ...
        return popup;
    }
}
```

---

## IUiComponentFactory

**Путь:** `Services/ProceduralUI/IUiComponentFactory.cs`

```csharp
public interface IUiComponentFactory
{
    /// <summary>
    /// Создать компонент по модели.
    /// </summary>
    IProceduralUiComponent CreateComponent(UiComponentModel model, Transform parent);

    /// <summary>
    /// Создать компонент по типу с конфигурацией.
    /// </summary>
    IProceduralUiComponent CreateComponent(
        UiComponentType type,
        IUiConfigModel config,
        Transform parent,
        string workflowId = null);

    /// <summary>
    /// Проверить поддержку типа.
    /// </summary>
    bool IsTypeSupported(UiComponentType type);

    /// <summary>
    /// Зарегистрировать prefab для типа.
    /// </summary>
    void RegisterPrefab(UiComponentType type, GameObject prefab);

    /// <summary>
    /// Получить prefab для типа.
    /// </summary>
    GameObject GetPrefab(UiComponentType type);
}
```

### Назначение

Создаёт UI компоненты из конфигураций. Используется ProceduralUI системой.

### Пример использования

```csharp
// Регистрация prefabs
_factory.RegisterPrefab(UiComponentType.Button, _buttonPrefab);
_factory.RegisterPrefab(UiComponentType.Card, _cardPrefab);
_factory.RegisterPrefab(UiComponentType.Progress, _progressPrefab);

// Создание компонента
var component = _factory.CreateComponent(
    UiComponentType.Button,
    new UiButtonConfigModel { Label = "Click me" },
    _container,
    workflowId
);
```

---

## 🏗️ Паттерн Factory

### Когда использовать

1. **Сложная инициализация** — много зависимостей, настроек
2. **Полиморфизм** — разные реализации одного интерфейса
3. **DI контейнер** — когда объект создаётся вне контейнера
4. **Object pooling** — переиспользование объектов

### Структура

```
┌─────────────────────────────┐
│      IXxxFactory            │
│  + Create(): IXxx           │
└─────────────────────────────┘
              │
              │ implements
              ▼
┌─────────────────────────────┐
│      XxxFactory             │
│  - _container: Container    │
│  - _config: Config          │
│  + Create(): IXxx           │
└─────────────────────────────┘
              │
              │ creates
              ▼
┌─────────────────────────────┐
│          Xxx                │
│  (configured instance)      │
└─────────────────────────────┘
```

---

## 🎯 Best Practices

1. **Factory в DI** — регистрируй как Singleton
2. **Не создавай в Factory** другие Factory
3. **Используй Pool** для часто создаваемых объектов
4. **Конфигурация через параметры** — не hardcode
5. **Документируй зависимости** создаваемых объектов
