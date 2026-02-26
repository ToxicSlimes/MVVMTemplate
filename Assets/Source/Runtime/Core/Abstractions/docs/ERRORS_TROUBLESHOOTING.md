# ERRORS & TROUBLESHOOTING — Abstractions

Типичные ошибки при работе со слоем Abstractions и как их решать.

---

## 1. Missing Interface Implementation

### Симптом

```
MissingMethodException: Interface method 'IChatService.DeleteChatAsync' not implemented
```
или Reflex/DI ошибка:
```
Unable to resolve service for type 'Core.Abstractions.Services.Chat.IChatService'
```

### Причина

- Добавил метод в интерфейс, но не реализовал в Implementation
- Не зарегистрировал реализацию в DI-контейнере

### Решение

1. Найди все реализации интерфейса:
   ```bash
   unitygrep --refs "IChatService"
   ```
2. Добавь недостающий метод во все реализации
3. Проверь регистрацию в DI (Reflex Installer)

### Профилактика

- Перед коммитом: `Ctrl+Shift+B` (Build) — компилятор покажет missing implementations
- При добавлении метода в интерфейс — сразу ищи все реализации

---

## 2. Wrong NavigationTarget

### Симптом

```
KeyNotFoundException: No state registered for navigation target 'InventoryNavigation'
```
или экран не открывается, навигация молча проваливается.

### Причина

- `NavigationTarget` struct добавлен, но не зарегистрирован маппинг в state machine
- Опечатка в типе при вызове `Enter<TState>()`

### Решение

1. Проверь что struct существует в `NavigationTargets.cs`:
   ```csharp
   public readonly struct InventoryNavigation : INavigationTarget { }
   ```
2. Проверь регистрацию в GameStateMachine (маппинг target → state)
3. Убедись что ViewModel/State зарегистрирован в DI

---

## 3. Abstract Class Misuse — неправильное наследование

### Симптом A: Наследуешь от неправильного Config base

```
// ❌ UI конфиг наследует от ServiceConfigBase
public class ChatViewConfig : ServiceConfigBase { }
```

В Inspector не появляются секции Prefabs/Styles/Animations.

### Решение

Выбери правильный базовый класс:
- **ServiceConfigBase** — настройки сервиса (pool, cache, network, debug)
- **DataConfigBase** — статические данные, справочники
- **ViewConfigBase** — UI (prefabs, styles, animations, sounds)

### Симптом B: Забыл override OnBind в BindableView

```
NullReferenceException при показе view
```

### Решение

```csharp
public class MyView : BindableView<MyViewModel>
{
    // ❌ Забыл — будет NRE при bind
    // ✅ Обязательный override:
    protected override void OnBind(ref DisposableBag disposables)
    {
        // подписки
    }
}
```

### Симптом C: Забыл override OnBind в BindableElement

Элемент создаётся, но не отображает данные.

### Решение

```csharp
public class MyElement : BindableElement<MyData>
{
    // Обязательно:
    protected override void OnBind(MyData data)
    {
        _text.text = data.Name;
    }
}
```

---

## 4. Dependency Direction Violations

### Симптом

```
error CS0246: The type or namespace name 'ChatViewModel' could not be found
```
в файле из Abstractions.

### Причина

Abstractions ссылается на конкретную реализацию (MVVM, Infrastructure). Нарушена инверсия зависимостей.

### Правильное направление зависимостей

```
Infrastructure → Abstractions ← MVVM
                      ↑
                   Domain/Data
```

Abstractions зависит **только** от:
- `Core.Data.Domain` — доменные модели
- `Core.Data.Network` — DTO
- `Core.Data.UI` — UI data types
- Сторонние пакеты: `R3`, `Cysharp.Threading.Tasks`, `UnityEngine`, `TriInspector`, `Reflex`

### Решение

Если нужен тип из MVVM/Infrastructure — создай интерфейс в Abstractions, реализуй в нужном слое.

---

## 5. DisposableBag не очищается

### Симптом

- Memory leak: подписки продолжают работать после закрытия view
- `ObjectDisposedException` при повторном открытии view
- Двойные обработки событий

### Причина

Подписки не добавлены в `DisposableBag` через `.AddTo(ref disposables)`.

### Решение

```csharp
protected override void OnBind(ref DisposableBag disposables)
{
    // ✅ Всегда AddTo:
    ViewModel.SomeObservable
        .Subscribe(x => Handle(x))
        .AddTo(ref disposables);
    
    // ❌ Утечка — не привязано к lifecycle:
    ViewModel.SomeObservable
        .Subscribe(x => Handle(x));
}
```

---

## 6. Event-based Model — пропущенные отписки

### Симптом

Для `IModel` с `event Action<T>` паттерном — при переключении экранов старые обработчики продолжают стрелять.

### Решение

В ViewModel не забывай отписываться в `Dispose()`:

```csharp
public override void Dispose()
{
    if (_model != null)
    {
        _model.IsLoadingChanged -= OnLoadingChanged;
        _model.StatusTextChanged -= OnStatusChanged;
    }
    base.Dispose();
}
```

---

## 7. CancellationToken не передан

### Симптом

- При закрытии экрана HTTP-запросы продолжают выполняться
- Ответ приходит на уже уничтоженный view → `MissingReferenceException`

### Решение

Всегда передавай `CancellationToken` из ViewModel:

```csharp
// В DisposableObject (базовый ViewModel):
var cts = CreateNewRequestCts();
await _chatService.GetChatHistoryAsync(chatId, cts.Token);
```

`CreateNewRequestCts()` автоматически отменяет предыдущий запрос.

---

## 8. IElementData без Id

### Симптом

```
NullReferenceException: ElementId is null
```

### Причина

Data struct не реализует `IElementData.Id` корректно.

### Решение

```csharp
public struct MyItemData : IElementData
{
    public string Id { get; set; }  // ← обязательно!
    public string Name;
}
```

---

*Обновлено: 2026-02-11*
