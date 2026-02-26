# MVVM Elements

## Overview

Bindable UI Elements — типизированные компоненты для динамических списков (карточки, слоты, строки).

## Architecture

```
BindableElement<TData> : MonoBehaviour, ISelectableElement<TData>
    where TData : struct, IElementData
```

## Creating a New Element

### 1. Create Data Struct

```csharp
// Core/Data/Domain/Models/Elements/MyItemData.cs
public readonly struct MyItemData : IElementData, IEquatable<MyItemData>
{
    public string Id { get; }
    public readonly string Name;
    // ... other fields
}
```

### 2. Create Element Class

```csharp
// Core/MVVM/Elements/MyItemElement.cs
public class MyItemElement : BindableElement<MyItemData>
{
    [SerializeField] private TMP_Text _nameText;
    
    protected override void OnBind(MyItemData data)
    {
        _nameText.text = data.Name;
    }
}
```

### 3. Create Element Builder (Editor)

```csharp
// Editor/PrefabBuilderSystem/Builders/Elements/MyItemElementBuilder.cs
public class MyItemElementBuilder : BaseBindableElementBuilder<MyItemElement, MyItemData>
{
    protected override string PrefabPath => "Assets/Prefabs/Elements/MyItem.prefab";
    protected override string ElementName => "MyItem";
    
    protected override void BuildElementStructure() { /* create UI */ }
    protected override void WireFields(MyItemElement element) { /* wire refs */ }
}
```

### 4. Generate Mapping

Use `Tools > MVVM > Element Mapping Generator` to generate:
- `MyItemElementMapping.cs`
- `MyItemElementMapping.asset`

### 5. Register in ElementConfig

Add mapping to `Assets/Configs/MVVM/ElementConfig.asset`

## Naming Conventions

| Location | Convention | Example |
|----------|------------|---------|
| C# Field | `_camelCase` | `_abilityIcon` |
| Context Key | `PascalCase` | `"AbilityIcon"` |
| Prefab Child | `prefix_Name` | `img_AbilityIcon` |

### Prefixes

| Prefix | Type |
|--------|------|
| `txt_` | TMP_Text |
| `img_` | Image |
| `btn_` | Button |
| `cnt_` | Container (layout) |
| `go_` | Generic GameObject |

## WireFields Pattern

```csharp
protected override void WireFields(MyElement element)
{
    var so = new SerializedObject(element);
    
    SerializationHelper.Wire(so, "_nameText", Context.Get<TMP_Text>("NameText"));
    SerializationHelper.Wire(so, "_icon", Context.Get<Image>("Icon"));
    
    so.ApplyModifiedProperties();
}
```

## Base Fields (auto-wired)

These are wired automatically by `BaseBindableElementBuilder`:

| Field | Type | Context Key |
|-------|------|-------------|
| `_clickButton` | Button | `"ClickButton"` |
| `_selectionIndicator` | GameObject | `"SelectionIndicator"` |
| `_popupTrigger` | PopupTrigger | `"PopupTrigger"` |

## Usage in ViewModel

```csharp
// Create single element
var element = await _mediator.Send(new CreateElementRequest<MyElement, MyData>(
    container: _container,
    data: itemData,
    onClick: OnItemClicked
));

// Reconcile list (diff update)
await _mediator.Send(new ReconcileElementsRequest<MyElement, MyData>(
    container: _container,
    newData: items,
    onClick: OnItemClicked
));

// Clear container
await _mediator.Send(new ClearContainerRequest(_container));
```
