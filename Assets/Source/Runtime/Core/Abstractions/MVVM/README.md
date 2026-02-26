# MVVM Abstractions

Контракты и базовые типы для MVVM архитектуры.

## Структура

```
MVVM/
├── Views/           # IView, BindableView<T>, ViewShowParams
├── ViewModels/      # IViewModel, DisposableObject
├── Models/          # IModel + feature models
├── Services/        # IViewService
├── Factory/         # IViewFactory
├── Animation/       # IViewAnimator
├── Canvas/          # ICanvasProvider, CanvasLayer
└── Config/          # IViewConfig
```

## Core Interfaces

| Interface | Purpose |
|-----------|---------|
| `IView` | Base view (`IsPersistent`, `IDisposable`) |
| `IViewModel` | Base viewmodel (`IDisposable`) |
| `IModel` | Base model (`IsValid`, `Reset()`, `IDisposable`) |
| `IViewService` | Navigation, history, show/hide |
| `IViewFactory` | View instantiation via Addressables |
| `IUIElementFactory` | UI element/prefab instantiation |
| `IViewAnimator` | View transition animations |
| `ICanvasProvider` | Canvas layer management |
| `IViewConfig` | View configuration |

## Base Classes

| Class | Purpose |
|-------|---------|
| `BindableView<T>` | Abstract base для Views с reactive binding (BindTo, ObserveViewModel, Show/Hide lifecycle) |
| `DisposableObject` | Base class для ViewModels с R3 DisposableBag + CancellationToken (virtual Dispose, CreateNewRequestCts) |

## Helper Types

| Type | Purpose |
|------|---------|
| `ViewShowParams` | Fluent builder для параметров показа View |
| `ViewTransitionStyle` | Flags enum: None, Fade, SlideUp, SlideDown, SlideLeft, SlideRight, Scale |
| `ViewDisplayMode` | Enum: Single, SingleClearAll, Additive, Replace |
| `CanvasLayer` | Enum: World, Screen, Overlay, System |

## View Lifecycle Interfaces

| Interface | Purpose |
|-----------|---------|
| `IShowable` | `UniTask ShowAsync()` |
| `IHideable` | `UniTask HideAsync()` |
| `IUnbindable` | `void Unbind()` |

## Feature Models

| Interface | Feature |
|-----------|---------|
| `IChatModel` | Chat screen |
| `IHeroesGridPanelModel` | Heroes grid panel (MyHeroes, ShopHeroes, DeadHeroes, tabs, selection) |
| `ILoginModel` | Login screen |
| `IMainModel` | Main screen (Heroes, Stats, Resources) |
| `ISettingsModel` | Settings screen |
| `IImageGenModel` | Image generation |
| `IUIConstructorModel` | UI constructor |
| `IStandaloneWorkflowModel` | Workflow editor |
| `INetworkSandboxModel` | Network testing |
| `IMenuSelectionModel` | Menu selection |
| `IPerformanceTestModel` | Performance testing |

## API Reference

### IUIElementFactory

```csharp
public interface IUIElementFactory
{
    GameObject Instantiate(GameObject prefab, Transform parent);
    T Instantiate<T>(T prefab, Transform parent) where T : Component;
}
```

### IViewFactory

```csharp
public interface IViewFactory
{
    UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null)
        where TView : BindableView<TViewModel>
        where TViewModel : DisposableObject;
    
    UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null)
        where TView : BindableView<TViewModel>
        where TViewModel : DisposableObject;
    
    UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams)
        where TView : BindableView<TViewModel>
        where TViewModel : DisposableObject;
    
    void Destroy<TView>(TView view) where TView : IView;
}
```

### IViewService

```csharp
public interface IViewService
{
    IView CurrentView { get; }
    IReadOnlyCollection<IView> ViewHistory { get; }
    IReadOnlyCollection<IView> ActiveViews { get; }
    
    // Show by ViewModel type only (address resolved from config)
    UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null) where TViewModel : DisposableObject;
    
    // Show with explicit View and ViewModel types
    UniTask<TView> ShowAsync<TView, TViewModel>(bool hideCurrentView = true);
    UniTask<TView> ShowAsync<TView, TViewModel>(ViewShowParams showParams);
    
    // Hide
    UniTask<bool> HideViewAsync<TViewModel>() where TViewModel : DisposableObject;
    UniTask HideAsync<TView>() where TView : IView;
    UniTask HideAsync(IView view);
    UniTask HideCurrentAsync();
    
    // Get ViewModel from active view
    TViewModel GetViewModel<TViewModel>() where TViewModel : DisposableObject;
    
    // Navigation
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

## Usage

```csharp
// Show view
await _viewService.ShowAsync<ChatView, ChatViewModel>();

// Show with params
await _viewService.ShowAsync<SettingsView, SettingsViewModel>(
    new ViewShowParams()
        .SetDisplayMode(ViewDisplayMode.Additive)
        .SetTransition(ViewTransitionStyle.SlideUp)
        .SetCanvas(CanvasLayer.Overlay)
);
```

## Implementation

Реализации находятся в `Infrastructure/MVVM/`.
