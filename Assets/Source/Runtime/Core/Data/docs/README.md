# Data Layer — Архитектура

## Обзор

Data Layer — слой данных приложения, отвечающий за:
- **Domain Models** — бизнес-модели приложения
- **DTO (Data Transfer Objects)** — объекты для передачи данных по сети
- **Mappers** — преобразование DTO → Model
- **Value Objects** — объекты-значения (immutable data)
- **UI Data** — структуры данных для UI

## Структура папок

```
Data/
├── Domain/
│   ├── Models/           # Бизнес-модели
│   │   ├── Chat/         # Чаты и сообщения
│   │   │   └── Extensions/ # UI Extensions для чата
│   │   ├── Character/    # Персонажи
│   │   ├── Elements/     # UI элементы (Grid, Slots) — readonly structs
│   │   │   └── Extensions/ # Расширения элементов
│   │   ├── HeroDetailPanel/ # Панель деталей героя
│   │   ├── HeroesGridPanel/ # Грид героев
│   │   ├── ImageGen/     # Генерация изображений
│   │   ├── Main/         # Главный экран
│   │   ├── Message/      # Сообщения
│   │   ├── ServicePanel/ # Сервисная панель
│   │   ├── UIGeneration/ # AI генерация UI
│   │   ├── Workflow/     # Workflow
│   │   ├── AudioSettingsData.cs # Настройки аудио
│   │   └── SettingsData.cs # Настройки (Video, Display, Controls, Game)
│   ├── Mappers/          # DTO → Model mappers
│   ├── Enums/            # Перечисления домена
│   │   ├── AudioEnums.cs # Audio enums (AudioMixerGroupType, SoundPlaybackScope)
│   │   ├── SettingsEnums.cs # Settings enums (QualityPreset, ColorblindType, etc.)
│   │   └── SoundType.cs  # Enum всех звуков в игре
│   ├── Audio/            # Audio данные (namespace: Core.Data.Configs.Audio)
│   │   ├── AudioClipEntry.cs
│   │   ├── AudioMixerGroupSetup.cs
│   │   └── SoundSetup.cs
│   └── ValueObjects/     # Value Objects (PerformanceMetrics)
├── Network/
│   ├── DTO/              # Data Transfer Objects
│   │   ├── Auth/         # Авторизация
│   │   ├── Chat/         # Чаты
│   │   │   └── Extensions/ # Chat UI Extensions DTO
│   │   ├── Character/    # Персонажи
│   │   ├── Message/      # Сообщения
│   │   ├── Workflow/     # Воркфлоу
│   │   │   └── Standalone/ # Standalone UI DTO
│   │   ├── Persona/      # Персоны
│   │   └── Gemini/       # Gemini API (image generation)
│   ├── ApiEndpoints.cs   # Константы API endpoints (20+ категорий)
│   └── HttpResponse.cs   # Generic HTTP response wrapper
├── UI/                   # UI data structures (Tooltip, HeroTooltip, AbilityTooltip)
├── ValueObjects/         # Value Objects (SceneLoadPayload, SceneLoadType)
└── docs/                 # Документация (вы тут)
```

## Принципы архитектуры

### 1. Разделение DTO и Domain Models

**DTO** — отражают структуру API ответов:
- Содержат `[JsonProperty]` атрибуты
- Не содержат бизнес-логики
- Могут иметь nullable поля

**Domain Models** — чистые бизнес-модели:
- Не зависят от способа сериализации
- Содержат computed properties и методы
- Используются в слое бизнес-логики

### 2. Mapper Pattern

Все преобразования DTO → Model выполняются через статические классы Mappers:

```csharp
// Пример
var model = ChatMapper.ToModel(dto);
var models = ChatMapper.ToModels(dtos);
```

### 3. Null Safety

Mappers обрабатывают null:
```csharp
public static ChatModel ToModel(ChatResponseDto dto)
{
    if (dto == null) return null;
    // ...
}
```

### 4. Коллекции

Всегда инициализируются пустыми списками:
```csharp
Messages = dto.Messages?.Select(ToModel).ToList() ?? new List<ChatMessage>();
```

## Таблица маппинга (сводная)

| DTO | Model | Mapper |
|-----|-------|--------|
| ChatResponseDto | ChatModel | ChatMapper |
| ChatMessageDto | ChatMessage | ChatMapper |
| CharacterResponseDto | CharacterModel | CharacterMapper |
| CharacterListItemDto | CharacterModel | CharacterMapper |
| CharacterGreetingDto | CharacterGreeting | CharacterMapper |
| MessageResponseDto | MessageModel | MessageMapper |
| WorkflowResponseDto | WorkflowModel | WorkflowMapper |
| WorkflowRunDto | WorkflowRunModel | WorkflowMapper |
| WorkflowTriggerResponseDto | WorkflowTriggerResult | WorkflowMapper |
| StandaloneUiResponseDto | StandaloneUiResult | WorkflowMapper |
| ChatExtensionsUiResponseDto | UiExtensionsModel | ChatExtensionsUiMapper |
| ChatUiComponentDto | UiComponentModel | ChatExtensionsUiMapper |

## Файлы документации

### Core Documentation
- [DomainModels.md](./DomainModels.md) — Бизнес-модели (Chat, Character, Message, Workflow, UI Extensions)
- [DTO.md](./DTO.md) — Data Transfer Objects по категориям (Auth, Chat, Character, Message, Workflow, Persona, Gemini)
- [Mappers.md](./Mappers.md) — Mappers для DTO → Model преобразований
- [ValueObjects.md](./ValueObjects.md) — Value objects (StorageContext, WorkflowExecutionOptions, etc.)
- [UIData.md](./UIData.md) — UI data structures (Tooltip, HeroTooltip, AbilityTooltip)

### Additional Categories
- [Audio.md](./Audio.md) — Audio data structures (AudioClipEntry, AudioMixerGroupSetup, SoundSetup, Audio enums)
- [Settings.md](./Settings.md) — Settings data classes (Audio, Video, Display, Controls, Game settings)
- [Elements.md](./Elements.md) — Element data structures для UI (EquipmentSlotData, HeroAbilityData, HeroSlotData, etc.)
- [GameUI.md](./GameUI.md) — Game UI models (HeroDetailPanel, HeroesGridPanel, Main, ServicePanel)
- [Network.md](./Network.md) — Network layer (ApiEndpoints, HttpResponse)

## Namespaces

| Namespace | Описание |
|-----------|----------|
| `Core.Data.Domain.Models.*` | Бизнес-модели |
| `Core.Data.Domain.Mappers` | Mappers |
| `Core.Data.Network.DTO.*` | DTO классы |
| `Core.Data.UI` | UI data structures |
| `Core.Domain.Models.Workflow` | Модели Workflow |
| `Core.Domain.Mappers` | Workflow mappers |
