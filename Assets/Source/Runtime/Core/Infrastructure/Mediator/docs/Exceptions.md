# Exceptions — Исключения Mediator

Кастомные исключения, которые выбрасывает Mediator.

**Namespace:** `Core.Infrastructure.Mediator.Exceptions`

---

## Обзор

| Исключение | Когда выбрасывается |
|-----------|---------------------|
| `MediatorException` | Базовое исключение для всех ошибок Mediator |
| `HandlerNotFoundException` | Handler не зарегистрирован для request/stream |

---

## MediatorException

Базовое исключение, от которого наследуются все ошибки Mediator.

```csharp
public class MediatorException : Exception
{
    public MediatorException() { }
    
    public MediatorException(string message) : base(message) { }
    
    public MediatorException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

### Когда выбрасывается

| Ситуация | Сообщение |
|----------|-----------|
| Wrapper creation failed | `Failed to create handler wrapper for {type}` |
| Invalid request type | `Type {type} does not implement IRequest<>` |
| Internal errors | Различные внутренние ошибки |

### Примеры

```csharp
try
{
    await _mediator.Send(invalidRequest, ct);
}
catch (MediatorException ex)
{
    _logger.Error($"Mediator error: {ex.Message}");
}
```

---

## HandlerNotFoundException

Выбрасывается, когда handler не зарегистрирован для request.

```csharp
public sealed class HandlerNotFoundException : MediatorException
{
    public Type RequestType { get; }

    public HandlerNotFoundException(Type requestType)
        : base($"No handler registered for request type {requestType.FullName}")
    {
        RequestType = requestType;
    }

    public HandlerNotFoundException(Type requestType, string message)
        : base(message)
    {
        RequestType = requestType;
    }

    public HandlerNotFoundException(Type requestType, string message, Exception innerException)
        : base(message, innerException)
    {
        RequestType = requestType;
    }
}
```

### Свойства

| Свойство | Тип | Описание |
|----------|-----|----------|
| `RequestType` | `Type` | Тип request, для которого не найден handler |

### Когда выбрасывается

```csharp
// 1. Request handler не зарегистрирован
await _mediator.Send(new UnregisteredQuery(), ct);
// ❌ HandlerNotFoundException: No handler registered for request type UnregisteredQuery

// 2. Stream handler не зарегистрирован
await foreach (var item in _mediator.CreateStream(new UnregisteredStreamRequest(), ct))
{
    // ❌ HandlerNotFoundException
}
```

### Примеры

```csharp
// Обработка отсутствующего handler
try
{
    var hero = await _mediator.Send(new GetHeroQuery("123"), ct);
}
catch (HandlerNotFoundException ex)
{
    _logger.Error($"Handler not found for {ex.RequestType.Name}");
    
    // Fallback
    return Hero.Empty;
}
```

```csharp
// Проверка наличия handler перед вызовом
var registry = _container.Resolve<IHandlerRegistry>();

if (!registry.HasHandler(typeof(GetHeroQuery)))
{
    _logger.Warning("GetHeroQuery handler not registered");
    return Hero.Empty;
}

var hero = await _mediator.Send(new GetHeroQuery("123"), ct);
```

### Избежание ошибки

```csharp
// ✅ SendOrDefault — возвращает default вместо исключения
var hero = await _mediator.SendOrDefault(
    new GetHeroQuery("123"),
    defaultValue: Hero.Empty);
```

---

## Обработка исключений

### Глобальный Exception Handler

```csharp
public class GlobalExceptionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IErrorReporter _errorReporter;

    public GlobalExceptionBehavior(IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
    }

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (HandlerNotFoundException ex)
        {
            _errorReporter.Report(ex, new { RequestType = ex.RequestType.Name });
            throw;
        }
        catch (MediatorException ex)
        {
            _errorReporter.Report(ex, new { Request = request });
            throw;
        }
    }
}
```

### Специфичная обработка

```csharp
public async UniTask<Hero> LoadHeroAsync(string heroId, CancellationToken ct)
{
    try
    {
        return await _mediator.Send(new GetHeroQuery(heroId), ct);
    }
    catch (HandlerNotFoundException ex)
    {
        _logger.Error($"GetHeroQuery handler not registered: {ex.Message}");
        return Hero.Empty;
    }
    catch (MediatorException ex)
    {
        _logger.Error($"Mediator error: {ex.Message}");
        throw new GameException("Failed to load hero", ex);
    }
}
```

---

## Best Practices

### ✅ Регистрируй все handlers

```csharp
// ✅ Правильно — handler зарегистрирован
builder.AddMediator(cfg =>
{
    cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
});

// Использование — работает
await _mediator.Send(new GetHeroQuery("123"), ct);
```

### ✅ Используй SendOrDefault для опциональных handlers

```csharp
// ✅ Правильно — для optional features
var plugin = await _mediator.SendOrDefault(
    new GetPluginDataQuery("optional_plugin"),
    defaultValue: null);

if (plugin != null)
{
    ApplyPlugin(plugin);
}
```

### ✅ Валидируй регистрацию при старте

```csharp
public class MediatorValidator
{
    private readonly IHandlerRegistry _registry;

    public MediatorValidator(IHandlerRegistry registry)
    {
        _registry = registry;
    }

    public void ValidateRequiredHandlers()
    {
        var requiredRequests = new[]
        {
            typeof(GetHeroQuery),
            typeof(SaveSettingsCommand),
            typeof(LoadGameDataQuery)
        };

        foreach (var requestType in requiredRequests)
        {
            if (!_registry.HasHandler(requestType))
            {
                throw new MediatorException(
                    $"Required handler not registered: {requestType.Name}");
            }
        }
    }
}
```

### ❌ Не используй HandlerNotFoundException для flow control

```csharp
// ❌ Неправильно — exceptions для flow control
try
{
    var data = await _mediator.Send(new GetCachedDataQuery());
}
catch (HandlerNotFoundException)
{
    // Fallback
    var data = await _mediator.Send(new GetFreshDataQuery());
}

// ✅ Правильно — используй SendOrDefault
var cached = await _mediator.SendOrDefault(new GetCachedDataQuery());
if (cached == null)
{
    cached = await _mediator.Send(new GetFreshDataQuery());
}
```

---

## Hierarchy

```
Exception
└── MediatorException
    └── HandlerNotFoundException
```

---

## Связанные исключения

### TimeoutException (из Extensions)

```csharp
try
{
    await _mediator.SendWithTimeout(request, TimeSpan.FromSeconds(5));
}
catch (TimeoutException ex)
{
    // ex.Message: "Request GetHeroQuery timed out after 5s"
}
```

### AggregateException (из Publishing)

```csharp
try
{
    await _mediator.Publish(new OrderCreatedEvent(order), ct);
}
catch (AggregateException ex)
{
    foreach (var inner in ex.InnerExceptions)
    {
        _logger.Error($"Handler failed: {inner.Message}");
    }
}
```

---

## См. также

- [Handlers.md](./Handlers.md) — регистрация handlers
- [Extensions.md](./Extensions.md) — SendOrDefault, SendWithTimeout
- [Registration.md](./Registration.md) — регистрация handlers
