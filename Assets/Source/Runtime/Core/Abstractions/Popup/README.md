# Popup Abstractions

Контракты для Popup системы — tooltips, info boxes, context menus.

## Структура

```
Popup/
├── IPopup.cs             # Базовый интерфейс попапа
├── IPopupService.cs      # Сервис управления попапами
├── IPopupPoolFactory.cs  # Фабрика и пул попапов
├── PopupShowParams.cs    # Параметры показа (fluent builder)
├── PopupPosition.cs      # Enum позиционирования
├── PopupTrigger.cs       # MonoBehaviour-триггер (hover, click)
└── PopupTriggerType.cs   # Enum триггеров (hover, click, manual)
```

## Interfaces

### IPopup

Базовый интерфейс для всех попапов.

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

### IPopupService

Сервис для показа/скрытия попапов.

```csharp
public interface IPopupService
{
    // Показ типизированного попапа с данными
    UniTask<TPopup> ShowAsync<TPopup, TData>(
        RectTransform target,
        TData data,
        PopupShowParams showParams = null
    ) where TPopup : class, IPopup<TData> where TData : IPopupData;

    // Показ попапа без данных
    UniTask<TPopup> ShowAsync<TPopup>(
        RectTransform target,
        PopupShowParams showParams = null
    ) where TPopup : class, IPopup;

    // Показ из prefab
    UniTask<IPopup> ShowFromPrefabAsync(
        GameObject prefab,
        RectTransform target,
        PopupShowParams showParams = null);

    UniTask<IPopup> ShowFromPrefabAsync(
        GameObject prefab,
        RectTransform target,
        IPopupData data,
        PopupShowParams showParams = null);

    // Скрытие
    UniTask HideAsync(IPopup popup);
    UniTask HideAllAsync();
    UniTask HideTopPopupAsync();
    void HideTopPopup();
    void HideAll();
    
    // Регистрация prefab
    void RegisterPrefab<TPopup>(GameObject prefab) where TPopup : class, IPopup;
    
    // Активные попапы
    IReadOnlyCollection<IPopup> ActivePopups { get; }
    
    // События
    event Action<IPopup> PopupShown;
    event Action<IPopup> PopupHidden;
}
```

### IPopupPool & IPopupPoolFactory

Пулинг попапов для производительности.

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

## Helper Types

### PopupShowParams (Fluent Builder)

```csharp
var params = PopupShowParams.Default()
    .SetPosition(PopupPosition.Top)
    .SetOffset(new Vector2(0, 10f))
    .SetDelay(0.3f)
    .SetScreenPadding(15f)
    .EnableFollowTarget()
    .SetExclusive()          // Скрыть все активные попапы перед показом
    .DisableClamping()       // Отключить screen clamping
    .DisableFlip();          // Отключить auto-flip

// Shortcuts
PopupShowParams.Top(offsetY: 10f);
PopupShowParams.Bottom(offsetY: 10f);
PopupShowParams.Left(offsetX: 10f);
PopupShowParams.Right(offsetX: 10f);
```

### PopupPosition (Enum)

```csharp
public enum PopupPosition
{
    Auto,        // Автоматический выбор позиции
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center,
    Mouse        // Следовать за курсором
}
```

### PopupTriggerType (Enum)

```csharp
public enum PopupTriggerType
{
    Hover,   // Показ при наведении
    Click,   // Показ по клику
    Manual   // Программный показ
}
```

## Usage

```csharp
// Показать tooltip
var tooltip = await _popupService.ShowAsync<TooltipPopup, TooltipData>(
    targetRect,
    new TooltipData { Title = "Item", Description = "Description" },
    PopupShowParams.Top(10f)
);

// Скрыть
await _popupService.HideAsync(tooltip);

// Скрыть все попапы
_popupService.HideAll();
```

## Data Interface

Попапы используют `IPopupData` из `Core.Data.UI`:

```csharp
public interface IPopupData { }

public class TooltipData : IPopupData
{
    public string Title { get; }
    public string Description { get; }
    
    public TooltipData(string title, string description = null);
}
```

## Implementation

Реализации находятся в `Infrastructure/Services/Popup/`.
