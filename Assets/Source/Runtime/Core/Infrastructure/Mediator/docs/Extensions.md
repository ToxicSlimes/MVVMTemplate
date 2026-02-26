# Extensions — Вспомогательные методы

Extension methods для расширения функциональности Mediator.

**Namespace:** `Core.Infrastructure.Mediator.Extensions`

---

## Обзор методов

| Метод | Описание |
|-------|----------|
| `SendWithTimeout<TResponse>` | Отправка request с таймаутом |
| `SendWithTimeout` | Отправка command с таймаутом |
| `PublishFireAndForget` | Публикация без ожидания |
| `SendOrDefault<TResponse>` | Отправка с fallback на default |

---

## SendWithTimeout<TResponse>

Отправляет request с ограничением по времени. При превышении таймаута выбрасывает `TimeoutException`.

```csharp
public static async UniTask<TResponse> SendWithTimeout<TResponse>(
    this IMediator mediator,
    IRequest<TResponse> request,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
```

### Реализация

```csharp
public static async UniTask<TResponse> SendWithTimeout<TResponse>(
    this IMediator mediator,
    IRequest<TResponse> request,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
{
    using var timeoutCts = new CancellationTokenSource(timeout);
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken,
        timeoutCts.Token);

    try
    {
        return await mediator.Send(request, linkedCts.Token);
    }
    catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
    {
        throw new TimeoutException(
            $"Request {request.GetType().Name} timed out after {timeout.TotalSeconds}s");
    }
}
```

### Примеры

```csharp
// Загрузка с таймаутом 5 секунд
try
{
    var hero = await _mediator.SendWithTimeout(
        new GetHeroQuery(heroId),
        TimeSpan.FromSeconds(5));
}
catch (TimeoutException ex)
{
    _logger.Warning($"Loading hero timed out: {ex.Message}");
    // Показать UI ошибки или retry
}
```

```csharp
// Таймаут с внешним CancellationToken
var hero = await _mediator.SendWithTimeout(
    new GetHeroQuery(heroId),
    TimeSpan.FromSeconds(10),
    _cts.Token);
```

### Когда использовать

| ✅ Подходит | ❌ Не подходит |
|-------------|----------------|
| Сетевые запросы | Быстрые локальные операции |
| Загрузка ресурсов | Уже есть внешний таймаут |
| Долгие операции | Fire-and-forget |

---

## SendWithTimeout (command)

Версия для commands без ответа.

```csharp
public static async UniTask SendWithTimeout(
    this IMediator mediator,
    IRequest request,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
```

### Пример

```csharp
try
{
    await _mediator.SendWithTimeout(
        new SaveSettingsCommand(settings),
        TimeSpan.FromSeconds(3));
}
catch (TimeoutException)
{
    _logger.Error("Failed to save settings: timeout");
    // Retry или показать ошибку
}
```

---

## PublishFireAndForget

Публикует notification без ожидания завершения handlers. Ошибки логируются, но не пробрасываются.

```csharp
public static UniTask PublishFireAndForget<TNotification>(
    this IMediator mediator,
    TNotification notification)
    where TNotification : INotification
```

### Реализация

```csharp
public static UniTask PublishFireAndForget<TNotification>(
    this IMediator mediator,
    TNotification notification)
    where TNotification : INotification
{
    mediator.Publish(notification, CancellationToken.None).Forget();
    return UniTask.CompletedTask;
}
```

### Примеры

```csharp
// Analytics — не блокируем UI
_mediator.PublishFireAndForget(new ButtonClickedEvent("buy_button"));

// Telemetry
_mediator.PublishFireAndForget(new SessionStartedEvent(DateTime.UtcNow));

// Background sync
_mediator.PublishFireAndForget(new DataChangedEvent(data));
```

### Сравнение с обычным Publish

```csharp
// Ждём завершения всех handlers
await _mediator.Publish(new ImportantEvent(data), ct);

// Не ждём — сразу продолжаем
_mediator.PublishFireAndForget(new NonCriticalEvent(data));
```

### Когда использовать

| ✅ Подходит | ❌ Не подходит |
|-------------|----------------|
| Analytics | Критичные события |
| Логирование | Операции с гарантией |
| Telemetry | Зависимые handlers |
| Background tasks | Нужен результат |

### ⚠️ Предупреждения

1. **Нет гарантий выполнения** — handlers могут не завершиться
2. **Ошибки теряются** — только логирование
3. **CancellationToken игнорируется** — используется `CancellationToken.None`

---

## SendOrDefault<TResponse>

Отправляет request и возвращает default значение, если handler не найден.

```csharp
public static async UniTask<TResponse> SendOrDefault<TResponse>(
    this IMediator mediator,
    IRequest<TResponse> request,
    TResponse defaultValue = default,
    CancellationToken cancellationToken = default)
```

### Реализация

```csharp
public static async UniTask<TResponse> SendOrDefault<TResponse>(
    this IMediator mediator,
    IRequest<TResponse> request,
    TResponse defaultValue = default,
    CancellationToken cancellationToken = default)
{
    try
    {
        return await mediator.Send(request, cancellationToken);
    }
    catch (HandlerNotFoundException)
    {
        return defaultValue;
    }
}
```

### Примеры

```csharp
// С explicit default
var hero = await _mediator.SendOrDefault(
    new GetHeroQuery(heroId),
    defaultValue: Hero.Empty);

// С implicit default (null для reference types)
var settings = await _mediator.SendOrDefault(
    new GetUserSettingsQuery(userId));

if (settings == null)
{
    settings = Settings.CreateDefault();
}
```

### Когда использовать

| ✅ Подходит | ❌ Не подходит |
|-------------|----------------|
| Опциональные features | Handler должен быть |
| Graceful degradation | Критичные операции |
| Plugin-like архитектура | Ошибки нужно обрабатывать |

---

## Дополнительные Extension Ideas

### SendWithRetry

```csharp
public static async UniTask<TResponse> SendWithRetry<TResponse>(
    this IMediator mediator,
    IRequest<TResponse> request,
    int maxRetries = 3,
    TimeSpan? delay = null,
    CancellationToken cancellationToken = default)
{
    delay ??= TimeSpan.FromSeconds(1);
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await mediator.Send(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch when (attempt < maxRetries)
        {
            await UniTask.Delay(delay.Value, cancellationToken: cancellationToken);
        }
    }
    
    // Последняя попытка без catch
    return await mediator.Send(request, cancellationToken);
}
```

**Использование:**

```csharp
var data = await _mediator.SendWithRetry(
    new FetchDataQuery(),
    maxRetries: 3,
    delay: TimeSpan.FromSeconds(2));
```

### PublishAll

```csharp
public static async UniTask PublishAll<TNotification>(
    this IMediator mediator,
    IEnumerable<TNotification> notifications,
    CancellationToken cancellationToken = default)
    where TNotification : INotification
{
    foreach (var notification in notifications)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await mediator.Publish(notification, cancellationToken);
    }
}
```

**Использование:**

```csharp
var events = new[]
{
    new ItemAddedEvent("item1"),
    new ItemAddedEvent("item2"),
    new ItemAddedEvent("item3")
};

await _mediator.PublishAll(events, ct);
```

### SendMany

```csharp
public static async UniTask<TResponse[]> SendMany<TRequest, TResponse>(
    this IMediator mediator,
    IEnumerable<TRequest> requests,
    CancellationToken cancellationToken = default)
    where TRequest : IRequest<TResponse>
{
    var tasks = requests.Select(r => mediator.Send(r, cancellationToken));
    return await UniTask.WhenAll(tasks);
}
```

**Использование:**

```csharp
var queries = heroIds.Select(id => new GetHeroQuery(id));
var heroes = await _mediator.SendMany<GetHeroQuery, Hero>(queries, ct);
```

### TrySend

```csharp
public static async UniTask<(bool Success, TResponse Result)> TrySend<TResponse>(
    this IMediator mediator,
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default)
{
    try
    {
        var result = await mediator.Send(request, cancellationToken);
        return (true, result);
    }
    catch (Exception)
    {
        return (false, default);
    }
}
```

**Использование:**

```csharp
var (success, hero) = await _mediator.TrySend(new GetHeroQuery(id));

if (success)
{
    ShowHero(hero);
}
else
{
    ShowError();
}
```

---

## Паттерны использования

### Graceful Degradation

```csharp
public async UniTask<HeroData> LoadHeroDataAsync(string heroId, CancellationToken ct)
{
    // Пробуем загрузить свежие данные с таймаутом
    try
    {
        return await _mediator.SendWithTimeout(
            new FetchHeroFromServerQuery(heroId),
            TimeSpan.FromSeconds(5),
            ct);
    }
    catch (TimeoutException)
    {
        _logger.Warning($"Server timeout, using cached data for {heroId}");
    }
    
    // Fallback на кеш
    var cached = await _mediator.SendOrDefault(
        new GetCachedHeroQuery(heroId),
        HeroData.Empty);
    
    if (cached != HeroData.Empty)
        return cached;
    
    // Последний fallback — default
    return HeroData.CreateDefault(heroId);
}
```

### Analytics с Fire-and-Forget

```csharp
public void OnButtonClick(string buttonId)
{
    // UI реагирует мгновенно
    ShowClickFeedback();
    
    // Analytics в фоне
    _mediator.PublishFireAndForget(new ButtonClickedEvent(buttonId));
}
```

### Параллельная загрузка с таймаутами

```csharp
public async UniTask<GameData> LoadGameDataAsync(CancellationToken ct)
{
    var timeout = TimeSpan.FromSeconds(10);
    
    // Параллельная загрузка с индивидуальными таймаутами
    var heroTask = _mediator.SendWithTimeout(new LoadHeroQuery(), timeout, ct);
    var inventoryTask = _mediator.SendWithTimeout(new LoadInventoryQuery(), timeout, ct);
    var settingsTask = _mediator.SendWithTimeout(new LoadSettingsQuery(), timeout, ct);
    
    try
    {
        await UniTask.WhenAll(heroTask, inventoryTask, settingsTask);
        
        return new GameData
        {
            Hero = heroTask.GetAwaiter().GetResult(),
            Inventory = inventoryTask.GetAwaiter().GetResult(),
            Settings = settingsTask.GetAwaiter().GetResult()
        };
    }
    catch (TimeoutException ex)
    {
        _logger.Error($"Game data loading timed out: {ex.Message}");
        throw;
    }
}
```

---

## Best Practices

### ✅ Используй осмысленные таймауты

```csharp
// ✅ Правильно — разные таймауты для разных операций
await _mediator.SendWithTimeout(networkRequest, TimeSpan.FromSeconds(30));
await _mediator.SendWithTimeout(localDbRequest, TimeSpan.FromMilliseconds(500));

// ❌ Неправильно — один таймаут для всего
await _mediator.SendWithTimeout(anyRequest, TimeSpan.FromSeconds(5));
```

### ✅ Логируй таймауты

```csharp
try
{
    return await _mediator.SendWithTimeout(request, TimeSpan.FromSeconds(5));
}
catch (TimeoutException ex)
{
    _logger.Warning($"Request timeout: {ex.Message}");
    throw; // Или fallback
}
```

### ✅ Fire-and-Forget только для некритичных операций

```csharp
// ✅ Правильно
_mediator.PublishFireAndForget(new AnalyticsEvent("click"));

// ❌ Неправильно
_mediator.PublishFireAndForget(new SaveProgressEvent(progress)); // Можем потерять прогресс!
```

### ✅ Предоставляй meaningful defaults

```csharp
// ✅ Правильно
var settings = await _mediator.SendOrDefault(
    new GetSettingsQuery(),
    defaultValue: Settings.CreateDefault());

// ❌ Неправильно — null может вызвать NRE
var settings = await _mediator.SendOrDefault(new GetSettingsQuery());
// settings может быть null!
```

---

## Исключения

### TimeoutException

Выбрасывается `SendWithTimeout` при превышении таймаута.

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

### HandlerNotFoundException

Выбрасывается, если handler не зарегистрирован.

```csharp
try
{
    await _mediator.Send(new UnregisteredQuery());
}
catch (HandlerNotFoundException ex)
{
    // ex.RequestType: typeof(UnregisteredQuery)
    // ex.Message: "No handler registered for request type UnregisteredQuery"
}
```

`SendOrDefault` перехватывает это исключение и возвращает default.

---

## См. также

- [Abstractions.md](./Abstractions.md) — IMediator API
- [Handlers.md](./Handlers.md) — обработчики
- [Publishing.md](./Publishing.md) — стратегии публикации
