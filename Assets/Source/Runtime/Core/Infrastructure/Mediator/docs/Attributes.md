# Attributes — Атрибуты для кодогенерации

Атрибуты для автоматической генерации Mediator запросов.

**Namespace:** `Core.Infrastructure.Mediator.Attributes`

---

## Обзор

| Атрибут | Назначение |
|---------|------------|
| `GenerateViewCommandsAttribute` | Генерация Show/Hide requests для ViewModel |

---

## GenerateViewCommandsAttribute

Маркер-атрибут для автоматической генерации `ShowEmbeddedViewRequest` и `HideEmbeddedViewRequest` для ViewModel.

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GenerateViewCommandsAttribute : Attribute
{
}
```

### Применение

```csharp
[GenerateViewCommands]
public class HeroesGridPanelViewModel : DisposableObject
{
    // Ваш код
}
```

### Что генерируется

Для `HeroesGridPanelViewModel` генератор создаёт:

```csharp
// Сгенерированный код (пример)

public readonly struct ShowEmbeddedViewRequest_HeroesGridPanel 
    : IRequest<bool>
{
    public readonly IViewContext Context;
    
    public ShowEmbeddedViewRequest_HeroesGridPanel(IViewContext context = null)
    {
        Context = context ?? EmptyContext.Instance;
    }
}

public readonly struct HideEmbeddedViewRequest_HeroesGridPanel 
    : IRequest
{
}
```

### Использование сгенерированных requests

```csharp
// Показать панель
await _mediator.Send(
    new ShowEmbeddedViewRequest_HeroesGridPanel(
        new HeroesGridContext(HeroesGridMode.Select)));

// Скрыть панель
await _mediator.Send(new HideEmbeddedViewRequest_HeroesGridPanel());
```

---

## Naming Convention

| ViewModel | Generated Request |
|-----------|------------------|
| `HeroesGridPanelViewModel` | `ShowEmbeddedViewRequest_HeroesGridPanel` |
| `SettingsPanelViewModel` | `ShowEmbeddedViewRequest_SettingsPanel` |
| `InventoryPanelViewModel` | `ShowEmbeddedViewRequest_InventoryPanel` |

Генератор удаляет суффикс `ViewModel` и добавляет префикс `ShowEmbeddedViewRequest_` или `HideEmbeddedViewRequest_`.

---

## Handler Registration

Handlers для сгенерированных requests нужно регистрировать вручную:

```csharp
builder.AddMediator(cfg =>
{
    cfg.AddHandler<
        ShowEmbeddedViewRequest_HeroesGridPanel,
        bool,
        EmbeddedViewRequestHandler<HeroesGridPanelViewModel>>();
    
    cfg.AddHandler<
        HideEmbeddedViewRequest_HeroesGridPanel,
        HideEmbeddedViewHandler<HeroesGridPanelViewModel>>();
});
```

---

## Когда использовать

### ✅ Подходит

- View commands (Show/Hide)
- Стандартизированные операции для ViewModels
- Уменьшение boilerplate кода

### ❌ Не подходит

- Нестандартные requests
- Requests с дополнительными параметрами
- Нет генератора в проекте

---

## Альтернативы

### Вариант 1: Generic Requests

Если генератор недоступен, используй generic requests:

```csharp
public readonly struct ShowEmbeddedViewRequest<TViewModel> : IRequest<bool>
    where TViewModel : DisposableObject
{
    public readonly IViewContext Context;
    
    public ShowEmbeddedViewRequest(IViewContext context = null)
    {
        Context = context ?? EmptyContext.Instance;
    }
}

// Использование
await _mediator.Send(
    new ShowEmbeddedViewRequest<HeroesGridPanelViewModel>(context));
```

### Вариант 2: Manual Requests

Создавай requests вручную:

```csharp
public readonly struct ShowHeroesGridPanelRequest : IRequest<bool>
{
    public readonly HeroesGridContext Context;
    
    public ShowHeroesGridPanelRequest(HeroesGridContext context)
    {
        Context = context;
    }
}
```

---

## Best Practices

### ✅ Применяй к ViewModels с стандартными операциями

```csharp
// ✅ Правильно — стандартный ViewModel
[GenerateViewCommands]
public class SettingsPanelViewModel : DisposableObject
{
}

// ❌ Неправильно — специфичный ViewModel
[GenerateViewCommands]
public class ComplexDashboardViewModel : DisposableObject
{
    // Нужны дополнительные параметры — лучше написать вручную
}
```

### ✅ Документируй сгенерированные requests

```csharp
/// <summary>
/// ViewModel для панели настроек.
/// </summary>
/// <remarks>
/// Генерирует:
/// - ShowEmbeddedViewRequest_SettingsPanel
/// - HideEmbeddedViewRequest_SettingsPanel
/// </remarks>
[GenerateViewCommands]
public class SettingsPanelViewModel : DisposableObject
{
}
```

---

## Troubleshooting

### Генератор не работает

1. Проверь наличие генератора в проекте
2. Пересобери проект (`Build → Rebuild All`)
3. Проверь namespace — должен быть `Core.Infrastructure.Mediator.Attributes`

### Request не генерируется

1. Атрибут применён к классу?
2. Класс наследует `DisposableObject` или подходит под условия генератора?
3. Проверь логи сборки на ошибки

### Naming conflicts

Если имена конфликтуют, используй explicit requests:

```csharp
// Если генератор создаёт конфликтующие имена
public readonly struct ShowHeroGridRequest : IRequest<bool> // Вручную
{
}
```

---

## См. также

- [Abstractions.md](./Abstractions.md) — IRequest интерфейсы
- [Handlers.md](./Handlers.md) — generic handlers
- [Registration.md](./Registration.md) — регистрация handlers
