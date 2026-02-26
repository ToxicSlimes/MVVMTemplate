# Unity Mediator System

MediatR-подобная реализация для Unity с поддержкой **UniTask**, **Reflex DI**, и полноценным **Pipeline**.

## Содержание

1. [Обзор системы](#обзор-системы)
2. [Архитектура](#архитектура)
3. [Как это работает](#как-это-работает)
4. [Основные концепции](#основные-концепции)
5. [Регистрация и настройка](#регистрация-и-настройка)
6. [Примеры использования](#примеры-использования)
7. [Pipeline Behaviors](#pipeline-behaviors)
8. [Notification Publishers](#notification-publishers)
9. [Extensions](#extensions)
10. [Паттерны и Best Practices](#паттерны-и-best-practices)
11. [Возможные расширения](#возможные-расширения)
12. [FAQ](#faq)
13. [Quick Reference](#quick-reference)

---

## Обзор системы

### Что это?

Mediator — паттерн, который обеспечивает **loose coupling** между компонентами приложения. Вместо прямых вызовов сервисов, компоненты отправляют **Request** или **Notification** через единую точку — `IMediator`.

### Зачем нужен?

- **Decoupling** — ViewModel не знает о конкретных сервисах
- **Single Responsibility** — каждый Handler делает одну вещь
- **Cross-cutting concerns** — логирование, валидация, кеширование через Pipeline
- **Testability** — легко мокать `IMediator`
- **Предсказуемый flow** данных

### Ключевые интерфейсы

```csharp
public interface IMediator : ISender, IPublisher { }

public interface ISender
{
    UniTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct);
    UniTask Send(IRequest request, CancellationToken ct);
    IUniTaskAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken ct);
}

public interface IPublisher
{
    UniTask Publish<TNotification>(TNotification notification, CancellationToken ct) where TNotification : INotification;
}
```

---

## Архитектура

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                                    IMediator                                        │
│                         (ISender + IPublisher combined)                             │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  ┌─────────────────────────────────┐    ┌─────────────────────────────────────────┐ │
│  │         ISender                 │    │             IPublisher                  │ │
│  │                                 │    │                                         │ │
│  │  • Send<TResponse>(IRequest)   │    │  • Publish<T>(INotification)            │ │
│  │  • Send(IRequest)              │    │                                         │ │
│  │  • CreateStream<T>(IStream)    │    │                                         │ │
│  └─────────────┬───────────────────┘    └──────────────────┬──────────────────────┘ │
│                │                                           │                        │
│                ▼                                           ▼                        │
│  ┌─────────────────────────────────┐    ┌─────────────────────────────────────────┐ │
│  │    Pipeline Behaviors           │    │      Notification Publishers            │ │
│  │                                 │    │                                         │ │
│  │  • Pre-processors              │    │  • SequentialPublisher (default)        │ │
│  │  • IPipelineBehavior<T,R>      │    │  • ParallelPublisher                    │ │
│  │  • Post-processors             │    │  • FireAndForgetPublisher               │ │
│  └─────────────┬───────────────────┘    └──────────────────┬──────────────────────┘ │
│                │                                           │                        │
│                ▼                                           ▼                        │
│  ┌─────────────────────────────────┐    ┌─────────────────────────────────────────┐ │
│  │   IRequestHandler<TReq,TRes>    │    │    INotificationHandler<TNotif>         │ │
│  │                                 │    │    (1 notification → N handlers)        │ │
│  │   1 request → 1 handler         │    │                                         │ │
│  └─────────────────────────────────┘    └─────────────────────────────────────────┘ │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### Структура файлов

```
Mediator/
├── Abstractions/              # Marker interfaces
│   ├── IMediator.cs           # ISender + IPublisher
│   ├── IRequest.cs            # Request markers
│   ├── IStreamRequest.cs      # Stream request marker
│   ├── INotification.cs       # Notification marker
│   ├── ISender.cs             # Send requests
│   ├── IPublisher.cs          # Publish notifications
│   └── Nothing.cs             # Unit type для commands
│
├── Handlers/                  # Handler interfaces
│   ├── IRequestHandler.cs
│   ├── INotificationHandler.cs
│   └── IStreamRequestHandler.cs
│
├── Implementation/            # Core implementation
│   ├── Mediator.cs
│   └── Wrappers/              # Internal wrappers для type resolution
│       ├── RequestHandlerWrapperImpl.cs
│       ├── NotificationHandlerWrapperImpl.cs
│       └── StreamRequestHandlerWrapperImpl.cs
│
├── Pipeline/                  # Pipeline extensibility
│   ├── IPipelineBehavior.cs
│   ├── IRequestPreProcessor.cs
│   ├── IRequestPostProcessor.cs
│   ├── IStreamPipelineBehavior.cs
│   └── RequestHandlerDelegate.cs
│
├── Publishing/                # Notification strategies
│   ├── INotificationPublisher.cs
│   ├── SequentialPublisher.cs
│   ├── ParallelPublisher.cs
│   ├── FireAndForgetPublisher.cs
│   └── NotificationHandlerExecutor.cs
│
├── Registration/              # DI integration
│   ├── MediatorBuilder.cs
│   ├── HandlerRegistry.cs
│   ├── HandlerDescriptor.cs
│   ├── Extensions/
│   │   └── MediatorReflexExtensions.cs
│   └── Generated/
│       └── MediatorHandlerRegistration.generated.cs
│
├── Extensions/                # Helper extensions
│   └── MediatorExtensions.cs
│
└── Exceptions/
    ├── MediatorException.cs
    └── HandlerNotFoundException.cs
```

---

## Как это работает

### Request Flow (Send)

```
1. _mediator.Send(new GetHeroQuery("123"), ct)
   │
   ▼
2. Mediator.Send<TResponse>()
   • Извлекает тип request через GetType()
   • Получает или создаёт RequestHandlerWrapper через ConcurrentDictionary кеш
   │
   ▼
3. RequestHandlerWrapperImpl<TRequest, TResponse>.Handle()
   • Resolve handler через DI: container.Resolve<IRequestHandler<TRequest, TResponse>>()
   • Resolve behaviors: container.All<IPipelineBehavior<TRequest, TResponse>>()
   │
   ▼
4. ExecutePipeline() [если есть behaviors]
   • Строит цепочку делегатов в обратном порядке:
     pipeline = Behavior3 → Behavior2 → Behavior1 → Handler
   • Выполняет цепочку: await pipeline()
   │
   ▼
5. Handler.Handle(request, ct)
   • Выполняет бизнес-логику
   • Возвращает TResponse
```

**Важно:** Pipeline behaviors оборачиваются в **обратном порядке** регистрации. Если зарегистрированы `[Logging, Validation, Exception]`, выполнение будет: `Logging → Validation → Exception → Handler → Exception → Validation → Logging`.

### Notification Flow (Publish)

```
1. _mediator.Publish(new HeroSelectedNotification("123"), ct)
   │
   ▼
2. Mediator.Publish<TNotification>()
   • Извлекает тип notification
   • Получает или создаёт NotificationHandlerWrapper через ConcurrentDictionary кеш
   │
   ▼
3. NotificationHandlerWrapperImpl<TNotification>.Handle()
   • Resolve все handlers через DI: container.All<INotificationHandler<TNotification>>()
   • Создаёт список executor'ов для каждого handler'а
   │
   ▼
4. INotificationPublisher.Publish()
   • SequentialPublisher: выполняет handlers последовательно, агрегирует ошибки
   • ParallelPublisher: выполняет через UniTask.WhenAll
   • FireAndForgetPublisher: запускает и забывает, логирует ошибки
```

### Кеширование Wrappers

Все wrapper'ы кешируются в `ConcurrentDictionary` на уровне статических полей класса `Mediator`:

```csharp
private static readonly ConcurrentDictionary<Type, object> RequestHandlerWrappers = new();
private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> NotificationHandlerWrappers = new();
private static readonly ConcurrentDictionary<Type, object> StreamHandlerWrappers = new();
```

Это обеспечивает:
- **Производительность** — нет повторного создания wrapper'ов
- **Thread-safety** — ConcurrentDictionary безопасен для многопоточности
- **Memory efficiency** — один экземпляр на тип request/notification

### Handler Resolution

Handlers резолвятся через **Reflex DI Container** при каждом вызове (если не Singleton). Это позволяет:
- Использовать зависимости в handlers
- Поддерживать разные lifetime (Transient, Singleton, Scoped)
- Легко тестировать через моки

---

## Основные концепции

### 1. Request/Response (1:1)

Request — команда или запрос данных, который обрабатывается **одним** handler'ом и возвращает результат.

```csharp
public readonly struct GetHeroQuery : IRequest<Hero>
{
    public readonly string HeroId;
    public GetHeroQuery(string heroId) => HeroId = heroId;
}

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

**Использование:**

```csharp
var hero = await _mediator.Send(new GetHeroQuery("123"), ct);
```

### 2. Command без response (Nothing)

Команда без возвращаемого значения использует `IRequest` (наследует от `IRequest<Nothing>`):

```csharp
public readonly struct SaveSettingsCommand : IRequest
{
    public readonly Settings Settings;
    public SaveSettingsCommand(Settings settings) => Settings = settings;
}

public class SaveSettingsHandler : IRequestHandler<SaveSettingsCommand>
{
    public async UniTask<Nothing> Handle(SaveSettingsCommand request, CancellationToken ct)
    {
        await SaveAsync(request.Settings, ct);
        return Nothing.Value;
    }
}
```

**Использование:**

```csharp
await _mediator.Send(new SaveSettingsCommand(settings), ct);
```

### 3. Notification (Pub/Sub, 1:N)

Notification — событие, которое могут обработать **несколько** handler'ов. Идеально для event-driven архитектуры.

```csharp
public readonly struct HeroSelectedNotification : INotification
{
    public readonly string HeroId;
    public readonly HeroesGridMode SourceMode;

    public HeroSelectedNotification(string heroId, HeroesGridMode mode)
    {
        HeroId = heroId;
        SourceMode = mode;
    }
}

public class UpdateUIOnHeroSelected : INotificationHandler<HeroSelectedNotification>
{
    public UniTask Handle(HeroSelectedNotification notification, CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }
}

public class TrackAnalyticsOnHeroSelected : INotificationHandler<HeroSelectedNotification>
{
    public UniTask Handle(HeroSelectedNotification notification, CancellationToken ct)
    {
        Analytics.Track("hero_selected", notification.HeroId);
        return UniTask.CompletedTask;
    }
}
```

**Использование:**

```csharp
await _mediator.Publish(new HeroSelectedNotification("123", HeroesGridMode.Select), ct);
```

### 4. Stream Requests

Для операций, которые возвращают данные постепенно (прогресс загрузки, pagination):

```csharp
public readonly struct LoadAssetsRequest : IStreamRequest<float>
{
    public readonly string[] Paths;
    public LoadAssetsRequest(string[] paths) => Paths = paths;
}

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
```

**Использование:**

```csharp
await foreach (var progress in _mediator.CreateStream(new LoadAssetsRequest(paths), ct))
{
    _progressBar.Value = progress;
}
```

---

## Регистрация и настройка

### Базовая настройка

```csharp
public class GameInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddMediator(cfg =>
        {
            cfg.UseNotificationPublisher<SequentialPublisher>();
            
            cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
            cfg.AddHandler<SaveSettingsCommand, SaveSettingsHandler>();
            cfg.AddNotificationHandler<HeroSelectedNotification, UpdateUIOnHeroSelected>();
            cfg.AddStreamHandler<LoadAssetsRequest, float, LoadAssetsHandler>();
        });
    }
}
```

### Группировка регистраций

Рекомендуется группировать хендлеры в extension-методы:

```csharp
public static class ViewHandlerRegistration
{
    public static MediatorBuilder RegisterViewHandlers(this MediatorBuilder builder)
    {
        builder.AddHandler<
            ShowEmbeddedViewRequest<HeroesGridPanelViewModel>,
            bool,
            EmbeddedViewRequestHandler<HeroesGridPanelViewModel>>();

        builder.AddHandler<
            HideEmbeddedViewRequest<HeroesGridPanelViewModel>,
            HideEmbeddedViewHandler<HeroesGridPanelViewModel>>();

        return builder;
    }
}

builder.AddMediator(cfg =>
{
    cfg.UseNotificationPublisher<SequentialPublisher>();
    cfg.RegisterViewHandlers();
    cfg.RegisterHeroHandlers();
});
```

### Auto-Discovery (опционально)

Запусти `Tools/Mediator/Scan & Generate Handler Registration` для автогенерации:

```csharp
builder.AddMediator(cfg =>
{
    MediatorHandlerRegistration.RegisterAllHandlers(cfg);
});
```

### MediatorBuilder API

| Метод | Описание |
|-------|----------|
| `AddHandler<TRequest, TResponse, THandler>()` | Request с ответом |
| `AddHandler<TRequest, THandler>()` | Request без ответа (Nothing) |
| `AddNotificationHandler<TNotif, THandler>()` | Notification handler |
| `AddStreamHandler<TRequest, TResponse, THandler>()` | Stream handler |
| `AddBehavior<TBehavior>()` | Pipeline behavior |
| `AddPreProcessor<TRequest, TProcessor>()` | Pre-processor |
| `AddPostProcessor<TRequest, TResponse, TProcessor>()` | Post-processor |
| `UseNotificationPublisher<TPublisher>()` | Стратегия публикации |
| `SetDefaultLifetime(ServiceLifetime)` | Default lifetime для handlers |

### Service Lifetime

Handlers могут быть зарегистрированы с разными lifetime:

```csharp
cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>(ServiceLifetime.Singleton);
cfg.AddHandler<SaveSettingsCommand, SaveSettingsHandler>(ServiceLifetime.Transient);
```

- **Transient** (default) — новый экземпляр при каждом resolve
- **Singleton** — один экземпляр на всё приложение
- **Scoped** — один экземпляр на scope (если поддерживается)

---

## Примеры использования

### Реальный пример из проекта: View Management

**Request для показа View:**

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
```

**Handler:**

```csharp
public sealed class EmbeddedViewRequestHandler<TViewModel>
    : IRequestHandler<ShowEmbeddedViewRequest<TViewModel>, bool>
    where TViewModel : DisposableObject
{
    private readonly IEmbeddedPanelHost _host;

    public EmbeddedViewRequestHandler(IEmbeddedPanelHost host)
    {
        _host = host;
    }

    public UniTask<bool> Handle(
        ShowEmbeddedViewRequest<TViewModel> request,
        CancellationToken ct)
    {
        return _host.ShowEmbeddedAsync<TViewModel>(request.Context, ct);
    }
}
```

**Использование в ViewModel:**

```csharp
public class MainViewModel : DisposableObject
{
    private readonly IMediator _mediator;

    public async UniTaskVoid OnOpenHeroesGridAsync(HeroesGridMode mode)
    {
        await _mediator.Send(new ShowEmbeddedViewRequest<HeroesGridPanelViewModel>(
            new HeroesGridContext(mode)));
    }

    public async UniTask CloseHeroesGrid()
    {
        await _mediator.Send(new HideEmbeddedViewRequest<HeroesGridPanelViewModel>());
    }
}
```

### ViewModel как NotificationHandler

ViewModel может реализовать `INotificationHandler<T>` для реакции на события:

```csharp
public class MainViewModel : DisposableObject,
    INotificationHandler<HeroSelectedNotification>
{
    private readonly IMainModel _model;

    public UniTask Handle(HeroSelectedNotification notification, CancellationToken ct)
    {
        _model.SelectHero(notification.HeroId);
        return UniTask.CompletedTask;
    }
}
```

**Важно:** Не забудь зарегистрировать ViewModel как handler:

```csharp
cfg.AddNotificationHandler<HeroSelectedNotification, MainViewModel>();
```

### Публикация Notification

```csharp
public class HeroesGridPanelViewModel : DisposableObject
{
    private readonly IMediator _mediator;

    private async void OnSelectHero(string heroId)
    {
        _model.SelectHero(heroId);
        await _mediator.Publish(new HeroSelectedNotification(heroId, CurrentMode.Value));
    }
}
```

---

## Pipeline Behaviors

Pipeline behaviors оборачивают выполнение request'а. Позволяют добавить cross-cutting concerns без модификации хендлеров.

### Порядок выполнения

```
Request → Behavior1 → Behavior2 → ... → Handler → ... → Behavior2 → Behavior1 → Response
```

**Важно:** Behaviors выполняются в **обратном порядке** регистрации. Если зарегистрированы `[Logging, Validation]`, выполнение будет: `Logging → Validation → Handler → Validation → Logging`.

### Logging Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger;

    public LoggingBehavior(ILogger logger) => _logger = logger;

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.Info("Handling {0}", typeof(TRequest).Name);
        var stopwatch = Stopwatch.StartNew();
        
        var response = await next();
        
        stopwatch.Stop();
        _logger.Info("Handled {0} in {1}ms", typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        
        return response;
    }
}

cfg.AddBehavior<LoggingBehavior<,>>();
```

### Validation Behavior

```csharp
public interface IValidatable
{
    ValidationResult Validate();
}

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IValidatable
{
    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        var result = request.Validate();
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        return await next();
    }
}
```

### Exception Handling Behavior

```csharp
public class ExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Mediator] {typeof(TRequest).Name} failed: {ex.Message}");
            throw;
        }
    }
}
```

### Pre/Post Processors

Для более точечной обработки:

```csharp
public class RequestTimingPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public UniTask Process(TRequest request, CancellationToken ct)
    {
        PerformanceMarker.Begin(typeof(TRequest).Name);
        return UniTask.CompletedTask;
    }
}

public class RequestTimingPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : notnull
{
    public UniTask Process(TRequest request, TResponse response, CancellationToken ct)
    {
        PerformanceMarker.End(typeof(TRequest).Name);
        return UniTask.CompletedTask;
    }
}

cfg.AddPreProcessor<MyRequest, RequestTimingPreProcessor<MyRequest>>();
cfg.AddPostProcessor<MyRequest, MyResponse, RequestTimingPostProcessor<MyRequest, MyResponse>>();
```

---

## Notification Publishers

Выбор стратегии публикации notifications:

### SequentialPublisher (default)

Хендлеры выполняются **последовательно**. Если один упал — остальные всё равно выполнятся, ошибки агрегируются в `AggregateException`.

```csharp
cfg.UseNotificationPublisher<SequentialPublisher>();
```

**Когда использовать:** 
- Порядок важен
- Нужна гарантия выполнения всех хендлеров
- Зависимости между handlers

**Особенности:**
- Ошибки не прерывают выполнение остальных handlers
- Все ошибки собираются и выбрасываются как `AggregateException` в конце

### ParallelPublisher

Все хендлеры выполняются **параллельно** через `UniTask.WhenAll`.

```csharp
cfg.UseNotificationPublisher<ParallelPublisher>();
```

**Когда использовать:**
- Хендлеры независимы друг от друга
- Нужна максимальная скорость
- Порядок не важен

**Особенности:**
- Первая ошибка может прервать выполнение остальных (зависит от реализации)
- Быстрее для независимых handlers

### FireAndForgetPublisher

Запускает хендлеры и **не ждёт** их завершения. Ошибки логируются.

```csharp
cfg.UseNotificationPublisher<FireAndForgetPublisher>();
```

**Когда использовать:**
- Analytics
- Некритичные операции
- Когда не нужно ждать результат

**Особенности:**
- Не блокирует caller
- Ошибки логируются, но не пробрасываются
- Нельзя гарантировать выполнение

---

## Extensions

### Timeout

```csharp
var hero = await _mediator.SendWithTimeout(
    new GetHeroQuery(id),
    TimeSpan.FromSeconds(5));
```

**Реализация:** Создаёт `CancellationTokenSource` с таймаутом и связывает с переданным `CancellationToken`. При таймауте выбрасывает `TimeoutException`.

### Fire and Forget

```csharp
_mediator.PublishFireAndForget(new AnalyticsEvent("button_click"));
```

**Реализация:** Вызывает `Publish` с `CancellationToken.None` и вызывает `.Forget()` для UniTask, чтобы не ждать завершения.

### Default on Missing Handler

```csharp
var hero = await _mediator.SendOrDefault(
    new GetHeroQuery(id),
    defaultValue: Hero.Empty);
```

**Реализация:** Ловит `HandlerNotFoundException` и возвращает `defaultValue` вместо исключения.

---

## Паттерны и Best Practices

### 1. CQRS (Command Query Responsibility Segregation)

Разделяй requests на:
- **Queries** — получение данных (read)
- **Commands** — изменение состояния (write)

```csharp
public readonly struct GetHeroQuery : IRequest<Hero> { ... }
public readonly struct UpdateHeroCommand : IRequest { ... }
```

**Naming convention:**
- Queries: `GetXxxQuery`, `LoadXxxQuery`
- Commands: `SaveXxxCommand`, `DeleteXxxCommand`, `UpdateXxxCommand`

### 2. Generic Handlers

Для типовых операций используй generic handlers:

```csharp
public sealed class EmbeddedViewRequestHandler<TViewModel>
    : IRequestHandler<ShowEmbeddedViewRequest<TViewModel>, bool>
    where TViewModel : DisposableObject
{
    public UniTask<bool> Handle(ShowEmbeddedViewRequest<TViewModel> request, CancellationToken ct)
    {
        return _host.ShowEmbeddedAsync<TViewModel>(request.Context, ct);
    }
}
```

**Преимущества:**
- Меньше дублирования кода
- Один handler для множества типов
- Легче поддерживать

### 3. Context Objects

Используй context objects для передачи параметров:

```csharp
public interface IViewContext { }

public readonly struct EmptyContext : IViewContext
{
    public static readonly EmptyContext Instance = new();
}

public readonly struct HeroesGridContext : IViewContext
{
    public readonly HeroesGridMode Mode;
    public HeroesGridContext(HeroesGridMode mode) => Mode = mode;
}
```

**Использование:**

```csharp
await _mediator.Send(new ShowEmbeddedViewRequest<HeroesGridPanelViewModel>(
    new HeroesGridContext(HeroesGridMode.MyHeroes)));
```

### 4. Domain Events через Notifications

```csharp
public readonly struct OrderCompletedEvent : INotification
{
    public readonly Order Order;
    public OrderCompletedEvent(Order order) => Order = order;
}

public class SendEmailOnOrderCompleted : INotificationHandler<OrderCompletedEvent>
{
    public UniTask Handle(OrderCompletedEvent notification, CancellationToken ct)
    {
        return _emailService.SendOrderConfirmation(notification.Order);
    }
}

public class UpdateInventoryOnOrderCompleted : INotificationHandler<OrderCompletedEvent>
{
    public UniTask Handle(OrderCompletedEvent notification, CancellationToken ct)
    {
        return _inventoryService.Deduct(notification.Order.Items);
    }
}
```

### 5. Request должен быть readonly struct

Для избежания аллокаций и immutability:

```csharp
public readonly struct GetHeroQuery : IRequest<Hero>
{
    public readonly string HeroId;
    public GetHeroQuery(string heroId) => HeroId = heroId;
}
```

**Почему struct:**
- Нет heap allocation
- Value semantics
- Immutability через `readonly`

### 6. CancellationToken всегда передавай

```csharp
public async UniTask<Hero> Handle(GetHeroQuery request, CancellationToken ct)
{
    ct.ThrowIfCancellationRequested();
    return await _repository.GetByIdAsync(request.HeroId, ct);
}
```

**Почему:**
- Поддержка отмены операций
- Корректная работа с Unity lifecycle
- Предотвращение работы после dispose

### 7. Организация файлов

```
MVVM/
├── Requests/
│   ├── Context/           # IViewContext implementations
│   │   ├── IViewContext.cs
│   │   └── HeroesGridContext.cs
│   └── Views/             # View-related requests
│       ├── ShowEmbeddedViewRequest.cs
│       └── HideEmbeddedViewRequest.cs
│
├── Notifications/         # Domain events
│   ├── HeroSelectedNotification.cs
│   └── EmbeddedPanelStateChangedNotification.cs
│
└── Handlers/              # Handler implementations
    └── Views/
        ├── EmbeddedViewRequestHandler.cs
        └── HideEmbeddedViewHandler.cs
```

---

## Возможные расширения

### 1. Retry Policy Behavior

```csharp
public interface IRetryable
{
    int MaxRetries { get; }
}

public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IRetryable
{
    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        int attempts = 0;
        while (true)
        {
            try
            {
                return await next();
            }
            catch when (++attempts < request.MaxRetries)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)), cancellationToken: ct);
            }
        }
    }
}
```

### 2. Caching Behavior

```csharp
public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheable
{
    private readonly ICache _cache;

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        var key = request.CacheKey;
        if (_cache.TryGet<TResponse>(key, out var cached))
            return cached;

        var response = await next();
        _cache.Set(key, response, request.CacheDuration);
        return response;
    }
}
```

### 3. Authorization Behavior

```csharp
public interface IAuthorizedRequest
{
    string RequiredPermission { get; }
}

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuthorizedRequest
{
    private readonly IAuthService _auth;

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        if (!await _auth.HasPermission(request.RequiredPermission, ct))
            throw new UnauthorizedException();

        return await next();
    }
}
```

### 4. Transaction Behavior

```csharp
public interface ITransactional { }

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactional
{
    private readonly IUnitOfWork _uow;

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        await _uow.BeginTransaction(ct);
        try
        {
            var response = await next();
            await _uow.Commit(ct);
            return response;
        }
        catch
        {
            await _uow.Rollback(ct);
            throw;
        }
    }
}
```

### 5. Request Deduplication

```csharp
public interface IIdempotent
{
    string IdempotencyKey { get; }
}

public class DeduplicationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IIdempotent
{
    private readonly IIdempotencyStore _store;

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        if (_store.HasProcessed(request.IdempotencyKey))
            return _store.GetResult<TResponse>(request.IdempotencyKey);

        var response = await next();
        _store.Store(request.IdempotencyKey, response);
        return response;
    }
}
```

### 6. Performance Monitoring Behavior

```csharp
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMetrics _metrics;
    private const float SlowRequestThresholdMs = 500f;

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            return await next();
        }
        finally
        {
            sw.Stop();
            var requestName = typeof(TRequest).Name;
            
            _metrics.RecordTiming($"mediator.{requestName}", sw.ElapsedMilliseconds);
            
            if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
            {
                Debug.LogWarning($"[Mediator] Slow request: {requestName} took {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}
```

### 7. Request Batching

Для группировки множественных запросов одного типа:

```csharp
public interface IBatchableRequest<TResponse>
{
    string BatchKey { get; }
}

public class BatchingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IBatchableRequest<TResponse>
{
    private readonly IBatchProcessor _batchProcessor;

    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        return await _batchProcessor.ProcessBatch(request, next, ct);
    }
}
```

---

## FAQ

### Когда использовать Request vs Notification?

| Request | Notification |
|---------|--------------|
| Нужен ответ | Не нужен ответ |
| 1 handler | N handlers |
| Синхронный flow | Event-driven |
| Query/Command | Domain Event |

### Mediator vs прямые вызовы сервисов?

Mediator даёт:
- **Loose coupling** между компонентами
- **Единую точку** для cross-cutting concerns (logging, validation, caching)
- **Легкое тестирование** (mock IMediator)
- **Предсказуемый flow** данных

**Когда НЕ использовать Mediator:**
- Простые utility методы
- Внутренние вызовы внутри одного класса
- Когда нужна максимальная производительность (overhead есть)

### Как тестировать handlers?

```csharp
[Test]
public async UniTask GetHeroQuery_ReturnsHero()
{
    var repo = Substitute.For<IHeroRepository>();
    repo.GetByIdAsync("1", default).Returns(new Hero { Id = "1" });
    
    var handler = new GetHeroQueryHandler(repo);
    var result = await handler.Handle(new GetHeroQuery("1"), default);
    
    Assert.AreEqual("1", result.Id);
}
```

### Как тестировать с IMediator?

```csharp
[Test]
public async UniTask ViewModel_SendsRequest()
{
    var mediator = Substitute.For<IMediator>();
    mediator.Send(Arg.Any<ShowEmbeddedViewRequest<HeroesGridPanelViewModel>>(), default)
        .Returns(true);
    
    var vm = new MainViewModel(model, mediator, viewService, logger);
    await vm.OnOpenHeroesGridAsync(HeroesGridMode.Select);
    
    await mediator.Received(1).Send(
        Arg.Is<ShowEmbeddedViewRequest<HeroesGridPanelViewModel>>(r => 
            r.Context is HeroesGridContext ctx && ctx.Mode == HeroesGridMode.Select),
        default);
}
```

### Где создавать Requests и Notifications?

Следуй структуре проекта:
- **Requests** — в `MVVM/Requests/` или `Domain/Requests/`
- **Notifications** — в `MVVM/Notifications/` или `Domain/Events/`
- **Handlers** — в `MVVM/Handlers/` или рядом с requests

### Как добавить новый Request?

1. Создай Request struct в `MVVM/Requests/`:

```csharp
public readonly struct MyRequest : IRequest<MyResponse>
{
    public readonly string Param;
    public MyRequest(string param) => Param = param;
}
```

2. Создай Handler в `MVVM/Handlers/`:

```csharp
public class MyRequestHandler : IRequestHandler<MyRequest, MyResponse>
{
    public UniTask<MyResponse> Handle(MyRequest request, CancellationToken ct)
    {
        return UniTask.FromResult(new MyResponse());
    }
}
```

3. Зарегистрируй в Registration:

```csharp
public static MediatorBuilder RegisterMyHandlers(this MediatorBuilder builder)
{
    builder.AddHandler<MyRequest, MyResponse, MyRequestHandler>();
    return builder;
}
```

4. Добавь вызов в Installer:

```csharp
builder.AddMediator(cfg =>
{
    cfg.RegisterMyHandlers();
});
```

### Можно ли использовать Mediator в MonoBehaviour?

Да, но нужно быть осторожным с lifecycle:

```csharp
public class MyMonoBehaviour : MonoBehaviour
{
    private IMediator _mediator;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _mediator = Container.Resolve<IMediator>();
        _cts = new CancellationTokenSource();
    }

    private async void Start()
    {
        try
        {
            var result = await _mediator.Send(new MyRequest(), _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Ожидаемо при уничтожении объекта
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

### Как работает кеширование wrapper'ов?

Wrapper'ы кешируются в статических `ConcurrentDictionary` на уровне класса `Mediator`. Это означает:
- Один экземпляр wrapper'а на тип request/notification
- Thread-safe доступ
- Нет повторного создания при каждом вызове

### Можно ли использовать несколько Pipeline Behaviors?

Да, behaviors выполняются в обратном порядке регистрации:

```csharp
cfg.AddBehavior<LoggingBehavior<,>>();
cfg.AddBehavior<ValidationBehavior<,>>();
cfg.AddBehavior<ExceptionBehavior<,>>();

// Выполнение: Logging → Validation → Exception → Handler → Exception → Validation → Logging
```

---

## Quick Reference

### Создание Request с ответом

```csharp
public readonly struct GetXxxQuery : IRequest<Xxx> { }
public class GetXxxHandler : IRequestHandler<GetXxxQuery, Xxx> { }
```

### Создание Command без ответа

```csharp
public readonly struct DoXxxCommand : IRequest { }
public class DoXxxHandler : IRequestHandler<DoXxxCommand> { }
```

### Создание Notification

```csharp
public readonly struct XxxEvent : INotification { }
public class OnXxxHandler : INotificationHandler<XxxEvent> { }
```

### Отправка Request

```csharp
var result = await _mediator.Send(new GetXxxQuery(param), ct);
await _mediator.Send(new DoXxxCommand(param), ct);
```

### Публикация Notification

```csharp
await _mediator.Publish(new XxxEvent(data), ct);
_mediator.PublishFireAndForget(new XxxEvent(data));
```

### Регистрация Handler

```csharp
cfg.AddHandler<GetXxxQuery, Xxx, GetXxxHandler>();
cfg.AddHandler<DoXxxCommand, DoXxxHandler>();
cfg.AddNotificationHandler<XxxEvent, OnXxxHandler>();
```

### Pipeline Behavior

```csharp
public class MyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async UniTask<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        // Before
        var response = await next();
        // After
        return response;
    }
}

cfg.AddBehavior<MyBehavior<,>>();
```

---

## Дополнительные ресурсы

- **MediatR (оригинал):** https://github.com/jbogard/MediatR
- **UniTask:** https://github.com/Cysharp/UniTask
- **Reflex DI:** https://github.com/gustavopsantos/reflex

---

**Последнее обновление:** 2026-01-29
