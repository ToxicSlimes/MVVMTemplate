# Elements Data

Element data structures — readonly struct'ы для UI элементов (грид, слоты, списки).

**Путь:** `Domain/Models/Elements/`  
**Namespace:** `Core.Data.Domain.Models.Elements`, `Source.Runtime.Core.Data.Domain.Models.Elements`

---

## IElementData

**Путь:** `Domain/Models/Elements/IElementData.cs`  
**Namespace:** `Source.Runtime.Core.Data.Domain.Models.Elements`

Базовый интерфейс для всех элементов.

```csharp
public interface IElementData
{
    string Id { get; }
}
```

Все элементы ниже implement этот интерфейс.

---

## EquipmentSlotData

**Путь:** `Domain/Models/Elements/EquipmentSlotData.cs`

**Readonly struct** для слота экипировки.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| SlotId | `string` | ID слота (не null) |
| SlotType | `string` | Тип слота (Weapon, Armor, etc.) |
| ItemName | `string` | Имя предмета |
| ItemIcon | `Sprite` | Иконка предмета |
| ItemLevel | `int` | Уровень предмета |
| IsEmpty | `bool` | Пустой ли слот |
| Rarity | `int` | Редкость (0-5) |

### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| Id | `string` | Alias для SlotId |

### Конструктор

```csharp
public EquipmentSlotData(
    string slotId,
    string slotType,
    string itemName = null,
    Sprite itemIcon = null,
    int itemLevel = 0,
    bool isEmpty = true,
    int rarity = 0)
```

### Equality

Implements `IEquatable<EquipmentSlotData>`. Сравнивается по `SlotId`.

---

## HeroAbilityData

**Путь:** `Domain/Models/Elements/HeroAbilityData.cs`

**Readonly struct** для способности героя.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID способности (не null) |
| Name | `string` | Название |
| Description | `string` | Описание |
| Icon | `Sprite` | Иконка |
| IsUnlocked | `bool` | Разблокирована ли |
| CooldownTurns | `int` | Кулдаун в ходах |
| ManaCost | `int` | Стоимость маны |

### Конструктор

```csharp
public HeroAbilityData(
    string id,
    string name,
    string description,
    Sprite icon,
    bool isUnlocked,
    int cooldownTurns = 0,
    int manaCost = 0)
```

### Equality

Implements `IEquatable<HeroAbilityData>`. Сравнивается по `Id`.

---

## HeroSlotData

**Путь:** `Domain/Models/Elements/HeroSlotData.cs`

**Readonly struct** для слота героя в гриде.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| HeroId | `string` | ID героя (не null) |
| Name | `string` | Имя героя |
| Portrait | `Texture2D` | Портрет |
| Level | `int` | Уровень |
| IsSelected | `bool` | Выбран ли |
| IsDead | `bool` | Мёртв ли |
| HealthPercent | `float` | Процент здоровья (0-1, clamp) |

### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| Id | `string` | Alias для HeroId |

### Конструктор

```csharp
public HeroSlotData(
    string heroId,
    string name,
    Texture2D portrait,
    int level,
    bool isSelected = false,
    bool isDead = false,
    float healthPercent = 1f)
```

### Equality

Implements `IEquatable<HeroSlotData>`. Сравнивается по `HeroId`.

---

## ResourceItemData

**Путь:** `Domain/Models/Elements/ResourceItemData.cs`

**Readonly struct** для ресурса (золото, кристаллы и т.д.).

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| ResourceId | `string` | ID ресурса (не null) |
| Name | `string` | Название |
| Amount | `int` | Количество |
| Description | `string` | Описание |
| Icon | `Sprite` | Иконка |

### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| Id | `string` | Alias для ResourceId |

### Конструктор

```csharp
public ResourceItemData(
    string resourceId,
    string name,
    int amount,
    string description = null,
    Sprite icon = null)
```

### Equality

Implements `IEquatable<ResourceItemData>`. Сравнивается по `ResourceId`.

---

## ServiceSlotElementData

**Путь:** `Domain/Models/Elements/ServiceSlotElementData.cs`

**Readonly struct** для слота сервисной панели (кузня, госпиталь и т.д.).

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| SlotId | `string` | ID слота (не null) |
| State | `ServiceSlotState` | Состояние слота (enum) |
| HeroId | `string` | ID героя в слоте |
| HeroName | `string` | Имя героя |
| HeroAvatar | `Texture2D` | Аватар героя |
| Progress | `float` | Прогресс (0-1, clamp) |
| TimeText | `string` | Текст времени ("2:30:15") |
| UnlockCost | `int` | Стоимость разблокировки |

### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| Id | `string` | Alias для SlotId |

### Конструктор

```csharp
public ServiceSlotElementData(
    string slotId,
    ServiceSlotState state,
    string heroId = null,
    string heroName = null,
    Texture2D heroAvatar = null,
    float progress = 0f,
    string timeText = null,
    int unlockCost = 0)
```

### Factory Method

```csharp
public static ServiceSlotElementData FromModel(ServiceSlotData data)
```

### Equality

Implements `IEquatable<ServiceSlotElementData>`. Сравнивается по `SlotId`.

---

## StatRowData

**Путь:** `Domain/Models/Elements/StatRowData.cs`

**Readonly struct** для строки статов (HP, Attack, Defense и т.д.).

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| StatId | `string` | ID стата (не null) |
| Label | `string` | Название |
| Value | `string` | Значение (строка для гибкости) |
| Icon | `Sprite` | Иконка |
| BarPercent | `float?` | Процент заполнения бара (0-1, clamp) |

### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| Id | `string` | Alias для StatId |

### Конструктор

```csharp
public StatRowData(
    string statId,
    string label,
    string value,
    Sprite icon = null,
    float? barPercent = null)
```

### Equality

Implements `IEquatable<StatRowData>`. Сравнивается по `StatId`.

---

## Использование

### Virtual Scroll / Grid

```csharp
public class HeroGridView
{
    private List<HeroSlotData> _heroes;
    
    public void UpdateHero(HeroSlotData updatedHero)
    {
        var index = _heroes.FindIndex(h => h.Id == updatedHero.HeroId);
        if (index >= 0)
        {
            _heroes[index] = updatedHero;
            RefreshCell(index);
        }
    }
}
```

### Equality

```csharp
var hero1 = new HeroSlotData("hero_1", "Артас", portrait, 45);
var hero2 = new HeroSlotData("hero_1", "Другое имя", otherPortrait, 50);

// Они равны, т.к. Id совпадает
Assert.True(hero1 == hero2);
```

---

## Архитектура

### Почему readonly struct?

1. **Performance** — value type, без аллокаций на куче
2. **Immutability** — нельзя изменить после создания
3. **Cache locality** — лучше для больших списков (grid)
4. **Equality** — простое сравнение по ID

### Factory Pattern

Для конвертации из Domain Models:

```csharp
public static class ElementDataFactory
{
    public static HeroSlotData CreateHeroSlot(HeroModel hero)
    {
        return new HeroSlotData(
            heroId: hero.Id,
            name: hero.Name,
            portrait: hero.Portrait,
            level: hero.Level,
            healthPercent: (float)hero.CurrentHealth / hero.MaxHealth
        );
    }
}
```
