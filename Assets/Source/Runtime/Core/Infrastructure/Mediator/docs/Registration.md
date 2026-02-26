# Registration — DI регистрация

Система регистрации handlers, behaviors и настройки Mediator через Reflex DI.

**Namespace:** `Core.Infrastructure.Mediator.Registration`

---

## Обзор компонентов

| Компонент | Назначение |
|-----------|------------|
| `MediatorBuilder` | Fluent builder для настройки |
| `MediatorReflexExtensions` | Extension method для Reflex |
| `HandlerRegistry` | Реестр зарегистрированных handlers |
| `HandlerDescriptor` | Описание handler (тип, lifetime) |
| `ServiceLifetime` | Enum: Singleton, Scoped, Transient |

---

## Базовая регистрация

```csharp
public class GameInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddMediator(cfg =>
        {
            // Настройка publisher
            cfg.UseNotificationPublisher<SequentialPublisher>();
            
            // Request handlers
            cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
            cfg.AddHandler<SaveSettingsCommand, SaveSettingsHandler>();
            
            // Notification handlers
            cfg.AddNotificationHandler<HeroSelectedEvent, UIUpdateHandler>();
            cfg.AddNotificationHandler<HeroSelectedEvent, AnalyticsHandler>();
            
            // Stream handlers
            cfg.AddStreamHandler<LoadAssetsRequest, float, LoadAssetsHandler>();
            
            // Pipeline behaviors
            cfg.AddBehavior<LoggingBehavior<,>>();
        });
    }
}
```

---

## MediatorBuilder API

### AddHandler<TRequest, TResponse, THandler>

Регистрирует request handler с ответом.

```csharp
public MediatorBuilder AddHandler<TRequest, TResponse, THandler>(
    ServiceLifetime lifetime = ServiceLifetime.Transient)
    where TRequest : IRequest<TResponse>
    where THandler : class, IRequestHandler<TRequest, TResponse>
```

**Пример:**

```csharp
cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
cfg.AddHandler<CreateOrderCommand, OrderId, CreateOrderHandler>(ServiceLifetime.Singleton);
```

### AddHandler<TRequest, THandler>

Регистрирует request handler без ответа (command).

```csharp
public MediatorBuilder AddHandler<TRequest, THandler>(
    ServiceLifetime lifetime = ServiceLifetime.Transient)
    where TRequest : IRequest
    where THandler : class, IRequestHandler<TRequest>
```

**Пример:**

```csharp
cfg.AddHandler<SaveSettingsCommand, SaveSettingsHandler>();
cfg.AddHandler<DeleteHeroCommand, DeleteHeroHandler>();
```

### AddNotificationHandler<TNotification, THandler>

Регистрирует notification handler.

```csharp
public MediatorBuilder AddNotificationHandler<TNotification, THandler>(
    ServiceLifetime lifetime = ServiceLifetime.Transient)
    where TNotification : INotification
    where THandler : class, INotificationHandler<TNotification>
```

**Пример:**

```csharp
// Несколько handlers для одного notification
cfg.AddNotificationHandler<OrderCreatedEvent, SendEmailHandler>();
cfg.AddNotificationHandler<OrderCreatedEvent, UpdateInventoryHandler>();
cfg.AddNotificationHandler<OrderCreatedEvent, AnalyticsHandler>();
```

### AddStreamHandler<TRequest, TResponse, THandler>

Регистрирует stream request handler.

```csharp
public MediatorBuilder AddStreamHandler<TRequest, TResponse, THandler>(
    ServiceLifetime lifetime = ServiceLifetime.Transient)
    where TRequest : IStreamRequest<TResponse>
    where THandler : class, IStreamRequestHandler<TRequest, TResponse>
```

**Пример:**

```csharp
cfg.AddStreamHandler<LoadAssetsRequest, float, LoadAssetsHandler>();
cfg.AddStreamHandler<GetHeroesPagedRequest, Hero, GetHeroesPagedHandler>();
```

### AddBehavior<TBehavior>

Регистрирует pipeline behavior.

```csharp
public MediatorBuilder AddBehavior<TBehavior>()
    where TBehavior : class
```

**Пример:**

```csharp
cfg.AddBehavior<LoggingBehavior<,>>();
cfg.AddBehavior<ValidationBehavior<,>>();
cfg.AddBehavior<CachingBehavior<,>>();
```

### AddPreProcessor<TRequest, TProcessor>

Регистрирует pre-processor.

```csharp
public MediatorBuilder AddPreProcessor<TRequest, TProcessor>()
    where TRequest : notnull
    where TProcessor : class, IRequestPreProcessor<TRequest>
```

**Пример:**

```csharp
cfg.AddPreProcessor<SaveHeroCommand, AuditPreProcessor<SaveHeroCommand>>();
```

### AddPostProcessor<TRequest, TResponse, TProcessor>

Регистрирует post-processor.

```csharp
public MediatorBuilder AddPostProcessor<TRequest, TResponse, TProcessor>()
    where TRequest : notnull
    where TProcessor : class, IRequestPostProcessor<TRequest, TResponse>
```

**Пример:**

```csharp
cfg.AddPostProcessor<GetHeroQuery, Hero, MetricsPostProcessor<GetHeroQuery, Hero>>();
```

### UseNotificationPublisher<TPublisher>

Устанавливает стратегию публикации notifications.

```csharp
public MediatorBuilder UseNotificationPublisher<TPublisher>()
    where TPublisher : class, INotificationPublisher
```

**Пример:**

```csharp
cfg.UseNotificationPublisher<SequentialPublisher>(); // default
cfg.UseNotificationPublisher<ParallelPublisher>();
cfg.UseNotificationPublisher<FireAndForgetPublisher>();
```

### SetDefaultLifetime

Устанавливает default lifetime для handlers.

```csharp
public MediatorBuilder SetDefaultLifetime(ServiceLifetime lifetime)
```

**Пример:**

```csharp
cfg.SetDefaultLifetime(ServiceLifetime.Singleton);
cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>(); // Singleton
```

---

## ServiceLifetime

```csharp
public enum ServiceLifetime
{
    Singleton,  // Один экземпляр на всё приложение
    Scoped,     // Один экземпляр на scope
    Transient   // Новый экземпляр при каждом resolve
}
```

| Lifetime | Когда использовать |
|----------|-------------------|
| `Transient` | Stateless handlers, default |
| `Singleton` | Stateless handlers с тяжёлыми зависимостями |
| `Scoped` | Handlers с per-request state |

### Пример

```csharp
cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>(ServiceLifetime.Transient);
cfg.AddHandler<CacheableQuery, Data, CacheHandler>(ServiceLifetime.Singleton);
```

---

## Группировка регистраций

### Extension Methods

Рекомендуется группировать handlers в extension methods:

```csharp
// ViewHandlerRegistration.cs
public static class ViewHandlerRegistration
{
    public static MediatorBuilder RegisterViewHandlers(this MediatorBuilder builder)
    {
        // Show/Hide requests
        builder.AddHandler<
            ShowEmbeddedViewRequest<HeroesGridPanelViewModel>,
            bool,
            EmbeddedViewRequestHandler<HeroesGridPanelViewModel>>();

        builder.AddHandler<
            HideEmbeddedViewRequest<HeroesGridPanelViewModel>,
            HideEmbeddedViewHandler<HeroesGridPanelViewModel>>();

        builder.AddHandler<
            ShowEmbeddedViewRequest<SettingsPanelViewModel>,
            bool,
            EmbeddedViewRequestHandler<SettingsPanelViewModel>>();

        return builder;
    }
}

// HeroHandlerRegistration.cs
public static class HeroHandlerRegistration
{
    public static MediatorBuilder RegisterHeroHandlers(this MediatorBuilder builder)
    {
        builder.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
        builder.AddHandler<GetAllHeroesQuery, IReadOnlyList<Hero>, GetAllHeroesHandler>();
        builder.AddHandler<SaveHeroCommand, SaveHeroHandler>();
        builder.AddHandler<DeleteHeroCommand, DeleteHeroHandler>();
        
        builder.AddNotificationHandler<HeroSelectedEvent, HeroSelectedHandler>();
        
        return builder;
    }
}
```

### Использование в Installer

```csharp
public class GameInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddMediator(cfg =>
        {
            cfg.UseNotificationPublisher<SequentialPublisher>();
            
            // Группы handlers
            cfg.RegisterViewHandlers();
            cfg.RegisterHeroHandlers();
            cfg.RegisterOrderHandlers();
            cfg.RegisterSettingsHandlers();
            
            // Pipeline
            cfg.AddBehavior<LoggingBehavior<,>>();
            cfg.AddBehavior<ValidationBehavior<,>>();
        });
    }
}
```

---

## Generic Handler Registration

Для generic handlers используй explicit регистрацию для каждого типа:

```csharp
// Generic handler
public sealed class EmbeddedViewRequestHandler<TViewModel>
    : IRequestHandler<ShowEmbeddedViewRequest<TViewModel>, bool>
    where TViewModel : DisposableObject

// Регистрация для каждого ViewModel
public static MediatorBuilder RegisterAllViewHandlers(this MediatorBuilder builder)
{
    // HeroesGridPanelViewModel
    builder.AddHandler<
        ShowEmbeddedViewRequest<HeroesGridPanelViewModel>,
        bool,
        EmbeddedViewRequestHandler<HeroesGridPanelViewModel>>();
    
    // SettingsPanelViewModel
    builder.AddHandler<
        ShowEmbeddedViewRequest<SettingsPanelViewModel>,
        bool,
        EmbeddedViewRequestHandler<SettingsPanelViewModel>>();
    
    // InventoryPanelViewModel
    builder.AddHandler<
        ShowEmbeddedViewRequest<InventoryPanelViewModel>,
        bool,
        EmbeddedViewRequestHandler<InventoryPanelViewModel>>();
    
    return builder;
}
```

---

## MediatorReflexExtensions

Extension method для интеграции с Reflex:

```csharp
public static class MediatorReflexExtensions
{
    public static ContainerBuilder AddMediator(
        this ContainerBuilder builder,
        Action<MediatorBuilder> configure)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var mediatorBuilder = new MediatorBuilder(builder);
        configure(mediatorBuilder);
        mediatorBuilder.Build();

        return builder;
    }
}
```

### Что регистрируется

При вызове `Build()` регистрируются:

| Тип | Интерфейс | Lifetime |
|-----|-----------|----------|
| `Mediator` | `IMediator`, `ISender`, `IPublisher` | Singleton |
| `HandlerRegistry` | `IHandlerRegistry` | Singleton |
| `NotificationPublisher` | `INotificationPublisher` | Singleton |
| Request handlers | `IRequestHandler<,>` | Configurable |
| Notification handlers | `INotificationHandler<>` | Configurable |
| Stream handlers | `IStreamRequestHandler<,>` | Configurable |
| Behaviors | Direct | Transient |

---

## HandlerRegistry

Внутренний реестр зарегистрированных handlers:

```csharp
internal interface IHandlerRegistry
{
    bool HasHandler(Type requestType);
    Type GetHandlerType(Type requestType);
    IReadOnlyList<Type> GetNotificationHandlerTypes(Type notificationType);
    IReadOnlyList<Type> GetBehaviorTypes();
}
```

### Использование (advanced)

```csharp
public class DebugService
{
    private readonly IHandlerRegistry _registry;

    public DebugService(IHandlerRegistry registry)
    {
        _registry = registry;
    }

    public void PrintRegisteredHandlers()
    {
        // Проверка наличия handler
        var hasHandler = _registry.HasHandler(typeof(GetHeroQuery));
        Debug.Log($"GetHeroQuery handler: {hasHandler}");
    }
}
```

---

## HandlerDescriptor

Описание зарегистрированного handler:

```csharp
public sealed class HandlerDescriptor
{
    public Type RequestType { get; }      // Тип request/notification
    public Type ResponseType { get; }     // Тип response (null для notifications)
    public Type HandlerType { get; }      // Тип handler
    public ServiceLifetime? Lifetime { get; } // Lifetime (или null для default)
}
```

---

## Auto-Discovery (Generated)

### Генерация регистрации

1. Запусти `Tools/Mediator/Scan & Generate Handler Registration`
2. Генерируется `MediatorHandlerRegistration.generated.cs`

### Использование

```csharp
builder.AddMediator(cfg =>
{
    // Авто-регистрация всех handlers
    MediatorHandlerRegistration.RegisterAllHandlers(cfg);
    
    // Можно добавить дополнительные настройки
    cfg.AddBehavior<LoggingBehavior<,>>();
});
```

### Пример сгенерированного файла

```csharp
// Auto-generated. Do not edit.
public static class MediatorHandlerRegistration
{
    public static void RegisterAllHandlers(MediatorBuilder builder)
    {
        // Request handlers
        builder.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
        builder.AddHandler<SaveSettingsCommand, SaveSettingsHandler>();
        
        // Notification handlers
        builder.AddNotificationHandler<HeroSelectedEvent, UIUpdateHandler>();
        builder.AddNotificationHandler<HeroSelectedEvent, AnalyticsHandler>();
        
        // Stream handlers
        builder.AddStreamHandler<LoadAssetsRequest, float, LoadAssetsHandler>();
    }
}
```

---

## Внутренняя реализация Build()

```csharp
internal void Build()
{
    // 1. Создаём реестр
    var registry = new HandlerRegistry(
        _requestHandlers,
        _notificationHandlers,
        _streamHandlers,
        _behaviorTypes,
        _preProcessorTypes,
        _postProcessorTypes);

    // 2. Регистрируем реестр
    _containerBuilder.AddSingleton<IHandlerRegistry>(_ => registry);
    
    // 3. Регистрируем publisher
    RegisterNotificationPublisher();
    
    // 4. Регистрируем Mediator
    RegisterMediator();
    
    // 5. Регистрируем handlers
    RegisterHandlers(_requestHandlers);
    RegisterHandlers(_notificationHandlers);
    RegisterHandlers(_streamHandlers);
    
    // 6. Регистрируем behaviors и processors
    RegisterTypes(_behaviorTypes, ServiceLifetime.Transient);
    RegisterTypes(_preProcessorTypes, ServiceLifetime.Transient);
    RegisterTypes(_postProcessorTypes, ServiceLifetime.Transient);
}
```

---

## Best Practices

### ✅ Группируй handlers по feature

```
Registration/
├── Heroes/
│   └── HeroHandlerRegistration.cs
├── Orders/
│   └── OrderHandlerRegistration.cs
├── Views/
│   └── ViewHandlerRegistration.cs
└── MediatorInstallerExtensions.cs
```

### ✅ Используй extension methods

```csharp
// ✅ Правильно — чистая регистрация
cfg.RegisterHeroHandlers();
cfg.RegisterOrderHandlers();

// ❌ Неправильно — всё в одном месте
cfg.AddHandler<...>();
cfg.AddHandler<...>();
cfg.AddHandler<...>();
// 50+ строк handlers
```

### ✅ Явно указывай lifetime для stateful handlers

```csharp
// ✅ Правильно
cfg.AddHandler<CacheQuery, Data, CacheHandler>(ServiceLifetime.Singleton);

// ❌ Неправильно — полагаемся на default
cfg.AddHandler<CacheQuery, Data, CacheHandler>();
```

### ✅ Регистрируй behaviors в правильном порядке

```csharp
cfg.AddBehavior<LoggingBehavior<,>>();      // 1. Внешний
cfg.AddBehavior<AuthorizationBehavior<,>>(); // 2
cfg.AddBehavior<ValidationBehavior<,>>();    // 3
cfg.AddBehavior<CachingBehavior<,>>();       // 4
cfg.AddBehavior<ExceptionBehavior<,>>();     // 5. Внутренний
```

### ✅ Один Installer — один контекст

```csharp
// ✅ Правильно — отдельные installers
public class CoreInstaller : IInstaller { }
public class GameInstaller : IInstaller { }
public class UIInstaller : IInstaller { }

// ❌ Неправильно — всё в одном
public class MegaInstaller : IInstaller { /* 1000 строк */ }
```

---

## Troubleshooting

### HandlerNotFoundException

```csharp
// Ошибка: No handler registered for request type GetHeroQuery
```

**Причина:** Handler не зарегистрирован.

**Решение:**
```csharp
cfg.AddHandler<GetHeroQuery, Hero, GetHeroQueryHandler>();
```

### Duplicate Registration

```csharp
// Warning: Handler for GetHeroQuery already registered
```

**Причина:** Handler зарегистрирован дважды.

**Решение:** Проверь, что не вызываешь регистрацию дважды.

### Dependency Resolution Error

```csharp
// Error: Unable to resolve IHeroRepository
```

**Причина:** Зависимость handler не зарегистрирована.

**Решение:**
```csharp
builder.AddSingleton<IHeroRepository, HeroRepository>();
```

---

## См. также

- [Handlers.md](./Handlers.md) — типы handlers
- [Pipeline.md](./Pipeline.md) — behaviors
- [Publishing.md](./Publishing.md) — notification publishers
