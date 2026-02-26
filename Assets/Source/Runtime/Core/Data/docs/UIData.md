# UI Data Structures

Структуры данных для UI компонентов.

**Путь:** `Data/UI/`  
**Namespace:** `Core.Data.UI`

---

## IPopupData

**Путь:** `UI/IPopupData.cs`

Базовый интерфейс для данных popup'ов. Marker interface без методов.

```csharp
public interface IPopupData
{
}
```

### Использование

```csharp
public class PopupService
{
    public void Show<T>(T data) where T : IPopupData
    {
        // Определяем тип popup по типу данных
        var popup = ResolvePopup<T>();
        popup.SetData(data);
        popup.Show();
    }
}
```

---

## TooltipData

**Путь:** `UI/TooltipData.cs`

Данные для базового тултипа. **Struct** (value type).

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Title | `string` | Заголовок тултипа |
| Description | `string` | Описание (опционально) |

### Конструктор

```csharp
public TooltipData(string title, string description = null)
```

### Factory Method

```csharp
public static TooltipData Simple(string text) => new(text);
```

### Использование

```csharp
// С заголовком и описанием
var tooltip = new TooltipData("Атака", "Наносит 50 урона врагу");
tooltipService.Show(tooltip);

// Простой тултип
var simpleTooltip = TooltipData.Simple("Нажмите для продолжения");
tooltipService.Show(simpleTooltip);
```

---

## HeroTooltipData

**Путь:** `UI/HeroTooltipData.cs`

Данные для тултипа героя. **Class**.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Name | `string` | Имя героя |
| Level | `int` | Уровень |
| HealthPercent | `float` | Процент здоровья (0-1) |
| IsDead | `bool` | Мёртв ли герой |
| Class | `string` | Класс героя |

### Использование

```csharp
var heroTooltip = new HeroTooltipData
{
    Name = "Артас",
    Level = 42,
    HealthPercent = 0.75f,
    IsDead = false,
    Class = "Паладин"
};

tooltipService.Show(heroTooltip);
```

---

## AbilityTooltipData

**Путь:** `UI/AbilityTooltipData.cs`

Данные для тултипа способности. **Class**.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Name | `string` | Название способности |
| Description | `string` | Описание |
| CooldownTurns | `int` | Кулдаун в ходах |
| ManaCost | `int` | Стоимость маны |
| IsUnlocked | `bool` | Разблокирована ли |

### Использование

```csharp
var abilityTooltip = new AbilityTooltipData
{
    Name = "Огненный шар",
    Description = "Запускает огненный шар, наносящий 100 урона всем врагам в области",
    CooldownTurns = 3,
    ManaCost = 50,
    IsUnlocked = true
};

tooltipService.Show(abilityTooltip);
```

---

## Паттерн использования

### UI Popup System

```csharp
public interface IPopup<T> where T : IPopupData
{
    void SetData(T data);
    void Show();
    void Hide();
}

public class TooltipPopup : IPopup<TooltipData>
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    
    public void SetData(TooltipData data)
    {
        titleText.text = data.Title;
        descriptionText.text = data.Description;
        descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(data.Description));
    }
    
    public void Show() { /* ... */ }
    public void Hide() { /* ... */ }
}

public class HeroTooltipPopup : IPopup<HeroTooltipData>
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject deadOverlay;
    
    public void SetData(HeroTooltipData data)
    {
        nameText.text = data.Name;
        levelText.text = $"Уровень {data.Level}";
        healthBar.value = data.HealthPercent;
        deadOverlay.SetActive(data.IsDead);
    }
    
    public void Show() { /* ... */ }
    public void Hide() { /* ... */ }
}
```

### Generic Popup Service

```csharp
public class PopupService
{
    private readonly Dictionary<Type, object> _popups = new();
    
    public void Register<T>(IPopup<T> popup) where T : IPopupData
    {
        _popups[typeof(T)] = popup;
    }
    
    public void Show<T>(T data) where T : IPopupData
    {
        if (_popups.TryGetValue(typeof(T), out var popup))
        {
            var typedPopup = (IPopup<T>)popup;
            typedPopup.SetData(data);
            typedPopup.Show();
        }
    }
    
    public void Hide<T>() where T : IPopupData
    {
        if (_popups.TryGetValue(typeof(T), out var popup))
        {
            var typedPopup = (IPopup<T>)popup;
            typedPopup.Hide();
        }
    }
}
```

---

## Связь с Domain Models

UI Data structures используются для отображения Domain Models:

```csharp
// Конвертация Domain Model → UI Data
public static class UiDataFactory
{
    public static HeroTooltipData CreateHeroTooltip(HeroModel hero)
    {
        return new HeroTooltipData
        {
            Name = hero.Name,
            Level = hero.Level,
            HealthPercent = (float)hero.CurrentHealth / hero.MaxHealth,
            IsDead = hero.CurrentHealth <= 0,
            Class = hero.Class.ToString()
        };
    }
    
    public static AbilityTooltipData CreateAbilityTooltip(AbilityModel ability)
    {
        return new AbilityTooltipData
        {
            Name = ability.Name,
            Description = ability.Description,
            CooldownTurns = ability.Cooldown,
            ManaCost = ability.ManaCost,
            IsUnlocked = ability.IsUnlocked
        };
    }
}
```

---

## Таблица структур

| Тип | Value/Reference | Implements | Назначение |
|-----|-----------------|------------|------------|
| `IPopupData` | Interface | — | Базовый интерфейс |
| `TooltipData` | Struct | `IPopupData` | Простой тултип |
| `HeroTooltipData` | Class | `IPopupData` | Тултип героя |
| `AbilityTooltipData` | Class | `IPopupData` | Тултип способности |
