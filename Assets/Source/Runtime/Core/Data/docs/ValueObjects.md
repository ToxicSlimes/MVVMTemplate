# Value Objects

Value Objects — объекты-значения, идентифицируемые по значению, а не по ссылке.

## StorageContext

**Путь:** `Domain/Models/Workflow/StorageContext.cs`  
**Namespace:** `Core.Domain.Models.Workflow`

Контекст хранилища для воркфлоу — определяет scope хранения данных.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Type | `StorageContextType` | Тип контекста (User/Session) |
| SessionId | `string` | ID сессии (для Session типа) |

### Enum StorageContextType

```csharp
public enum StorageContextType
{
    User,     // Хранилище привязано к пользователю
    Session   // Хранилище привязано к сессии
}
```

### Factory Methods

```csharp
// Создать user-scoped контекст
var userContext = StorageContext.User();

// Создать session-scoped контекст
var sessionContext = StorageContext.Session();
var sessionContextWithId = StorageContext.Session("my-session-123");
```

### Использование

```csharp
// В запросе выполнения воркфлоу
var executeDto = WorkflowMapper.ToExecuteRequest(
    payload: new Dictionary<string, object> { { "action", "click" } },
    storageContext: StorageContext.User(),
    options: new WorkflowExecutionOptions { IncludeStorage = true }
);
```

---

## WorkflowExecutionOptions

Опции выполнения воркфлоу.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| IncludeSteps | `bool` | `false` | Включить шаги выполнения в ответ |
| IncludeStorage | `bool` | `false` | Включить storage в ответ |
| IncludeProfile | `bool` | `false` | Включить профиль в ответ |

### Использование

```csharp
var options = new WorkflowExecutionOptions
{
    IncludeSteps = true,
    IncludeStorage = true
};

var result = await workflowService.Execute(workflowId, payload, options);
// result.Steps — список шагов
// result.Storage — текущее storage
```

---

## StandaloneError

**Путь:** `Domain/Models/Workflow/StandaloneUiResult.cs`

Ошибка standalone UI.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Code | `string` | Код ошибки |
| Message | `string` | Сообщение |

### Константы кодов ошибок

```csharp
public static class Codes
{
    public const string WorkflowNotFound = "WORKFLOW_NOT_FOUND";
    public const string EntrypointNotFound = "ENTRYPOINT_NOT_FOUND";
    public const string AccessDenied = "ACCESS_DENIED";
    public const string ContextError = "STANDALONE_CONTEXT_ERROR";
    public const string ValidationError = "OUTPUT_VALIDATION_ERROR";
    public const string ExecutionError = "EXECUTION_ERROR";
}
```

### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| IsNotFound | `bool` | Code == WORKFLOW_NOT_FOUND |
| IsAccessDenied | `bool` | Code == ACCESS_DENIED |

### Использование

```csharp
var result = await workflowService.GetStandaloneUi(workflowId);
if (result.HasError)
{
    if (result.Error.IsNotFound)
    {
        ShowError("Воркфлоу не найден");
    }
    else if (result.Error.IsAccessDenied)
    {
        ShowError("Нет доступа");
    }
    else
    {
        ShowError(result.Error.Message);
    }
}
```

---

## StandaloneUiMetadata

**Путь:** `Domain/Models/Workflow/StandaloneUiResult.cs`

Метаданные standalone UI компонента.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| Timestamp | `DateTime` | — | Время создания |
| Ttl | `int` | 300 | TTL в секундах |

---

## UiSelectOptionModel

**Путь:** `Domain/Models/Chat/Extensions/UiSelectConfigModel.cs`

Опция для Select компонента.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Value | `string` | Значение (отправляется на бэкенд) |
| Label | `string` | Отображаемый текст |
| Disabled | `bool` | Заблокирована |
| Icon | `string` | Идентификатор иконки |

### Использование

```csharp
var selectConfig = new UiSelectConfigModel
{
    Label = "Выберите опцию",
    Options = new List<UiSelectOptionModel>
    {
        new() { Value = "option1", Label = "Первая опция" },
        new() { Value = "option2", Label = "Вторая опция", Icon = "star" },
        new() { Value = "option3", Label = "Недоступно", Disabled = true }
    },
    SelectedValue = "option1"
};
```

---

## UiListItemModel

**Путь:** `Domain/Models/Chat/Extensions/UiListConfigModel.cs`

Элемент списка для List компонента.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Value | `string` | Идентификатор элемента |
| Primary | `string` | Основной текст |
| Secondary | `string` | Подзаголовок |
| Icon | `string` | Идентификатор иконки |
| ImageUrl | `string` | URL изображения |
| Disabled | `bool` | Заблокирован |
| OnClick | `UiActionModel` | Действие при клике |

---

## UiModalContentModel

**Путь:** `Domain/Models/Chat/Extensions/UiModalConfigModel.cs`

Контент модального окна.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Type | `string` | Тип: "text", "markdown", "image", "component" |
| Text | `string` | Текст (для text/markdown) |
| ImageUrl | `string` | URL изображения (для image) |
| Component | `UiComponentModel` | Вложенный компонент (для component) |

### Использование

```csharp
var modalConfig = new UiModalConfigModel
{
    Title = "Подтверждение",
    Content = new List<UiModalContentModel>
    {
        new() { Type = "text", Text = "Вы уверены?" },
        new() { Type = "image", ImageUrl = "https://example.com/img.png" },
        new() 
        { 
            Type = "markdown", 
            Text = "**Важно:** это действие необратимо" 
        }
    },
    Actions = new List<UiButtonConfigModel>
    {
        new() { Label = "Отмена", Variant = UiButtonVariant.Outline },
        new() { Label = "Подтвердить", Variant = UiButtonVariant.Primary }
    }
};
```

---

## SceneLoadPayload

**Путь:** `ValueObjects/SceneLoadPayload.cs`  
**Namespace:** (root)

**Readonly struct** для данных загрузки сцены.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| SceneIndex | `int` | Индекс сцены (-1 если используется SceneName) |
| SceneName | `string` | Имя сцены (null если используется SceneIndex) |
| LoadType | `SceneLoadType` | Тип загрузки (enum) |
| Mode | `LoadSceneMode` | Режим загрузки Unity (Single/Additive) |
| SceneLoaded | `Action` | Callback после загрузки сцены |
| ActivationAwaiter | `Func<CancellationToken, UniTask>` | Awaiter для активации |

### Конструкторы

```csharp
// По индексу
public SceneLoadPayload(
    int sceneIndex, 
    SceneLoadType loadType, 
    LoadSceneMode mode = LoadSceneMode.Single, 
    Action sceneLoaded = null, 
    Func<CancellationToken, UniTask> activationAwaiter = null)

// По имени
public SceneLoadPayload(
    string sceneName, 
    SceneLoadType loadType, 
    LoadSceneMode mode = LoadSceneMode.Single, 
    Action sceneLoaded = null, 
    Func<CancellationToken, UniTask> activationAwaiter = null)
```

---

## SceneLoadType

**Путь:** `ValueObjects/SceneLoadType.cs`

```csharp
public enum SceneLoadType
{
    Default,
    WithTransition,
    Background
}
```

---

## Характеристики Value Objects

| Тип | Immutable | Equality by Value | Factory Methods |
|-----|-----------|-------------------|-----------------|
| StorageContext | ❌ (можно изменять) | ❌ | ✅ User(), Session() |
| StandaloneError | ❌ | ❌ | ❌ |
| UiSelectOptionModel | ❌ | ❌ | ❌ |
| UiListItemModel | ❌ | ❌ | ❌ |
| UiModalContentModel | ❌ | ❌ | ❌ |

> **Примечание:** Текущие Value Objects не являются классическими immutable value objects. Они используются как структуры данных для конфигурации.
