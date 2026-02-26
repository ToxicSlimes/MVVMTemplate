# Infrastructure Layer — Архитектура

## Обзор

Infrastructure Layer — слой реализаций. Содержит конкретные имплементации интерфейсов из Abstractions.

**Отвечает за:**
- Реализация сервисов (Chat, Network, ImageGen, Workflow, ProceduralUI)
- Mediator паттерн (маршрутизация Commands/Queries)
- StateMachine (GameStateMachine)
- MVVM инфраструктура (ViewService, ViewFactory, ViewAnimator)

---

## Структура

```
Infrastructure/
├── Services/           # Реализации бизнес-сервисов
│   ├── AssetManagement/    # Asset loading & management
│   ├── Audio/              # Audio playback services
│   ├── Character/          # Character-related services
│   ├── Chat/               # IChatService, IMessageService
│   ├── Config/             # Configuration services
│   ├── Data/               # Data persistence services
│   ├── ImageGen/           # IImageGenService
│   ├── Input/              # Input handling services
│   ├── LoadScene/          # Scene loading services
│   ├── Logging/            # Logging infrastructure
│   ├── Media/              # Media handling services
│   ├── Message/            # Message processing services
│   ├── Network/            # IApiClient, IHttpTransport
│   ├── Platform/           # Platform-specific services
│   ├── ProceduralUI/       # IUIElementService, IEmbeddedPanelService
│   ├── UI/                 # UI-related services
│   ├── UIGeneration/       # Dynamic UI generation
│   └── Workflow/           # IWorkflowService, IWorkflowRunner
│
├── Mediator/           # CQRS маршрутизация
│   ├── Abstractions/   # IRequest, INotification, базовые интерфейсы
│   ├── Attributes/     # Атрибуты для handlers
│   ├── Exceptions/     # Mediator-специфичные исключения
│   ├── Extensions/     # Extension методы
│   ├── Handlers/       # Base handlers
│   ├── Implementation/ # Mediator.cs реализация
│   ├── Pipeline/       # Pre/Post processing pipeline
│   ├── Publishing/     # Publisher.cs реализация
│   └── Registration/   # DI регистрация helpers
│
├── MVVM/               # UI инфраструктура
│   ├── Animation/      # ViewAnimator (LitMotion)
│   ├── Canvas/         # Canvas management
│   ├── Config/         # MVVM configuration
│   ├── Elements/       # UI element implementations
│   ├── Services/       # ViewService
│   ├── UIElementFactory.cs  # UI element factory
│   └── ViewFactory.cs       # View factory с DI
│
├── StateMachines/      # Game State Machine
│   ├── GameStateMachine.cs  # IGameStateMachine реализация
│   ├── Factories/           # StateMachine factory
│   └── States/              # (пустая, States в Presentation/)
│
├── Extensions/         # Unity extensions
│   └── UnityObjectExtensions.cs
│
├── UI/                 # UI компоненты инфраструктуры
│   └── Components/     # (пустая заготовка)
│
└── docs/               # Эта документация
```

---

## Правила зависимостей

```
Infrastructure МОЖЕТ зависеть от:
├── Abstractions (реализует интерфейсы)
├── Data (использует DTOs, Models)
├── Application (Handlers могут вызываться)
└── Mediator.Abstractions (IRequest, IRequestHandler)

Infrastructure НЕ МОЖЕТ зависеть от:
├── MVVM (Views, ViewModels)
└── Presentation (States)
```

---

## Подмодули

### Services

| Сервис | Docs | Описание |
|--------|------|----------|
| **AssetManagement** | [docs/](Services/AssetManagement/docs/) | Загрузка и управление ассетами |
| **Audio** | [docs/](Services/Audio/docs/) | Аудио-сервисы и воспроизведение |
| **Character** | [docs/](Services/Character/docs/) | Сервисы для работы с персонажами |
| **Chat** | [docs/](Services/Chat/docs/) | Чат, сообщения, история |
| **Config** | [docs/](Services/Config/docs/) | Конфигурационные сервисы |
| **Data** | [docs/](Services/Data/docs/) | Сервисы сохранения/загрузки данных |
| **ImageGen** | [docs/](Services/ImageGen/docs/) | Генерация изображений |
| **Input** | [docs/](Services/Input/docs/) | Обработка пользовательского ввода |
| **LoadScene** | [docs/](Services/LoadScene/docs/) | Загрузка сцен |
| **Logging** | [docs/](Services/Logging/docs/) | Логирование |
| **Media** | [docs/](Services/Media/docs/) | Работа с медиа-файлами |
| **Message** | [docs/](Services/Message/docs/) | Обработка сообщений |
| **Network** | [docs/](Services/Network/docs/) | HTTP API, WebSocket |
| **Platform** | [docs/](Services/Platform/docs/) | Platform-специфичные сервисы |
| **ProceduralUI** | [docs/](Services/ProceduralUI/docs/) | Динамические UI элементы |
| **UI** | [docs/](Services/UI/docs/) | UI-сервисы |
| **UIGeneration** | [docs/](Services/UIGeneration/docs/) | Генерация UI |
| **Workflow** | [docs/](Services/Workflow/docs/) | Workflow engine |

### Mediator

[📖 Mediator docs](Mediator/docs/)

Реализация CQRS паттерна:
- `Abstractions/` — IRequest, INotification, базовые интерфейсы
- `Attributes/` — Атрибуты для handler registration
- `Exceptions/` — Mediator-специфичные исключения
- `Extensions/` — Extension методы для удобства
- `Handlers/` — Base handler классы
- `Implementation/` — Mediator реализация (IRequest → IRequestHandler)
- `Pipeline/` — Pre/Post processing pipeline
- `Publishing/` — Publisher (broadcast INotification → подписчики)
- `Registration/` — DI registration helpers

### MVVM

[📖 MVVM docs](MVVM/docs/)

UI инфраструктура:
- `Animation/` — ViewAnimator, анимации переходов (LitMotion)
- `Canvas/` — Управление Canvas
- `Config/` — Конфигурация MVVM
- `Elements/` — Реализации UI элементов
- `Services/` — ViewService (показ/скрытие Views)
- `UIElementFactory.cs` — Фабрика UI элементов
- `ViewFactory.cs` — Создание Views с DI

### StateMachines

`GameStateMachine` — реализация IGameStateMachine из Abstractions.

```csharp
public class GameStateMachine : IGameStateMachine
{
    public void Enter<TState>() where TState : IEnterState { ... }
    public void Enter<TState, TPayload>(TPayload payload) where TState : IPayloadState<TPayload> { ... }
}
```

**Структура:**
- `GameStateMachine.cs` — основная реализация
- `Factories/` — фабрика для создания StateMachine
- `States/` — (пустая папка, States теперь в `Presentation/States/`)

### UI

`UI/Components/` — заготовка для UI компонентов Infrastructure уровня (пока пустая).

---

## Регистрация в DI

Все сервисы регистрируются в `DependencyInjection/`:

```csharp
// ServiceRegistration.cs
builder.Register<ChatService>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<ApiClient>(Lifetime.Singleton).AsImplementedInterfaces();
builder.Register<GameStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();
```

---

## См. также

- [Architecture.md](../../Documentation/Architecture.md) — Общая архитектура
- [DependencyRules.md](../../Documentation/DependencyRules.md) — Правила зависимостей
- [Abstractions/](../../Abstractions/) — Интерфейсы которые реализуем
