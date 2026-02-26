# Mediator — DO / DON'T

*Обновлено: 2026-02-11*

---

## ✅ DO

### Request / Handler
- **Используй `struct` для Request** — zero-alloc, value semantics:
```csharp
public readonly struct GetHeroQuery : IRequest<Hero>
{
    public readonly string HeroId;
    public GetHeroQuery(string heroId) => HeroId = heroId;
}
```

- **Один Handler — одна ответственность** — не мешай логику нескольких операций
- **Передавай `CancellationToken`** — он приходит из `Handle()`, пробрасывай дальше
- **Именуй по конвенции:**
  - Query: `GetHeroQuery`, `LoadSettingsQuery`
  - Command: `SaveHeroCommand`, `DeleteItemCommand`
  - Handler: `GetHeroQueryHandler`, `SaveHeroCommandHandler`
  - Notification: `HeroSelectedEvent`, `OrderCompletedNotification`

### Notification
- **Используй `INotification` для broadcast-событий** — 1:N, все подписчики получат
- **Выбирай правильный Publisher:**
  - `SequentialPublisher` — по умолчанию, предсказуемый порядок
  - `ParallelPublisher` — если handlers независимы и нужна скорость
  - `FireAndForgetPublisher` — для non-critical events (логирование, аналитика)

### Pipeline
- **Используй `IPipelineBehavior`** для cross-cutting concerns (логирование, валидация, кеширование)
- **Pre/PostProcessor** — для простых действий до/после handler'а

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async UniTask<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        _logger.Debug("Handling {0}", typeof(TRequest).Name);
        var response = await next();
        _logger.Debug("Handled {0}", typeof(TRequest).Name);
        return response;
    }
}
```

### Регистрация
- **Используй `MediatorBuilder`** для fluent регистрации:
```csharp
builder.AddMediator(cfg =>
{
    cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
    cfg.AddNotificationHandler<HeroSelectedEvent, HeroSelectedHandler>();
    cfg.AddBehavior<LoggingBehavior<,>>();
    cfg.UseNotificationPublisher<SequentialPublisher>();
});
```

### Observable
- **Используй `CreateNotificationObservable<T>()`** для реактивных подписок на notifications через R3:
```csharp
_mediator.CreateNotificationObservable<HeroSelectedEvent>()
    .Subscribe(e => UpdateUI(e))
    .AddTo(ref disposables);
```

---

## ❌ DON'T

### Request / Handler
- **НЕ используй class для Request** — лишние аллокации, struct достаточно
- **НЕ бросай generic `Exception`** — используй `MediatorException` или domain-specific
- **НЕ делай handler stateful** — регистрируй как Transient, не храни состояние между вызовами
- **НЕ вызывай другие handlers напрямую** — отправляй через `IMediator.Send()`

```csharp
// ❌ Нельзя
_otherHandler.Handle(request, ct);

// ✅ Правильно
await _mediator.Send(new OtherRequest(), ct);
```

### Notification
- **НЕ полагайся на порядок handlers** (кроме `SequentialPublisher`) — они могут выполниться в любом порядке
- **НЕ бросай исключения в notification handler** — это сломает доставку остальным подписчикам
- **НЕ используй Notification для request-response** — для этого есть `IRequest<T>`

### Pipeline
- **НЕ забывай вызвать `next()`** в PipelineBehavior — иначе handler не выполнится:
```csharp
// ❌ Забыли next() — handler никогда не вызовется
public async UniTask<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
{
    _logger.Debug("Logging...");
    return default; // Handler не вызван!
}
```

### Регистрация
- **НЕ забывай зарегистрировать handler** — иначе `HandlerNotFoundException` в runtime
- **НЕ регистрируй handler как Singleton** если он stateful или зависит от scoped сервисов
- **НЕ дублируй регистрацию** одного handler'а — приведёт к неопределённому поведению
