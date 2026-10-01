# UI Toolkit UXML Guidelines

Runtime UI Toolkit screens in this template use **UXML + USS** with **query-based bindings** in C# (`Root.Q<Button>("increment-button")`).

## Naming contract

- Use **kebab-case** `name` attributes in UXML.
- Keep names stable — they are the binding contract with view classes.
- Group related names with a prefix: `counter-label`, `increment-button`, `confirm-overlay`.

## USS tokens

Shared tokens live in `Assets/Source/UI/Toolkit/Theme/design-tokens.uss`:

- `--color-primary`, `--color-accent`, `--color-danger`
- `--color-bg-deep`, `--color-bg-surface`, `--color-bg-elevated`
- `--color-text-primary`, `--color-text-secondary`
- `--space-xs` through `--space-xl`
- `--radius-md`, `--radius-lg`

Screen USS files should `@import` or reference design tokens and only add layout rules.

## Toolkit view workflow

1. Edit layout in UI Builder or `ToolkitCounterEdit` scene (`Tools → UI Toolkit → Open Counter Edit Scene`).
2. Save UXML/USS under `Assets/Source/UI/Toolkit/`.
3. Run `Tools → UI Toolkit → Setup Counter Toolkit Assets` to refresh prefab + Addressables refs.
4. Bind in `*ToolkitView` via `Root.Q<T>("name")` and `ToolkitBindingExtensions`.

## AI / Figma prompts

When converting a mockup to UXML:

```
Convert this screen to Unity UI Toolkit UXML/USS for MVVMTemplate.
- kebab-case element names matching: counter-label, increment-button, gallery-panel
- import design-tokens.uss
- use **relative** `Style src` paths in UXML (e.g. `CounterScreen.uss`, `../Theme/design-tokens.uss`) — never `project://database/...` (breaks at runtime)
- modal overlay: confirm-overlay with confirm-title, confirm-message, confirm-button, cancel-button
- no inline C# bindings; layout only
```

## Element templates

Reusable list items use small UXML templates under `Assets/Source/UI/Toolkit/Elements/` and `ToolkitBindableElement<TData>` wrappers reconciled via `ReconcileToolkitElementsCommand`.

## Screen Spec pipeline

For AI/designer → Unity workflow, layout is authored as `*.screen.json` (not hand-edited UXML). See [ui-screen-spec.md](ui-screen-spec.md).

1. Edit spec in `Packages/ui-design/spec/` (or React reference in `Packages/ui-design/screens/`).
2. `cd Packages/ui-design/npm && npm run validate && npm run export`
3. Unity: `Tools → UI Toolkit → Import All Screen Specs`
4. `Tools → UI Toolkit → Validate Toolkit Bindings`

Pilot screens: Main Menu + Settings. Generated UXML uses relative `Style src` paths and a `Generated from *.screen.json` header comment.
