# Handlers — Обработчики запросов

Handlers — классы, которые содержат бизнес-логику обработки requests и notifications.

**Namespace:** `Core.Infrastructure.Mediator.Handlers`

---

## Обзор интерфейсов

| Интерфейс | Назначение | Соотношение |
|-----------|------------|-------------|
| `IRequestHandler<TRequest, TResponse>` | Обработка request с ответом | 1 request : 1 handler |
| `IRequestHandler<TRequest>` | Обработка request без ответа | 1 request : 1 handler |
| `INotificationHandler<TNotification>` | Обработка notification | 1 notification : N handlers |
| `IStreamRequestHandler<TRequest, TResponse>` | Обработка stream request | 1 request : 1 handler |

---

## IRequestHandler<TRequest, TResponse>

Обрабатывает request и возвращает response.

```csharp
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    UniTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
```

### Пример — Query Handler

```csharp
public class GetHeroQueryHandler : IRequestHandler<GetHeroQuery, Hero>
{
    private readonly IHeroRepository _repository;
    private readonly ILogger _logger;

    public GetHeroQueryHandler(IHeroRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async UniTask<Hero> Handle(GetHeroQuery request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        _logger.Debug($"Loading hero: {request.HeroId}");
        
        var hero = await _repository.GetByIdAsync(request.HeroId, ct);
        
        if (hero == null)
            throw new HeroNotFoundException(request.HeroId);
        
        return hero;
    }
}
```

### Пример — Command Handler с результатом

```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderId>
{
    private readonly IOrderService _orderService;
    private readonly IMediator _mediator;

    public CreateOrderHandler(IOrderService orderService, IMediator mediator)
    {
        _orderService = orderService;
        _mediator = mediator;
    }

    public async UniTask<OrderId> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var orderId = await _orderService.CreateAsync(request.Data, ct);
        
        // Публикуем событие
        await _mediator.Publish(new OrderCreatedEvent(orderId), ct);
        
        return orderId;
    }
}
```

---

## IRequestHandler<TRequest> (без ответа)

Обрабатывает command и возвращает `Nothing`.

```csharp
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Nothing>
    where TRequest : IRequest<Nothing>
{
}
```

### Пример — Command Handler

```csharp
public class SaveSettingsHandler : IRequestHandler<SaveSettingsCommand>
{
    private readonly ISettingsService _settings;

    public SaveSettingsHandler(ISettingsService settings)
    {
        _settings = settings;
    }

    public async UniTask<Nothing> Handle(SaveSettingsCommand request, CancellationToken ct)
    {
        await _settings.SaveAsync(request.Settings, ct);
        return Nothing.Value;
    }
}
```

### Пример — Delete Handler

```csharp
public class DeleteHeroHandler : IRequestHandler<DeleteHeroCommand>
{
    private readonly IHeroRepository _repository;
    private readonly IMediator _mediator;

    public DeleteHeroHandler(IHeroRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async UniTask<Nothing> Handle(DeleteHeroCommand request, CancellationToken ct)
    {
        var hero = await _repository.GetByIdAsync(request.HeroId, ct);
        
        if (hero == null)
            throw new HeroNotFoundException(request.HeroId);
        
        await _repository.DeleteAsync(request.HeroId, ct);
        await _mediator.Publish(new HeroDeletedEvent(hero), ct);
        
        return Nothing.Value;
    }
}
```

---

## INotificationHandler<TNotification>

Обрабатывает notification. Может быть несколько handlers на один тип notification.

```csharp
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    UniTask Handle(TNotification notification, CancellationToken cancellationToken);
}
```

### Пример — Несколько handlers для одного события

```csharp
// Notification
public readonly struct OrderCreatedEvent : INotification
{
    public readonly OrderId OrderId;
    public readonly Order Order;
    
    public OrderCreatedEvent(OrderId orderId, Order order)
    {
        OrderId = orderId;
        Order = order;
    }
}

// Handler 1: Отправка email
public class SendOrderEmailHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IEmailService _email;

    public SendOrderEmailHandler(IEmailService email)
    {
        _email = email;
    }

    public async UniTask Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        await _email.SendOrderConfirmation(notification.Order, ct);
    }
}

// Handler 2: Обновление инвентаря
public class UpdateInventoryHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IInventoryService _inventory;

    public UpdateInventoryHandler(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    public async UniTask Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        await _inventory.DeductItems(notification.Order.Items, ct);
    }
}

// Handler 3: Аналитика
public class TrackOrderAnalyticsHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IAnalytics _analytics;

    public TrackOrderAnalyticsHandler(IAnalytics analytics)
    {
        _analytics = analytics;
    }

    public UniTask Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        _analytics.Track("order_created", new { orderId = notification.OrderId.Value });
        return UniTask.CompletedTask;
    }
}
```

### ViewModel как NotificationHandler

ViewModel может реагировать на события напрямую:

```csharp
public class MainViewModel : DisposableObject, 
    INotificationHandler<HeroSelectedEvent>,
    INotificationHandler<SettingsChangedEvent>
{
    private readonly IMainModel _model;

    public UniTask Handle(HeroSelectedEvent notification, CancellationToken ct)
    {
        _model.SetSelectedHero(notification.HeroId);
        return UniTask.CompletedTask;
    }

    public UniTask Handle(SettingsChangedEvent notification, CancellationToken ct)
    {
        RefreshUI();
        return UniTask.CompletedTask;
    }
}
```

> ⚠️ **Важно:** Не забудь зарегистрировать ViewModel как handler — см. [Registration.md](./Registration.md)

---

## IStreamRequestHandler<TRequest, TResponse>

Возвращает асинхронную последовательность значений.

```csharp
public interface IStreamRequestHandler<in TRequest, out TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    IUniTaskAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
```

### Пример — Прогресс загрузки

```csharp
public class LoadAssetsHandler : IStreamRequestHandler<LoadAssetsRequest, float>
{
    private readonly IAssetService _assets;

    public LoadAssetsHandler(IAssetService assets)
    {
        _assets = assets;
    }

    public async IUniTaskAsyncEnumerable<float> Handle(
        LoadAssetsRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var total = request.Paths.Length;
        
        for (int i = 0; i < total; i++)
        {
            ct.ThrowIfCancellationRequested();
            
            await _assets.LoadAsync(request.Paths[i], ct);
            yield return (float)(i + 1) / total; // 0.0 → 1.0
        }
    }
}
```

### Пример — Пагинация

```csharp
public class GetHeroesPagedHandler : IStreamRequestHandler<GetHeroesPagedRequest, Hero>
{
    private readonly IHeroRepository _repository;

    public GetHeroesPagedHandler(IHeroRepository repository)
    {
        _repository = repository;
    }

    public async IUniTaskAsyncEnumerable<Hero> Handle(
        GetHeroesPagedRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        int offset = 0;
        
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            
            var batch = await _repository.GetBatchAsync(offset, request.PageSize, ct);
            
            if (batch.Count == 0)
                yield break;
            
            foreach (var hero in batch)
                yield return hero;
            
            offset += batch.Count;
            
            if (batch.Count < request.PageSize)
                yield break;
        }
    }
}
```

---

## Generic Handlers

Для типовых операций используй generic handlers:

### Пример — Show/Hide View

```csharp
// Requests
public readonly struct ShowEmbeddedViewRequest<TViewModel> : IRequest<bool>
    where TViewModel : DisposableObject
{
    public readonly IViewContext Context;
    public ShowEmbeddedViewRequest(IViewContext context = null) => Context = context ?? EmptyContext.Instance;
}

public readonly struct HideEmbeddedViewRequest<TViewModel> : IRequest
    where TViewModel : DisposableObject
{
}

// Generic Handler для Show
public sealed class EmbeddedViewRequestHandler<TViewModel>
    : IRequestHandler<ShowEmbeddedViewRequest<TViewModel>, bool>
    where TViewModel : DisposableObject
{
    private readonly IEmbeddedPanelHost _host;

    public EmbeddedViewRequestHandler(IEmbeddedPanelHost host)
    {
        _host = host;
    }

    public UniTask<bool> Handle(ShowEmbeddedViewRequest<TViewModel> request, CancellationToken ct)
    {
        return _host.ShowEmbeddedAsync<TViewModel>(request.Context, ct);
    }
}

// Generic Handler для Hide
public sealed class HideEmbeddedViewHandler<TViewModel>
    : IRequestHandler<HideEmbeddedViewRequest<TViewModel>>
    where TViewModel : DisposableObject
{
    private readonly IEmbeddedPanelHost _host;

    public HideEmbeddedViewHandler(IEmbeddedPanelHost host)
    {
        _host = host;
    }

    public async UniTask<Nothing> Handle(HideEmbeddedViewRequest<TViewModel> request, CancellationToken ct)
    {
        await _host.HideEmbeddedAsync<TViewModel>(ct);
        return Nothing.Value;
    }
}
```

### Использование

```csharp
// Показать панель
await _mediator.Send(new ShowEmbeddedViewRequest<HeroesGridPanelViewModel>(
    new HeroesGridContext(HeroesGridMode.Select)));

// Скрыть панель
await _mediator.Send(new HideEmbeddedViewRequest<HeroesGridPanelViewModel>());
```

---

## Dependency Injection

Handlers резолвятся через Reflex DI, поэтому можно инжектить любые зависимости:

```csharp
public class ComplexHandler : IRequestHandler<ComplexQuery, Result>
{
    private readonly IRepository _repository;
    private readonly ICache _cache;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly IValidator<ComplexQuery> _validator;

    public ComplexHandler(
        IRepository repository,
        ICache cache,
        IMediator mediator,
        ILogger logger,
        IValidator<ComplexQuery> validator)
    {
        _repository = repository;
        _cache = cache;
        _mediator = mediator;
        _logger = logger;
        _validator = validator;
    }

    public async UniTask<Result> Handle(ComplexQuery request, CancellationToken ct)
    {
        // Валидация
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        // Кеширование
        var cacheKey = $"complex:{request.Id}";
        if (_cache.TryGet<Result>(cacheKey, out var cached))
            return cached;

        // Бизнес-логика
        var result = await _repository.GetAsync(request.Id, ct);
        
        // Кешируем результат
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        
        // Публикуем событие
        await _mediator.Publish(new ResultLoadedEvent(result), ct);
        
        return result;
    }
}
```

---

## Регистрация Handlers

См. [Registration.md](./Registration.md) для полной информации.

### Краткий пример

```csharp
builder.AddMediator(cfg =>
{
    // Request с ответом
    cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
    
    // Request без ответа (command)
    cfg.AddHandler<SaveSettingsCommand, SaveSettingsHandler>();
    
    // Notification
    cfg.AddNotificationHandler<OrderCreatedEvent, SendOrderEmailHandler>();
    cfg.AddNotificationHandler<OrderCreatedEvent, UpdateInventoryHandler>();
    
    // Stream
    cfg.AddStreamHandler<LoadAssetsRequest, float, LoadAssetsHandler>();
});
```

---

## Best Practices

### ✅ Один Handler — одна ответственность

```csharp
// ✅ Правильно — Handler делает одну вещь
public class GetHeroQueryHandler : IRequestHandler<GetHeroQuery, Hero>
{
    public UniTask<Hero> Handle(GetHeroQuery request, CancellationToken ct)
    {
        return _repository.GetByIdAsync(request.HeroId, ct);
    }
}

// ❌ Неправильно — Handler делает много вещей
public class HeroHandler : 
    IRequestHandler<GetHeroQuery, Hero>,
    IRequestHandler<SaveHeroCommand>,
    IRequestHandler<DeleteHeroCommand>
{
    // Слишком много ответственности
}
```

### ✅ Всегда проверяй CancellationToken

```csharp
public async UniTask<Hero> Handle(GetHeroQuery request, CancellationToken ct)
{
    ct.ThrowIfCancellationRequested();
    
    var hero = await _repository.GetByIdAsync(request.HeroId, ct);
    
    ct.ThrowIfCancellationRequested();
    
    var skills = await _skillsService.GetForHeroAsync(hero.Id, ct);
    hero.Skills = skills;
    
    return hero;
}
```

### ✅ Используй [EnumeratorCancellation] для streams

```csharp
public async IUniTaskAsyncEnumerable<float> Handle(
    LoadAssetsRequest request,
    [EnumeratorCancellation] CancellationToken ct) // ← Атрибут обязателен
{
    for (int i = 0; i < request.Paths.Length; i++)
    {
        ct.ThrowIfCancellationRequested();
        yield return (float)i / request.Paths.Length;
    }
}
```

### ✅ Логируй начало и конец важных операций

```csharp
public async UniTask<Result> Handle(ImportDataCommand request, CancellationToken ct)
{
    _logger.Info($"Starting import: {request.FileName}");
    
    try
    {
        var result = await DoImport(request, ct);
        _logger.Info($"Import completed: {result.RecordsImported} records");
        return result;
    }
    catch (Exception ex)
    {
        _logger.Error($"Import failed: {ex.Message}");
        throw;
    }
}
```

### ✅ Не используй mediator внутри handler если можно напрямую

```csharp
// ✅ Правильно — прямой вызов сервиса
public async UniTask<Hero> Handle(GetHeroQuery request, CancellationToken ct)
{
    return await _repository.GetByIdAsync(request.HeroId, ct);
}

// ❌ Неправильно — лишний уровень абстракции
public async UniTask<Hero> Handle(GetHeroQuery request, CancellationToken ct)
{
    return await _mediator.Send(new InternalGetHeroQuery(request.HeroId), ct);
}
```

### ✅ Публикуй события после завершения операции

```csharp
public async UniTask<Nothing> Handle(SaveHeroCommand request, CancellationToken ct)
{
    // Сначала сохраняем
    await _repository.SaveAsync(request.Hero, ct);
    
    // Потом публикуем событие
    await _mediator.Publish(new HeroSavedEvent(request.Hero), ct);
    
    return Nothing.Value;
}
```

---

## Тестирование Handlers

```csharp
[Test]
public async UniTask GetHeroQuery_ReturnsHero_WhenExists()
{
    // Arrange
    var repository = Substitute.For<IHeroRepository>();
    var expectedHero = new Hero { Id = "1", Name = "Test" };
    repository.GetByIdAsync("1", default).Returns(expectedHero);
    
    var handler = new GetHeroQueryHandler(repository, NullLogger.Instance);
    
    // Act
    var result = await handler.Handle(new GetHeroQuery("1"), default);
    
    // Assert
    Assert.AreEqual("1", result.Id);
    Assert.AreEqual("Test", result.Name);
}

[Test]
public async UniTask GetHeroQuery_ThrowsNotFound_WhenNotExists()
{
    // Arrange
    var repository = Substitute.For<IHeroRepository>();
    repository.GetByIdAsync("unknown", default).Returns((Hero)null);
    
    var handler = new GetHeroQueryHandler(repository, NullLogger.Instance);
    
    // Act & Assert
    Assert.ThrowsAsync<HeroNotFoundException>(
        async () => await handler.Handle(new GetHeroQuery("unknown"), default));
}
```

---

## См. также

- [Abstractions.md](./Abstractions.md) — базовые интерфейсы
- [Pipeline.md](./Pipeline.md) — cross-cutting concerns
- [Registration.md](./Registration.md) — регистрация handlers
