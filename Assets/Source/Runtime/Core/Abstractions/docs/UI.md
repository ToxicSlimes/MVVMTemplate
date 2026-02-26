# ProceduralUI — Динамическая UI система

> Система для создания UI компонентов из JSON/конфигов в runtime.

## 📋 Обзор интерфейсов

| Интерфейс | Назначение | Путь |
|-----------|------------|------|
| `IProceduralUiService` | Главный сервис рендеринга | `Services/ProceduralUI/` |
| `IProceduralUiComponent` | Базовый UI компонент | `Services/ProceduralUI/` |
| `IUiComponentFactory` | Фабрика компонентов | `Services/ProceduralUI/` |
| `IUiComponentPool` | Пул для переиспользования | `Services/ProceduralUI/` |
| `IUiImageLoader` | Загрузчик изображений | `Services/ProceduralUI/` |
| `IUiActionHandler` | Обработчик действий | `Services/ProceduralUI/` |

### Builders

| Builder | Назначение |
|---------|------------|
| `IProceduralUiBuilder` | Главный entry point |
| `IButtonBuilder` | Кнопки |
| `IAlertBuilder` | Алерты/уведомления |
| `ICardBuilder` | Карточки |
| `IInputBuilder` | Поля ввода |
| `ICheckboxBuilder` | Чекбоксы |
| `ISelectBuilder` | Селекты/dropdown |
| `IProgressBuilder` | Progress bar |
| `IModalBuilder` | Модальные окна |
| `IActionBuilder` | Конфигурация действий |

---

## IProceduralUiService

**Путь:** `Services/ProceduralUI/IProceduralUiService.cs`

```csharp
public interface IProceduralUiService : IDisposable
{
    /// <summary>
    /// Рендерит компоненты в соответствующие контейнеры по placement.
    /// </summary>
    void RenderComponents(
        IReadOnlyList<UiComponentModel> components,
        ProceduralUiContainers containers);

    /// <summary>
    /// Рендерит компоненты только для конкретного placement.
    /// </summary>
    void RenderComponents(
        IReadOnlyList<UiComponentModel> components,
        UiPlacement placement,
        Transform container);

    /// <summary>
    /// Очищает все или конкретный placement.
    /// </summary>
    void ClearComponents(UiPlacement? placement = null);

    /// <summary>
    /// Обновляет конфигурацию компонента без пересоздания.
    /// </summary>
    bool UpdateComponent(string componentId, IUiConfigModel newConfig);

    /// <summary>
    /// Получить компонент по ID.
    /// </summary>
    IProceduralUiComponent GetComponent(string componentId);

    /// <summary>
    /// Все активные компоненты.
    /// </summary>
    IReadOnlyList<IProceduralUiComponent> GetAllComponents();

    /// <summary>
    /// Компоненты для конкретного placement.
    /// </summary>
    IReadOnlyList<IProceduralUiComponent> GetComponents(UiPlacement placement);

    /// <summary>
    /// Показать модальное окно.
    /// </summary>
    void ShowModal(UiModalConfigModel config, string workflowId = null);

    /// <summary>
    /// Скрыть текущее модальное окно.
    /// </summary>
    void HideModal();

    /// <summary>
    /// Видимо ли модальное окно.
    /// </summary>
    bool IsModalVisible { get; }

    /// <summary>
    /// Установить loading state для компонента.
    /// </summary>
    void SetComponentLoading(string componentId, bool isLoading);

    /// <summary>
    /// Установить контекст чата.
    /// </summary>
    void SetChatContext(string chatId);

    /// <summary>
    /// Текущий ID чата.
    /// </summary>
    string CurrentChatId { get; }
}
```

### ProceduralUiContainers

```csharp
public class ProceduralUiContainers
{
    /// <summary>
    /// Над полем ввода (suggestion buttons).
    /// </summary>
    public Transform AboveInput { get; set; }

    /// <summary>
    /// Под сообщениями чата.
    /// </summary>
    public Transform BelowChat { get; set; }

    /// <summary>
    /// Sidebar (stats, quests).
    /// </summary>
    public Transform Sidebar { get; set; }

    /// <summary>
    /// Root для модальных окон.
    /// </summary>
    public Transform ModalRoot { get; set; }
}
```

### Пример использования

```csharp
// Рендеринг из JSON extensions
var extensions = await _chatService.GetChatExtensionsUiAsync(chatId);
_proceduralUiService.RenderComponents(extensions.Components, _containers);

// Fluent builder
var components = _builder
    .ForWorkflow(workflowId)
    .DefaultPlacement(UiPlacement.AboveInput)
    .Button("Attack").SendsMessage("/attack").Build()
    .Button("Defend").SendsMessage("/defend").Build()
    .BuildAll();

_proceduralUiService.RenderComponents(components, _containers);
```

---

## IProceduralUiComponent

**Путь:** `Services/ProceduralUI/IProceduralUiComponent.cs`

```csharp
public interface IProceduralUiComponent : IDisposable
{
    /// <summary>
    /// Уникальный ID компонента.
    /// </summary>
    string ComponentId { get; }

    /// <summary>
    /// Тип компонента.
    /// </summary>
    UiComponentType ComponentType { get; }

    /// <summary>
    /// Текущее состояние.
    /// </summary>
    UiComponentState State { get; }

    /// <summary>
    /// Поток событий действий.
    /// </summary>
    Observable<UiActionEvent> OnAction { get; }

    /// <summary>
    /// Инициализация с моделью.
    /// </summary>
    void Initialize(UiComponentModel model);

    /// <summary>
    /// Обновить конфигурацию.
    /// </summary>
    void UpdateConfig(IUiConfigModel config);

    /// <summary>
    /// Установить состояние.
    /// </summary>
    void SetState(UiComponentState state);

    // Lifecycle
    void OnInit();
    void OnPoolGet();
    void OnPoolAdd();
    void ReturnToPool();
    void OnShowBegin();
    void OnShowEnd();
    void OnHideBegin();
    void OnHideEnd();
}

public enum UiComponentState
{
    Idle = 0,
    Loading = 1,
    Success = 2,
    Error = 3,
    Disabled = 4
}
```

---

## IUiComponentFactory

**Путь:** `Services/ProceduralUI/IUiComponentFactory.cs`

```csharp
public interface IUiComponentFactory
{
    /// <summary>
    /// Создать компонент по модели.
    /// </summary>
    IProceduralUiComponent CreateComponent(UiComponentModel model, Transform parent);

    /// <summary>
    /// Создать компонент по типу.
    /// </summary>
    IProceduralUiComponent CreateComponent(
        UiComponentType type,
        IUiConfigModel config,
        Transform parent,
        string workflowId = null);

    /// <summary>
    /// Поддерживается ли тип?
    /// </summary>
    bool IsTypeSupported(UiComponentType type);

    /// <summary>
    /// Зарегистрировать prefab для типа.
    /// </summary>
    void RegisterPrefab(UiComponentType type, GameObject prefab);

    /// <summary>
    /// Получить prefab для типа.
    /// </summary>
    GameObject GetPrefab(UiComponentType type);
}
```

---

## IUiComponentPool

**Путь:** `Services/ProceduralUI/IUiComponentPool.cs`

```csharp
public interface IUiComponentPool : IDisposable
{
    /// <summary>
    /// Получить компонент из пула или создать новый.
    /// </summary>
    IProceduralUiComponent Get(UiComponentModel model, Transform parent);

    /// <summary>
    /// Получить компонент по типу.
    /// </summary>
    IProceduralUiComponent Get(
        UiComponentType type,
        IUiConfigModel config,
        Transform parent,
        string workflowId = null);

    /// <summary>
    /// Вернуть компонент в пул.
    /// </summary>
    void Return(IProceduralUiComponent component);

    /// <summary>
    /// Вернуть все активные компоненты.
    /// </summary>
    void ReturnAll();

    /// <summary>
    /// Предзагрузка компонентов.
    /// </summary>
    void Prewarm(UiComponentType type, int count, Transform parent);

    /// <summary>
    /// Очистить пул.
    /// </summary>
    void Clear();
    void Clear(UiComponentType type);

    /// <summary>
    /// Количество в пуле для типа.
    /// </summary>
    int GetPooledCount(UiComponentType type);

    int ActiveCount { get; }
    int PooledCount { get; }
    int MaxPoolSizePerType { get; set; }
}
```

---

## IUiImageLoader

**Путь:** `Services/ProceduralUI/IUiImageLoader.cs`

```csharp
public interface IUiImageLoader : IDisposable
{
    /// <summary>
    /// Загрузить изображение по URL.
    /// </summary>
    UniTask<Sprite> LoadImageAsync(string url, CancellationToken ct = default);

    /// <summary>
    /// Загрузить из base64.
    /// </summary>
    Sprite LoadBase64Image(string base64Data);

    /// <summary>
    /// Автоопределение формата (URL или base64).
    /// </summary>
    UniTask<Sprite> LoadImageAutoAsync(string source, CancellationToken ct = default);

    /// <summary>
    /// Предзагрузка нескольких изображений.
    /// </summary>
    UniTask PreloadImagesAsync(string[] urls, CancellationToken ct = default);

    bool IsCached(string url);
    void ClearCache();
    void RemoveFromCache(string url);
    int CacheSize { get; }
    int MaxCacheSize { get; set; }
}
```

---

## IUiActionHandler

**Путь:** `Services/ProceduralUI/IUiActionHandler.cs`

```csharp
public interface IUiActionHandler
{
    /// <summary>
    /// Обработать действие от UI компонента.
    /// </summary>
    UniTask HandleActionAsync(UiActionEvent evt, StorageContext storageContext = null, CancellationToken ct = default);

    /// <summary>
    /// Применить шаблон к payload.
    /// </summary>
    string ApplyTemplate(string template, string value = null, bool? checkedState = null);
}
```

---

## Events

### UiActionEvent

**Путь:** `Services/ProceduralUI/Events/UiActionEvent.cs`

```csharp
public class UiActionEvent
{
    /// <summary>
    /// ID компонента-источника.
    /// </summary>
    public string ComponentId { get; set; }

    /// <summary>
    /// ID workflow для TriggerWorkflow.
    /// </summary>
    public string WorkflowId { get; set; }

    /// <summary>
    /// Тип действия.
    /// </summary>
    public UiActionType ActionType { get; set; }

    /// <summary>
    /// Payload (может содержать {{value}}, {{checked}}).
    /// </summary>
    public string Payload { get; set; }

    /// <summary>
    /// Дополнительные metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }

    /// <summary>
    /// Значение от select/input.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Состояние от checkbox.
    /// </summary>
    public bool? Checked { get; set; }

    public bool RequiresConfirmation { get; set; }
    public string ConfirmationMessage { get; set; }
}
```

### UiErrorEvent

```csharp
public class UiErrorEvent
{
    public UiErrorType Type { get; set; }
    public string ComponentId { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
    public string Context { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRecoverable { get; set; }
}

public enum UiErrorType
{
    Unknown = 0,
    ParseError = 1,
    RenderError = 2,
    ActionError = 3,
    NetworkError = 4,
    ImageLoadError = 5,
    ComponentNotFound = 6,
    PoolError = 7,
    WorkflowError = 8,
    ValidationError = 9
}
```

---

## Builders

### IProceduralUiBuilder

**Путь:** `Services/ProceduralUI/Builders/IProceduralUiBuilder.cs`

```csharp
public interface IProceduralUiBuilder
{
    IButtonBuilder Button();
    IButtonBuilder Button(string label);
    IProgressBuilder Progress();
    IProgressBuilder Progress(float value);
    ICardBuilder Card();
    ICardBuilder Card(string title);
    ISelectBuilder Select();
    ICheckboxBuilder Checkbox();
    ICheckboxBuilder Checkbox(string label);
    IAlertBuilder Alert();
    IAlertBuilder Alert(string message);
    IInputBuilder Input();
    IInputBuilder Input(string placeholder);
    IModalBuilder Modal();
    IModalBuilder Modal(string title);
    
    IProceduralUiBuilder DefaultPlacement(UiPlacement placement);
    IProceduralUiBuilder ForWorkflow(string workflowId);
    IReadOnlyList<UiComponentModel> BuildAll();
}
```

### IButtonBuilder

```csharp
public interface IButtonBuilder
{
    IButtonBuilder Id(string id);
    IButtonBuilder Label(string text);
    IButtonBuilder Variant(UiButtonVariant variant);
    IButtonBuilder Size(UiButtonSize size);
    IButtonBuilder Icon(string iconKey);
    IButtonBuilder Icon(string iconKey, string position);
    IButtonBuilder Disabled(bool disabled = true);
    IButtonBuilder Loading(bool loading = true);
    IButtonBuilder Tooltip(string tooltip);
    IButtonBuilder Placement(UiPlacement placement);
    IButtonBuilder OnClick(Action<IActionBuilder> configure);
    IButtonBuilder SendsMessage(string message);
    IButtonBuilder InsertsText(string text);
    IButtonBuilder TriggersWorkflow(string workflowId);
    UiComponentModel Build();
}
```

### ICardBuilder

```csharp
public interface ICardBuilder
{
    ICardBuilder Id(string id);
    ICardBuilder Title(string title);
    ICardBuilder Subtitle(string subtitle);
    ICardBuilder Content(string content);
    ICardBuilder Image(string imageUrl);
    ICardBuilder Image(string imageUrl, UiImageFit fit);
    ICardBuilder Icon(string iconKey);
    ICardBuilder Footer(string footer);
    ICardBuilder AddAction(Action<IButtonBuilder> configure);
    ICardBuilder AddActions(params Action<IButtonBuilder>[] configures);
    ICardBuilder AddChild(UiComponentModel component);
    ICardBuilder AddChildren(IEnumerable<UiComponentModel> components);
    ICardBuilder Collapsible(bool collapsible = true, bool startCollapsed = false);
    ICardBuilder OnClick(Action<IActionBuilder> configure);
    ICardBuilder Placement(UiPlacement placement);
    UiComponentModel Build();
}
```

### IAlertBuilder

```csharp
public interface IAlertBuilder
{
    IAlertBuilder Id(string id);
    IAlertBuilder Message(string message);
    IAlertBuilder Title(string title);
    IAlertBuilder Variant(UiAlertVariant variant);
    IAlertBuilder Info();
    IAlertBuilder Success();
    IAlertBuilder Warning();
    IAlertBuilder Error();
    IAlertBuilder Icon(string iconKey);
    IAlertBuilder Dismissible(bool dismissible = true);
    IAlertBuilder AutoDismiss(float seconds);
    IAlertBuilder AddAction(Action<IButtonBuilder> configure);
    IAlertBuilder Placement(UiPlacement placement);
    IAlertBuilder OnDismiss(Action<IActionBuilder> configure);
    UiComponentModel Build();
}
```

### IInputBuilder

```csharp
public interface IInputBuilder
{
    IInputBuilder Id(string id);
    IInputBuilder Label(string label);
    IInputBuilder Placeholder(string placeholder);
    IInputBuilder Type(UiInputType type);
    IInputBuilder Value(string value);
    IInputBuilder DefaultValue(string defaultValue);
    IInputBuilder Disabled(bool disabled = true);
    IInputBuilder ReadOnly(bool readOnly = true);
    IInputBuilder Required(bool required = true);
    IInputBuilder MaxLength(int maxLength);
    IInputBuilder MinLength(int minLength);
    IInputBuilder Pattern(string pattern);
    IInputBuilder Pattern(string pattern, string errorMessage);
    IInputBuilder NumericRange(float min, float max, float step = 1);
    IInputBuilder HelperText(string helperText);
    IInputBuilder ErrorMessage(string errorMessage);
    IInputBuilder Prefix(string prefix);
    IInputBuilder Suffix(string suffix);
    IInputBuilder ShowCharacterCount(bool show = true);
    IInputBuilder Rows(int rows);
    IInputBuilder AutoResize(bool autoResize = true);
    IInputBuilder Placement(UiPlacement placement);
    IInputBuilder OnChange(Action<IActionBuilder> configure);
    IInputBuilder OnBlur(Action<IActionBuilder> configure);
    IInputBuilder OnSubmit(Action<IActionBuilder> configure);
    UiComponentModel Build();
}
```

### ISelectBuilder

```csharp
public interface ISelectBuilder
{
    ISelectBuilder Id(string id);
    ISelectBuilder Label(string label);
    ISelectBuilder Placeholder(string placeholder);
    ISelectBuilder Variant(UiSelectVariant variant);
    ISelectBuilder AddOption(string value, string label);
    ISelectBuilder AddOption(string value, string label, string icon);
    ISelectBuilder AddOption(string value, string label, string icon, bool disabled);
    ISelectBuilder AddOptions(IDictionary<string, string> options);
    ISelectBuilder Selected(string value);
    ISelectBuilder Selected(params string[] values);
    ISelectBuilder MultiSelect(bool multiSelect = true);
    ISelectBuilder Disabled(bool disabled = true);
    ISelectBuilder Required(bool required = true);
    ISelectBuilder Placement(UiPlacement placement);
    ISelectBuilder OnChange(Action<IActionBuilder> configure);
    UiComponentModel Build();
}
```

### IProgressBuilder

```csharp
public interface IProgressBuilder
{
    IProgressBuilder Id(string id);
    IProgressBuilder Value(float value);
    IProgressBuilder Value(float current, float max);
    IProgressBuilder Label(string label);
    IProgressBuilder ShowPercentage(bool show = true);
    IProgressBuilder ShowValue(bool show = true);
    IProgressBuilder Color(string colorKey);
    IProgressBuilder Height(int height);
    IProgressBuilder Striped(bool animated = true);
    IProgressBuilder Indeterminate(bool indeterminate = true);
    IProgressBuilder Placement(UiPlacement placement);
    IProgressBuilder OnComplete(Action<IActionBuilder> configure);
    UiComponentModel Build();
}
```

### ICheckboxBuilder

```csharp
public interface ICheckboxBuilder
{
    ICheckboxBuilder Id(string id);
    ICheckboxBuilder Label(string label);
    ICheckboxBuilder Description(string description);
    ICheckboxBuilder Checked(bool @checked = true);
    ICheckboxBuilder Indeterminate(bool indeterminate = true);
    ICheckboxBuilder Disabled(bool disabled = true);
    ICheckboxBuilder Required(bool required = true);
    ICheckboxBuilder Value(string value);
    ICheckboxBuilder Placement(UiPlacement placement);
    ICheckboxBuilder OnChange(Action<IActionBuilder> configure);
    UiComponentModel Build();
}
```

### IModalBuilder

```csharp
public interface IModalBuilder
{
    IModalBuilder Id(string id);
    IModalBuilder Title(string title);
    IModalBuilder Subtitle(string subtitle);
    IModalBuilder Content(string content);
    IModalBuilder Size(int width, int height);
    IModalBuilder Size(string sizePreset);
    IModalBuilder Closable(bool closable = true);
    IModalBuilder CloseOnBackdrop(bool closeOnBackdrop = true);
    IModalBuilder CloseOnEscape(bool closeOnEscape = true);
    IModalBuilder Loading(bool loading = true);
    IModalBuilder AddContent(UiComponentModel component);
    IModalBuilder AddContent(IEnumerable<UiComponentModel> components);
    IModalBuilder AddContent<TBuilder>(Func<TBuilder> builderFactory, Action<TBuilder> configure) where TBuilder : class;
    IModalBuilder PrimaryAction(Action<IButtonBuilder> configure);
    IModalBuilder SecondaryAction(Action<IButtonBuilder> configure);
    IModalBuilder CancelAction(string label = "Cancel");
    IModalBuilder AddFooterAction(Action<IButtonBuilder> configure);
    IModalBuilder ForWorkflow(string workflowId);
    IModalBuilder OnClose(Action<IActionBuilder> configure);
    IModalBuilder OnOpen(Action<IActionBuilder> configure);
    UiModalConfigModel Build();
    UiComponentModel BuildAsComponent();
}
```

### IActionBuilder

```csharp
public interface IActionBuilder
{
    IActionBuilder TriggerWorkflow(string workflowId);
    IActionBuilder TriggerWorkflow(string workflowId, object payload);
    IActionBuilder SendMessage(string message);
    IActionBuilder InsertText(string text);
    IActionBuilder ExecuteCommand(string command);
    IActionBuilder SaveToStorage(string key, object value);
    IActionBuilder WithConfirmation(string message);
    IActionBuilder WithConfirmation(string title, string message);
    IActionBuilder ShowLoading();
    IActionBuilder CloseModal();
    UiActionModel Build();
}
```

---

## 🔄 Flow

```
JSON/Config
    │
    ▼
IProceduralUiBuilder (fluent)
    │
    ▼
UiComponentModel[]
    │
    ▼
IProceduralUiService.RenderComponents()
    │
    ▼
IUiComponentPool.Get()
    │
    ▼
IUiComponentFactory.CreateComponent()
    │
    ▼
IProceduralUiComponent.Initialize()
    │
    ▼
User Interaction
    │
    ▼
Observable<UiActionEvent>
    │
    ▼
IUiActionHandler.HandleActionAsync()
    │
    ▼
IWorkflowService.ExecuteAsync()
```
