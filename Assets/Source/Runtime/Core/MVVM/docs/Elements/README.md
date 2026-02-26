# Elements — Переиспользуемые UI компоненты

## Обзор

Elements — переиспользуемые UI компоненты, которые биндятся к data struct через `BindableElement<TData>`. Используются для отображения списков: герои, статы, ресурсы, слоты и т.д.

## Базовый класс

```csharp
[RequireComponent(typeof(RectTransform))]
public abstract class BindableElement<TData> : MonoBehaviour, ISelectableElement<TData>
    where TData : struct, IElementData
{
    public string ElementId { get; }
    public bool IsBound { get; }
    public bool IsSelected { get; }
    public bool HasPopup { get; }
    public Action<TData> OnClicked { set; }
    
    public void Bind(TData data) { ... }
    public void Refresh(TData data) { ... }
    public void SetSelected(bool selected) { ... }
    public void Unbind() { ... }
    public void Dispose() { ... }
    
    protected abstract void OnBind(TData data);
    protected virtual void OnRefresh(TData oldData, TData newData) => OnBind(newData);
    protected virtual void OnUnbind() { }
    protected virtual void OnSelectionChanged(bool selected) { }
    protected virtual void OnDispose() { }
    protected virtual IPopupData GetPopupData(TData data) => null;
}
```

---

## Каталог Elements

| Element | Data Type | Описание |
|---------|-----------|----------|
| `AbilityCardElement` | `HeroAbilityData` | Карточка способности героя |
| `EquipmentSlotElement` | `EquipmentSlotData` | Слот экипировки |
| `HeroSlotElement` | `HeroSlotData` | Слот героя (главный экран) |
| `ResourceItemElement` | `ResourceItemData` | Отображение ресурса (золото, кристаллы) |
| `ServiceSlotElement` | `ServiceSlotElementData` | Слот в сервисных панелях (Healing/Training) |
| `StatRowElement` | `StatRowData` | Строка характеристики |

---

## AbilityCardElement

**Путь:** `Elements/AbilityCardElement.cs`  
**Data:** `HeroAbilityData`

Карточка способности героя с иконкой, описанием, кулдауном и стоимостью маны.

### Поля данных

```csharp
public struct HeroAbilityData : IElementData
{
    public string Id;
    public string Name;
    public string Description;
    public Sprite Icon;
    public bool IsUnlocked;
    public int CooldownTurns;
    public int ManaCost;
}
```

### Визуальные компоненты

| Поле | Тип | Описание |
|------|-----|----------|
| `_abilityIcon` | `Image` | Иконка способности |
| `_abilityName` | `TMP_Text` | Название |
| `_description` | `TMP_Text` | Описание |
| `_lockOverlay` | `GameObject` | Оверлей заблокированной способности |
| `_cooldownText` | `TMP_Text` | Кулдаун (количество ходов) |
| `_manaCostText` | `TMP_Text` | Стоимость маны |

### Tooltip

Показывает название и описание способности при наведении.

---

## EquipmentSlotElement

**Путь:** `Elements/EquipmentSlotElement.cs`  
**Data:** `EquipmentSlotData`

Слот экипировки с поддержкой редкости и уровня предмета.

### Поля данных

```csharp
public struct EquipmentSlotData : IElementData
{
    public string SlotId;
    public string SlotType;      // "Weapon", "Armor", "Accessory"
    public bool IsEmpty;
    public string ItemName;
    public Sprite ItemIcon;
    public int ItemLevel;
    public int Rarity;           // 0-4: common, uncommon, rare, epic, legendary
}
```

### Визуальные компоненты

| Поле | Тип | Описание |
|------|-----|----------|
| `_itemIcon` | `Image` | Иконка предмета |
| `_itemName` | `TMP_Text` | Название предмета |
| `_slotTypeLabel` | `TMP_Text` | Тип слота |
| `_itemLevelText` | `TMP_Text` | Уровень предмета ("+5") |
| `_emptyIndicator` | `GameObject` | Индикатор пустого слота |
| `_rarityFrame` | `Image` | Рамка цвета редкости |

### Цвета редкости

| Rarity | Цвет |
|--------|------|
| 0 (Common) | Серый `#999999` |
| 1 (Uncommon) | Зелёный `#33CC33` |
| 2 (Rare) | Синий `#3366FF` |
| 3 (Epic) | Фиолетовый `#9933CC` |
| 4 (Legendary) | Оранжевый `#FF9933` |

---

## HeroSlotElement

**Путь:** `Elements/HeroSlotElement.cs`  
**Data:** `HeroSlotData`

Слот героя в главном экране. Показывает портрет, имя, уровень, здоровье.

### Поля данных

```csharp
public readonly struct HeroSlotData : IElementData
{
    public readonly string HeroId;
    public readonly string Name;
    public readonly Texture2D Portrait;
    public readonly int Level;
    public readonly bool IsSelected;
    public readonly bool IsDead;
    public readonly float HealthPercent;   // 0.0 - 1.0

    public string Id => HeroId;

    public HeroSlotData(
        string heroId,
        string name,
        Texture2D portrait,
        int level,
        bool isSelected = false,
        bool isDead = false,
        float healthPercent = 1f);
}
```

### Визуальные компоненты

| Поле | Тип | Описание |
|------|-----|----------|
| `_portrait` | `RawImage` | Портрет героя |
| `_heroName` | `TMP_Text` | Имя героя |
| `_levelText` | `TMP_Text` | Уровень ("Lv.45") |
| `_healthBar` | `Image` | Прогресс-бар здоровья |
| `_deadOverlay` | `GameObject` | Оверлей мёртвого героя |

### Tooltip

Показывает имя героя, уровень и статус (мёртв/жив).

---

## ResourceItemElement

**Путь:** `Elements/ResourceItemElement.cs`  
**Data:** `ResourceItemData`

Отображение ресурса игрока с иконкой и количеством.

### Поля данных

```csharp
public struct ResourceItemData : IElementData
{
    public string ResourceId;
    public string Name;
    public string Description;
    public int Amount;
    public Sprite Icon;
}
```

### Визуальные компоненты

| Поле | Тип | Описание |
|------|-----|----------|
| `_icon` | `Image` | Иконка ресурса |
| `_amountText` | `TMP_Text` | Количество (форматируется с разделителями: "15,420") |
| `_nameText` | `TMP_Text` | Название ресурса |

### Форматирование

Количество форматируется через `ToString("N0")` для добавления разделителей тысяч.

---

## ServiceSlotElement

**Путь:** `Elements/ServiceSlotElement.cs`  
**Data:** `ServiceSlotElementData`

Слот в сервисных панелях (Healing/Training) с тремя состояниями: Empty, Occupied, Locked.

### Поля данных

```csharp
public struct ServiceSlotElementData : IElementData
{
    public string SlotId;
    public ServiceSlotState State;   // Empty, Occupied, Locked
    
    // For Occupied state
    public Texture HeroAvatar;
    public string HeroName;
    public float Progress;           // 0.0 - 1.0
    public string TimeText;          // "5m 30s"
    
    // For Locked state
    public int UnlockCost;
}

public enum ServiceSlotState
{
    Empty,
    Occupied,
    Locked
}
```

### Визуальные компоненты

#### Контейнеры состояний
| Поле | Тип | Описание |
|------|-----|----------|
| `_emptyState` | `GameObject` | Empty state UI |
| `_occupiedState` | `GameObject` | Occupied state UI |
| `_lockedState` | `GameObject` | Locked state UI |

#### Empty State
| Поле | Тип | Описание |
|------|-----|----------|
| `_addLabel` | `TMP_Text` | Метка "+" |

#### Occupied State
| Поле | Тип | Описание |
|------|-----|----------|
| `_heroAvatar` | `RawImage` | Аватар героя |
| `_heroName` | `TMP_Text` | Имя героя |
| `_progressBar` | `Slider` | Прогресс-бар |
| `_timeText` | `TMP_Text` | Оставшееся время |

#### Locked State
| Поле | Тип | Описание |
|------|-----|----------|
| `_lockIcon` | `Image` | Иконка замка |
| `_costText` | `TMP_Text` | Стоимость разблокировки |

### Tooltips по состояниям

| State | Title | Description |
|-------|-------|-------------|
| Empty | "Свободный слот" | "Нажмите чтобы назначить героя" |
| Occupied | `{HeroName}` | `"Осталось: {TimeText}"` |
| Locked | "Заблокировано" | `"Стоимость: {UnlockCost}"` |

---

## StatRowElement

**Путь:** `Elements/StatRowElement.cs`  
**Data:** `StatRowData`

Строка характеристики с меткой, значением и опциональным прогресс-баром.

### Поля данных

```csharp
public struct StatRowData : IElementData
{
    public string StatId;
    public string Label;       // "Сила", "Ловкость"
    public string Value;       // "150/200"
    public Sprite Icon;
    public float? BarPercent;  // 0.0 - 1.0 (null = скрыть бар)
}
```

### Визуальные компоненты

| Поле | Тип | Описание |
|------|-----|----------|
| `_statIcon` | `Image` | Иконка характеристики |
| `_labelText` | `TMP_Text` | Название характеристики |
| `_valueText` | `TMP_Text` | Значение |
| `_progressBar` | `Image` | Прогресс-бар (fillAmount) |
| `_barContainer` | `GameObject` | Контейнер прогресс-бара |

### Tooltip

Показывает название характеристики и её значение.

---

## Создание Element через Mediator

Elements создаются через `CreateElementRequest`:

```csharp
// Создание одного элемента
var element = await _mediator.Send(
    new CreateElementRequest<HeroSlotElement, HeroSlotData>(
        container: _heroSlotsContainer,
        data: heroData,
        onClick: OnHeroClicked
    )
);

// Reconciliation списка (как React)
await _mediator.Send(
    new ReconcileElementsRequest<StatRowElement, StatRowData>(
        container: _statsContainer,
        newData: statsList,
        onClick: null
    )
);
```

---

## IElementData Interface

Все data struct должны реализовывать:

```csharp
public interface IElementData
{
    string Id { get; }  // Уникальный ID для reconciliation
}
```

---

## Best Practices

1. **Prefab Setup:** Создавай prefab для Element с нужными UI компонентами
2. **Nullable fields:** Используй `?` для опциональных полей (например, `float? BarPercent`)
3. **null checks:** Проверяй `!= null` перед установкой Sprite/Texture
4. **Tooltip:** Переопредели `GetPopupData()` для кастомных tooltips
5. **OnUnbind:** Очищай все поля в `OnUnbind()` для пулинга
6. **SetSelected:** Используй базовый метод для визуального выделения

---

## См. также

- [Views/README.md](../Views/README.md) — Использование Elements в Views
- [ReconcileElementsRequest](../Commands.md#reconcileelementsrequest) — Efficient list updates
- [CreateElementRequest](../Commands.md#createelementrequest) — Создание элементов
