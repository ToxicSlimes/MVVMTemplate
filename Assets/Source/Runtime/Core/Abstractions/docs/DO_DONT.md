# Abstractions — DO / DON'T

> Слой Abstractions — контракты между Core и Infrastructure/MVVM-реализациями. Здесь живут **интерфейсы, базовые классы, enums и value types**. Никакой бизнес-логики и конкретных реализаций.

---

## ✅ DO — что делать надо

### 1. Интерфейс для каждого сервиса

Любой сервис, доступный другим слоям — через интерфейс в Abstractions:

```csharp
// ✅ Правильно — контракт в Abstractions
public interface IChatService
{
    UniTask<ChatModel> CreateChatAsync(CreateChatRequestDto dto, CancellationToken ct = default);
    UniTask<ChatModel> GetChatHistoryAsync(string chatId, CancellationToken ct = default);
    UniTask<bool> DeleteChatAsync(string chatId, CancellationToken ct = default);
}
```

### 2. Минимальные контракты

Интерфейс описывает **только то, что нужно** потребителю. Один интерфейс — одна ответственность:

```csharp
// ✅ Чёткая ответственность
public interface IClipboardService
{
    void SetText(string text);
    string GetText();
}

public interface IFileSystemService
{
    byte[] ReadAllBytes(string path);
    void WriteAllBytes(string path, byte[] bytes);
    bool FileExists(string path);
}
```

### 3. Reactive types (R3) в интерфейсах

Для input и observable-данных используй `Observable<T>` из R3:

```csharp
public interface IInputService : IDisposable
{
    Observable<Unit> OnCancel { get; }
    Observable<Unit> OnSubmit { get; }
    Observable<Vector2> OnNavigate { get; }
    Observable<Vector2> OnMove { get; }
    Observable<Unit> OnAttack { get; }
    // ...
}
```

### 4. NavigationTarget как readonly struct

Навигационные цели — легковесные struct-маркеры:

```csharp
public interface INavigationTarget { }

public readonly struct LoginNavigation : INavigationTarget { }
public readonly struct ChatNavigation : INavigationTarget { }
public readonly struct MainNavigation : INavigationTarget { }
```

### 5. Базовые классы для Config, View, Element

Иерархия конфигов и UI-базовых классов живёт здесь:

```csharp
// Config иерархия
ConfigBase → ServiceConfigBase  // для сервисов
           → DataConfigBase     // для статических данных
           → ViewConfigBase     // для UI конфигов

// MVVM базовые классы
BindableView<TViewModel>        // базовая view
BindableElement<TData>          // базовый элемент списка
DisposableObject                // базовый ViewModel
```

### 6. CancellationToken в async методах

Все async-методы **обязаны** принимать `CancellationToken`:

```csharp
// ✅
UniTask<ChatModel> CreateChatAsync(CreateChatRequestDto dto, CancellationToken ct = default);

// ❌ Без CancellationToken
UniTask<ChatModel> CreateChatAsync(CreateChatRequestDto dto);
```

### 7. Fluent Builder интерфейсы

Для сложных конфигураций используй fluent builder pattern:

```csharp
public interface IButtonBuilder
{
    IButtonBuilder Id(string id);
    IButtonBuilder Label(string text);
    IButtonBuilder Variant(UiButtonVariant variant);
    IButtonBuilder OnClick(Action<IActionBuilder> configure);
    UiComponentModel Build();
}
```

### 8. IDisposable где нужно

Сервисы и модели, владеющие ресурсами, наследуют `IDisposable`:

```csharp
public interface IInputService : IDisposable { }
public interface IModel : IDisposable { }
public interface IViewModel : IDisposable { }
```

---

## ❌ DON'T — что делать нельзя

### 1. Никакой реализации бизнес-логики

```csharp
// ❌ НЕЛЬЗЯ — логика в Abstractions
public interface IChatService
{
    UniTask<ChatModel> CreateChatAsync(CreateChatRequestDto dto, CancellationToken ct = default)
    {
        // default implementation — НЕТ!
    }
}
```

Исключение: базовые классы (`BindableView`, `BindableElement`, `DisposableObject`, `ConfigBase`) содержат минимальную инфраструктурную логику (lifecycle, dispose pattern).

### 2. Никаких ссылок на Infrastructure

```csharp
// ❌ НЕЛЬЗЯ — зависимость от конкретной реализации
using Infrastructure.Network;

public interface IAuthService
{
    UnityWebRequest GetRawRequest(); // конкретный тип Unity
}
```

### 3. Никаких конкретных типов в сигнатурах

```csharp
// ❌ Конкретный тип
public interface IViewService
{
    ChatView ShowChat(); // конкретная view
}

// ✅ Абстракция
public interface IViewService
{
    UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null)
        where TViewModel : DisposableObject;
}
```

### 4. Не раздувай интерфейсы

Если интерфейс > 10 методов — подумай о разделении. Пример хорошего разделения:

```
IHttpClient          — raw HTTP (SendAsync)
IHttpTransportService — typed HTTP (Get/Post/Put/Patch/Delete)
IApiClient           — бизнес-уровень HTTP (с endpoint prefix)
IUrlBuilder          — построение URL
```

### 5. Не добавляй утилитные/helper методы

```csharp
// ❌ Utility метод в интерфейсе
public interface ILogger
{
    string FormatTimestamp(DateTime dt); // это helper, не контракт
}
```

### 6. Не используй event-based patterns для новых интерфейсов, если можно Observable

```csharp
// ⚠️ Легаси — event Action<T>
public interface ISettingsModel
{
    event Action<bool> IsVisibleChanged;
}

// ✅ Предпочтительно для новых интерфейсов
public interface IInputService
{
    Observable<Unit> OnCancel { get; }
}
```

### 7. Не дублируй данные между Model интерфейсами

Каждый `IModel` — один экран/панель. Если нужны общие данные — выноси в отдельный shared-сервис.

---

*Обновлено: 2026-02-11*
