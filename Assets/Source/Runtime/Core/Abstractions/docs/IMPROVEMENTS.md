# IMPROVEMENTS — Техдолг и идеи улучшений

Найденные проблемы и предложения по улучшению слоя Abstractions.

---

## 🔴 Критичное

### 1. Неконсистентные namespace

Разные файлы используют разные namespace-паттерны:

```csharp
// Вариант 1: Core.Abstractions.Services.Chat
namespace Core.Abstractions.Services.Chat { interface IChatService {} }

// Вариант 2: Abstractions.Services (без Core)
namespace Abstractions.Services { interface ILogger {} }

// Вариант 3: Без namespace вообще
public interface IGameStateMachine {} // StateMachines/
public interface IEnterState {}       // StateMachines/States/
public interface ILoadSceneService {} // Services/
public interface IDataPreloadService {} // Services/
```

**Предложение:** Унифицировать все в `Core.Abstractions.*`. State machine интерфейсы и отдельные сервисы (`ILoadSceneService`, `IDataPreloadService`) добавить в правильный namespace.

### 2. PopupTrigger — MonoBehaviour с логикой в Abstractions

`PopupTrigger.cs` — это полноценный MonoBehaviour с реализацией (pointer handling, delays, async show/hide). Это **не контракт**, а реализация.

**Предложение:** Перенести `PopupTrigger` в MVVM/Implementation слой. В Abstractions оставить только `IPopup`, `IPopupService`, enums.

### 3. PopupShowParams — конкретный класс с логикой

`PopupShowParams` содержит static factory methods и fluent builder. Аналогично `ViewShowParams`. Это data-классы с поведением.

**Предложение:** Допустимо оставить как есть (это value-like объекты), но стоит задокументировать это как осознанное исключение из правила "только интерфейсы".

---

## 🟡 Средний приоритет

### 4. Event vs Observable — два паттерна для одного

Model-интерфейсы используют `event Action<T>`:
```csharp
public interface IChatModel : IModel
{
    event Action<bool> IsLoadingChanged;
}
```

А `IInputService` использует `Observable<T>`:
```csharp
public interface IInputService : IDisposable
{
    Observable<Unit> OnCancel { get; }
}
```

**Предложение:** Для новых интерфейсов использовать **только** `Observable<T>` (R3). Существующие event-based модели — мигрировать постепенно. Это упростит lifecycle management и уберёт проблему с отписками.

### 5. `ConfigEntryStyleAttribute` — editor-only в runtime

`ConfigEntryStyleAttribute` и `ColorPresets` с цветами — чисто editor функциональность, но лежат в Runtime Abstractions.

**Предложение:** Перенести в Editor assembly или обернуть в `#if UNITY_EDITOR`. Сейчас `ColorPresets.GetColors()` попадает в билд.

### 6. IModel.Reset() / IsValid — неиспользуемые контракты?

Базовый `IModel` требует `IsValid` и `Reset()`, но не очевидно, используются ли они всеми реализациями.

**Предложение:** Проверить использование. Если не нужны — убрать из базового интерфейса (ISP).

### 7. Дублирование HTTP абстракций

Три уровня HTTP:
- `IHttpClient` — raw (RawHttpRequest/Response)
- `IHttpTransportService` — typed (Get/Post<T>)
- `IApiClient` — с endpoint prefix

**Предложение:** Проверить, нужны ли все три. Возможно `IHttpClient` избыточен если всё работает через `IApiClient`.

---

## 🟢 Низкий приоритет / Nice-to-have

### 8. Отсутствует INavigationService

Навигация идёт через `IGameStateMachine.Enter<TState>()`, что требует знания конкретных state-типов. Нет высокоуровневого навигационного API.

**Предложение:**
```csharp
public interface INavigationService
{
    UniTask NavigateToAsync<TTarget>() where TTarget : INavigationTarget;
    UniTask GoBackAsync();
    bool CanGoBack { get; }
}
```

### 9. ProceduralUI builders — очень много интерфейсов

~10 builder-интерфейсов (`IButtonBuilder`, `ICardBuilder`, `IInputBuilder`, etc.) — это правильно по ISP, но навигация по ним сложная.

**Предложение:** Добавить summary doc в `IProceduralUiBuilder` со списком всех доступных builders и ссылками.

### 10. ValidateMissingRefsAttribute — одинокий атрибут

`Validation/ValidateMissingRefsAttribute.cs` — единственный файл в подпапке.

**Предложение:** Если будут ещё validation атрибуты — ок. Если нет — перенести в `Attributes/`.

### 11. SceneLoadPayload — неизвестный тип

`ILoadSceneService` использует `SceneLoadPayload` который не определён в Abstractions. Вероятно живёт в другом месте.

**Предложение:** Либо определить payload тут, либо добавить using/reference в документацию.

---

## 📊 Метрики

| Метрика | Значение |
|---------|----------|
| Всего .cs файлов | ~100 |
| Интерфейсов | ~60 |
| Базовых классов | 6 (ConfigBase, DataConfigBase, ServiceConfigBase, ViewConfigBase, BindableView, BindableElement, DisposableObject) |
| Enums | ~10 |
| Inconsistent namespaces | ~8 файлов |
| MonoBehaviour в Abstractions | 2 (PopupTrigger, BindableElement, BindableView) |

---

*Обновлено: 2026-02-11*
