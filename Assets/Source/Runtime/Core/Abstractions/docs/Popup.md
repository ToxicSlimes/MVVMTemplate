# Popup — Система всплывающих окон

> Система popup/tooltip с пулингом, позиционированием и анимациями.

## 📋 Обзор интерфейсов

| Интерфейс | Назначение | Путь |
|-----------|------------|------|
| `IPopup` | Базовый popup | `Popup/IPopup.cs` |
| `IPopupService` | Управление popup | `Popup/IPopupService.cs` |
| `IPopupPool` | Пул popup | `Popup/IPopupPoolFactory.cs` |
| `IPopupPoolFactory` | Фабрика пулов | `Popup/IPopupPoolFactory.cs` |
| `PopupTrigger` | Триггер на элементе | `Popup/PopupTrigger.cs` |

### Вспомогательные типы

| Тип | Назначение |
|-----|------------|
| `PopupPosition` | Позиция относительно target |
| `PopupTriggerType` | Тип триггера (Hover/Click/Manual) |
| `PopupShowParams` | Параметры показа |

---

## IPopup

**Путь:** `Popup/IPopup.cs`

```csharp
public interface IPopup : IDisposable
{
    RectTransform RectTransform { get; }
    RectTransform Target { get; }
    bool IsVisible { get; }
    
    void SetTarget(RectTransform target);
    void SetCanvasRect(RectTransform canvasRect);
    void SetMousePositionProvider(Func<Vector2> provider);
    void UpdatePosition(PopupShowParams showParams);
    
    UniTask ShowAsync();
    UniTask HideAsync();
    
    void OnGetFromPool();
    void OnReturnToPool();
    
    void SetData(IPopupData data);
}

public interface IPopup<TData> : IPopup where TData : IPopupData
{
    TData Data { get; }
    void SetData(TData data);
}
```

### Назначение

Базовый контракт для всех popup. Управляет:
- Позиционированием относительно target
- Lifecycle (show/hide с анимацией)
- Pool integration

### Реализация

**Путь:** `Core/Infrastructure/Services/UI/Popup/PopupBase.cs`

### Пример реализации

```csharp
public class TooltipPopup : PopupBase<TooltipData>
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    
    protected override void OnDataChanged(TooltipData data)
    {
        _titleText.text = data.Title;
        _descriptionText.text = data.Description;
        _descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(data.Description));
    }
}

public class TooltipData : IPopupData
{
    public string Title { get; set; }
    public string Description { get; set; }
    
    public TooltipData(string title, string description = null)
    {
        Title = title;
        Description = description;
    }
}
```

---

## IPopupService

**Путь:** `Popup/IPopupService.cs`

```csharp
public interface IPopupService
{
    /// <summary>
    /// Показать типизированный popup с данными.
    /// </summary>
    UniTask<TPopup> ShowAsync<TPopup, TData>(
        RectTransform target,
        TData data,
        PopupShowParams showParams = null
    ) where TPopup : class, IPopup<TData> where TData : IPopupData;

    /// <summary>
    /// Показать popup без данных.
    /// </summary>
    UniTask<TPopup> ShowAsync<TPopup>(
        RectTransform target,
        PopupShowParams showParams = null
    ) where TPopup : class, IPopup;

    /// <summary>
    /// Показать popup из prefab.
    /// </summary>
    UniTask<IPopup> ShowFromPrefabAsync(
        GameObject prefab,
        RectTransform target,
        PopupShowParams showParams = null);

    /// <summary>
    /// Показать popup из prefab с данными.
    /// </summary>
    UniTask<IPopup> ShowFromPrefabAsync(
        GameObject prefab,
        RectTransform target,
        IPopupData data,
        PopupShowParams showParams = null);

    /// <summary>
    /// Скрыть конкретный popup.
    /// </summary>
    UniTask HideAsync(IPopup popup);
    
    /// <summary>
    /// Скрыть все popup.
    /// </summary>
    UniTask HideAllAsync();
    
    /// <summary>
    /// Скрыть верхний popup.
    /// </summary>
    UniTask HideTopPopupAsync();
    
    void HideTopPopup();
    void HideAll();
    
    /// <summary>
    /// Зарегистрировать prefab для типа.
    /// </summary>
    void RegisterPrefab<TPopup>(GameObject prefab) where TPopup : class, IPopup;
    
    /// <summary>
    /// Активные popup.
    /// </summary>
    IReadOnlyCollection<IPopup> ActivePopups { get; }
    
    event Action<IPopup> PopupShown;
    event Action<IPopup> PopupHidden;
}
```

### Реализация

**Путь:** `Core/Infrastructure/Services/UI/Popup/PopupService.cs`

### Пример использования

```csharp
// Показ tooltip
var popup = await _popupService.ShowAsync<TooltipPopup, TooltipData>(
    targetRect,
    new TooltipData("Sword of Power", "+10 Attack"),
    PopupShowParams.Top(10f)
);

// Скрытие
await _popupService.HideAsync(popup);

// Скрытие всех
_popupService.HideAll();
```

---

## PopupTrigger

**Путь:** `Popup/PopupTrigger.cs`

```csharp
public class PopupTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    ITooltipTarget
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject _popupPrefab;
    [SerializeField] private PopupTriggerType _triggerType = PopupTriggerType.Hover;

    [Header("Position")]
    [SerializeField] private PopupPosition _position = PopupPosition.Auto;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private float _screenPadding = 10f;

    [Header("Timing")]
    [SerializeField] private float _showDelay = 0.3f;
    [SerializeField] private float _hideDelay = 0f;

    public IPopup CurrentPopup { get; }
    public bool IsShowing { get; }
    
    public void SetDataProvider<TData>(Func<TData> provider) where TData : IPopupData;
    public void SetDataProvider(Func<IPopupData> provider);
    public void SetTooltipData(string title, string description = null);
    
    public UniTask ShowAsync();
    public UniTask HideAsync();
}
```

### Назначение

MonoBehaviour-компонент для автоматического показа popup по событиям.

### Типы триггеров

```csharp
public enum PopupTriggerType
{
    Hover,   // При наведении мыши
    Click,   // При клике
    Manual   // Только через код
}
```

### Пример использования в Inspector

1. Добавить `PopupTrigger` на UI элемент
2. Назначить `_popupPrefab`
3. Выбрать `_triggerType`
4. Настроить `_position` и `_offset`

### Пример программной настройки

```csharp
var trigger = button.GetComponent<PopupTrigger>();

// Динамические данные
trigger.SetDataProvider(() => new TooltipData(
    _heroData.Name,
    $"Level {_heroData.Level}\nHP: {_heroData.HP}"
));

// Простой tooltip
trigger.SetTooltipData("Click to attack", "Deals damage to enemy");
```

---

## PopupPosition

**Путь:** `Popup/PopupPosition.cs`

```csharp
public enum PopupPosition
{
    Auto,         // Автовыбор лучшей позиции
    Top,          // Сверху от target
    Bottom,       // Снизу от target
    Left,         // Слева от target
    Right,        // Справа от target
    TopLeft,      // Сверху-слева
    TopRight,     // Сверху-справа
    BottomLeft,   // Снизу-слева
    BottomRight,  // Снизу-справа
    Center,       // По центру target
    Mouse         // Следует за мышью
}
```

---

## PopupShowParams

**Путь:** `Popup/PopupShowParams.cs`

```csharp
public class PopupShowParams
{
    public PopupPosition Position { get; private set; } = PopupPosition.Auto;
    public Vector2 Offset { get; private set; } = Vector2.zero;
    public float ShowDelay { get; private set; }
    public bool ClampToScreen { get; private set; } = true;
    public float ScreenPadding { get; private set; } = 10f;
    public bool AllowFlip { get; private set; } = true;
    public bool FollowTarget { get; private set; }

    // Factory methods
    public static PopupShowParams Default() => new();
    public static PopupShowParams Top(float offsetY = 10f);
    public static PopupShowParams Bottom(float offsetY = 10f);
    public static PopupShowParams Left(float offsetX = 10f);
    public static PopupShowParams Right(float offsetX = 10f);

    // Fluent setters
    public PopupShowParams SetPosition(PopupPosition position);
    public PopupShowParams SetOffset(Vector2 offset);
    public PopupShowParams SetDelay(float delay);
    public PopupShowParams SetScreenPadding(float padding);
    public PopupShowParams DisableClamping();
    public PopupShowParams DisableFlip();
    public PopupShowParams EnableFollowTarget();
}
```

### Пример

```csharp
// Preset
var popup = await _popupService.ShowAsync<Tooltip>(
    target,
    PopupShowParams.Top(15f)
);

// Fluent
var popup = await _popupService.ShowAsync<Tooltip>(
    target,
    new PopupShowParams()
        .SetPosition(PopupPosition.Right)
        .SetOffset(new Vector2(20f, 0))
        .SetScreenPadding(5f)
        .DisableFlip()
);
```

---

## IPopupPool & IPopupPoolFactory

**Путь:** `Popup/IPopupPoolFactory.cs`

```csharp
public interface IPopupPool
{
    IPopup Get();
    void Return(IPopup popup);
    void Clear();
    int CountActive { get; }
    int CountInactive { get; }
}

public interface IPopupPoolFactory
{
    IPopupPool CreatePool(GameObject prefab, Transform poolContainer);
}
```

### Реализация

**Путь:** `Core/Infrastructure/Services/UI/Popup/PopupPool.cs`
**Путь:** `Core/Infrastructure/Services/UI/Popup/PopupPoolFactory.cs`

---

## 🔄 Flow

```
PopupTrigger (MonoBehaviour)
    │
    ├── OnPointerEnter (Hover)
    │   └── ShowWithDelay()
    │
    └── OnPointerClick (Click)
        └── Toggle()
            │
            ▼
        IPopupService.ShowFromPrefabAsync()
            │
            ▼
        IPopupPool.Get()
            │
            ▼
        IPopup.OnGetFromPool()
            │
            ▼
        IPopup.SetData(data)
            │
            ▼
        IPopup.UpdatePosition(params)
            │
            ▼
        IPopup.ShowAsync()
```

---

## 🎨 Создание кастомного Popup

1. **Создать Data класс:**

```csharp
public class HeroTooltipData : IPopupData
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public Sprite Avatar { get; set; }
}
```

2. **Создать Popup класс:**

```csharp
public class HeroTooltipPopup : PopupBase<HeroTooltipData>
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Image _avatarImage;
    
    protected override void OnDataChanged(HeroTooltipData data)
    {
        _nameText.text = data.Name;
        _levelText.text = $"Lv.{data.Level}";
        _hpSlider.value = (float)data.HP / data.MaxHP;
        _avatarImage.sprite = data.Avatar;
    }
}
```

3. **Создать Prefab** с `HeroTooltipPopup` компонентом

4. **Использовать:**

```csharp
// Через сервис
var popup = await _popupService.ShowAsync<HeroTooltipPopup, HeroTooltipData>(
    heroCard.RectTransform,
    new HeroTooltipData
    {
        Name = hero.Name,
        Level = hero.Level,
        HP = hero.CurrentHP,
        MaxHP = hero.MaxHP,
        Avatar = hero.Avatar
    }
);

// Через триггер
heroCard.PopupTrigger.SetDataProvider(() => new HeroTooltipData { ... });
```
