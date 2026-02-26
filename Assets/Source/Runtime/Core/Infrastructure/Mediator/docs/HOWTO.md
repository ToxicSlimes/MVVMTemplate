# Mediator — HOWTO

*Обновлено: 2026-02-11*

---

## Как зарегистрировать Request Handler

### 1. Создай Request

```csharp
// Query (возвращает данные)
public readonly struct GetHeroQuery : IRequest<Hero>
{
    public readonly string HeroId;
    public GetHeroQuery(string heroId) => HeroId = heroId;
}

// Command (без возврата)
public readonly struct DeleteItemCommand : IRequest
{
    public readonly string ItemId;
    public DeleteItemCommand(string itemId) => ItemId = itemId;
}
```

### 2. Создай Handler

```csharp
// Query handler
public class GetHeroQueryHandler : IRequestHandler<GetHeroQuery, Hero>
{
    private readonly IHeroRepository _repository;

    public GetHeroQueryHandler(IHeroRepository repository)
    {
        _repository = repository;
    }

    public async UniTask<Hero> Handle(GetHeroQuery request, CancellationToken ct)
    {
        return await _repository.GetByIdAsync(request.HeroId, ct);
    }
}

// Command handler (возвращает Nothing)
public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand>
{
    public async UniTask Handle(DeleteItemCommand request, CancellationToken ct)
    {
        // удаление
    }
}
```

### 3. Зарегистрируй в MediatorBuilder

```csharp
builder.AddMediator(cfg =>
{
    // Query с response
    cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
    
    // Command без response
    cfg.AddHandler<DeleteItemCommand, DeleteItemCommandHandler>();
});
```

### 4. Используй

```csharp
// Query
var hero = await _mediator.Send(new GetHeroQuery("hero-1"), ct);

// Command
await _mediator.Send(new DeleteItemCommand("item-1"), ct);
```

---

## Как зарегистрировать Pipeline Behavior

Pipeline behaviors оборачивают handler — для логирования, валидации, кеширования.

### 1. Создай Behavior

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        // Pre-processing
        Validate(request);
        
        // Вызов следующего в цепочке (или handler)
        var response = await next();
        
        // Post-processing
        return response;
    }
}
```

### 2. Зарегистрируй

```csharp
builder.AddMediator(cfg =>
{
    cfg.AddBehavior<ValidationBehavior<,>>();
});
```

### Pre/Post Processors

Для простых случаев — без обёртки всего pipeline:

```csharp
// Pre-processor
public class LoggingPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
{
    public UniTask Process(TRequest request, CancellationToken ct)
    {
        Debug.Log($"Processing {typeof(TRequest).Name}");
        return UniTask.CompletedTask;
    }
}

// Post-processor
public class LoggingPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
{
    public UniTask Process(TRequest request, TResponse response, CancellationToken ct)
    {
        Debug.Log($"Processed {typeof(TRequest).Name} → {response}");
        return UniTask.CompletedTask;
    }
}
```

Регистрация:
```csharp
cfg.AddPreProcessor<MyRequest, LoggingPreProcessor<MyRequest>>();
cfg.AddPostProcessor<MyRequest, MyResponse, LoggingPostProcessor<MyRequest, MyResponse>>();
```

---

## Как зарегистрировать Notification Handler

Notifications — broadcast события, 1:N (один publisher, много subscribers).

### 1. Создай Notification

```csharp
public readonly struct HeroSelectedEvent : INotification
{
    public readonly string HeroId;
    public HeroSelectedEvent(string heroId) => HeroId = heroId;
}
```

### 2. Создай Handler(ы)

```csharp
public class UpdateUIOnHeroSelected : INotificationHandler<HeroSelectedEvent>
{
    public UniTask Handle(HeroSelectedEvent notification, CancellationToken ct)
    {
        // обновить UI
        return UniTask.CompletedTask;
    }
}

public class LogHeroSelected : INotificationHandler<HeroSelectedEvent>
{
    public UniTask Handle(HeroSelectedEvent notification, CancellationToken ct)
    {
        Debug.Log($"Hero selected: {notification.HeroId}");
        return UniTask.CompletedTask;
    }
}
```

### 3. Зарегистрируй

```csharp
builder.AddMediator(cfg =>
{
    cfg.AddNotificationHandler<HeroSelectedEvent, UpdateUIOnHeroSelected>();
    cfg.AddNotificationHandler<HeroSelectedEvent, LogHeroSelected>();
    
    // Стратегия публикации (по умолчанию Sequential)
    cfg.UseNotificationPublisher<SequentialPublisher>();
});
```

### 4. Публикуй

```csharp
await _mediator.Publish(new HeroSelectedEvent("hero-1"), ct);
```

### Реактивная подписка (R3)

```csharp
_mediator.CreateNotificationObservable<HeroSelectedEvent>()
    .Subscribe(e => OnHeroSelected(e))
    .AddTo(ref disposables);
```

---

## Как зарегистрировать Stream Handler

Stream — для потоковых данных (прогресс загрузки, пагинация).

### 1. Создай StreamRequest

```csharp
public readonly struct LoadChunksRequest : IStreamRequest<Chunk>
{
    public readonly string ResourceId;
    public LoadChunksRequest(string resourceId) => ResourceId = resourceId;
}
```

### 2. Создай Handler

```csharp
public class LoadChunksHandler : IStreamRequestHandler<LoadChunksRequest, Chunk>
{
    public async IUniTaskAsyncEnumerable<Chunk> Handle(
        LoadChunksRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var chunk in GetChunks(request.ResourceId))
        {
            ct.ThrowIfCancellationRequested();
            yield return chunk;
        }
    }
}
```

### 3. Зарегистрируй и используй

```csharp
cfg.AddStreamHandler<LoadChunksRequest, Chunk, LoadChunksHandler>();

// Использование
await foreach (var chunk in _mediator.CreateStream(new LoadChunksRequest("res-1"), ct))
{
    ProcessChunk(chunk);
}
```

---

## Как настроить Publisher стратегию

```csharp
builder.AddMediator(cfg =>
{
    // Последовательно (по умолчанию) — предсказуемый порядок
    cfg.UseNotificationPublisher<SequentialPublisher>();
    
    // Параллельно — все handlers одновременно
    cfg.UseNotificationPublisher<ParallelPublisher>();
    
    // Fire-and-forget — не ждём завершения
    cfg.UseNotificationPublisher<FireAndForgetPublisher>();
});
```
