# MVVM — Model-View-ViewModel

> Реализация MVVM паттерна для Unity с поддержкой async, pooling и анимаций.

## 📋 Обзор интерфейсов

| Интерфейс | Назначение | Реализация |
|-----------|------------|------------|
| `IView` | Базовый View | `BindableView<T>` |
| `IViewModel` | Базовый ViewModel | `DisposableObject` |
| `IModel` | Базовый Model | Доменные модели |
| `IViewService` | Управление View | `ViewService` |
| `IViewFactory` | Фабрика View | `ViewFactory` |
| `IViewConfig` | Конфиг View prefabs | `ViewConfig` |
| `IBindableElement` | UI элемент с данными | `BindableElement<T>` |
| `IUIElementFactory` | Фабрика UI элементов | `UIElementFactory` |
| `IUIElementService` | Управление элементами | `UIElementService` |
| `IElementConfig` | Конфиг Element prefabs | `ElementConfig` |
| `IViewAnimator` | Анимации View | `ViewAnimator` |
| `ICanvasProvider` | Canvas менеджмент | `CanvasProvider` |
| `IEmbeddedPanelHost` | Хост для embedded панелей | Custom implementations |

---

## IView

**Путь:** `MVVM/Views/IView.cs`

```csharp
public interface IView : IDisposable
{
    bool IsPersistent { get; }
    IViewModel ViewModel { get; }
}
```

### Назначение
Базовый контракт для всех View. Определяет связь с ViewModel и политику persistence.

### Properties

| Property | Тип | Описание |
|----------|-----|----------|
| `IsPersistent` | `bool` | Если `true`, View не уничтожается при переходах |
| `ViewModel` | `IViewModel` | Связанный ViewModel |

### Связанные интерфейсы

```csharp
public interface IShowable
{
    UniTask ShowAsync();
}

public interface IHideable
{
    UniTask HideAsync();
}

public interface IUnbindable
{
    void Unbind();
}
```

### Реализация
**Путь:** `Core/Infrastructure/MVVM/Views/BindableView.cs`

### Пример использования

```csharp
public class ChatView : BindableView<ChatViewModel>
{
    [SerializeField] private TMP_Text _statusText;
    
    protected override void OnBind(ChatViewModel viewModel)
    {
        viewModel.StatusText
            .Subscribe(text => _statusText.text = text)
            .AddTo(ref Disposables);
    }
    
    protected override void OnUnbind()
    {
        // Cleanup
    }
}
```

---

## IViewModel

**Путь:** `MVVM/ViewModels/IViewModel.cs`

```csharp
public interface IViewModel : IDisposable
{
}
```

### Назначение
Минимальный контракт ViewModel. Все ViewModel должны быть disposable.

### Базовая реализация: DisposableObject

**Путь:** `MVVM/ViewModels/DisposableObject.cs`

```csharp
public class DisposableObject : IViewModel
{
    private DisposableBag _disposables;
    private CancellationTokenSource _lifetimeCts = new();
    protected CancellationTokenSource _requestCts;
    
    public bool IsDisposed { get; private set; }
    protected CancellationToken LifetimeToken => _lifetimeCts.Token;
    protected ref DisposableBag Disposables => ref _disposables;
    
    protected void SafeCancelAndDisposeRequestCts();
    protected void CancelRequestCts();
    protected CancellationTokenSource CreateNewRequestCts();
    
    protected static Observable<Unit> ObserveEvent(Action<Action> add, Action<Action> remove);
    protected static Observable<T> ObserveEvent<T>(Action<Action<T>> add, Action<Action<T>> remove);
    
    public virtual void Dispose();
}
```

### Пример использования

```csharp
public class ChatViewModel : DisposableObject
{
    private readonly IChatModel _model;
    private readonly IChatService _chatService;
    
    public ReadOnlyReactiveProperty<string> StatusText { get; }
    public ReadOnlyReactiveProperty<bool> IsLoading { get; }
    
    public ChatViewModel(IChatModel model, IChatService chatService)
    {
        _model = model;
        _chatService = chatService;
        
        StatusText = _model.StatusText
            .ToReadOnlyReactiveProperty()
            .AddTo(ref Disposables);
    }
    
    public async UniTask SendMessageAsync(string text)
    {
        var cts = CreateNewRequestCts();
        try
        {
            await _chatService.SendAsync(text, cts.Token);
        }
        catch (OperationCanceledException) { }
    }
}
```

---

## IModel

**Путь:** `MVVM/Models/IModel.cs`

```csharp
public interface IModel : IDisposable
{
    bool IsValid { get; }
    void Reset();
}
```

### Назначение
Базовый контракт для доменных моделей. Хранит состояние с change notifications.

### Доменные модели

| Model | Путь | Назначение |
|-------|------|------------|
| `IChatModel` | `Models/Chat/` | Состояние чата |
| `ILoginModel` | `Models/Login/` | Форма логина |
| `IMainModel` | `Models/Main/` | Главное меню |
| `ISettingsModel` | `Models/Settings/` | Настройки |
| `IImageGenModel` | `Models/ImageGen/` | Генерация изображений |
| `IUIConstructorModel` | `Models/UIConstructor/` | UI конструктор |
| `INetworkSandboxModel` | `Models/NetworkSandbox/` | Sandbox тесты |
| `IStandaloneWorkflowModel` | `Models/StandaloneWorkflow/` | Workflow |
| `IHeroesGridPanelModel` | `Models/HeroesGridPanel/` | Грид героев |
| `IPerformanceTestModel` | `Models/PerformanceTest/` | Тесты производительности |
| `IMenuSelectionModel` | `Models/MenuSelection/` | Выбор меню |

### Пример: IChatModel

```csharp
public interface IChatModel : IModel
{
    event Action<string> StatusTextChanged;
    event Action<string> ResponseTextChanged;
    event Action<bool> IsLoadingChanged;
    event Action<CharacterModel> CharacterChanged;
    event Action<string> CurrentChatIdChanged;
    event Action<UiExtensionsModel> UiExtensionsChanged;
    event Action<List<ChatMessage>> ChatMessagesChanged;

    string StatusText { get; set; }
    string ResponseText { get; set; }
    bool IsLoading { get; set; }
    CharacterModel Character { get; set; }
    string CurrentChatId { get; set; }
    UiExtensionsModel UiExtensions { get; set; }
    List<ChatMessage> ChatMessages { get; set; }

    void AddChatMessage(ChatMessage message);
    void ClearChat();
}
```

---

## IViewService

**Путь:** `MVVM/Services/IViewService.cs`

```csharp
public interface IViewService
{
    IView CurrentView { get; }
    IReadOnlyCollection<IView> ViewHistory { get; }
    IReadOnlyCollection<IView> ActiveViews { get; }
    
    UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null) 
        where TViewModel : DisposableObject;
    
    UniTask<TView> ShowAsync<TView, TViewModel>(bool hideCurrentView = true) 
        where TView : BindableView<TViewModel> 
        where TViewModel : DisposableObject;
    
    UniTask<TView> ShowAsync<TView, TViewModel>(ViewShowParams showParams) 
        where TView : BindableView<TViewModel> 
        where TViewModel : DisposableObject;
    
    UniTask<bool> HideViewAsync<TViewModel>() where TViewModel : DisposableObject;
    UniTask HideAsync<TView>() where TView : IView;
    UniTask HideAsync(IView view);
    
    TViewModel GetViewModel<TViewModel>() where TViewModel : DisposableObject;
    
    UniTask HideCurrentAsync();
    UniTask GoBackAsync();
    UniTask GoToRootAsync();
    UniTask CloseAllNonPersistentAsync();
    
    void ClearHistory();
    void ClearCache();
    
    bool IsViewActive<TView>() where TView : IView;
    TView GetActiveView<TView>() where TView : IView;
    
    event Action<IView> ViewShown;
    event Action<IView> ViewHidden;
}
```

### Назначение
Центральный сервис управления View. Отвечает за:
- Создание/показ View
- Navigation history
- Кэширование View
- Анимации переходов

### Реализация
**Путь:** `Core/Infrastructure/MVVM/Services/ViewService.cs`

### Пример использования

```csharp
public class LoginState : IEnterState
{
    private readonly IViewService _viewService;
    
    public LoginState(IViewService viewService) => _viewService = viewService;
    
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

## IBindableElement

**Путь:** `MVVM/Elements/IBindableElement.cs`

```csharp
public interface IBindableElement : IDisposable
{
    string ElementId { get; }
    bool IsBound { get; }
    void Unbind();
}

public interface IBindableElement<TData> : IBindableElement
    where TData : struct
{
    void Bind(TData data);
    void Refresh(TData data);
}
```

### Расширения

```csharp
public interface IClickableElement<TData> : IBindableElement<TData>
    where TData : struct
{
    Action<TData> OnClicked { set; }
}

public interface ISelectableElement<TData> : IClickableElement<TData>
    where TData : struct
{
    bool IsSelected { get; }
    void SetSelected(bool selected);
}
```

### Базовая реализация

**Путь:** `MVVM/Elements/BindableElement.cs`

```csharp
public abstract class BindableElement<TData> : MonoBehaviour, ISelectableElement<TData>
    where TData : struct, IElementData
{
    [SerializeField] protected Button _clickButton;
    [SerializeField] protected GameObject _selectionIndicator;
    [SerializeField] protected PopupTrigger _popupTrigger;
    
    public string ElementId { get; }
    public bool IsBound { get; }
    public bool IsSelected { get; }
    public Action<TData> OnClicked { set; }
    
    public void Bind(TData data);
    public void Refresh(TData data);
    public void SetSelected(bool selected);
    public void Unbind();
    public void Dispose();
    
    protected abstract void OnBind(TData data);
    protected virtual void OnRefresh(TData oldData, TData newData);
    protected virtual void OnUnbind();
    protected virtual void OnSelectionChanged(bool selected);
    protected virtual void OnDispose();
    protected virtual IPopupData GetPopupData(TData data);
}
```

### Пример использования

```csharp
public struct HeroCardData : IElementData
{
    public string Id { get; set; }
    public string Name;
    public Sprite Avatar;
    public int Level;
}

public class HeroCardElement : BindableElement<HeroCardData>
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private TMP_Text _levelText;
    
    protected override void OnBind(HeroCardData data)
    {
        _nameText.text = data.Name;
        _avatarImage.sprite = data.Avatar;
        _levelText.text = $"Lv.{data.Level}";
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
Фабрика для создания View через Addressables. Используется IViewService внутри.

### Пример использования

```csharp
public class ViewService : IViewService
{
    private readonly IViewFactory _viewFactory;
    
    public async UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null)
    {
        var view = await _viewFactory.CreateAsync(typeof(TViewModel), showParams);
        await view.ShowAsync();
        return view;
    }
    
    public async UniTask HideAsync(IView view)
    {
        await view.HideAsync();
        _viewFactory.Destroy(view);
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
Простая фабрика для инстанциации UI элементов. Может включать pooling.

### Пример

```csharp
public class HeroCardListView : MonoBehaviour
{
    [SerializeField] private Transform _cardContainer;
    [SerializeField] private HeroCardElement _cardPrefab;
    [Inject] private IUIElementFactory _elementFactory;
    
    public void ShowHeroes(List<HeroData> heroes)
    {
        foreach (var hero in heroes)
        {
            var card = _elementFactory.Instantiate(_cardPrefab, _cardContainer);
            card.Bind(hero);
        }
    }
}
```

---

## IUIElementService

**Путь:** `MVVM/Services/IUIElementService.cs`

```csharp
public interface IUIElementService
{
    // Методы для управления UI элементами (если есть)
}
```

### Назначение
Управление lifecycle UI элементов (bind/unbind, pool).

---

## IEmbeddedPanelHost

**Путь:** `MVVM/Hosts/IEmbeddedPanelHost.cs`

```csharp
public interface IEmbeddedPanelHost
{
    // Методы для хостинга embedded панелей
}
```

### Назначение
Интерфейс для View, которые могут хостить embedded панели (например, sidebar панели в главном экране).

### Пример

```csharp
public class MainView : BindableView<MainViewModel>, IEmbeddedPanelHost
{
    [SerializeField] private Transform _sidebarContainer;
    
    public void EmbedPanel<TPanel>(TPanel panel) where TPanel : MonoBehaviour
    {
        panel.transform.SetParent(_sidebarContainer, false);
    }
}
```

---

## IViewConfig

**Путь:** `MVVM/Config/IViewConfig.cs`

```csharp
public interface IViewConfig
{
    string GetViewAddress<TViewModel>() where TViewModel : DisposableObject;
    string GetViewAddress(Type viewModelType);
    AssetReferenceT<GameObject> GetViewReference<TViewModel>() where TViewModel : class;
    AssetReferenceT<GameObject> GetViewReference(Type viewModelType);
}
```

### Назначение
Маппинг ViewModel → View prefab address для Addressables.

### Связанный: IElementConfig

```csharp
public interface IElementConfig
{
    string GetElementAddress<TElement, TData>()
        where TElement : MonoBehaviour
        where TData : struct, IElementData;
    string GetElementAddress(Type elementType);
    AssetReferenceT<GameObject> GetElementReference<TElement, TData>()
        where TElement : MonoBehaviour
        where TData : struct, IElementData;
    AssetReferenceT<GameObject> GetElementReference(Type elementType);
}
```

---

## IViewAnimator

**Путь:** `MVVM/Animation/IViewAnimator.cs`

```csharp
public interface IViewAnimator
{
    UniTask AnimateShowAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null);
    UniTask AnimateHideAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null);
    void StopAnimation(IView view);
}
```

### ViewTransitionStyle

```csharp
[Flags]
public enum ViewTransitionStyle
{
    None = 0,
    Fade = 1 << 0,
    SlideUp = 1 << 1,
    SlideDown = 1 << 2,
    SlideLeft = 1 << 3,
    SlideRight = 1 << 4,
    Scale = 1 << 5,
    Bounce = 1 << 6
}
```

### ViewDisplayMode

```csharp
public enum ViewDisplayMode
{
    Single,          // Одна View, остальные скрыты
    SingleClearAll,  // Одна View, остальные уничтожены
    Additive,        // Добавить поверх
    Replace          // Заменить текущую
}
```

---

## ICanvasProvider

**Путь:** `MVVM/Canvas/ICanvasProvider.cs`

```csharp
public interface ICanvasProvider
{
    Canvas GetCanvas(CanvasLayer layer);
    Transform GetCanvasTransform(CanvasLayer layer);
    void RegisterCanvas(CanvasLayer layer, Canvas canvas);
    void UnregisterCanvas(CanvasLayer layer);
    bool HasCanvas(CanvasLayer layer);
}
```

### CanvasLayer

```csharp
public enum CanvasLayer
{
    World,    // 3D world space UI
    Screen,   // Main game UI
    Overlay,  // Overlays, notifications
    System    // System dialogs, loading
}
```

---

## 🔄 MVVM Flow

```
┌─────────────┐    Bind     ┌─────────────┐    Subscribe    ┌─────────────┐
│    Model    │ ◀────────── │  ViewModel  │ ◀────────────── │    View     │
│  (State)    │             │  (Logic)    │                 │  (Display)  │
└─────────────┘             └─────────────┘                 └─────────────┘
       │                           │                               │
       │                           │                               │
       ▼                           ▼                               ▼
   Events                     Commands                        User Input
   (Changed)                  (Actions)                      (Button click)
```

1. **User** кликает кнопку в **View**
2. **View** вызывает команду в **ViewModel**
3. **ViewModel** обновляет **Model** и/или вызывает сервисы
4. **Model** emits change events
5. **ViewModel** обновляет reactive properties
6. **View** обновляется через R3 subscriptions
