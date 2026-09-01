## Learned User Preferences

- Keep this repo as an architectural Unity template: strip concrete game implementations (Abduction, grandpa, pets, saucer, house) and retain reusable MVVM, network, and UtilityAI patterns only.
- Delete game-specific docs outright when asked; do not relocate them to another folder.
- Implement requested work directly when the user says to proceed; fix reported compile errors before moving on.
- When cleaning the branch, prefer architecture-only samples (network + UtilityAI framework) over embedded game demos.

## Learned Workspace Facts

- Unity 6 MVVM + CQRS template (`MVVMTemplate`); entry scene is `Assets/Source/Assets/Scenes/BootstrapScene.unity`.
- `Core.AI` provides the UtilityAI framework (`UtilityResponse`, `UtilitySelector`, `UtilityAgentProfile`); `Core.Network.FusionAdapter` does not reference `Core.AI`.
- Network template keeps Fusion lobby/matchmaking and session FSM (Lobby → Countdown → Playing → Ended); matches do not auto-end and avatars are not auto-spawned—games call `FinishMatch` and register spawn handlers.
- Main menu is a demo hub (Counter, Settings, Inventory, Multiplayer) using a view stack; Back returns to the hub.
- Editor workflow lives under `Tools/MVVM/`: Scan & Generate All, New Feature..., Validate Template Setup, Rebuild Main Menu Prefab.
- `CodeGenPipeline` class shares its namespace `Core.Editor.CodeGenPipeline`; other editor code should alias it as `CodeGenRunner = Core.Editor.CodeGenPipeline.CodeGenPipeline` to avoid CS0234.
- UPM package is `Assets/Source/package.json` (`com.toxicslimes.mvvm-template`); host projects submodule the repo under `Packages/MVVMTemplate` with `"com.toxicslimes.mvvm-template": "file:MVVMTemplate/Assets/Source"`—never add a root-level `package.json`.
- Headless compile check uses `Tools/ci-compile.ps1` / `Tools/ci-compile.sh` with `Core.Editor.CI.UnityCompileCheck.Run`; `dotnet build MVVMTemplate.sln` is unreliable without Unity-generated Editor assembly references.
