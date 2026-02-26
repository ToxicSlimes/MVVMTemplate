# Unity Mediator System

MediatR-like mediator for Unity with UniTask, Reflex DI, and zero runtime reflection.

## Quick Start

### 1. Register Mediator in your Installer

```csharp
public class GameInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddMediator(cfg =>
        {
            // Auto-register all handlers (recommended)
            MediatorHandlerRegistration.RegisterAllHandlers(cfg);
            
            // Or manual registration
            // cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
            // cfg.AddHandler<DoSomethingCommand, DoSomethingHandler>(); // IRequest returns Nothing
            // cfg.AddNotificationHandler<HeroSelectedEvent, UIUpdateHandler>();
        });
    }
}
```

### 2. Create Request/Response

```csharp
public readonly struct GetHeroQuery : IRequest<Hero>
{
    public readonly string HeroId;
    public GetHeroQuery(string heroId) => HeroId = heroId;
}
```

### 3. Create Handler

```csharp
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
```

### 4. Use in ViewModel

```csharp
public class HeroViewModel : DisposableObject
{
    private readonly IMediator _mediator;

    public HeroViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async UniTask LoadHeroAsync(string heroId, CancellationToken ct)
    {
        var hero = await _mediator.Send(new GetHeroQuery(heroId), ct);
        // use hero
    }
}
```

## Auto-Discovery

Instead of manually registering each handler, use the scanner:

1. **Unity Menu:** `Tools/Mediator/Scan & Generate Handler Registration`
2. This generates `MediatorHandlerRegistration.generated.cs`
3. Use `MediatorHandlerRegistration.RegisterAllHandlers(cfg)` in your installer

## Notifications (Pub/Sub)

```csharp
// Notification
public readonly struct HeroSelectedEvent : INotification
{
    public readonly string HeroId;
    public HeroSelectedEvent(string heroId) => HeroId = heroId;
}

// Handler (can have multiple)
public class UpdateUIHandler : INotificationHandler<HeroSelectedEvent>
{
    public UniTask Handle(HeroSelectedEvent notification, CancellationToken ct)
    {
        // update UI
        return UniTask.CompletedTask;
    }
}

// Publish
await _mediator.Publish(new HeroSelectedEvent(heroId));
```

## Streaming

```csharp
// Stream Request
public readonly struct LoadAssetsRequest : IStreamRequest<float>
{
    public readonly string[] Paths;
    public LoadAssetsRequest(string[] paths) => Paths = paths;
}

// Stream Handler
public class LoadAssetsHandler : IStreamRequestHandler<LoadAssetsRequest, float>
{
    public async IUniTaskAsyncEnumerable<float> Handle(
        LoadAssetsRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = 0; i < request.Paths.Length; i++)
        {
            await LoadAssetAsync(request.Paths[i], ct);
            yield return (float)(i + 1) / request.Paths.Length;
        }
    }
}

// Usage
await foreach (var progress in _mediator.CreateStream(new LoadAssetsRequest(paths), ct))
{
    _progressBar.Value = progress;
}
```

## Pipeline Behaviors

Add cross-cutting concerns:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        Debug.Log($"Handling {typeof(TRequest).Name}");
        var response = await next();
        Debug.Log($"Handled {typeof(TRequest).Name}");
        return response;
    }
}

// Register
cfg.AddBehavior<LoggingBehavior<,>>();
```

## Extensions

```csharp
// With timeout
var hero = await _mediator.SendWithTimeout(
    new GetHeroQuery(id), 
    TimeSpan.FromSeconds(5));

// Fire and forget notifications
_mediator.PublishFireAndForget(new AnalyticsEvent("click"));

// Return default if handler not found
var hero = await _mediator.SendOrDefault(query, defaultHero);
```

## Structure

```
Mediator/
├── Abstractions/          - Interfaces (IMediator, IRequest, etc.)
├── Handlers/              - Handler interfaces
├── Implementation/        - Mediator implementation
├── Pipeline/              - Pipeline behaviors
├── Publishing/            - Notification publishers
├── Registration/          - DI registration
│   └── Generated/         - Auto-generated registrations
└── Extensions/            - Extension methods
```
