# MVVM Template

Unity 6 project template with **MVVM + CQRS** architecture, reactive bindings, and dependency injection.

Ready-to-use foundation for any Unity project — just add your game logic on top.

## Quick Start

1. Open in **Unity 6000.x**
2. Wait for Package Manager to download dependencies (R3, UniTask, Reflex, etc.)
3. Wait for compilation
4. Run code generators: `Tools → MVVM → Scan & Generate` (all scanners)
5. Open `BootstrapScene` → Press Play

## Stack

| Package | Purpose |
|---------|---------|
| **R3** | Reactive properties, commands, subscriptions |
| **UniTask** | Zero-allocation async/await |
| **Reflex** | Dependency injection |
| **LitMotion** | High-performance tweening |
| **TriInspector** | Custom inspector attributes |
| **Addressables** | Asset loading & management |
| **InputSystem** | New Input System |

## Architecture

```
┌─────────────────────────────────────────────────┐
│                    View (UI)                     │
│         BindableView<TViewModel>                 │
│     Subscribe to properties, bind buttons        │
└──────────────────┬──────────────────────────────┘
                   │ OnBind (R3 Subscribe)
┌──────────────────▼──────────────────────────────┐
│               ViewModel                          │
│     ReactiveProperty, ReactiveCommand            │
│     Sends Commands/Queries via Mediator          │
└──────────┬───────────────────┬──────────────────┘
           │ Events            │ IMediator.Send()
┌──────────▼──────────┐ ┌─────▼──────────────────┐
│       Model         │ │   Command / Query       │
│  Pure C# state      │ │   Handler (CQRS)        │
│  Business logic     │ │   Publishes Notifications│
└─────────────────────┘ └─────────────────────────┘
```

### MVVM

- **Model** — pure C# classes, no Unity dependencies, easy to test
- **ViewModel** — bridge between Model and View, exposes `ReactiveProperty<T>` and `ReactiveCommand<T>`
- **View** — `BindableView<TViewModel>` MonoBehaviour, subscribes to ViewModel in `OnBind()`

### CQRS via Custom Mediator

- **Command** — `IRequest<Nothing>` — fire-and-forget ("do something")
- **Query** — `IRequest<TResult>` — returns data ("give me something")
- **Notification** — `INotification` — pub/sub ("something happened")
- **Handler** — `IRequestHandler<TRequest, TResult>` — executes the work

### State Machine

```
BootstrapState → DataPreloadState → MainMenuState → (your states)
```

Add states by implementing `GameStateBase<T>` + `IEnterState`. Auto-registered via DI.

### Code Generators

| Generator | Menu | What it does |
|-----------|------|-------------|
| **ViewModelScanner** | `Tools → MVVM → Scan & Generate ViewModel Registration` | Auto-registers ViewModels + Models in DI |
| **ViewHandlerScanner** | `Tools → MVVM → Scan & Generate View Handler Registration` | Auto-registers Show/Hide view handlers |
| **MediatorHandlerScanner** | `Tools → Mediator → Scan & Generate` | Auto-registers CQRS handlers |
| **ElementHandlerScanner** | `Tools → MVVM → Scan & Generate Element Handler Registration` | Auto-registers UI element handlers |
| **PrefabBuilderWizard** | `Tools → UI Prefab Builder → Create New Builder...` | Generates Builder classes for new view prefabs |
| **ViewStubGenerator** | `Tools → UI Prefab Builder → Generate View Stub` | Scans a prefab → generates BindableView C# class |

## Project Structure

```
Assets/Source/
├── CodeGen/Views/              # Generated ViewMappings (SO classes)
│   ├── MainMenu/
│   ├── Settings/
│   └── Counter/
├── Editor/
│   ├── ViewModelScanner/       # ViewModel DI registration generator
│   ├── ViewHandlerScanner/     # View handler registration generator
│   ├── Mediator/               # CQRS handler scanner
│   ├── PrefabBuilderSystem/    # UI prefab builder framework
│   │   ├── Base/Elements/      # 25+ element builders (Button, Card, Modal...)
│   │   ├── Base/Templates/     # Reusable UI templates
│   │   ├── Base/Validation/    # Prefab validation system
│   │   └── Base/Wizard/        # New builder wizard
│   ├── DesignSystem/           # Design token applicator
│   └── SceneToolbar/           # Main toolbar scene switcher
└── Runtime/Core/
    ├── Abstractions/           # Interfaces & contracts
    │   ├── MVVM/               # IModel, BindableView, IViewModel
    │   ├── Navigation/         # INavigationTarget
    │   ├── Services/           # IAudioService, ICacheService...
    │   └── StateMachines/      # IGameState, IEnterState
    ├── Application/            # CQRS layer
    │   ├── Commands/           # IncrementCounterCommand, NavigateCommand...
    │   ├── Queries/            # GetCounterQuery, GetAudioStateQuery...
    │   ├── Handlers/           # Command & Query handlers
    │   ├── Notifications/      # CounterChangedNotification...
    │   └── Requests/Views/     # ShowViewRequest, HideViewRequest
    ├── MVVM/
    │   ├── Models/             # CounterModel, SettingsModel, MainMenuModel
    │   ├── ViewModels/         # CounterViewModel, SettingsViewModel...
    │   └── Views/              # CounterView, SettingsView, MainMenuView
    ├── Infrastructure/         # Service implementations
    │   ├── MVVM/               # ViewService, ViewFactory, ViewConfig
    │   ├── Mediator/           # Mediator implementation
    │   └── Services/           # AudioService, CacheService, Logger...
    ├── Presentation/
    │   ├── States/             # BootstrapState, MainMenuState...
    │   └── Handlers/           # NavigationHandlers
    ├── Configs/                # DesignSystem, AudioConfig...
    └── DependencyInjection/
        ├── Bindings/           # Service registration
        ├── Installers/         # ProjectInstaller (entry point)
        └── Generated/          # Auto-generated DI registrations
```

## Included Examples

### MainMenu (MVVM basics)

Demonstrates: View ↔ ViewModel binding, button commands, state machine navigation.

| Layer | File |
|-------|------|
| Model | `MainMenuModel` — visibility state |
| ViewModel | `MainMenuViewModel` — Play, Settings, Quit commands |
| View | `MainMenuView` — 3 buttons bound via `OnClickAsObservable()` |
| State | `MainMenuState` — shows MainMenuView via ViewService |

### Counter (full CQRS cycle)

Demonstrates: Commands, Queries, Notifications, Handlers — the complete CQRS pattern.

| Layer | File | Pattern |
|-------|------|---------|
| Command | `IncrementCounterCommand` | Fire-and-forget with parameter |
| Command | `ResetCounterCommand` | Fire-and-forget, no params |
| Query | `GetCounterQuery` → `CounterState` | Request → Response DTO |
| Notification | `CounterChangedNotification` | Pub/sub broadcast |
| Handler | `IncrementCounterHandler` | Modifies model + publishes notification |
| Handler | `ResetCounterHandler` | Modifies model + publishes notification |
| Handler | `GetCounterHandler` | Returns data, no side effects |
| Model | `CounterModel` | Pure C#, events for ViewModel sync |
| ViewModel | `CounterViewModel` | ReactiveProperty + ReactiveCommand + Mediator |
| View | `CounterView` | TMP_Text + Buttons, conditional interactable |

### Settings (modal view)

Demonstrates: Complex modal with tabs, sliders, toggles, audio integration.

## How to Add a New Feature

### 1. Model

```csharp
// Abstractions/MVVM/Models/MyFeature/IMyFeatureModel.cs
public interface IMyFeatureModel : IDisposable
{
    int Score { get; set; }
    event Action<int> ScoreChanged;
}

// MVVM/Models/MyFeature/MyFeatureModel.cs
public class MyFeatureModel : IMyFeatureModel { /* implement */ }
```

### 2. Commands & Handlers

```csharp
// Application/Commands/MyFeature/UpdateScoreCommand.cs
public readonly struct UpdateScoreCommand : IRequest<Nothing>
{
    public readonly int NewScore;
    public UpdateScoreCommand(int newScore) => NewScore = newScore;
}

// Application/Handlers/MyFeature/UpdateScoreHandler.cs
public sealed class UpdateScoreHandler : IRequestHandler<UpdateScoreCommand, Nothing>
{
    private readonly IMyFeatureModel _model;
    public UpdateScoreHandler(IMyFeatureModel model) => _model = model;

    public UniTask<Nothing> Handle(UpdateScoreCommand cmd, CancellationToken ct)
    {
        _model.Score = cmd.NewScore;
        return UniTask.FromResult(Nothing.Value);
    }
}
```

### 3. ViewModel

```csharp
// MVVM/ViewModels/MyFeature/MyFeatureViewModel.cs
[GenerateViewCommands]
public class MyFeatureViewModel : DisposableObject
{
    public ReactiveProperty<int> Score { get; }
    public ReactiveCommand<Unit> UpdateCommand { get; set; }

    public MyFeatureViewModel(IMyFeatureModel model, IMediator mediator) { /* wire up */ }
}
```

### 4. View

```csharp
// MVVM/Views/MyFeature/MyFeatureView.cs
public class MyFeatureView : BindableView<MyFeatureViewModel>
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private Button _updateButton;

    protected override void OnBind(ref DisposableBag disposables)
    {
        ViewModel.Score
            .Subscribe(_scoreText, static (v, t) => t.text = $"Score: {v}")
            .AddTo(ref disposables);

        _updateButton.OnClickAsObservable()
            .Subscribe(ViewModel.UpdateCommand, static (_, cmd) => cmd.Execute(Unit.Default))
            .AddTo(ref disposables);
    }
}
```

### 5. ViewMapping

```csharp
// CodeGen/Views/MyFeature/MyFeatureViewMapping.cs
public class MyFeatureViewMapping : ViewMappingBase<MyFeatureView, MyFeatureViewModel> { }
```

### 6. Register

Run all code generators: `Tools → MVVM → Scan & Generate` — done.

## Services

| Service | Interface | Description |
|---------|-----------|-------------|
| Audio | `IAudioService` | AudioMixer integration, volume, mute |
| Assets | `IAssetProvider` | Addressables loading wrapper |
| Cache | `ICacheService` | Disk cache with TTL |
| Config | `IConfigProvider` | ScriptableObject config loading |
| Clipboard | `IClipboardService` | Copy/paste |
| File Dialog | `IFileDialogService` | Native file picker |
| File System | `IFileSystemService` | File I/O |
| Input | `IInputService` | InputSystem wrapper |
| Logger | `ILogger` | Color-coded logging with formatting |
| Prefs | `IPrefsService` | PlayerPrefs wrapper |
| View | `IViewService` | Show/Hide views (MVVM) |
| Popup | `IPopupService` | Tooltip/popup system |
| Embedded Panel | `IEmbeddedPanelHost` | Inline panel views |

## Editor Tools

| Tool | Menu | Description |
|------|------|-------------|
| Scene Toolbar | Main toolbar (auto) | Scene switcher + additive scene loader |
| Design System | `Tools → Design System → Apply` | Apply colors/typography to prefabs |
| Prefab Builder | `Tools → UI Prefab Builder` | Visual prefab construction system |
| View Stub Gen | `Tools → UI Prefab Builder → Generate View Stub` | Prefab → C# BindableView code |

## Popup System

Built-in popup/tooltip system with pooling, animations, and auto-positioning.

### Quick Tooltip (no code)

Add `PopupTrigger` component to any UI element:

1. Create tooltip prefab via `Tools → UI Prefab Builder → Build Tooltip Popup`
2. Add `PopupTrigger` component to your button/element
3. Drag tooltip prefab into `Popup Prefab` field
4. Set trigger type: Hover (tooltip) or Click (popover)

### Code Tooltip

```csharp
// In your View's OnBind:
var trigger = _myButton.gameObject.AddComponent<PopupTrigger>();
trigger.SetTooltipData("Button Title", "Description text here");
```

### Custom Popup Data

```csharp
// 1. Define data class
public class ItemTooltipData : IPopupData
{
    public string Name { get; set; }
    public int Damage { get; set; }
    public string Rarity { get; set; }
}

// 2. Create popup that uses it
public class ItemTooltipPopup : PopupBase<ItemTooltipData>
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _damageText;

    protected override void OnDataSet(ItemTooltipData data)
    {
        _nameText.text = data.Name;
        _damageText.text = $"Damage: {data.Damage}";
    }
}

// 3. Show via service
await _popupService.ShowAsync<ItemTooltipPopup, ItemTooltipData>(
    targetRect,
    new ItemTooltipData { Name = "Sword", Damage = 42 });
```

### Popup Features

- **Auto-positioning** — tooltips avoid screen edges
- **Pooling** — popups are pooled for zero-allocation show/hide
- **Animations** — fade + scale via LitMotion (customizable per popup)
- **Stacking** — multiple popups, HideAll/HideTop support
- **Events** — `PopupShown` / `PopupHidden` for tracking

## Troubleshooting

**"Missing assembly reference"** → Wait for Package Manager to finish downloading (bottom-right progress bar).

**"Handler not found" at runtime** → Run `Tools → Mediator → Scan & Generate`.

**"ViewModel not registered"** → Run `Tools → MVVM → Scan & Generate ViewModel Registration`.

**"ViewConfig not found"** → Check `00. Configs/` folder, assign ViewConfig SO to the config provider.

**"Scene not in build"** → `File → Build Settings → Add Open Scenes`.

## License

MIT — use freely in personal and commercial projects.

---

**Unity:** 6000.x | **Version:** 1.0.0
