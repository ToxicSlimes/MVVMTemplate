# EndlessAdventureTemplate

Unity 6000.2.7f2 base project template with MVVM + CQRS architecture extracted from EndlessAdventure.

## 🚀 Quick Start

1. Open project in Unity 6000.2.7f2
2. Unity Package Manager auto-downloads dependencies (R3, UniTask, Reflex DI, etc.)
3. Wait for compilation
4. Run code generators: `Tools → MVVM → ViewModelScanner`, `Tools → Mediator → Scan & Generate`
5. Build your game on top of this base

## 📦 Stack

- **Unity:** 6000.2.7f2
- **Reactive:** R3
- **Async:** UniTask
- **DI:** Reflex 13.0.3
- **Animation:** LitMotion
- **Inspector:** TriInspector
- **Assets:** Addressables
- **Input:** InputSystem

## 🏗️ Architecture

### MVVM + CQRS
- **Model** — domain logic
- **View** — UI prefabs (BindableView base)
- **ViewModel** — presentation logic + reactive bindings
- **Commands/Queries/Handlers** — CQRS via custom Mediator

### Custom Mediator
Lightweight CQRS implementation:
- `IRequest<TResponse>` — commands/queries
- `INotification` — events
- `IRequestHandler` / `INotificationHandler`
- Pipeline support (pre/post processors)
- Auto-registration via code generator

### State Machine
- `GameStateBase` — base state
- `BootstrapState` → `DataPreloadState` → `LoadSceneState`
- Add your game states (BattleState, MenuState, etc.)

### Code Generators
- **ViewMappingGenerator** — auto-generates ViewConfig mappings
- **ViewModelScanner** — auto-registers ViewModels in DI
- **MediatorHandlerScanner** — auto-registers CQRS handlers
- **PrefabBuilderSystem** — UI prefab builder framework

## 📂 Structure

```
Assets/Source/
├── 00. Configs/          # ScriptableObject configs
├── Editor/               # Code generators & tools
│   ├── CodeGenPipeline
│   ├── ViewMappingGenerator
│   ├── ViewModelScanner
│   ├── Mediator
│   └── PrefabBuilderSystem/Base  # UI builder framework
└── Runtime/Core/
    ├── Abstractions/     # Interfaces
    ├── Infrastructure/   # Implementations
    ├── MVVM/             # Views/ViewModels/Models
    ├── Application/      # Commands/Handlers/Queries
    ├── Data/             # Domain models, DTOs
    ├── Configs/          # Config definitions
    ├── Presentation/     # States, Navigation
    └── DependencyInjection/
        ├── Bindings/     # Service bindings
        ├── Installers/   # DI installers
        └── Generated/    # Auto-generated registrations
```

## 🔧 Base Services

### Platform Services
- `IClipboardService` — clipboard access
- `IFileDialogService` — file picker
- `IFileSystemService` — file I/O
- `IDialogService` — native dialogs
- `IResourceService` — Resources folder access

### Core Services
- `IAudioService` — audio playback (AudioMixer integration)
- `IAssetProvider` — Addressables wrapper
- `ICacheService` — disk cache with TTL
- `IConfigProvider` — config loading
- `IInputService` — InputSystem wrapper
- `ILoggerService` — colored logging
- `IPrefsService` — PlayerPrefs wrapper
- `IViewService` — MVVM view management
- `IPopupService` — tooltip/popup system

## 📝 Example Feature: Settings

Included as a pattern reference:

**Model:** `SettingsModel` (audio state)  
**ViewModel:** `SettingsViewModel` (bindings + commands)  
**View:** `SettingsView` (prefab reference)  
**Commands:** `SetAudioVolumeCommand`, `SetAudioMuteCommand`, `ResetAudioCommand`  
**Handlers:** `SetAudioVolumeHandler`, `GetAudioStateHandler`, etc.  
**Queries:** `GetAudioStateQuery`

## 🚦 How to Add a New Feature

### 1. Create View Prefab
Use `PrefabBuilderSystem` or manually:
- Extend `BindableView<TViewModel>`
- Add UI elements
- Wire interactions

### 2. Create ViewModel
```csharp
public class MyFeatureViewModel : DisposableObject, IViewModel
{
    private readonly ReactiveProperty<string> _title = new();
    public ReadOnlyReactiveProperty<string> Title => _title;

    public void Initialize() { /* ... */ }
}
```

### 3. Create Model (optional)
```csharp
public interface IMyFeatureModel : IModel { /* ... */ }
public class MyFeatureModel : IMyFeatureModel { /* ... */ }
```

### 4. Create Commands/Queries
```csharp
public record DoSomethingCommand(string Param) : IRequest;
public class DoSomethingHandler : IRequestHandler<DoSomethingCommand>
{
    public async UniTask Handle(DoSomethingCommand request, CancellationToken ct)
    {
        // ...
    }
}
```

### 5. Wire in DI
Run `Tools → Mediator → Scan & Generate` — handlers auto-register.  
Run `Tools → MVVM → ViewModelScanner` — ViewModels auto-register.

### 6. Navigation
Create handler:
```csharp
public class MyViewNavigationHandler : NavigateHandlerBase<MyView, MyViewModel> { }
```

Usage:
```csharp
await _mediator.Send(new NavigateCommand<MyView>());
```

## 🧩 Removed from Source Project

These features were **NOT included** in the template (game-specific):

**Services:**
- `INetworkService` / `IApiClient` — HTTP transport
- `IChatService` / `IMessageService` — chat features
- `IHeroService` / `ICharacterService` — hero management
- `IWorkflowService` — workflow engine
- `IImageGenService` / `IUIGenerationService` — AI features
- `IProceduralUIService` — procedural UI generation
- `IGameDataService` — game data domain

**Views/ViewModels:**
- MainView, BattleView, HeroesGridPanel, etc.
- Chat, Login, NetworkSandbox, ImageGen, etc.

**Domain Models:**
- Hero, Character, Ability, Equipment, etc.

**Reason:** Keep the template minimal and domain-agnostic. Add your own game features on top.

## ⚙️ Generated Files

These files are **empty stubs** and regenerate when you run code generators:

- `Generated/MediatorHandlerRegistration.generated.cs`
- `Generated/ViewModelRegistration.generated.cs`
- `Generated/ViewHandlerRegistration.generated.cs`
- `Generated/ElementHandlerRegistration.generated.cs`
- `Generated/StorageMapperRegistration.generated.cs`

**When to regenerate:**
- After adding new ViewModels → `ViewModelScanner`
- After adding new CQRS handlers → `Mediator/Scan & Generate`
- After adding new UI Elements → `ElementHandlerScanner`

## 🧪 Testing

No tests included. Add your own using Unity Test Framework:
- `[TestFixture]` for unit tests
- `[UnityTest]` for integration tests

Recommended: Test your ViewModels, Commands/Handlers, and business logic separately from Views.

## 📖 Documentation

See `Assets/Source/Runtime/Core/*/docs/` for detailed documentation on each module:

- `Abstractions/docs/` — interfaces & contracts
- `Infrastructure/docs/` — implementations
- `MVVM/docs/` — MVVM patterns
- `Mediator/docs/` — CQRS patterns
- `Presentation/docs/` — state machine & navigation

## 🔗 Unity Package Manager Dependencies

All dependencies auto-download on first project open (internet required):

```json
{
  "com.annulusgames.lit-motion": "github:AnnulusGames/LitMotion",
  "com.codewriter.triinspector": "github:codewriter-packages/Tri-Inspector",
  "com.cysharp.r3": "github:Cysharp/R3",
  "com.cysharp.unitask": "github:Cysharp/UniTask",
  "com.gustavopsantos.reflex": "github:gustavopsantos/reflex#13.0.3",
  "com.unity.addressables": "2.7.6",
  "com.unity.inputsystem": "1.12.1"
}
```

## 🐛 Troubleshooting

### "Missing assembly reference" errors
→ Wait for Unity Package Manager to finish downloading dependencies (check bottom-right progress bar).

### "Handler not found" exceptions
→ Run `Tools → Mediator → Scan & Generate` to regenerate handler registrations.

### "ViewModel not registered" exceptions
→ Run `Tools → MVVM → ViewModelScanner`.

### "Config not found" warnings
→ Check `00. Configs/` folder — configs may need to be assigned in Editor.

## 📜 License

Same as source project EndlessAdventure (all rights reserved by author).

## 🙏 Credits

Extracted from **EndlessAdventure** Unity project.  
Architecture inspired by MediatR, MVVM, CQRS patterns.

---

**Version:** 1.0.0  
**Unity:** 6000.2.7f2  
**Created:** 2026-02-27
