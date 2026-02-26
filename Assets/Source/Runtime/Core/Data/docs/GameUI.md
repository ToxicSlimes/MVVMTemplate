# Game UI Data

Data models для специфичных игровых UI панелей.

**Путь:** `Domain/Models/`  
**Namespace:** `Core.Data.Domain.Models.*`

---

## HeroDetailPanel

**Namespace:** `Core.Data.Domain.Models.HeroDetailPanel`

### HeroDetailData

**Путь:** `Domain/Models/HeroDetailPanel/HeroDetailData.cs`

**Struct** для данных панели деталей героя.

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID героя |
| Name | `string` | Имя |
| Class | `string` | Класс |
| Race | `string` | Раса |
| Level | `int` | Уровень |
| CurrentXP | `int` | Текущий опыт |
| MaxXP | `int` | Опыт для следующего уровня |
| Attributes | `Dictionary<string, int>` | Атрибуты (Strength, Agility, etc.) |
| Abilities | `List<AbilityData>` | Список способностей |

#### Использование

```csharp
var heroDetail = new HeroDetailData
{
    Id = "hero_1",
    Name = "Артас",
    Class = "Паладин",
    Level = 45,
    Attributes = new Dictionary<string, int>
    {
        { "Strength", 150 },
        { "Agility", 80 },
        { "Intelligence", 45 }
    },
    Abilities = new List<AbilityData> { /* ... */ }
};
```

---

### AbilityData

**Путь:** `Domain/Models/HeroDetailPanel/AbilityData.cs`

**Struct** для данных способности в панели героя.

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID способности |
| Name | `string` | Название |
| Description | `string` | Описание |
| IconPath | `string` | Путь к иконке |
| Level | `int` | Уровень способности |
| MaxLevel | `int` | Макс. уровень |
| Cooldown | `int` | Кулдаун в ходах |
| ManaCost | `int` | Стоимость маны |

---

## HeroesGridPanel

**Namespace:** `Core.Data.Domain.Models.HeroesGridPanel`

### HeroCardData

**Путь:** `Domain/Models/HeroesGridPanel/HeroesGridPanelData.cs`

**Readonly struct** для карточки героя в гриде.

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID героя |
| Name | `string` | Имя |
| Class | `string` | Класс |
| Level | `int` | Уровень |
| Health | `int` | Текущее здоровье |
| MaxHealth | `int` | Максимальное здоровье |
| IsSelected | `bool` | Выбран ли |
| IsNew | `bool` | Новый герой (не видели) |
| NotificationCount | `int` | Количество уведомлений |
| Avatar | `Sprite` | Аватар |

#### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| HealthPercent | `float` | Health / MaxHealth |

#### Конструктор

```csharp
public HeroCardData(
    string id,
    string name,
    string heroClass,
    int level,
    int health,
    int maxHealth,
    bool isSelected = false,
    bool isNew = false,
    int notificationCount = 0,
    Sprite avatar = null)
```

---

### HeroesGridTab

```csharp
public enum HeroesGridTab
{
    MyHeroes = 0,
    Shop = 1,
    DeadHeroes = 2
}
```

---

### HeroesGridMode

```csharp
public enum HeroesGridMode
{
    Upgrades,          // Режим улучшений
    Healing,           // Режим лечения
    TeamManagement     // Управление командой
}
```

---

### HeroesGridTestData

**⚠️ Obsolete** — Test data, replace with backend API.

```csharp
[Obsolete("Test data - replace with backend API")]
public static class HeroesGridTestData
{
    public static IReadOnlyList<HeroCardData> GetTestHeroes() { /* ... */ }
    public static IReadOnlyList<HeroCardData> GetTestShopHeroes() { /* ... */ }
    public static IReadOnlyList<HeroCardData> GetTestDeadHeroes() { /* ... */ }
}
```

---

## Main View

**Namespace:** `Core.Data.Domain.Models.Main`

**Путь:** `Domain/Models/Main/MainViewData.cs`

### HeroSlotData

**Readonly struct** для слота героя в главном экране.

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID героя |
| Name | `string` | Имя |
| Class | `string` | Класс |
| Level | `int` | Уровень |
| Health | `int` | Текущее здоровье |
| MaxHealth | `int` | Максимальное здоровье |
| IsSelected | `bool` | Выбран ли |

#### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| HealthPercent | `float` | Health / MaxHealth |

#### Конструкторы

```csharp
public HeroSlotData(string id, string name, string heroClass, int level, bool isSelected)

public HeroSlotData(string id, string name, string heroClass, int level, 
                   int health, int maxHealth, bool isSelected)
```

---

### StatRowData

**Readonly struct** для строки статов.

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Name | `string` | Название стата |
| Value | `int` | Текущее значение |
| MaxValue | `int` | Максимальное значение |

#### Конструктор

```csharp
public StatRowData(string name, int value, int maxValue)
```

---

### ResourceData

**Readonly struct** для ресурса (золото, кристаллы и т.д.).

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Type | `string` | Тип ресурса |
| Amount | `int` | Количество |
| Description | `string` | Описание |

#### Конструктор

```csharp
public ResourceData(string type, int amount, string description = null)
```

---

### MainTestData

**⚠️ Obsolete** — Test data, replace with backend.

```csharp
[Obsolete("Test data - replace with backend")]
public static class MainTestData
{
    public static IReadOnlyList<HeroSlotData> GetTestHeroes() { /* ... */ }
    public static IReadOnlyList<StatRowData> GetTestStats() { /* ... */ }
    public static IReadOnlyList<ResourceData> GetTestResources() { /* ... */ }
}
```

---

## ServicePanel

**Namespace:** `Core.Data.Domain.Models.ServicePanel`

### ServiceRowData

**Путь:** `Domain/Models/ServicePanel/ServiceRowData.cs`

**Readonly struct** для ряда сервисной панели.

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| RowIndex | `int` | Индекс ряда |
| Label | `string` | Название ряда (Forge, Hospital, etc.) |
| Slots | `IReadOnlyList<ServiceSlotData>` | Список слотов в ряду |

#### Конструктор

```csharp
public ServiceRowData(int rowIndex, string label, IReadOnlyList<ServiceSlotData> slots)
```

---

### ServiceSlotData

**Путь:** `Domain/Models/ServicePanel/ServiceSlotData.cs`

**Class** для данных слота сервиса.

#### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| SlotId | `string` | ID слота |
| State | `ServiceSlotState` | Состояние (enum) |
| HeroId | `string` | ID героя в слоте |
| HeroName | `string` | Имя героя |
| HeroAvatar | `Texture2D` | Аватар героя |
| Progress | `float` | Прогресс (0-1) |
| StartTime | `DateTime?` | Время начала |
| EndTime | `DateTime?` | Время завершения |
| UnlockCost | `int` | Стоимость разблокировки |

#### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| IsLocked | `bool` | Заблокирован ли |
| IsEmpty | `bool` | Пустой ли |
| IsActive | `bool` | Активен ли (герой работает) |
| IsComplete | `bool` | Завершён ли |
| RemainingTime | `TimeSpan?` | Оставшееся время |
| FormattedTime | `string` | Отформатированное время ("2:30:15") |

---

### ServiceSlotState

**Путь:** `Domain/Models/ServicePanel/ServiceSlotState.cs`

```csharp
public enum ServiceSlotState
{
    Locked,     // Заблокирован, требует разблокировки
    Empty,      // Разблокирован, но пуст
    Active,     // Герой работает
    Complete    // Работа завершена, можно забрать
}
```

---

## Архитектура

### UI-First Design

Эти модели созданы **UI-first** — они максимально близки к нуждам UI и не зависят от бэкенда.

### Test Data Pattern

```csharp
#if UNITY_EDITOR
    var heroes = HeroesGridTestData.GetTestHeroes();
#else
    var heroes = await heroService.LoadHeroes();
#endif
```

### Adapter Pattern

```csharp
public static class GameUiAdapter
{
    public static HeroCardData ToHeroCard(HeroModel model)
    {
        return new HeroCardData(
            id: model.Id,
            name: model.Name,
            heroClass: model.Class,
            level: model.Level,
            health: model.CurrentHealth,
            maxHealth: model.MaxHealth,
            avatar: model.AvatarSprite
        );
    }
}
```

### Reactive UI

```csharp
public class HeroesGridViewModel
{
    private readonly ReactiveProperty<IReadOnlyList<HeroCardData>> _heroes = new();
    
    public IObservable<IReadOnlyList<HeroCardData>> Heroes => _heroes;
    
    public void UpdateHeroes(List<HeroModel> models)
    {
        var cards = models.Select(GameUiAdapter.ToHeroCard).ToList();
        _heroes.Value = cards;
    }
}
```
