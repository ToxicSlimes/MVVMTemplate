# Models — UI State Management

## Обзор

Models — классы состояния UI. Содержат properties с `event Action` для уведомления ViewModels об изменениях. Реализуют интерфейс `IModel`:

```csharp
public interface IModel : IDisposable
{
    bool IsValid { get; }
    void Reset();
}
```

## Архитектура

```
Model (property changed) ──► event ──► ViewModel ──► ReactiveProperty ──► View
```

Models **НЕ используют** R3 или UniTask напрямую — только plain C# events.

---

## Каталог Models

| Model | Namespace | Описание |
|-------|-----------|----------|
| `ChatModel` | `Chat` | Состояние чата с персонажем |
| `HealingPanelModel` | `HealingPanel` | Состояние панели лечения |
| `HeroesGridPanelModel` | `HeroesGridPanel` | Состояние грида героев |
| `ImageGenModel` | `ImageGen` | Состояние генерации изображений |
| `LoginModel` | `Login` | Состояние экрана авторизации |
| `MainModel` | `Main` | Состояние главного экрана |
| `MenuSelectionModel` | `MenuSelection` | Состояние меню выбора |
| `NetworkSandboxModel` | `NetworkSandbox` | Состояние тестового экрана Workflow API |
| `PerformanceTestModel` | `PerformanceTest` | Состояние экрана тестирования производительности |
| `SettingsModel` | `Settings` | Состояние настроек игры |
| `StandaloneWorkflowModel` | `StandaloneWorkflow` | Состояние standalone workflow |
| `TrainingPanelModel` | `TrainingPanel` | Состояние панели тренировки |
| `UIConstructorModel` | `UIConstructor` | Состояние конструктора UI |

---

## ChatModel

**Путь:** `Models/Chat/ChatModel.cs`  
**Interface:** `IChatModel`

Состояние чата с AI персонажем.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `StatusText` | `string` | Статус операции |
| `ResponseText` | `string` | JSON ответ (debug) |
| `IsLoading` | `bool` | Флаг загрузки |
| `Character` | `CharacterModel` | Текущий персонаж |
| `CurrentChatId` | `string` | ID активного чата |
| `UiExtensions` | `UiExtensionsModel` | Procedural UI расширения |
| `ChatMessages` | `List<ChatMessage>` | История сообщений |

### Events

```csharp
event Action<string> StatusTextChanged;
event Action<string> ResponseTextChanged;
event Action<bool> IsLoadingChanged;
event Action<CharacterModel> CharacterChanged;
event Action<string> CurrentChatIdChanged;
event Action<UiExtensionsModel> UiExtensionsChanged;
event Action<List<ChatMessage>> ChatMessagesChanged;
```

### Методы

```csharp
void AddChatMessage(ChatMessage message);
void ClearChat();
```

---

## HealingPanelModel

**Путь:** `Models/HealingPanel/HealingPanelModel.cs`  
**Interface:** `IHealingPanelModel`

Состояние панели лечения героев.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `Title` | `string` | Заголовок панели (readonly: "Лечение героев") |
| `Rows` | `IReadOnlyList<ServiceRowData>` | Строки со слотами (3 ряда по 3 слота) |
| `SelectedSlotId` | `string` | ID выбранного слота |

### Events

```csharp
event Action<IReadOnlyList<ServiceRowData>> RowsChanged;
event Action<string> SelectedSlotIdChanged;
```

### Методы

```csharp
void LoadData();
void SelectSlot(string slotId);
bool AssignHeroToSlot(string slotId, string heroId);
bool RemoveHeroFromSlot(string slotId);
bool UnlockSlot(string slotId);
string AssignHeroToFirstAvailable(string heroId);
```

### Структура данных

```csharp
public readonly struct ServiceRowData
{
    public int RowIndex;
    public string RowLabel;
    public IReadOnlyList<ServiceSlotData> Slots;
}

public readonly struct ServiceSlotData
{
    public string SlotId;
    public ServiceSlotState State;  // Empty, Occupied, Locked
    public string HeroId;
    public string HeroName;
    public float Progress;
    public int RemainingSeconds;
    public int UnlockCost;
}
```

---

## HeroesGridPanelModel

**Путь:** `Models/HeroesGridPanel/HeroesGridPanelModel.cs`  
**Interface:** `IHeroesGridPanelModel`

Состояние грида с карточками героев (3 вкладки: My Heroes, Shop, Dead Heroes).

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `MyHeroes` | `IReadOnlyList<HeroCardData>` | Герои игрока |
| `ShopHeroes` | `IReadOnlyList<HeroCardData>` | Герои в магазине |
| `DeadHeroes` | `IReadOnlyList<HeroCardData>` | Погибшие герои |
| `CurrentTab` | `HeroesGridTab` | Активная вкладка |
| `CurrentMode` | `HeroesGridMode` | Режим (Upgrades/Healing/TeamManagement) |
| `SelectedHeroId` | `string` | ID выбранного героя |

### Events

```csharp
event Action<IReadOnlyList<HeroCardData>> MyHeroesChanged;
event Action<IReadOnlyList<HeroCardData>> ShopHeroesChanged;
event Action<IReadOnlyList<HeroCardData>> DeadHeroesChanged;
event Action<HeroesGridTab> TabChanged;
event Action<string> SelectedHeroIdChanged;
```

### Методы

```csharp
void SetMode(HeroesGridMode mode);
void SelectTab(HeroesGridTab tab);
void SelectHero(string heroId);
void LoadData();
```

### Enums

```csharp
public enum HeroesGridTab
{
    MyHeroes,
    Shop,
    DeadHeroes
}

public enum HeroesGridMode
{
    TeamManagement,
    Healing,
    Upgrades
}
```

---

## ImageGenModel

**Путь:** `Models/ImageGen/ImageGenModel.cs`  
**Interface:** `IImageGenModel`

Состояние генерации изображений через Gemini API.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `Messages` | `List<ImageGenMessage>` | История сообщений с изображениями |
| `SelectedSettings` | `ImageGenSettings` | Настройки генерации |
| `PendingAttachments` | `List<IImageHandle>` | Прикреплённые изображения (до 14) |
| `IsGenerating` | `bool` | Флаг генерации |
| `StatusText` | `string` | Статус операции |

### Events

```csharp
event Action<List<ImageGenMessage>> MessagesChanged;
event Action<ImageGenSettings> SelectedSettingsChanged;
event Action<List<IImageHandle>> PendingAttachmentsChanged;
event Action<bool> IsGeneratingChanged;
event Action<string> StatusTextChanged;
```

### Методы

```csharp
void AddMessage(ImageGenMessage message);
void UpdateMessage(string id, ImageGenMessage updated);
ImageGenMessage FindMessage(string id);
void RemoveMessage(string id);
void ClearMessages();
ImageGenMessage GetLastMessage();
ImageGenMessage GetLastAssistantMessage();

void AddAttachment(IImageHandle image);
void ClearAttachments();
void RemoveAttachment(int index);
void RemoveAttachment(IImageHandle image);
```

### ImageGenMessage

```csharp
public readonly struct ImageGenMessage
{
    public string Id;
    public string Text;
    public IReadOnlyList<IImageHandle> Images;
    public bool IsAssistantMessage;
    public string ThoughtSignature;
}
```

---

## LoginModel

**Путь:** `Models/Login/LoginModel.cs`  
**Interface:** `ILoginModel`

Состояние экрана авторизации.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `Email` | `string` | Email пользователя |
| `RememberMe` | `bool` | Запомнить данные |
| `StatusText` | `string` | Статус операции |
| `IsLoading` | `bool` | Флаг загрузки |
| `IsAuthorized` | `bool` | Флаг авторизации |

### Events

```csharp
event Action<string> EmailChanged;
event Action<bool> RememberMeChanged;
event Action<string> StatusTextChanged;
event Action<bool> IsLoadingChanged;
event Action<bool> IsAuthorizedChanged;
```

---

## MainModel

**Путь:** `Models/Main/MainModel.cs`  
**Interface:** `IMainModel`

Состояние главного экрана игры.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `Heroes` | `IReadOnlyList<HeroSlotData>` | Слоты героев |
| `Stats` | `IReadOnlyList<StatRowData>` | Характеристики героя |
| `Resources` | `IReadOnlyList<ResourceData>` | Ресурсы игрока |
| `SelectedHeroId` | `string` | ID выбранного героя |

### Events

```csharp
event Action<IReadOnlyList<HeroSlotData>> HeroesChanged;
event Action<IReadOnlyList<StatRowData>> StatsChanged;
event Action<IReadOnlyList<ResourceData>> ResourcesChanged;
event Action<string> SelectedHeroIdChanged;
```

### Методы

```csharp
void LoadData();
void SelectHero(string heroId);
```

### Data Structs

```csharp
public readonly struct HeroSlotData
{
    public string Id;
    public string Name;
    public string Class;
    public int Level;
    public bool IsSelected;
}

public readonly struct StatRowData
{
    public string StatId;
    public string Label;
    public string Value;
    public Sprite Icon;
    public float? BarPercent;
}

public readonly struct ResourceData
{
    public string ResourceId;
    public string Name;
    public int Amount;
    public string Description;
    public Sprite Icon;
}
```

---

## MenuSelectionModel

**Путь:** `Models/MenuSelection/MenuSelectionModel.cs`  
**Interface:** `IMenuSelectionModel`

Состояние экрана выбора направления.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `StatusText` | `string` | Статус (по умолчанию: "Выберите направление") |

### Events

```csharp
event Action<string> StatusTextChanged;
```

---

## NetworkSandboxModel

**Путь:** `Models/NetworkSandbox/NetworkSandboxModel.cs`  
**Interface:** `INetworkSandboxModel`

Состояние тестового экрана Workflow API.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `WorkflowId` | `string` | ID workflow для загрузки |
| `ResponseText` | `string` | Текстовый ответ API |
| `StatusText` | `string` | Статус операции |
| `IsLoading` | `bool` | Флаг загрузки |
| `CurrentWorkflow` | `WorkflowModel` | Загруженный workflow |

### Events

```csharp
event Action<string> WorkflowIdChanged;
event Action<string> ResponseTextChanged;
event Action<string> StatusTextChanged;
event Action<bool> IsLoadingChanged;
event Action<WorkflowModel> CurrentWorkflowChanged;
```

---

## PerformanceTestModel

**Путь:** `Models/PerformanceTest/PerformanceTestModel.cs`  
**Interface:** `IPerformanceTestModel`

Состояние экрана тестирования производительности чата.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `AvailableCharacters` | `List<CharacterModel>` | Список персонажей |
| `SelectedCharacterIndex` | `int` | Индекс выбранного персонажа |
| `Metrics` | `List<PerformanceMetrics>` | Метрики запросов |
| `Statistics` | `PerformanceStatistics` | Статистика |
| `IsRunning` | `bool` | Флаг запущенного теста |
| `StatusText` | `string` | Статус операции |
| `GreetingMessage` | `string` | Приветствие персонажа |
| `ResponseMessage` | `string` | Ответ персонажа |
| `ChatMessages` | `List<ChatMessage>` | История чата |
| `CurrentChatId` | `string` | ID чата |

### Events

```csharp
event Action<List<CharacterModel>> AvailableCharactersChanged;
event Action<int> SelectedCharacterIndexChanged;
event Action<PerformanceMetrics> MetricAdded;
event Action<List<PerformanceMetrics>> MetricsChanged;
event Action<PerformanceStatistics> StatisticsChanged;
event Action<bool> IsRunningChanged;
event Action<string> StatusTextChanged;
event Action<string> GreetingMessageChanged;
event Action<string> ResponseMessageChanged;
event Action<List<ChatMessage>> ChatMessagesChanged;
```

### Методы

```csharp
void AddMetric(PerformanceMetrics metric);
void AddChatMessage(ChatMessage message);
void ClearChat();
void ClearMetrics();
CharacterModel GetSelectedCharacter();
```

---

## SettingsModel

**Путь:** `Models/Settings/SettingsModel.cs`  
**Interface:** `ISettingsModel`

Состояние настроек игры. Делегирует настройки к sub-interfaces.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `IsVisible` | `bool` | Видимость окна настроек |
| `CurrentTab` | `SettingsTab` | Активная вкладка |
| `Audio` | `IAudioSettings` | Настройки звука |
| `Video` | `IVideoSettings` | Настройки видео |
| `Display` | `IDisplaySettings` | Настройки экрана |
| `Controls` | `IControlsSettings` | Настройки управления |
| `Game` | `IGameSettings` | Игровые настройки |

### Events

```csharp
event Action<bool> IsVisibleChanged;
event Action<SettingsTab> CurrentTabChanged;
```

### Методы

```csharp
void Apply();
void ResetToDefaults();
void LoadFromPlayerPrefs();
void SaveToPlayerPrefs();
```

### Sub-Interfaces

#### IAudioSettings

```csharp
public interface IAudioSettings
{
    float MasterVolume { get; set; }
    float MusicVolume { get; set; }
    float SfxVolume { get; set; }
    float VoiceVolume { get; set; }
    bool IsMuteAll { get; set; }
    bool IsMuteMusic { get; set; }
    bool IsMuteSfx { get; set; }
    
    event Action AudioSettingsChanged;
    void Reset();
}
```

#### IVideoSettings

```csharp
public interface IVideoSettings
{
    int ResolutionIndex { get; set; }
    QualityPreset Quality { get; set; }
    bool IsFullscreen { get; set; }
    bool IsVSync { get; set; }
    bool ShowFps { get; set; }
    TargetFpsOption TargetFps { get; set; }
    
    event Action VideoSettingsChanged;
    void Reset();
}
```

#### IDisplaySettings

```csharp
public interface IDisplaySettings
{
    float Brightness { get; set; }
    float Gamma { get; set; }
    float UiScale { get; set; }
    bool IsColorblindMode { get; set; }
    ColorblindType ColorblindType { get; set; }
    
    event Action DisplaySettingsChanged;
    void Reset();
}
```

---

## StandaloneWorkflowModel

**Путь:** `Models/StandaloneWorkflow/StandaloneWorkflowModel.cs`  
**Interface:** `IStandaloneWorkflowModel`

Состояние standalone workflow экрана.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `SelectedWorkflowId` | `string` | ID выбранного workflow |
| `AvailableWorkflows` | `List<WorkflowListItem>` | Список workflows |
| `IsLoading` | `bool` | Флаг загрузки |
| `StatusText` | `string` | Статус операции |
| `ErrorText` | `string` | Текст ошибки |
| `StorageType` | `StorageContextType` | Тип хранилища (User/System) |
| `SessionId` | `string` | ID сессии |

### Events

```csharp
event Action<string> SelectedWorkflowIdChanged;
event Action<List<WorkflowListItem>> AvailableWorkflowsChanged;
event Action<bool> IsLoadingChanged;
event Action<string> StatusTextChanged;
event Action<string> ErrorTextChanged;
event Action<StorageContextType> StorageTypeChanged;
event Action<string> SessionIdChanged;
```

---

## TrainingPanelModel

**Путь:** `Models/TrainingPanel/TrainingPanelModel.cs`  
**Interface:** `ITrainingPanelModel`

Состояние панели тренировки героев. Структура аналогична `HealingPanelModel`.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `Title` | `string` | Заголовок (readonly: "Улучшение героев") |
| `Rows` | `IReadOnlyList<ServiceRowData>` | Строки со слотами |
| `SelectedSlotId` | `string` | ID выбранного слота |

### Методы

Аналогичны `HealingPanelModel`.

---

## UIConstructorModel

**Путь:** `Models/UIConstructor/UIConstructorModel.cs`  
**Interface:** `IUIConstructorModel`

Состояние конструктора UI для генерации UI элементов через AI.

### Properties

| Property | Type | Описание |
|----------|------|----------|
| `SelectedElementType` | `UIElementType` | Тип элемента (Button, Icon, Card и т.д.) |
| `Blocks` | `List<UIPromptBlock>` | Блоки prompt-полей |
| `SelectedStylePreset` | `UIStylePreset` | Выбранный стиль (Minimal, Playful и т.д.) |
| `ImageSettings` | `ImageGenSettings` | Настройки генерации |
| `RemoveBackground` | `bool` | Удалять фон |
| `IsGenerating` | `bool` | Флаг генерации |
| `StatusText` | `string` | Статус операции |
| `PreviewImage` | `IImageHandle` | Превью изображения |
| `ProcessedImage` | `IImageHandle` | Обработанное изображение |

### Events

```csharp
event Action ElementTypeChanged;
event Action BlocksChanged;
event Action StylePresetChanged;
event Action ImageSettingsChanged;
event Action IsGeneratingChanged;
event Action StatusTextChanged;
event Action PreviewImageChanged;
```

### Методы

```csharp
void SetElementType(UIElementType type);
void ApplyStylePreset(UIStylePreset preset);
void UpdateBlock(string blockName, UIPromptBlock block);
void UpdateField(string blockName, string fieldPath, object value);
void ToggleBlockIncluded(string blockName, bool included);
void AddCustomField(string blockName, UIPromptField field);
void RemoveField(string blockName, string fieldPath);
string GetPreviewJson();
UIGenerationRequest BuildRequest();
void SetGenerating(bool value);
void SetStatus(string text);
void SetPreviewImage(IImageHandle image);
void SetProcessedImage(IImageHandle image);
```

---

## Best Practices

1. **Events**: Используй `event Action<T>` для уведомлений
2. **Readonly lists**: Возвращай `IReadOnlyList<T>` для коллекций
3. **Struct data**: Используй readonly structs для immutable data
4. **Dispose**: Очищай events в `Dispose()`
5. **IsValid**: Реализуй проверку валидности состояния
6. **Reset**: Возвращай Model к начальному состоянию

---

## См. также

- [ViewModels/README.md](../ViewModels/README.md) — ViewModels и биндинг к Models
- [Patterns.md](../Patterns.md) — Best practices
