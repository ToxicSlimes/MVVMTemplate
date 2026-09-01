# MVVM Template

Unity 6 project template with **MVVM + CQRS** architecture, reactive bindings, and dependency injection.

Ready-to-use foundation for any Unity project — just add your game logic on top.

## Quick Start

1. Open in **Unity 6000.x**
2. Wait for Package Manager to download dependencies (R3, UniTask, Reflex, etc.)
3. Wait for compilation (codegen runs automatically on script reload)
4. Run `Tools → MVVM → Scan & Generate All` if generated files are missing
5. Run `Tools → MVVM → Rebuild Main Menu Prefab` after pulling template updates
6. Optional: `Tools → MVVM → Validate Template Setup` — checks scenes, configs, codegen
7. Open `BootstrapScene` → Press Play

The main menu is a **demo hub** — open Counter, Settings, Inventory, or Multiplayer without writing code first.

## Use as a Git submodule

This repo is a full Unity project. Downstream games pin **this repository** to a commit and import `Assets/Source` as a UPM package so scripts compile under `Packages/`.

In the host Unity project:

```bash
git submodule add https://github.com/ToxicSlimes/MVVMTemplate.git Packages/MVVMTemplate
```

The host records the submodule SHA. That is the pinned commit.

Add a local file dependency in `Packages/manifest.json` (copy the git package URLs from this repo's `Packages/manifest.json` as well — R3, UniTask, Reflex, LitMotion, TriInspector, Fusion):

```json
{
  "dependencies": {
    "com.toxicslimes.mvvm-template": "file:MVVMTemplate/Assets/Source"
  }
}
```

Update to a newer commit:

```bash
git fetch origin
git -C Packages/MVVMTemplate checkout <commit-sha>
git add Packages/MVVMTemplate
git commit -m "Bump MVVM template"
```

Or track the submodule remote branch:

```bash
git submodule update --remote Packages/MVVMTemplate
git add Packages/MVVMTemplate
git commit -m "Bump MVVM template"
```

After clone of a host that already has the submodule:

```bash
git submodule update --init --recursive
```

Do not place this repo under the host `Assets/` folder. Unity would import a nested project (`ProjectSettings`, Photon, TMP samples). `Packages/MVVMTemplate` plus the `file:` entry keeps one compilation of `Assets/Source`.

Prefabs, Addressables, and scenes stay in this project. Copy or Addressable-link what a game needs, or keep developing by opening this repo as a Unity project.

Without a submodule, Package Manager can pin the same commit via git URL:

```json
"com.toxicslimes.mvvm-template": "https://github.com/ToxicSlimes/MVVMTemplate.git?path=Assets/Source#<commit-sha>"
```

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
| **Photon Fusion 2** | Multiplayer (isolated behind `Core.Network.Abstractions`) |

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
    ├── AI/                     # UtilityAI (UtilitySelector, UtilityAgentProfile)
    ├── Application/            # CQRS layer
    │   ├── Commands/           # IncrementCounterCommand, NavigateCommand...
    │   ├── Queries/            # GetCounterQuery, GetAudioStateQuery...
    │   ├── Handlers/           # Command & Query handlers
    │   ├── Notifications/      # CounterChangedNotification...
    │   └── Requests/Views/     # ShowViewRequest, HideViewRequest
    ├── MVVM/
    │   ├── Models/             # CounterModel, SettingsModel, MainMenuModel, LobbyModel
    │   ├── ViewModels/         # CounterViewModel, MatchmakingViewModel...
    │   └── Views/              # CounterView, MatchmakingView, LobbyRoomView...
    ├── Network/                # Fusion ports + FusionAdapter
    ├── Infrastructure/         # Service implementations
    │   ├── MVVM/               # ViewService, ViewFactory, ViewConfig
    │   ├── Mediator/           # Mediator implementation
    │   └── Services/           # AudioService, CacheService, Logger...
    ├── Presentation/
    │   ├── States/             # BootstrapState, MainMenuState...
    │   └── Handlers/           # NavigationHandlers
    ├── Configs/                # DesignSystem, AudioConfig, NetworkConfig...
    └── DependencyInjection/
        ├── Bindings/           # Service registration
        ├── Installers/         # ProjectInstaller (entry point)
        └── Generated/          # Auto-generated DI registrations
```

## Included Examples

### MainMenu (demo hub)

Entry point after bootstrap. Each button opens a built-in example on top of the menu (view stack — Back returns to the hub).

| Button | Example | Pattern |
|--------|---------|---------|
| Counter | CQRS commands, queries, notifications, element gallery | `ShowViewCommand<TViewModel>` |
| Settings | Modal with tabs, audio sliders, PlayerPrefs | `ShowViewRequest<TView, TViewModel>` |
| Inventory | Filterable list, selection, drag-reorder | `ShowViewRequest` + `NavigateBackCommand` |
| Multiplayer | Photon Fusion matchmaking flow | `ShowViewRequest` |
| Quit | Exit play mode / application | — |

| Layer | File |
|-------|------|
| Model | `MainMenuModel` — visibility state |
| ViewModel | `MainMenuViewModel` — routes to example views |
| View | `MainMenuView` — demo hub buttons |
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
| Popup | `ConfirmPopup` | Reset shows "Are you sure?" confirmation dialog |

### Settings (modal view)

Demonstrates: Complex modal with tabs, sliders, toggles, audio integration.

### Matchmaking (network template)

Photon Fusion 2 is isolated behind `Core.Network.Abstractions`. The Fusion adapter is the only assembly that references Fusion types.

Play on the Main Menu opens Matchmaking. Create or join a room, ready up, add lobby bots, start a match. The session FSM goes Lobby → Countdown → Playing → Ended. Playing loads `GameplayScene` (empty arena + spawn points) and does not spawn player bodies or apply win rules — a game calls `GameSessionMachine.FinishMatch` and registers its own spawn handlers plus `FusionInputSource.Register`.

| Layer | File |
|-------|------|
| Ports | `INetworkService`, `INetworkRunnerController`, `INetworkSpawner`, `INetworkLobby` |
| Adapter | `FusionRunnerController`, `FusionSpawner`, `FusionMessenger`, `FusionInputSource` |
| Session | `GameSessionMachine`, `NetworkPlayer`, `BotRoster` |
| UI | Matchmaking, LobbyRoom, ConnectingOverlay, Pause |

Rebuild network prefabs with `Tools → Network → Build Network Prefabs`. Photon FusionMenu is unused.

### Utility AI

`Core.AI` is a game-agnostic utility selector. A game supplies its own goal enum, context snapshot, and scorer; the template does not ship sample goals.

| Type | Role |
|------|------|
| `UtilityResponse` | `weight × AnimationCurve` consideration |
| `UtilitySelector.Choose<TGoal>` | score goals, apply inertia, pick the max |
| `UtilityAgentProfile` | ScriptableObject base: decision period + inertia |

Inherit `UtilityAgentProfile`, add per-goal `UtilityResponse` fields, score a context struct, then call `UtilitySelector.Choose`. FusionAdapter does not reference `Core.AI`.

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
| **New Feature** | `Tools → MVVM → New Feature...` | Wizard: Model + ViewModel + View + ViewMapping in one step |
| Codegen (all) | `Tools → MVVM → Scan & Generate All` | Regenerates ViewModel, ViewHandler, Mediator, Element, StorageMapper DI |
| Template check | `Tools → MVVM → Validate Template Setup` | Verifies BootstrapScene, ViewConfig, generated files |
| Rebuild main menu | `Tools → MVVM → Rebuild Main Menu Prefab` | Rebuilds demo hub prefab from builder |
| Scene Toolbar | Main toolbar (auto) | Scene switcher + additive scene loader |
| Design System | `Tools → Design System → Apply` | Apply colors/typography to prefabs |
| Prefab Builder | `Tools → UI Prefab Builder` | Visual prefab construction system |
| View Stub Gen | `Tools → UI Prefab Builder → Generate View Stub` | Prefab → C# BindableView code |
| Network Prefabs | `Tools → Network → Build Network Prefabs` | Session, player, empty avatar NetworkObjects |
| Gameplay Scene | `Tools → Network → Build Gameplay Scene` | Empty networked arena + spawn points |

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

## CI / compile without opening the Editor

Local (batchmode, no Editor window):

```powershell
.\Tools\ci-compile.ps1
```

```bash
chmod +x ./Tools/ci-compile.sh && ./Tools/ci-compile.sh
```

GitHub Actions (`.github/workflows/compile.yml`) runs the same check on push/PR. Add repository secrets:

- `UNITY_LICENSE` — Unity activation file content
- `UNITY_EMAIL` — Unity account email
- `UNITY_PASSWORD` — Unity account password

Unity C# projects reference Editor assemblies in generated `.csproj` files, so `dotnet build MVVMTemplate.sln` only works on a machine where Unity has already generated those projects with a valid local Editor path. Use the batchmode scripts above for a reliable headless compile check.

## License

MIT — use freely in personal and commercial projects.

---

**Unity:** 6000.x | **Version:** 1.0.0
