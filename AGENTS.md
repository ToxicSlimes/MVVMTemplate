## Learned User Preferences

- Keep this repo as an architectural Unity template: strip concrete game implementations (Abduction, grandpa, pets, saucer, house) and retain reusable MVVM, network, and UtilityAI patterns only.
- Delete game-specific docs outright when asked; do not relocate them to another folder.
- Implement requested work directly when the user says to proceed; fix reported compile errors before moving on.
- When cleaning the branch, prefer architecture-only samples (network + UtilityAI framework) over embedded game demos.
- Prefer runtime UI Toolkit (UXML/USS) for new UI; keep layouts text-based for AI/mockup-driven workflows.

## Learned Workspace Facts

- Unity 6 MVVM + CQRS template (`MVVMTemplate`); entry scene is `Assets/Source/Assets/Scenes/BootstrapScene.unity`.
- `Core.AI` provides the UtilityAI framework (`UtilityResponse`, `UtilitySelector`, `UtilityAgentProfile`); `Core.Network.FusionAdapter` does not reference `Core.AI`.
- Network template keeps Fusion lobby/matchmaking and session FSM (Lobby → Countdown → Playing → Ended); matches do not auto-end and avatars are not auto-spawned—games call `FinishMatch` and register spawn handlers.
- Main menu is a demo hub (Counter, Settings, Inventory, Multiplayer) using a view stack; modal demo views close via `NavigateBackCommand` (not `HideViewRequest`) so `GoBackAsync` restores the hub from history.
- Editor workflow lives under `Tools/MVVM/`: Scan & Generate All, New Feature..., Validate Template Setup, Rebuild Main Menu Prefab; `CodeGenPipeline` class shares namespace `Core.Editor.CodeGenPipeline`—alias as `CodeGenRunner = Core.Editor.CodeGenPipeline.CodeGenPipeline` to avoid CS0234.
- UPM package is `Assets/Source/package.json` (`com.toxicslimes.mvvm-template`); host projects submodule the repo under `Packages/MVVMTemplate` with `"com.toxicslimes.mvvm-template": "file:MVVMTemplate/Assets/Source"`—never add a root-level `package.json`.
- Headless compile check uses `Tools/ci-compile.ps1` / `Tools/ci-compile.sh` with `Core.Editor.CI.UnityCompileCheck.Run`; `dotnet build MVVMTemplate.sln` is unreliable without Unity-generated Editor assembly references.
- `Core.UI.Toolkit` (`ToolkitBindableView`, `ToolkitPanelView`, element reconcile) is the runtime UI Toolkit layer; all demo views (MainMenu, Counter, Settings, Inventory, Matchmaking, LobbyRoom, Pause, ConnectingOverlay) use toolkit hosts + UXML/USS under `Assets/Source/UI/Toolkit/`; call `EnsureVisualTreeReady()` before bind/query (`UIDocument` builds the tree only when active); UXML `<Style src>` must use relative paths (e.g. `CounterScreen.uss`, `../Theme/design-tokens.uss`)—`project://database/...` breaks USS at runtime; toolkit element `ScriptableObject` mapping types require one class per file matching the filename.
- `Tools/UI Toolkit/`: Setup All Toolkit Views, Validate Toolkit Bindings, per-view setup/edit-scene menus; `Tools/MVVM/Setup Audio Assets` wires `MainAudioMixer.mixer` and `AudioSource` prefab (`NullAudioService` fallback if `AudioConfig` assets are missing); `Validate Template Setup` also runs toolkit binding and `AudioConfig` checks.
- `ILocalizationService.Localize(table, key)` wraps Unity Localization (Smart Strings via `arguments`); pilot UI keys in `UiLocalizationKeys`; run `Tools/MVVM/Setup Localization` for locales, `UI` string table, and `LocalizationConfig` (Main Menu + Settings bound in v1).
- Pixel-art UI theme: `Assets/Source/UI/Toolkit/Theme/pixel-theme.uss` + `PressStart2P` font; chunky borders, retro palette inspired by pxlkit aesthetics; include `pixel-theme.uss` after `design-tokens.uss` in UXML.
- Screen Spec pipeline: specs are layout source of truth (hand-written view logic stays in C#); `Packages/ui-design/spec/*.screen.json` → `cd Packages/ui-design/npm && npm run export` → `Assets/Source/UI/Toolkit/_specs/` → `Tools/UI Toolkit/Import All Screen Specs` generates UXML + `*.Bindings.generated.cs` partials; `Packages/ui-design/package.json` is Unity UPM only (`type: tool`)—npm deps/scripts live in `Packages/ui-design/npm/`; see `docs/ui-screen-spec.md`.
