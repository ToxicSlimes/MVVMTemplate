# UI Abstractions

Контракты для UI компонентов.

## Типы

| Тип | Описание |
|-----|----------|
| `ITooltipTarget` | Интерфейс для элементов, которые могут показывать tooltip |

## ITooltipTarget

```csharp
public interface ITooltipTarget
{
    void SetTooltipData(string title, string description = null);
}
```

## Использование

Реализуй интерфейс на UI элементе, который должен показывать tooltip при наведении:

```csharp
public class ItemSlot : MonoBehaviour, ITooltipTarget
{
    private string _tooltipTitle;
    private string _tooltipDescription;
    
    public void SetTooltipData(string title, string description = null)
    {
        _tooltipTitle = title;
        _tooltipDescription = description;
    }
    
    // Tooltip система автоматически подхватит данные при hover
}
```

## Связь с Popup системой

`ITooltipTarget` работает совместно с `IPopupService` — при наведении на элемент с этим интерфейсом, система показывает tooltip popup с данными из `SetTooltipData`.

## Реализация

Tooltip trigger находится в `Infrastructure/UI/`.
