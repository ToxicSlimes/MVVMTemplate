# Mappers

Статические классы для преобразования DTO → Domain Model.

## Принципы

1. **Null Safety** — все mappers проверяют входной DTO на null
2. **Статические методы** — без состояния, thread-safe
3. **Naming Convention:**
   - `ToModel(dto)` — единичный объект
   - `ToModels(dtos)` — коллекция
   - `ToDto(model)` — обратное преобразование (редко)

---

## ChatMapper

**Путь:** `Domain/Mappers/ChatMapper.cs`  
**Namespace:** `Core.Data.Domain.Mappers`

### Методы

| Метод | Входной тип | Возвращает |
|-------|-------------|------------|
| `ToModel` | `ChatResponseDto` | `ChatModel` |
| `ToModels` | `IEnumerable<ChatResponseDto>` | `List<ChatModel>` |
| `ToModel` | `ChatMessageDto` | `ChatMessage` |
| `ToMessages` | `IEnumerable<ChatMessageDto>` | `List<ChatMessage>` |

### Пример использования

```csharp
// Единичный объект
var chatDto = await apiClient.GetChat(chatId);
var chatModel = ChatMapper.ToModel(chatDto);

// Коллекция
var chatsDto = await apiClient.GetChats();
var chatModels = ChatMapper.ToModels(chatsDto);

// Сообщения из чата уже маппятся автоматически
var messages = chatModel.Messages; // List<ChatMessage>
```

### Особенности маппинга

```csharp
// Коллекции инициализируются пустыми списками
Messages = dto.Messages?.Select(ToModel).ToList() ?? new List<ChatMessage>();
```

---

## CharacterMapper

**Путь:** `Domain/Mappers/CharacterMapper.cs`  
**Namespace:** `Core.Data.Domain.Mappers`

### Методы

| Метод | Входной тип | Возвращает |
|-------|-------------|------------|
| `ToModel` | `CharacterResponseDto` | `CharacterModel` |
| `ToModel` | `CharacterListItemDto` | `CharacterModel` |
| `ToModels` | `IEnumerable<CharacterResponseDto>` | `List<CharacterModel>` |
| `ToModels` | `IEnumerable<CharacterListItemDto>` | `List<CharacterModel>` |
| `ToModel` | `CharacterGreetingDto` | `CharacterGreeting` |

### Пример использования

```csharp
// Полный персонаж
var characterDto = await apiClient.GetCharacter(id);
var character = CharacterMapper.ToModel(characterDto);

// Список персонажей (CharacterListItemDto)
var listResponse = await apiClient.GetCharacters();
var characters = CharacterMapper.ToModels(listResponse.Data);
```

### Особенности маппинга

```csharp
// Description берётся из первого значения словаря Descriptions
Description = dto.Descriptions != null && dto.Descriptions.Count > 0
    ? dto.Descriptions.FirstOrDefault().Value
    : null,

// Для CharacterListItemDto — UserSlug берётся из Creator
UserSlug = dto.Creator?.Slug,
```

---

## MessageMapper

**Путь:** `Domain/Mappers/MessageMapper.cs`  
**Namespace:** `Core.Data.Domain.Mappers`

### Методы

| Метод | Входной тип | Возвращает |
|-------|-------------|------------|
| `ToModel` | `MessageResponseDto` | `MessageModel` |
| `ToModels` | `IEnumerable<MessageResponseDto>` | `List<MessageModel>` |

### Пример использования

```csharp
var messageDto = await apiClient.GetMessage(messageId);
var message = MessageMapper.ToModel(messageDto);

var historyDto = await apiClient.GetMessageHistory(chatId);
var messages = MessageMapper.ToModels(historyDto);
```

---

## WorkflowMapper

**Путь:** `Domain/Mappers/WorkflowMapper.cs`  
**Namespace:** `Core.Domain.Mappers`

### Методы

| Метод | Входной тип | Возвращает |
|-------|-------------|------------|
| `ToModel` | `WorkflowResponseDto` | `WorkflowModel` |
| `ToModels` | `IEnumerable<WorkflowResponseDto>` | `List<WorkflowModel>` |
| `ToModel` | `WorkflowCreatorDto` | `WorkflowCreator` |
| `ToModel` | `WorkflowTagDto` | `WorkflowTag` |
| `ToModel` | `WorkflowCountsDto` | `WorkflowCounts` |
| `ToModel` | `WorkflowTriggerConfigDto` | `WorkflowTriggerConfig` |
| `ToModel` | `WorkflowRunDto` | `WorkflowRunModel` |
| `ToModels` | `IEnumerable<WorkflowRunDto>` | `List<WorkflowRunModel>` |
| `ToModel` | `WorkflowRunContextDto` | `WorkflowRunContext` |
| `ToModel` | `WorkflowTriggerResponseDto` | `WorkflowTriggerResult` |
| `ToModel` | `StandaloneUiResponseDto` | `StandaloneUiResult` |
| `ToModel` | `StandaloneUiComponentDto` | `StandaloneUiComponent` |
| `ToCreateDto` | `WorkflowModel` | `CreateWorkflowDto` |
| `ToUpdateDto` | `WorkflowModel` | `UpdateWorkflowDto` |
| `ToDto` | `StorageContext` | `StorageContextDto` |
| `ToExecuteRequest` | params | `WorkflowExecuteRequestDto` |
| `ToExecutionResult` | `WorkflowTriggerResponseDto` | `WorkflowExecutionResult` |
| `ToModel` | `ChatWorkflowResponseDto` | `ChatWorkflowModel` |
| `ToModels` | `IEnumerable<ChatWorkflowResponseDto>` | `List<ChatWorkflowModel>` |
| `ToModel` | `WorkflowEntrypointDto` | `WorkflowEntrypointModel` |
| `ToEntrypointModels` | `IEnumerable<WorkflowEntrypointDto>` | `List<WorkflowEntrypointModel>` |

### Пример использования

```csharp
// Получение воркфлоу
var dto = await apiClient.GetWorkflow(id);
var workflow = WorkflowMapper.ToModel(dto);

// Триггер воркфлоу
var triggerDto = await apiClient.TriggerWorkflow(id, payload);
var result = WorkflowMapper.ToModel(triggerDto);
if (result.Success)
{
    var output = result.Output;
}

// Создание DTO для API
var createDto = WorkflowMapper.ToCreateDto(newWorkflow);
await apiClient.CreateWorkflow(createDto);

// Запрос на выполнение
var executeDto = WorkflowMapper.ToExecuteRequest(
    payload: new Dictionary<string, object> { { "action", "click" } },
    storageContext: StorageContext.User(),
    options: new WorkflowExecutionOptions { IncludeSteps = true }
);
```

### Особенности маппинга

```csharp
// JObject/JArray преобразуется в Dictionary/List
var rawOutput = dto.Output ?? dto.Result;
var output = rawOutput switch
{
    JObject jobj => jobj.ToObject<Dictionary<string, object>>(),
    JArray jarr => new Dictionary<string, object>
    {
        { "components", jarr.ToObject<List<object>>() }
    },
    Dictionary<string, object> dict => dict,
    _ => null
};

// Обратное преобразование Model → DTO
public static CreateWorkflowDto ToCreateDto(WorkflowModel model)
{
    return new CreateWorkflowDto
    {
        Name = model.Name,
        Description = model.Description,
        Visibility = model.Visibility,
        // ...
    };
}
```

---

## ChatExtensionsUiMapper

**Путь:** `Domain/Mappers/ChatExtensionsUiMapper.cs`  
**Namespace:** `Core.Domain.Mappers`

### Методы

| Метод | Входной тип | Возвращает |
|-------|-------------|------------|
| `ToModel` | `ChatExtensionsUiResponseDto` | `UiExtensionsModel` |
| `ToModel` | `ChatUiComponentDto` | `UiComponentModel` |
| `ToModel` | `ChatUiMetadataDto` | `UiMetadataModel` |

### Пример использования

```csharp
var extensionsDto = await apiClient.GetChatExtensionsUi(chatId);
var extensions = ChatExtensionsUiMapper.ToModel(extensionsDto);

foreach (var component in extensions.Components)
{
    switch (component.Type)
    {
        case UiComponentType.Button:
            var config = component.Config as UiButtonConfigModel;
            RenderButton(config);
            break;
        case UiComponentType.Display:
            var displayConfig = component.Config as UiDisplayConfigModel;
            RenderDisplay(displayConfig);
            break;
    }
}
```

### Особенности маппинга

```csharp
// Парсинг placement из строки в enum
private static UiPlacement ParsePlacement(string value)
{
    return value?.Trim().ToLowerInvariant() switch
    {
        "above_input" => UiPlacement.AboveInput,
        "below_chat" => UiPlacement.BelowChat,
        "sidebar" => UiPlacement.Sidebar,
        _ => UiPlacement.Unknown
    };
}

// Config парсится из JObject в типизированную модель
private static IUiConfigModel ParseConfig(string type, JObject config)
{
    var componentType = ParseComponentType(type);
    return componentType switch
    {
        UiComponentType.Button => config.ToObject<ChatUiButtonConfigDto>()?.Let(ToModel),
        UiComponentType.Display => config.ToObject<ChatUiDisplayConfigDto>()?.Let(ToModel),
        _ => null
    };
}
```

---

## StandaloneUiMapper

**Путь:** `Domain/Mappers/StandaloneUiMapper.cs`  
**Namespace:** `Core.Domain.Mappers`

### Методы

| Метод | Входной тип | Возвращает |
|-------|-------------|------------|
| `ToComponentModels` | `StandaloneUiResult, string workflowId` | `List<UiComponentModel>` |
| `ToComponentModel` | `StandaloneUiComponent, string workflowId` | `UiComponentModel` |

### Пример использования

```csharp
var standaloneResult = await workflowService.GetStandaloneUi(workflowId);
var components = StandaloneUiMapper.ToComponentModels(standaloneResult, workflowId);

foreach (var component in components)
{
    RenderComponent(component);
}
```

### Особенности маппинга

Mapper создаёт типизированные Config модели на основе `type`:

```csharp
private static IUiConfigModel CreateConfig(StandaloneUiComponent component)
{
    return component.Type?.ToLowerInvariant() switch
    {
        "button" => CreateButtonConfig(config, component),
        "display" => CreateDisplayConfig(config),
        "progress" => CreateProgressConfig(config),
        "input" => CreateInputConfig(config),
        "select" => CreateSelectConfig(config),
        "slider" => CreateProgressConfig(config),
        "checkbox" => CreateCheckboxConfig(config),
        "image" => CreateImageConfig(config),
        "alert" => CreateAlertConfig(config),
        _ => CreateDisplayConfig(config)
    };
}
```

Хелперы для извлечения значений из Dictionary:

```csharp
private static string GetStringFromConfig(Dictionary<string, object> config, string key, string defaultValue = null);
private static int GetIntFromConfig(Dictionary<string, object> config, string key, int defaultValue = 0);
private static float GetFloatFromConfig(Dictionary<string, object> config, string key, float defaultValue = 0);
private static bool GetBoolFromConfig(Dictionary<string, object> config, string key, bool defaultValue = false);
private static List<UiSelectOptionModel> GetOptionsFromConfig(Dictionary<string, object> config);
```

---

## Таблица маппинга (сводная)

| DTO | Model | Mapper | Namespace |
|-----|-------|--------|-----------|
| `ChatResponseDto` | `ChatModel` | `ChatMapper` | `Core.Data.Domain.Mappers` |
| `ChatMessageDto` | `ChatMessage` | `ChatMapper` | `Core.Data.Domain.Mappers` |
| `CharacterResponseDto` | `CharacterModel` | `CharacterMapper` | `Core.Data.Domain.Mappers` |
| `CharacterListItemDto` | `CharacterModel` | `CharacterMapper` | `Core.Data.Domain.Mappers` |
| `CharacterGreetingDto` | `CharacterGreeting` | `CharacterMapper` | `Core.Data.Domain.Mappers` |
| `MessageResponseDto` | `MessageModel` | `MessageMapper` | `Core.Data.Domain.Mappers` |
| `WorkflowResponseDto` | `WorkflowModel` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowRunDto` | `WorkflowRunModel` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowCreatorDto` | `WorkflowCreator` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowTagDto` | `WorkflowTag` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowCountsDto` | `WorkflowCounts` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowTriggerConfigDto` | `WorkflowTriggerConfig` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowRunContextDto` | `WorkflowRunContext` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowTriggerResponseDto` | `WorkflowTriggerResult` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `StandaloneUiResponseDto` | `StandaloneUiResult` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `StandaloneUiComponentDto` | `StandaloneUiComponent` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `ChatExtensionsUiResponseDto` | `UiExtensionsModel` | `ChatExtensionsUiMapper` | `Core.Domain.Mappers` |
| `ChatUiComponentDto` | `UiComponentModel` | `ChatExtensionsUiMapper` | `Core.Domain.Mappers` |
| `ChatUiMetadataDto` | `UiMetadataModel` | `ChatExtensionsUiMapper` | `Core.Domain.Mappers` |
| `StandaloneUiResult` | `List<UiComponentModel>` | `StandaloneUiMapper` | `Core.Domain.Mappers` |
| `ChatWorkflowResponseDto` | `ChatWorkflowModel` | `WorkflowMapper` | `Core.Domain.Mappers` |
| `WorkflowEntrypointDto` | `WorkflowEntrypointModel` | `WorkflowMapper` | `Core.Domain.Mappers` |
