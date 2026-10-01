# Screen Spec Pipeline

Screen specs are the **layout source of truth** between design (React/pxlkit reference) and Unity UI Toolkit. ViewModels and hand-written view logic stay in C#.

## Flow

```
Packages/ui-design/spec/*.screen.json
        │  npm run validate && npm run export
        ▼
Assets/Source/UI/Toolkit/_specs/*.screen.json
        │  Tools → UI Toolkit → Import All Screen Specs
        ▼
MainMenuScreen.uxml / SettingsScreen.uxml  (+ *.Bindings.generated.cs)
        │  Tools → UI Toolkit → Validate Toolkit Bindings
        ▼
Play BootstrapScene
```

## Directories

| Path | Role |
|------|------|
| `Packages/ui-design/spec/` | Canonical JSON specs + JSON Schema |
| `Packages/ui-design/screens/` | React reference layouts (pxlkit-style) |
| `Assets/Source/UI/Toolkit/_specs/` | Unity copy of validated specs |
| `Assets/Source/Editor/UIToolkit/ScreenSpec/` | Editor importer + generators |
| `Assets/Source/Runtime/Core/Presentation/MVVM/Views/*/*.Bindings.generated.cs` | Generated localization bind stubs |

## Spec file structure

Top-level fields:

- `schemaVersion` — must be `1`
- `screenId` — kebab-case id (`main-menu`, `settings`)
- `viewType` / `viewModelType` / `viewNamespace` — target MVVM types
- `output` — `folder`, `uxmlFile`, `ussFile`, optional `includePixelControls`, `includeToolkitScreen`
- `theme.styles` — relative USS filenames (resolved under `Theme/`)
- `localization` — table name, prefix, optional `keys[]` for documentation
- `nodes` — layout tree (v1 types below)
- `bindings` — C# binding hints (commands, properties, localization)

### Node types (v1)

| `type` | Unity element |
|--------|----------------|
| `Container` | `VisualElement` |
| `Label` | `Label` |
| `Button` | `Button` (`tone` → pixel button USS) |
| `SliderRow` | row + `Slider` + value `Label` |
| `Toggle` | `Toggle` |
| `Dropdown` | `DropdownField` |
| `Tabs` | tab buttons + `ScrollView` tab panels |
| `ScrollView` | `ScrollView` |

### Binding kinds (v1)

| `kind` | Purpose |
|--------|---------|
| `command` | `Bind(button, ViewModel.XCommand)` |
| `property` | two-way slider/toggle (manual in view for smart formats) |
| `enum` | dropdown enum bind (manual in view) |
| `localized` | `BindLocalized` / `BindLocalizedByClass` (generated) |
| `display` | read-only label (future) |

## npm workflow

```bash
cd Packages/ui-design/npm
npm install
npm run validate    # JSON Schema check (ajv draft 2020-12)
npm run export      # validate + copy specs to Assets/.../_specs/
```

Optional: `npm run export:tokens` writes a pxlkit token snippet for `design-tokens.uss`.

## Unity workflow

1. `Tools → UI Toolkit → Import All Screen Specs` — regenerates UXML + `*.Bindings.generated.cs`
2. `Tools → MVVM → Setup Localization` — string table (keys documented in spec)
3. `Tools → UI Toolkit → Validate Toolkit Bindings` — UXML ↔ view ↔ spec drift
4. `Tools → MVVM → Validate Template Setup` — includes spec + binding checks

## Rules

- UXML `<Style src>` must use **relative** paths only.
- Generated UXML starts with `<!-- Generated from *.screen.json - do not edit -->`.
- Edit layout in `*.screen.json`, not hand-edited UXML (pilot screens are round-tripped).
- Hand-written tab logic, smart volume label, keyboard nav stay in `*ToolkitView.cs`.
- Generated partials call `ApplyGeneratedBindings` from `BindPanel`; do not edit generated files.

## Pilot screens

- `main-menu.screen.json` → `MainMenu/MainMenuScreen.uxml`
- `settings.screen.json` → `Settings/SettingsScreen.uxml`

