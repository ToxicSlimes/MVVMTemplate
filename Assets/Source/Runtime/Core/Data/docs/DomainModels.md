# Domain Models

Бизнес-модели приложения. Используются в слое бизнес-логики, не зависят от сериализации.

## Содержание

- [Chat](#chat)
- [Character](#character)
- [Message](#message)
- [Workflow](#workflow)
- [UI Extensions](#ui-extensions)

---

## Chat

### ChatModel

**Путь:** `Domain/Models/Chat/ChatModel.cs`  
**Namespace:** `Core.Data.Domain.Models.Chat`  
**Маппится из:** `ChatResponseDto` через `ChatMapper`

Основная модель чата.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | Уникальный ID чата |
| Title | `string` | Название чата |
| AvatarUrl | `string` | URL аватара |
| CreatedAt | `DateTime` | Дата создания |
| UpdatedAt | `DateTime` | Дата обновления |
| UserId | `string` | ID владельца |
| UserName | `string` | Имя владельца |
| CharacterId | `string` | ID персонажа |
| PersonaId | `string` | ID персоны |
| Messages | `List<ChatMessage>` | Список сообщений |
| MessageCount | `int` | Количество сообщений (загруженных) |
| TotalMessageCount | `int` | Общее количество сообщений |
| LastMessage | `string` | Текст последнего сообщения |
| LastMessageAt | `DateTime?` | Время последнего сообщения |
| LastUsedPromptId | `string` | Последний использованный промпт |
| LastUsedModelId | `string` | Последняя использованная модель |
| LastUsedTemperature | `float?` | Последняя температура |
| LastUsedTopP | `float?` | Последний TopP |
| LastUsedTopK | `int?` | Последний TopK |
| LastUsedMaxTokens | `int?` | Последний MaxTokens |
| Visibility | `string` | Видимость (public/private) |
| PinnedStatus | `bool` | Закреплён ли чат |
| IsEncrypted | `bool` | Зашифрован ли чат |
| ChatBackground | `string` | Фон чата |

**Computed Properties:**
- `HasMessages` → `bool` — есть ли сообщения

**Методы:**
- `GetLastMessage()` → `ChatMessage` — последнее сообщение
- `GetUserMessages()` → `List<ChatMessage>` — сообщения пользователя
- `GetAssistantMessages()` → `List<ChatMessage>` — сообщения ассистента
- `GetMessageCountByRole(role)` → `int` — количество по роли

---

### ChatMessage

**Путь:** `Domain/Models/Chat/ChatMessage.cs`  
**Namespace:** `Core.Data.Domain.Models.Chat`  
**Маппится из:** `ChatMessageDto` через `ChatMapper`

Сообщение в чате.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID сообщения |
| ChatId | `string` | ID чата |
| Role | `string` | Роль (user/assistant) |
| SendUserId | `string` | ID отправителя |
| SendType | `string` | Тип отправки |
| Content | `string` | Текст сообщения |
| TurnId | `string` | ID хода |
| VersionIndex | `int` | Индекс версии |
| ParentMessageId | `string` | ID родительского сообщения |
| CreatedAt | `DateTime` | Дата создания |
| UpdatedAt | `DateTime` | Дата обновления |
| IsBranchStart | `bool` | Начало ветки |

**Computed Properties:**
- `IsUserMessage` → `bool` — сообщение от пользователя
- `IsAssistantMessage` → `bool` — сообщение от ассистента

**Методы:**
- `GetPreview()` → `string` — превью (50 символов)

---

## Character

### CharacterModel

**Путь:** `Domain/Models/Character/CharacterModel.cs`  
**Namespace:** `Core.Data.Domain.Models.Character`  
**Маппится из:** `CharacterResponseDto`, `CharacterListItemDto` через `CharacterMapper`

Модель персонажа.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID персонажа |
| Name | `string` | Имя |
| Slug | `string` | URL slug |
| UserSlug | `string` | Slug создателя |
| CharacterUrl | `string` | URL страницы персонажа |
| Photo | `string` | URL фото |
| Description | `string` | Описание |
| Greetings | `List<CharacterGreeting>` | Список приветствий |
| Tags | `List<string>` | Теги |
| DefaultModelId | `string` | ID модели по умолчанию |
| DefaultTemperature | `float?` | Температура по умолчанию |
| DefaultTopP | `float?` | TopP по умолчанию |
| DefaultTopK | `int?` | TopK по умолчанию |
| DefaultMaxTokens | `int?` | MaxTokens по умолчанию |
| Visibility | `string` | Видимость |
| ChatCount | `int` | Количество чатов |
| LikeCount | `int` | Количество лайков |
| FavoriteCount | `int` | Количество в избранном |
| IsFavorite | `bool` | В избранном у текущего пользователя |

**Методы:**
- `GetDefaultGreeting()` → `CharacterGreeting` — приветствие по умолчанию
- `GetGreetingByLanguage(code)` → `CharacterGreeting` — приветствие по языку
- `GetDisplayName()` → `string` — имя для отображения

---

### CharacterGreeting

**Путь:** `Domain/Models/Character/CharacterGreeting.cs`  
**Namespace:** `Core.Data.Domain.Models.Character`  
**Маппится из:** `CharacterGreetingDto` через `CharacterMapper`

Приветствие персонажа.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID приветствия |
| Title | `string` | Заголовок |
| Content | `string` | Текст приветствия |
| IsDefault | `bool` | По умолчанию |
| Language | `string` | Код языка |
| Background | `string` | Фон |
| ChatBackground | `string` | Фон чата |

---

## Message

### MessageModel

**Путь:** `Domain/Models/Message/MessageModel.cs`  
**Namespace:** `Core.Data.Domain.Models.Message`  
**Маппится из:** `MessageResponseDto` через `MessageMapper`

Модель сообщения (отдельная от ChatMessage — для истории/рейтингов).

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID сообщения |
| ChatId | `string` | ID чата |
| Role | `string` | Роль (user/assistant) |
| Content | `string` | Текст |
| CreatedAt | `DateTime` | Дата создания |
| UpdatedAt | `DateTime` | Дата обновления |
| Tokens | `int` | Количество токенов |
| Rating | `int?` | Рейтинг |
| Feedback | `string` | Фидбек |

**Computed Properties:**
- `IsUserMessage` → `bool`
- `IsAssistantMessage` → `bool`
- `IsRated` → `bool`
- `HasFeedback` → `bool`

**Методы:**
- `GetPreview()` → `string` — превью (100 символов)
- `GetContentLength()` → `int` — длина контента
- `GetTokensToCharsRatio()` → `float` — соотношение токенов к символам

---

## Workflow

### WorkflowModel

**Путь:** `Domain/Models/Workflow/WorkflowModel.cs`  
**Namespace:** `Core.Domain.Models.Workflow`  
**Маппится из:** `WorkflowResponseDto` через `WorkflowMapper`

Модель воркфлоу.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID воркфлоу |
| Name | `string` | Название |
| Description | `string` | Описание |
| RootWorkflowId | `string` | ID корневого воркфлоу |
| PreviousVersionId | `string` | ID предыдущей версии |
| VersionNumber | `int` | Номер версии |
| IsLatestPublic | `bool` | Последняя публичная |
| IsApproved | `bool` | Одобрен |
| ContextSchema | `Dictionary<string, object>` | Схема контекста |
| StorageSchema | `Dictionary<string, object>` | Схема хранилища |
| DefaultStorage | `Dictionary<string, object>` | Хранилище по умолчанию |
| NodeCount | `int` | Количество узлов |
| HasValidFlow | `bool` | Валидный flow |
| LastValidatedAt | `DateTime?` | Дата валидации |
| UserId | `string` | ID владельца |
| Visibility | `WorkflowVisibility` | Видимость (enum) |
| CreatedAt | `DateTime` | Дата создания |
| UpdatedAt | `DateTime` | Дата обновления |
| DeletedAt | `DateTime?` | Дата удаления |
| UsageCount | `int` | Количество использований |
| Creator | `WorkflowCreator` | Создатель |
| Triggers | `List<WorkflowTriggerConfig>` | Триггеры |
| Tags | `List<WorkflowTag>` | Теги |
| Counts | `WorkflowCounts` | Счётчики |

**Computed Properties:**
- `IsPublic` → `bool`
- `IsPrivate` → `bool`
- `CanEdit` → `bool`

---

### WorkflowRunModel

**Путь:** `Domain/Models/Workflow/WorkflowRunModel.cs`  
**Namespace:** `Core.Domain.Models.Workflow`  
**Маппится из:** `WorkflowRunDto` через `WorkflowMapper`

Запуск воркфлоу.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID запуска |
| WorkflowId | `string` | ID воркфлоу |
| WorkflowVersion | `int` | Версия |
| Status | `WorkflowRunStatus` | Статус (enum) |
| CurrentNodeId | `string` | Текущий узел |
| StepsTaken | `int` | Выполнено шагов |
| StepLimit | `int` | Лимит шагов |
| StartedAt | `DateTime` | Время запуска |
| CompletedAt | `DateTime?` | Время завершения |
| ResumeAt | `DateTime?` | Время продолжения |
| TriggerType | `string` | Тип триггера |
| TriggerData | `Dictionary<string, object>` | Данные триггера |
| Error | `string` | Ошибка |
| RetryCount | `int` | Количество ретраев |
| FinalResult | `Dictionary<string, object>` | Финальный результат |
| EmitChannel | `string` | Канал emit |
| Context | `WorkflowRunContext` | Контекст |

**Computed Properties:**
- `IsRunning`, `IsPaused`, `IsCompleted`, `HasError`, `IsCancelled`, `IsTimeout` → `bool`
- `IsFinished` → `bool` — завершён (любым способом)
- `Duration` → `TimeSpan?` — длительность
- `Progress` → `float` — прогресс (0-1)

---

### WorkflowExecutionResult

**Путь:** `Domain/Models/Workflow/WorkflowExecutionResult.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Результат выполнения воркфлоу.

| Поле | Тип | Описание |
|------|-----|----------|
| Success | `bool` | Успешно |
| RunId | `string` | ID запуска |
| Output | `Dictionary<string, object>` | Вывод |
| DurationMs | `int` | Длительность (мс) |
| Error | `string` | Ошибка |
| Steps | `List<string>` | Шаги |
| Storage | `Dictionary<string, object>` | Хранилище |
| Profile | `Dictionary<string, object>` | Профиль |

**Computed Properties:**
- `HasError` → `bool`

**Методы:**
- `GetOutput<T>(key, default)` → `T` — получить значение из Output
- `HasOutput(key)` → `bool` — есть ли ключ

---

### ChatWorkflowModel

**Путь:** `Domain/Models/Workflow/ChatWorkflowModel.cs`  
**Namespace:** `Core.Domain.Models.Workflow`  
**Маппится из:** `ChatWorkflowResponseDto` через `WorkflowMapper`

Связь чата с воркфлоу.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID связи |
| ChatId | `string` | ID чата |
| WorkflowId | `string` | ID воркфлоу |
| IsActive | `bool` | Активен ли воркфлоу |
| Workflow | `WorkflowModel` | Полная модель воркфлоу |

**Computed Properties:**
- `WorkflowName` → `string` — Имя воркфлоу (из Workflow.Name или WorkflowId)

---

### WorkflowEntrypointModel

**Путь:** `Domain/Models/Workflow/WorkflowEntrypointModel.cs`  
**Namespace:** `Core.Domain.Models.Workflow`  
**Маппится из:** `WorkflowEntrypointDto` через `WorkflowMapper`

Точка входа в воркфлоу.

| Поле | Тип | Описание |
|------|-----|----------|
| Name | `string` | Имя entrypoint |
| Description | `string` | Описание |
| Type | `string` | Тип |

**Computed Properties:**
- `IsStandaloneTrigger` → `bool` — Name == "STANDALONE_TRIGGER"

---

### WorkflowSettings

**Путь:** `Domain/Models/Workflow/WorkflowSettings.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Настройки воркфлоу.

| Поле | Тип | Описание |
|------|-----|----------|
| ShareStorageBetweenChats | `bool` | Делить ли storage между чатами |
| Raw | `Dictionary<string, object>` | Сырые настройки с API |

---

### WorkflowGraphModel

**Путь:** `Domain/Models/Workflow/WorkflowGraphModel.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Граф воркфлоу — узлы и связи.

| Поле | Тип | Описание |
|------|-----|----------|
| Nodes | `Dictionary<string, RuntimeWorkflowNode>` | Словарь узлов по ID |
| Edges | `List<WorkflowGraphEdge>` | Список рёбер (связей) |
| RootNodeId | `string` | ID корневого узла |

**Методы:**
- `BuildFromNodes(nodes)` — построить граф из списка узлов
- `FindRootNode()` — найти корневой узел (без входящих рёбер)

---

### RuntimeWorkflowNode

**Путь:** `Domain/Models/Workflow/RuntimeWorkflowNode.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Узел воркфлоу во время выполнения.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID узла |
| Type | `string` | Тип узла |
| Params | `Dictionary<string, object>` | Параметры узла |
| Next | `string` | ID следующего узла |
| Definition | `WorkflowNodeTypeDefinition` | Определение типа узла |

**Методы:**
- `GetConnections()` → `List<string>` — получить ID всех связанных узлов (Next, then, else, body, cases)

---

### WorkflowGraphEdge

**Путь:** `Domain/Models/Workflow/WorkflowGraphEdge.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Ребро графа — связь между узлами.

| Поле | Тип | Описание |
|------|-----|----------|
| FromNodeId | `string` | ID исходного узла |
| ToNodeId | `string` | ID целевого узла |
| EdgeType | `string` | Тип связи (next, then, else, body, case_*, etc.) |

---

### WorkflowNodeTypeDefinition

**Путь:** `Domain/Models/Workflow/WorkflowNodeTypeDefinition.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Определение типа узла воркфлоу.

| Поле | Тип | Описание |
|------|-----|----------|
| Type | `string` | Тип узла |
| Label | `string` | Отображаемое название |
| Description | `string` | Описание |
| Category | `string` | Категория |
| Icon | `string` | Иконка |

---

### WorkflowTriggerResult

**Путь:** `Domain/Models/Workflow/WorkflowTriggerResult.cs`  
**Namespace:** `Core.Domain.Models.Workflow`  
**Наследует:** `WorkflowExecutionResult`

| Дополнительно | Тип | Описание |
|------|-----|----------|
| Result | `Dictionary<string, object>` | Алиас для Output |

**Computed Properties:**
- `HasSteps` → `bool`
- `DurationSeconds` → `float`

---

### StorageContext

**Путь:** `Domain/Models/Workflow/StorageContext.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Контекст хранилища.

| Поле | Тип | Описание |
|------|-----|----------|
| Type | `StorageContextType` | Тип (User/Session) |
| SessionId | `string` | ID сессии |

**Статические методы:**
- `User()` → `StorageContext`
- `Session(sessionId)` → `StorageContext`

---

### StandaloneUiResult

**Путь:** `Domain/Models/Workflow/StandaloneUiResult.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Результат standalone UI.

| Поле | Тип | Описание |
|------|-----|----------|
| Success | `bool` | Успешно |
| Components | `List<StandaloneUiComponent>` | Компоненты |
| Error | `StandaloneError` | Ошибка |

---

### Вспомогательные модели Workflow

#### WorkflowCreator
| Поле | Тип |
|------|-----|
| Id | `string` |
| Name | `string` |
| ImageUrl | `string` |

#### WorkflowTag
| Поле | Тип |
|------|-----|
| Id | `string` |
| Name | `string` |
| Slug | `string` |

#### WorkflowCounts
| Поле | Тип |
|------|-----|
| WorkflowRuns | `int` |
| Favorites | `int` |

#### WorkflowTriggerConfig
| Поле | Тип |
|------|-----|
| Id | `string` |
| WorkflowId | `string` |
| Entrypoint | `string` |
| NodeId | `string` |
| IsActive | `bool` |

#### WorkflowRunContext
| Поле | Тип |
|------|-----|
| WorkflowMetadata | `Dictionary<string, object>` |
| Storage | `Dictionary<string, object>` |
| Outputs | `Dictionary<string, object>` |

---

## ImageGen

**Namespace:** `EndlessAdventure.Core.Data.Domain.Models.ImageGen`

### ImageGenMessage

**Путь:** `Domain/Models/ImageGen/ImageGenMessage.cs`

Сообщение в image generation чате.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID сообщения |
| Role | `string` | Роль (user/assistant) |
| TextContent | `string` | Текстовое содержимое |
| Images | `List<IImageHandle>` | Список изображений |
| ImagesBase64 | `List<string>` | Список base64 изображений |
| ImageThoughtSignature | `string` | Thought signature для изображения |
| TextThoughtSignature | `string` | Thought signature для текста |
| ProcessedImage | `IImageHandle` | Обработанное изображение |
| CreatedAt | `DateTime` | Дата создания |
| IsGenerating | `bool` | Генерируется ли |

**Computed Properties:**
- `ImageContent` → `IImageHandle` — первое изображение (get/set)
- `ImageBase64` → `string` — первое base64 изображение (get/set)
- `HasProcessedImage` → `bool`
- `HasImage` → `bool` — есть ли изображение
- `IsUserMessage` → `bool`
- `IsAssistantMessage` → `bool`

**Factory Methods:**
- `CreateUserTextMessage(text)` → `ImageGenMessage`
- `CreateUserImageMessage(text, image, base64)` → `ImageGenMessage`
- `CreateUserImagesMessage(text, images)` → `ImageGenMessage`
- `CreateAssistantPlaceholder()` → `ImageGenMessage`
- `CreateAssistantMessage(text, image)` → `ImageGenMessage`

---

### ImageGenResult

**Путь:** `Domain/Models/ImageGen/ImageGenResult.cs`

Результат генерации изображения.

| Поле | Тип | Описание |
|------|-----|----------|
| IsSuccess | `bool` | Успешна ли генерация |
| TextResponse | `string` | Текстовый ответ |
| GeneratedImage | `IImageHandle` | Сгенерированное изображение |
| ErrorMessage | `string` | Сообщение об ошибке |
| StatusCode | `long` | HTTP статус код |
| ImageThoughtSignature | `string` | Thought signature для изображения |
| TextThoughtSignature | `string` | Thought signature для текста |

**Computed Properties:**
- `HasImage` → `bool`
- `IsFailure` → `bool`

**Factory Methods:**
- `Success(text, image)` → `ImageGenResult`
- `SuccessTextOnly(text)` → `ImageGenResult`
- `Failure(error, statusCode)` → `ImageGenResult`
- `HttpError(error, statusCode)` → `ImageGenResult`

---

### ImageGenSettings

**Путь:** `Domain/Models/ImageGen/ImageGenSettings.cs`

Настройки генерации изображений.

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| Model | `ImageGenModelEnum` | `Flash` | Модель генерации |
| AspectRatio | `string` | `"1:1"` | Соотношение сторон |
| ImageSize | `string` | `"1K"` | Размер изображения |
| UseStreaming | `bool` | `true` | Использовать стриминг |

**Presets (static properties):**
- `Default`, `Square`, `Portrait`, `Landscape`, `HighQuality`
- `UIButton`, `UIIcon`, `UIPanel`, `UICard`, `UIBanner`, `UIBackground`
- `UIButtonHD`, `UIIconHD`, `UIPanel4K`, `UICard4K`, `UIBackground4K`, `Ultra4K`

**Методы:**
- `Clone()` → `ImageGenSettings`
- `Matches(other)` → `bool`

---

### ImageGenModelEnum

**Путь:** `Domain/Models/ImageGen/ImageGenModelEnum.cs`

```csharp
public enum ImageGenModelEnum
{
    Flash,  // gemini-2.5-flash-image
    Pro     // gemini-3-pro-image-preview
}
```

---

### ImageGenUserSettings

**Путь:** `Domain/Models/ImageGen/ImageGenUserSettings.cs`

Пользовательские настройки image generation.

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| Preset | `ImageGenSettings` | — | Текущий пресет |
| CustomSettings | `ImageGenSettings` | — | Кастомные настройки |
| UseCustom | `bool` | — | Использовать кастомные |

---

## UI Extensions

### UiExtensionsModel

**Путь:** `Domain/Models/Chat/Extensions/UiExtensionsModel.cs`  
**Namespace:** `Core.Data.Domain.Models.Chat.Extensions`

| Поле | Тип | Описание |
|------|-----|----------|
| Components | `List<UiComponentModel>` | Компоненты |
| Total | `int` | Всего |

---

### UiComponentModel

**Путь:** `Domain/Models/Chat/Extensions/UiComponentModel.cs`

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `string` | ID компонента |
| Placement | `UiPlacement` | Размещение (enum) |
| Type | `UiComponentType` | Тип (enum) |
| WorkflowId | `string` | ID воркфлоу |
| Metadata | `UiMetadataModel` | Метаданные |
| Config | `IUiConfigModel` | Конфигурация (интерфейс) |

---

### UiMetadataModel

| Поле | Тип | Описание |
|------|-----|----------|
| TimestampUtc | `DateTime` | Время создания |
| TtlSeconds | `int?` | TTL в секундах |
| WorkflowId | `string` | ID воркфлоу |

---

### UiActionModel

| Поле | Тип | Описание |
|------|-----|----------|
| Type | `UiActionType` | Тип действия (enum) |
| Payload | `string` | Payload |
| Confirm | `bool` | Требует подтверждения |
| ConfirmMessage | `string` | Сообщение подтверждения |
| Metadata | `Dictionary<string, object>` | Метаданные |

---

### Config Models (IUiConfigModel)

Все config models имплементируют `IUiConfigModel`.

#### UiButtonConfigModel
| Поле | Тип |
|------|-----|
| Label | `string` |
| Variant | `UiButtonVariant` |
| Size | `UiButtonSize` |
| Icon | `string` |
| Tooltip | `string` |
| Disabled | `bool` |
| Action | `UiActionModel` |

#### UiDisplayConfigModel
| Поле | Тип |
|------|-----|
| Text | `string` |
| Label | `string` |
| IsMarkdown | `bool` |
| Content | `string` (alias для Text) |

#### UiProgressConfigModel
| Поле | Тип |
|------|-----|
| Value | `float` |
| Max | `float` (default: 100) |
| Min | `float` |
| Label | `string` |
| ShowPercentage | `bool` |
| ShowValue | `bool` |
| Animated | `bool` |
| Color | `string` |
| Format | `string` |
| CustomFormat | `string` |

#### UiInputConfigModel
| Поле | Тип |
|------|-----|
| Label | `string` |
| Placeholder | `string` |
| InputType | `UiInputType` |
| Value | `string` |
| MaxLength | `int?` |
| Disabled | `bool` |
| Required | `bool` |
| HelperText | `string` |
| ErrorMessage | `string` |
| OnSubmit | `UiActionModel` |
| OnChange | `UiActionModel` |

#### UiSelectConfigModel
| Поле | Тип |
|------|-----|
| Label | `string` |
| Placeholder | `string` |
| Options | `List<UiSelectOptionModel>` |
| SelectedValue | `string` |
| Variant | `UiSelectVariant` |
| Disabled | `bool` |
| OnChange | `UiActionModel` |

#### UiCheckboxConfigModel
| Поле | Тип |
|------|-----|
| Label | `string` |
| Checked | `bool` |
| Disabled | `bool` |
| HelperText | `string` |
| OnChange | `UiActionModel` |

#### UiImageConfigModel
| Поле | Тип |
|------|-----|
| Url | `string` |
| Alt | `string` |
| Width | `int?` |
| Height | `int?` |
| Fit | `UiImageFit` |
| BorderRadius | `int?` |
| Caption | `string` |
| OnClick | `UiActionModel` |

#### UiAlertConfigModel
| Поле | Тип |
|------|-----|
| Title | `string` |
| Message | `string` |
| Variant | `UiAlertVariant` |
| Icon | `string` |
| Dismissible | `bool` |
| Actions | `List<UiButtonConfigModel>` |
| OnDismiss | `UiActionModel` |

#### UiCardConfigModel
| Поле | Тип |
|------|-----|
| Title | `string` |
| Content | `string` |
| ImageUrl | `string` |
| Footer | `string` |
| Actions | `List<UiButtonConfigModel>` |
| Collapsible | `bool` |
| Collapsed | `bool` |

#### UiListConfigModel
| Поле | Тип |
|------|-----|
| Title | `string` |
| Items | `List<UiListItemModel>` |
| Direction | `UiListDirection` |
| Gap | `int` |
| ListStyle | `string` |
| Selectable | `bool` |
| MultiSelect | `bool` |
| SelectedValues | `List<string>` |
| OnSelectionChange | `UiActionModel` |

#### UiDividerConfigModel
| Поле | Тип |
|------|-----|
| Label | `string` |
| Orientation | `string` |
| Style | `UiDividerStyle` |
| Thickness | `int` |
| Color | `string` |
| Margin | `int` |

#### UiModalConfigModel
| Поле | Тип |
|------|-----|
| Title | `string` |
| Content | `List<UiModalContentModel>` |
| Actions | `List<UiButtonConfigModel>` |
| Open | `bool` |
| BackdropClose | `bool` |
| ShowCloseButton | `bool` |
| Size | `string` |
| OnClose | `UiActionModel` |

---

### Enums (UiEnums.cs)

```csharp
enum UiPlacement { Unknown, AboveInput, BelowChat, Sidebar }

enum UiComponentType { 
    Unknown, Button, Display, Progress, Card, Select, 
    Checkbox, Alert, Input, Image, Divider, Modal, List 
}

enum UiButtonVariant { Unknown, Primary, Secondary, Outline }
enum UiButtonSize { Unknown, Small, Medium, Large }

enum UiActionType { 
    Unknown, TriggerWorkflow, SendMessage, 
    InsertText, ExecuteCommand, SaveToStorage 
}

enum UiAlertVariant { Unknown, Info, Success, Warning, Error }
enum UiSelectVariant { Unknown, Dropdown, Radio, Segmented }
enum UiInputType { Unknown, Text, Number, Email, Password, TextArea }
enum UiImageFit { Unknown, Cover, Contain, Fill }
enum UiDividerStyle { Unknown, Solid, Dashed, Dotted }
enum UiListDirection { Unknown, Vertical, Horizontal }
```
