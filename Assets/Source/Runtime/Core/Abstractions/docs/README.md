# Abstractions Layer — Обзор

> Слой интерфейсов, определяющий контракты всей системы EndlessAdventure.

## 📁 Структура

```
Abstractions/
├── MVVM/                    # Model-View-ViewModel паттерн
│   ├── Views/               # IView, IShowable, IHideable
│   ├── ViewModels/          # IViewModel, DisposableObject
│   ├── Models/              # IModel и доменные модели
│   ├── Elements/            # IBindableElement, IClickableElement
│   ├── Config/              # IViewConfig, IElementConfig
│   ├── Canvas/              # ICanvasProvider, CanvasLayer
│   ├── Animation/           # IViewAnimator
│   └── Services/            # IViewService
├── Services/                # Сервисные интерфейсы
│   ├── Network/             # IApiClient, IHttpClient, IAuthService
│   ├── Chat/                # IChatService
│   ├── Workflow/            # IWorkflowService
│   ├── Platform/            # IFileDialogService, IClipboardService
│   ├── ProceduralUI/        # IProceduralUiService, builders
│   ├── UIGeneration/        # IUIGenerationPipeline
│   ├── ImageGen/            # IGeminiImageService
│   ├── Character/           # ICharacterService
│   ├── Message/             # IMessageService
│   ├── Input/               # IInputService
│   └── Media/               # IImageCodec
├── StateMachines/           # IGameStateMachine, IGameState
├── Popup/                   # IPopup, IPopupService
├── Factories/               # IGameStateMachineFactory
├── Config/                  # ConfigBase, IConfigProvider
├── AssetManagement/         # IAssetProvider
├── Attributes/              # ConfigEntryStyleAttribute
├── Navigation/              # INavigationTarget, NavigationTargets
├── Validation/              # ValidateMissingRefsAttribute
└── UI/                      # ITooltipTarget
```

## 🗺️ Карта зависимостей

```
┌─────────────────────────────────────────────────────────────────────┐
│                           APPLICATION                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────┐     ┌─────────────────┐     ┌────────────────┐ │
│  │ IGameStateMachine│────▶│   IViewService  │────▶│  IAssetProvider│ │
│  └─────────────────┘     └─────────────────┘     └────────────────┘ │
│          │                       │                                   │
│          ▼                       ▼                                   │
│  ┌─────────────────┐     ┌─────────────────┐                        │
│  │   IGameState    │     │     IView       │                        │
│  │   IEnterState   │     │   IViewModel    │                        │
│  │   IExitState    │     │   IModel        │                        │
│  └─────────────────┘     └─────────────────┘                        │
│                                  │                                   │
├──────────────────────────────────┼──────────────────────────────────┤
│                            SERVICES                                  │
│                                  │                                   │
│  ┌────────────┐  ┌────────────┐  │  ┌────────────┐  ┌────────────┐  │
│  │ IApiClient │  │IChatService│◀─┴─▶│IWorkflowSvc│  │IPopupService│ │
│  └────────────┘  └────────────┘     └────────────┘  └────────────┘  │
│         │               │                  │               │         │
│         ▼               ▼                  ▼               ▼         │
│  ┌────────────┐  ┌────────────┐     ┌────────────┐  ┌────────────┐  │
│  │IHttpClient │  │IMessageSvc │     │IProceduralUI│ │  IPopup    │  │
│  └────────────┘  └────────────┘     └────────────┘  └────────────┘  │
│                                                                      │
├─────────────────────────────────────────────────────────────────────┤
│                          INFRASTRUCTURE                              │
│                                                                      │
│  ┌────────────────┐  ┌──────────────┐  ┌──────────────────────────┐ │
│  │ IConfigProvider│  │  ILogger     │  │ IInputService            │ │
│  │ ConfigBase     │  │ ILogFormatter│  │ IAudioService            │ │
│  └────────────────┘  └──────────────┘  └──────────────────────────┘ │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

## 📋 Сводная таблица интерфейсов

| Категория | Интерфейс | Назначение |
|-----------|-----------|------------|
| **MVVM** | `IView` | Базовый контракт View |
| | `IViewModel` | Базовый контракт ViewModel |
| | `IModel` | Базовый контракт Model |
| | `IViewService` | Управление жизненным циклом View |
| | `IBindableElement` | UI элемент с биндингом данных |
| | `IViewConfig` | Конфигурация View prefabs |
| **Network** | `IApiClient` | High-level HTTP клиент с авторизацией |
| | `IHttpClient` | Low-level HTTP транспорт |
| | `IAuthService` | Авторизация и сессии |
| **Services** | `IChatService` | Чат API |
| | `IWorkflowService` | Workflow API |
| | `IMessageService` | Отправка сообщений |
| | `ICharacterService` | Персонажи |
| **UI** | `IProceduralUiService` | Динамический UI из JSON |
| | `IPopupService` | Popup/tooltip система |
| | `IUiComponentFactory` | Фабрика UI компонентов |
| **State** | `IGameStateMachine` | Машина состояний приложения |
| | `IGameState` | Базовое состояние |
| **Infra** | `IAssetProvider` | Addressables loader |
| | `IConfigProvider` | Загрузка конфигов |
| | `ILogger` | Логирование |
| **Navigation** | `INavigationTarget` | Типобезопасные навигационные маркеры |
| **Validation** | `ValidateMissingRefsAttribute` | Валидация missing references |

## 📖 Документация по разделам

- [MVVM.md](./MVVM.md) — IView, IViewModel, IViewConfig, IBindableElement
- [Services.md](./Services.md) — Все сервисные интерфейсы
- [UI.md](./UI.md) — ProceduralUI система
- [Popup.md](./Popup.md) — Popup/Tooltip система
- [StateMachines.md](./StateMachines.md) — IGameStateMachine, состояния
- [Factories.md](./Factories.md) — Фабрики
- [Config.md](./Config.md) — Конфигурации и атрибуты
- [AssetManagement.md](./AssetManagement.md) — IAssetProvider, загрузка ассетов
- [Navigation.md](./Navigation.md) — INavigationTarget, типобезопасная навигация
- [Validation.md](./Validation.md) — ValidateMissingRefsAttribute, валидация в Inspector

## 🔄 Жизненный цикл приложения

```
Bootstrap
    │
    ▼
IGameStateMachine.Enter<BootstrapState>()
    │
    ▼
IDataPreloadService.PreloadDataAsync()
    │
    ▼
IGameStateMachine.Enter<LoginState>()
    │
    ▼
IViewService.ShowAsync<LoginViewModel>()
    │
    ▼
IAuthService.SignInEmailAsync()
    │
    ▼
IGameStateMachine.Enter<MenuSelectionState>()
    │
    ▼
User selects feature...
    │
    ├──▶ ChatState ──▶ IChatService
    ├──▶ ImageGenState ──▶ IGeminiImageService  
    └──▶ StandaloneWorkflowState ──▶ IWorkflowService
```

## 🏗️ Принципы проектирования

1. **Dependency Inversion** — Зависимости только от абстракций
2. **Interface Segregation** — Маленькие специализированные интерфейсы
3. **Single Responsibility** — Каждый интерфейс делает одно дело
4. **Async by default** — UniTask для всех async операций
5. **Observable patterns** — R3 для реактивных streams

## 📦 Реализации

Все реализации находятся в:
- `Core/Infrastructure/` — Основные реализации
- `Core/Infrastructure/MVVM/Services/ViewService.cs`
- `Core/Infrastructure/Services/` — Сервисы
- `Core/Infrastructure/StateMachines/` — State machine
