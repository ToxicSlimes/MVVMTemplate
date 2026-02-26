# Infrastructure — HOWTO

*Обновлено: 2026-02-11*

---

## Как добавить новый сервис в Infrastructure

### 1. Создай интерфейс в Abstractions

```
Core/Abstractions/Services/IMyNewService.cs
```

```csharp
namespace Core.Abstractions.Services
{
    public interface IMyNewService
    {
        UniTask<Result> DoSomethingAsync(string input, CancellationToken ct = default);
    }
}
```

### 2. Создай реализацию в Infrastructure

Структура: `Infrastructure/Services/{Domain}/MyNewService.cs`

```csharp
namespace Core.Infrastructure.Services.MyDomain
{
    public class MyNewService : IMyNewService
    {
        private readonly ILogger _logger;
        private readonly IAssetProvider _assetProvider;

        public MyNewService(ILogger logger, IAssetProvider assetProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assetProvider = assetProvider ?? throw new ArgumentNullException(nameof(assetProvider));
        }

        public async UniTask<Result> DoSomethingAsync(string input, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _logger.Debug("[MyNewService] Processing: {0}", input);
            
            try
            {
                // логика
                return new Result();
            }
            catch (Exception ex)
            {
                _logger.Error("[MyNewService] Failed: {0}", ex.Message);
                throw;
            }
        }
    }
}
```

### 3. Зарегистрируй в DI (Installer)

В соответствующем Installer:

```csharp
builder.AddSingleton<IMyNewService, MyNewService>();
```

### 4. Используй через DI

```csharp
public class SomeHandler : IRequestHandler<SomeQuery, SomeResult>
{
    private readonly IMyNewService _myService;

    public SomeHandler(IMyNewService myService)
    {
        _myService = myService;
    }

    public async UniTask<SomeResult> Handle(SomeQuery request, CancellationToken ct)
    {
        var result = await _myService.DoSomethingAsync(request.Input, ct);
        return new SomeResult(result);
    }
}
```

---

## Как добавить HTTP endpoint

Используй `ApiClient` — он уже реализует `IApiClient`:

```csharp
// GET
var response = await _apiClient.GetAsync<MyDto>("my-endpoint", cancellationToken: ct);

// POST
var response = await _apiClient.PostAsync<MyDto>("my-endpoint", bodyObject, cancellationToken: ct);
```

Для нового domain-specific клиента — оберни `IApiClient`:

```csharp
public class MyDomainClient : IMyDomainClient
{
    private readonly IApiClient _apiClient;

    public async UniTask<Hero> GetHeroAsync(string id, CancellationToken ct)
    {
        var response = await _apiClient.GetAsync<Hero>($"heroes/{id}", cancellationToken: ct);
        return response.Data;
    }
}
```

---

## Как добавить конфиг-провайдер

`ConfigProvider` загружает `ConfigBase` ScriptableObject через Addressables:

```csharp
var audioConfig = await _configProvider.GetConfigAsync<AudioConfig>();
```

Для нового конфига:
1. Создай `MyConfig : ConfigBase` в `Core.Data`
2. Создай ScriptableObject asset
3. Добавь Addressable label (если нужен)
4. Загружай через `IConfigProvider.GetConfigAsync<MyConfig>()`

---

## Как добавить Platform-сервис

Для platform-specific логики (clipboard, file dialog, etc.):

1. Интерфейс в `Abstractions/Services/Platform/`
2. Реализация в `Infrastructure/Services/Platform/`
3. При необходимости — `#if UNITY_EDITOR` / `#if UNITY_WEBGL` для платформенных различий
