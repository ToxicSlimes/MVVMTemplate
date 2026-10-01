# AI prompt: Screen Spec JSON

Use this when asking an AI to design or extend a Unity UI Toolkit screen for MVVMTemplate.

## Task template

```
Design the [SCREEN NAME] screen for MVVMTemplate using the Screen Spec JSON format (schema v1).

Requirements:
- Pixel-art surface (pxlkit-inspired): chunky borders, Press Start 2P typography classes
- Output a single valid *.screen.json matching screen-spec.schema.json
- screenId: [kebab-case-id]
- viewType: [XxxToolkitView]
- viewModelType: [XxxViewModel]
- viewNamespace: Core.MVVM.Views.[Folder]
- output.folder: [Folder matching view]
- theme.styles: ["design-tokens.uss", "pixel-theme.uss"] (+ pixel-controls.uss for sliders/toggles/dropdowns)
- Every interactive control: kebab-case `name`
- Button tones: green=primary, cyan=outline, neutral=secondary, ghost=ghost, red=danger
- bindings: command/property/localized entries for every named control

Node types: Container, Label, Button, SliderRow, Toggle, Dropdown, Tabs, ScrollView.

SliderRow example:
{ "type": "SliderRow", "name": "master-volume", "label": "Master", "valueName": "master-volume-value", "low": 0, "high": 1 }

Tabs example:
{
  "type": "Tabs",
  "tabs": [
    { "id": "audio", "label": "Audio", "tabButtonName": "tab-audio", "children": [ ... ] }
  ]
}

Localized binding example:
{ "name": "apply-button", "kind": "localized", "localizationKey": "settings.apply", "elementType": "Button" }

Class-based label:
{ "name": "screen-title", "kind": "localized", "localizationKey": "settings.title", "queryByClass": true, "className": "screen-title", "elementType": "Label" }

After generating JSON:
1. Save to Packages/ui-design/spec/[screen-id].screen.json
2. Run: cd Packages/ui-design/npm && npm run validate && npm run export
3. Unity: Tools → UI Toolkit → Import All Screen Specs
```

## Reference files

- Schema: `Packages/ui-design/spec/screen-spec.schema.json`
- Main Menu: `Packages/ui-design/spec/main-menu.screen.json`
- Settings: `Packages/ui-design/spec/settings.screen.json`
- React reference: `Packages/ui-design/screens/MainMenuScreen.tsx`, `SettingsScreen.tsx`

## Settings tab extension example

```
Add a new "Accessibility" tab to settings.screen.json:
- tab id: accessibility, tabButtonName: tab-accessibility
- children: one SliderRow (text-scale), one Toggle (high-contrast)
- bindings for each control targeting ViewModel properties (document names only; C# bind stays manual for v1)
- localized tab button key: settings.tab_accessibility
```

Do not edit generated UXML directly. Spec is the layout source of truth.
