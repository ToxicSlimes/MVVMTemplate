# AGENTS.md - Bindable Elements

## Назначение

Конкретные реализации `BindableElement<TData>` — переиспользуемые UI компоненты для отображения данных в списках, гридах и коллекциях.

## Архитектура

```
Infrastructure/MVVM/Elements/
├── AGENTS.md                      # Эта документация
├── BindableElement.cs             # Базовый класс (generic)
└── [Feature]/                     # Конкретные элементы по фичам
    └── [Name]Element.cs
```

## Базовый класс BindableElement<TData>

Все элементы наследуются от `BindableElement<TData>` где `TData : struct, IElementData`.

### Встроенные SerializeField (доступны в наследниках)

| Поле | Тип | Назначение |
|------|-----|------------|
| `_clickButton` | Button | Клик по элементу |
| `_selectionIndicator` | GameObject | Индикатор выделения |
| `_popupTrigger` | PopupTrigger | Popup при long-press |

### Lifecycle методы

| Метод | Когда вызывается |
|-------|------------------|
| `OnBind(TData data)` | Первая привязка данных |
| `OnRefresh(TData old, TData new)` | Обновление данных |
| `OnUnbind()` | Отвязка от данных |
| `OnSelectionChanged(bool selected)` | Изменение выделения |
| `OnDispose()` | Уничтожение |
| `GetPopupData(TData data)` | Данные для popup |

---

## Naming Conventions

### SerializeField → Context Key Mapping

**Правило:** `_camelCase` поле → `PascalCase` ключ

| C# Field | Context Key | Пример типа |
|----------|-------------|-------------|
| `_abilityIcon` | "AbilityIcon" | Image |
| `_abilityName` | "AbilityName" | TMP_Text |
| `_description` | "Description" | TMP_Text |
| `_lockOverlay` | "LockOverlay" | GameObject |
| `_cooldownText` | "CooldownText" | TMP_Text |
| `_manaCostText` | "ManaCostText" | TMP_Text |
| `_rarityFrame` | "RarityFrame" | Image |
| `_levelBadge` | "LevelBadge" | GameObject |

### Преобразование имени

```
_fieldName → FieldName
│            │
└ убрать _   └ первая буква uppercase
```

### Prefab Hierarchy Naming

| Тип | Префикс | Пример |
|-----|---------|--------|
| Image | `img_` | `img_AbilityIcon` |
| TMP_Text | `txt_` | `txt_AbilityName` |
| Button | `btn_` | `btn_Click` |
| Container | `cnt_` | `cnt_Header` |
| GameObject | (нет) | `SelectionIndicator` |

---

## Template: Создание нового Element

### 1. Data struct (в Core/Data/Domain/Models/Elements/)

```csharp
using Source.Runtime.Core.Data.Domain.Models.Elements;

namespace Core.Data.Domain.Models.Elements
{
    public struct AbilityCardData : IElementData
    {
        public string Id { get; set; }
        
        public string AbilityId;
        public string Name;
        public string Description;
        public string IconPath;
        public int ManaCost;
        public float Cooldown;
        public bool IsLocked;
    }
}
```

### 2. Element class (в Infrastructure/MVVM/Elements/[Feature]/)

```csharp
using Core.Data.Domain.Models.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Infrastructure.MVVM.Elements
{
    public class AbilityCardElement : BindableElement<AbilityCardData>
    {
        [Header("Content")]
        [SerializeField] private Image _abilityIcon;
        [SerializeField] private TMP_Text _abilityName;
        [SerializeField] private TMP_Text _description;
        
        [Header("State")]
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private TMP_Text _cooldownText;
        [SerializeField] private TMP_Text _manaCostText;
        
        protected override void OnBind(AbilityCardData data)
        {
            _abilityName.text = data.Name;
            _description.text = data.Description;
            _lockOverlay.SetActive(data.IsLocked);
            _cooldownText.text = $"{data.Cooldown:F1}s";
            _manaCostText.text = data.ManaCost.ToString();
        }
    }
}
```

---

## Prefab Wiring (Editor-time)

### В PrefabBuilder регистрируем компоненты:

```csharp
protected override void BuildElementStructure()
{
    var icon = CreateImage("img_AbilityIcon");
    Context.Register("AbilityIcon", icon);
    
    var nameText = CreateText("txt_AbilityName");
    Context.Register("AbilityName", nameText);
}
```

### Wire через SerializationHelper:

```csharp
protected override void WireFields(AbilityCardElement element)
{
    var so = new SerializedObject(element);
    
    SerializationHelper.Wire(so, "_abilityIcon", Context.Get<Image>("AbilityIcon"));
    SerializationHelper.Wire(so, "_abilityName", Context.Get<TMP_Text>("AbilityName"));
    SerializationHelper.Wire(so, "_description", Context.Get<TMP_Text>("Description"));
    SerializationHelper.Wire(so, "_lockOverlay", Context.Get<GameObject>("LockOverlay"));
    SerializationHelper.Wire(so, "_cooldownText", Context.Get<TMP_Text>("CooldownText"));
    SerializationHelper.Wire(so, "_manaCostText", Context.Get<TMP_Text>("ManaCostText"));
    
    so.ApplyModifiedProperties();
}
```

### Критично: ApplyModifiedProperties()

**ВСЕГДА вызывай** `so.ApplyModifiedProperties()` после всех Wire операций. Без этого изменения НЕ сохранятся в prefab.

---

## Типичные ошибки

### ❌ Несовпадение имени поля

```csharp
SerializationHelper.Wire(so, "abilityIcon", icon);     // без _
SerializationHelper.Wire(so, "_AbilityIcon", icon);    // неверный регистр
```

**✅ Правильно:** `"_abilityIcon"` — точное имя C# поля

### ❌ Забыли ApplyModifiedProperties

```csharp
var so = new SerializedObject(element);
SerializationHelper.Wire(so, "_icon", icon);
// Где ApplyModifiedProperties???
```

### ❌ Несовпадение типов

```csharp
// Поле: [SerializeField] private Image _icon;
SerializationHelper.Wire(so, "_icon", iconGameObject);  // Нужен Image!
```

**✅ Правильно:** `Context.Get<Image>("Icon")` — точный тип

### ❌ Не зарегистрировали в Context

```csharp
var icon = iconGo.AddComponent<Image>();
// Забыли: Context.Register("AbilityIcon", icon);
```

---

## Checklist при создании Element

- [ ] Data struct реализует `IElementData`
- [ ] Element наследует `BindableElement<TData>`
- [ ] Все UI ссылки через `[SerializeField] private`
- [ ] Реализован `OnBind(TData data)`
- [ ] PrefabBuilder регистрирует компоненты в Context
- [ ] WireFields использует точные имена полей с `_`
- [ ] ApplyModifiedProperties() вызывается в конце
- [ ] ElementMapping сгенерирован и связан с префабом

---

## Element Mapping System

Система маппинга связывает типы `BindableElement<TData>` с их префабами для использования в `UIElementService`.

### Компоненты системы

| Компонент | Путь | Назначение |
|-----------|------|------------|
| `ElementMappingBase` | Infrastructure/MVVM/Config | Базовый ScriptableObject с ссылкой на префаб |
| `ElementConfig` | 00. Configs/ElementConfig.asset | Реестр всех маппингов |
| `UIElementService` | Infrastructure/MVVM/Services | Runtime создание элементов |

### Workflow: Полный цикл создания Element

```
1. Создать Data struct и Element class
                ↓
2. Tools → MVVM → Element Mapping Generator
   → Выбрать элемент
   → Generate Mapping
   → Создаётся {Element}Mapping.cs + .asset
                ↓
3. Tools → Elements → Build {Element} Prefab
   → Создаётся префаб
   → Автоматически связывается с маппингом ✓
```

### Editor Commands

| Команда | Назначение |
|---------|------------|
| `Tools/MVVM/Element Mapping Generator` | UI для генерации маппингов |
| `Tools/MVVM/Create Missing Element Mapping Assets` | Создать .asset для существующих .cs |
| `Tools/MVVM/Link Prefabs to Element Mappings` | Массовая привязка префабов к маппингам |

### Автоматическое связывание

`BaseBindableElementBuilder.SavePrefab()` автоматически обновляет `ElementMapping._elementPrefab` после создания префаба.

Если маппинг не найден, выведет предупреждение:
```
[ElementBuilder] No mapping found for AbilityCardElement. Run Element Mapping Generator first.
```

### Ручное связывание

Если автоматика не сработала:
1. `Tools/MVVM/Link Prefabs to Element Mappings`
2. Или вручную: откройте `ElementsMapping/AbilityCardElementMapping.asset` и назначьте префаб

---

## Использование в ViewModel / View

### Прямое использование через IUIElementService

```csharp
public class HeroesGridPanelView : BindableView<HeroesGridPanelViewModel>
{
    [SerializeField] private Transform _heroesContainer;

    [Inject] private IUIElementService _elementService;

    protected override void OnBind(ref DisposableBag disposables)
    {
        ViewModel.Heroes
            .Subscribe(heroes => UpdateHeroes(heroes))
            .AddTo(ref disposables);
    }

    private void UpdateHeroes(IReadOnlyList<HeroSlotData> heroes)
    {
        _elementService.ReconcileElements<HeroSlotElement, HeroSlotData>(
            _heroesContainer,
            heroes,
            OnHeroClicked);
    }

    private void OnHeroClicked(HeroSlotData hero)
    {
        ViewModel.SelectHeroCommand.Execute(hero.HeroId);
    }
}
```

### Через CQRS (IMediator)

```csharp
public class HeroDetailPanelViewModel : DisposableObject
{
    private readonly IMediator _mediator;
    private readonly Transform _abilitiesContainer;

    public async UniTask RefreshAbilities(IReadOnlyList<HeroAbilityData> abilities)
    {
        await _mediator.Send(new ReconcileElementsRequest<AbilityCardElement, HeroAbilityData>(
            _abilitiesContainer,
            abilities,
            OnAbilityClicked));
    }

    public async UniTask ClearAbilities()
    {
        await _mediator.Send(new ClearContainerRequest(_abilitiesContainer));
    }
}
```

### Создание одного элемента

```csharp
var element = await _mediator.Send(
    new CreateElementRequest<HeroSlotElement, HeroSlotData>(
        container: _container,
        data: heroData,
        onClick: OnHeroClicked));

// или напрямую
var element = _elementService.CreateElement<HeroSlotElement, HeroSlotData>(
    _container, heroData, OnHeroClicked);
```

### Удаление элемента

```csharp
await _mediator.Send(new DestroyElementRequest(element));

// или напрямую
_elementService.DestroyElement(element);
```

---

## CQRS Commands Reference

| Command | Response | Handler |
|---------|----------|---------|
| `CreateElementRequest<TElement, TData>` | `TElement` | `CreateElementHandler<T,D>` |
| `DestroyElementRequest` | `Nothing` | `DestroyElementHandler` |
| `ClearContainerRequest` | `Nothing` | `ClearContainerHandler` |
| `ReconcileElementsRequest<TElement, TData>` | `Nothing` | `ReconcileElementsHandler<T,D>` |

---

## Quick Start: Создание нового Element за 5 шагов

### 1️⃣ Data struct

```csharp
// Assets/Source/Runtime/Core/Data/Domain/Models/Elements/MyItemData.cs
public readonly struct MyItemData : IElementData, IEquatable<MyItemData>
{
    public string Id { get; }
    public readonly string Name;
    public readonly int Value;

    public MyItemData(string id, string name, int value)
    {
        Id = id; Name = name; Value = value;
    }

    public bool Equals(MyItemData other) => Id == other.Id;
    public override int GetHashCode() => Id?.GetHashCode() ?? 0;
}
```

### 2️⃣ Element class

```csharp
// Assets/Source/Runtime/Core/MVVM/Elements/MyItemElement.cs
public class MyItemElement : BindableElement<MyItemData>
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _valueText;

    protected override void OnBind(MyItemData data)
    {
        _nameText.text = data.Name;
        _valueText.text = data.Value.ToString();
    }
}
```

### 3️⃣ Generate Mapping

```
Unity Editor → Tools → MVVM → Element Mapping Generator
→ Find "MyItemElement" → Click "Generate Mapping"
```

### 4️⃣ Build Prefab (или создать вручную)

```
Unity Editor → Tools → UI Prefab Builder → Elements → My Item Element
→ Click BUILD
```

(Маппинг автоматически свяжется с префабом)

### 5️⃣ Использование

```csharp
_elementService.ReconcileElements<MyItemElement, MyItemData>(
    _container, items, OnItemClicked);
```

---

## Диагностика проблем

### Элемент не создаётся

1. Проверь `ElementConfig.asset` — есть ли маппинг в списке?
2. Проверь маппинг — указан ли префаб?
3. Проверь префаб — есть ли компонент Element?

### ReconcileElements не обновляет элементы

1. Data struct реализует `IEquatable<T>`?
2. `Equals()` сравнивает по `Id`?
3. Данные действительно изменились?

### Клик не работает

1. Префаб имеет `Button` компонент?
2. `_clickButton` связан в инспекторе?
3. `onClick` передан в `CreateElement` / `ReconcileElements`?

---

## Related

- **Base class:** `BindableElement.cs` в этой папке
- **Interface:** `Core.Abstractions.MVVM.Elements.IBindableElement`
- **Data models:** `Core.Data.Domain.Models.Elements/`
- **Prefab builders:** `Editor/PrefabBuilderSystem/Builders/Elements/`
- **Unit tests:** `Tests/MVVM/Elements/UIElementServiceTests.cs`
- **Element builders menu:** `Tools/UI Prefab Builder/Elements/*`
