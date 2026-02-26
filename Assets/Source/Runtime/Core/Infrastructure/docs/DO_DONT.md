# Infrastructure — DO / DON'T

*Обновлено: 2026-02-11*

---

## ✅ DO

### Реализация интерфейсов
- **Реализуй интерфейсы из `Core.Abstractions`** — каждый сервис в Infrastructure должен имплементировать интерфейс из Abstractions (`IApiClient`, `IChatService`, `IConfigProvider`, etc.)
- **Конструктор принимает только абстракции** — зависимости через DI, не конкретные классы

```csharp
// ✅ Правильно
public class ApiClient : IApiClient
{
    public ApiClient(IUrlBuilder urlBuilder, IHttpTransportService httpTransport, ILogger logger) { }
}
```

### Async / UniTask
- **Используй `UniTask`** для всех async операций — НЕ `Task`, НЕ `async void`
- **Передавай `CancellationToken`** во все async методы
- **Используй `.Forget()`** только для fire-and-forget операций с осознанным решением

```csharp
// ✅ Правильно
public async UniTask<T> GetConfigAsync<T>(CancellationToken ct = default) { }
```

### Error Handling
- **Бросай `ArgumentNullException`** для null-параметров в конструкторах и публичных методах
- **Логируй ошибки через `ILogger`** — не через `Debug.Log` напрямую (кроме критических fallback)
- **Оборачивай внешние вызовы** (HTTP, файловая система) в try-catch с логированием

### Регистрация
- **Регистрируй сервисы в DI** через `ContainerBuilder` в Installer-классах
- **Singleton** для stateful сервисов (ApiClient, ChatService, AudioService)
- **Transient** для stateless или per-request сервисов

### Структура
- **Группируй сервисы по доменам** — `Services/Network/`, `Services/Audio/`, `Services/Chat/`
- **Один сервис = один файл** — не мешай несколько классов в один файл

---

## ❌ DON'T

### Зависимости
- **НЕ ссылайся на MVVM** — Infrastructure НЕ знает о Views, ViewModels, BindableView
- **НЕ ссылайся на Presentation** — НЕ знает о States, Scopes, MonoBehaviour-контроллерах
- **НЕ добавляй UI логику в сервисы** — сервис не должен управлять Canvas, UI элементами, анимациями

```csharp
// ❌ НЕЛЬЗЯ в Infrastructure сервисе
using Core.Abstractions.MVVM.Views;  // Нет!
_viewService.ShowAsync<SomeView>();   // Нет!
```

### Async
- **НЕ используй `async void`** — только `async UniTask` или `async UniTaskVoid`
- **НЕ блокируй поток** — никаких `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`
- **НЕ глотай исключения** — всегда логируй или пробрасывай

```csharp
// ❌ Плохо
try { await DoSomething(); } catch { } // Проглотили ошибку
```

### Состояние
- **НЕ храни mutable static state** — всё через DI и instance-поля
- **НЕ создавай God-сервисы** — один сервис, одна ответственность
- **НЕ обращайся к `GameObject.Find`** или `Resources.Load` — используй `IAssetProvider`

### Прочее
- **НЕ хардкодь строки** — endpoints, адреса, ключи — в конфиги
- **НЕ дублируй логику** между сервисами — выноси в Extensions или shared utility
