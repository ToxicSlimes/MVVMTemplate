# Navigation — Система навигации

> Типобезопасные маркеры для навигации между экранами.

## 📋 Обзор интерфейсов

| Интерфейс/Структура | Назначение | Путь |
|---------------------|------------|------|
| `INavigationTarget` | Маркер навигации | `Navigation/INavigationTarget.cs` |
| `LoginNavigation` | → Login экран | `Navigation/NavigationTargets.cs` |
| `MenuSelectionNavigation` | → Выбор меню | `Navigation/NavigationTargets.cs` |
| `ChatNavigation` | → Чат | `Navigation/NavigationTargets.cs` |
| `ImageGenNavigation` | → Генерация изображений | `Navigation/NavigationTargets.cs` |
| `MainNavigation` | → Главное меню | `Navigation/NavigationTargets.cs` |
| `StandaloneWorkflowNavigation` | → Standalone Workflow | `Navigation/NavigationTargets.cs` |

---

## INavigationTarget

**Путь:** `Navigation/INavigationTarget.cs`

```csharp
namespace Core.Abstractions.Navigation
{
    public interface INavigationTarget { }
}
```

### Назначение

Маркерный интерфейс для типобезопасной навигации между экранами.
Используется как тип для передачи намерения навигации без привязки к конкретным State или ViewModel.

---

## NavigationTargets

**Путь:** `Navigation/NavigationTargets.cs`

```csharp
namespace Core.Abstractions.Navigation
{
    public readonly struct LoginNavigation : INavigationTarget { }

    public readonly struct MenuSelectionNavigation : INavigationTarget { }

    public readonly struct ChatNavigation : INavigationTarget { }

    public readonly struct ImageGenNavigation : INavigationTarget { }

    public readonly struct MainNavigation : INavigationTarget { }

    public readonly struct StandaloneWorkflowNavigation : INavigationTarget { }
}
```

### Назначение

Набор готовых навигационных целей для основных экранов приложения.
Каждая цель — пустая структура, работающая как типобезопасный маркер.

---

## 📖 Примеры использования

### Базовый пример навигации

```csharp
public class NavigationService
{
    private readonly IGameStateMachine _stateMachine;
    
    public void Navigate<TTarget>() where TTarget : struct, INavigationTarget
    {
        if (typeof(TTarget) == typeof(LoginNavigation))
        {
            _stateMachine.Enter<LoginState>();
        }
        else if (typeof(TTarget) == typeof(ChatNavigation))
        {
            _stateMachine.Enter<ChatState>();
        }
        // ...
    }
}

// Использование
_navigationService.Navigate<ChatNavigation>();
```

### Навигация с payload

```csharp
public class NavigationService
{
    public void Navigate<TTarget, TPayload>(TPayload payload) 
        where TTarget : struct, INavigationTarget
    {
        if (typeof(TTarget) == typeof(ChatNavigation))
        {
            _stateMachine.Enter<ChatState, ChatStatePayload>(payload as ChatStatePayload);
        }
    }
}

// Использование
_navigationService.Navigate<ChatNavigation, ChatStatePayload>(new ChatStatePayload
{
    ChatId = chatId,
    CharacterId = characterId
});
```

### Типобезопасная навигация через кнопки

```csharp
public class NavigationButton<TTarget> : MonoBehaviour 
    where TTarget : struct, INavigationTarget
{
    [Inject] private NavigationService _navigationService;
    [SerializeField] private Button _button;
    
    private void Start()
    {
        _button.onClick.AddListener(() => _navigationService.Navigate<TTarget>());
    }
}

// В Inspector назначаем NavigationButton<ChatNavigation> на кнопку "Open Chat"
```

### Навигация через ViewModel

```csharp
public class MenuSelectionViewModel : DisposableObject
{
    private readonly NavigationService _navigationService;
    
    public void OnChatButtonClicked()
    {
        _navigationService.Navigate<ChatNavigation>();
    }
    
    public void OnImageGenButtonClicked()
    {
        _navigationService.Navigate<ImageGenNavigation>();
    }
    
    public void OnWorkflowButtonClicked()
    {
        _navigationService.Navigate<StandaloneWorkflowNavigation>();
    }
}
```

### Создание кастомной навигационной цели

```csharp
// 1. Определить маркер
namespace Core.Abstractions.Navigation
{
    public readonly struct SettingsNavigation : INavigationTarget { }
    
    public readonly struct InventoryNavigation : INavigationTarget { }
}

// 2. Использовать
_navigationService.Navigate<SettingsNavigation>();
_navigationService.Navigate<InventoryNavigation>();
```

---

## 🗺️ Маппинг Navigation → State

```
┌──────────────────────────────────────────────────────────────────┐
│                     Navigation Targets                            │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  LoginNavigation               ──▶  LoginState                   │
│  MenuSelectionNavigation       ──▶  MenuSelectionState           │
│  ChatNavigation                ──▶  ChatState                    │
│  ImageGenNavigation            ──▶  ImageGenState                │
│  MainNavigation                ──▶  MainState                    │
│  StandaloneWorkflowNavigation  ──▶  StandaloneWorkflowState      │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Паттерн проектирования

### Зачем INavigationTarget?

1. **Типобезопасность** — компилятор проверяет существование навигационной цели
2. **Decoupling** — UI не зависит от конкретных State или ViewModel
3. **Простота** — readonly struct без полей и методов
4. **Performance** — структуры не создают аллокаций
5. **Рефакторинг** — можно изменить State не трогая UI код

### Альтернативы

```csharp
// ❌ BAD — строки, нет типобезопасности
_navigationService.Navigate("Chat");

// ❌ BAD — прямая зависимость от State
_stateMachine.Enter<ChatState>(); // UI знает о State

// ✅ GOOD — типобезопасный маркер
_navigationService.Navigate<ChatNavigation>();
```

---

## 🎯 Best Practices

1. **Один Target = один экран** — не переиспользуй маркеры
2. **Именование** — суффикс `Navigation` для ясности
3. **Readonly struct** — нулевые аллокации, immutable
4. **Группировка** — все targets в одном файле `NavigationTargets.cs`
5. **Не добавляй логику** — маркеры должны быть пустыми

---

## 🔗 Связанные интерфейсы

- `IGameStateMachine` — выполняет фактические переходы между состояниями
- `IGameState` — целевые состояния для навигации
- `IViewService` — может использовать navigation targets для показа View
