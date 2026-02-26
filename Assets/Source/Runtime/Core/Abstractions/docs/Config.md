# Config — Конфигурации и атрибуты

> Система конфигураций на ScriptableObjects с Tri-Inspector.

## 📋 Обзор

| Класс/Интерфейс | Назначение | Путь |
|-----------------|------------|------|
| `ConfigBase` | Базовый класс конфигов | `Config/ConfigBase.cs` |
| `DataConfigBase` | Конфиги данных | `Config/DataConfigBase.cs` |
| `ServiceConfigBase` | Конфиги сервисов | `Config/ServiceConfigBase.cs` |
| `ViewConfigBase` | Конфиги UI | `Config/ViewConfigBase.cs` |
| `IConfigProvider` | Загрузчик конфигов | `Config/IConfigProvider.cs` |
| `ConfigEntryStyleAttribute` | Стилизация entries | `Attributes/` |

---

## ConfigBase

**Путь:** `Config/ConfigBase.cs`

```csharp
[DeclareBoxGroup("header", Title = "📋 Config Info")]
[DeclareTabGroup("tabs")]
public abstract class ConfigBase : ScriptableObject
{
    [Group("header")]
    [ShowInInspector, ReadOnly]
    [LabelText("Config Type")]
    protected virtual string ConfigTypeName => GetType().Name;

    [Group("header")]
    [ShowInInspector][ReadOnly]
    [LabelText("Asset Path")]
    protected string AssetPath { get; }
    
    protected virtual void OnValidate();
    protected virtual void ValidateConfig();
    
    // Editor buttons
    private void PingInProject();
    private void OpenScript();
}
```

### Назначение

Базовый класс для всех ScriptableObject конфигов. Предоставляет:
- Красивый Inspector с Tri-Inspector
- Стандартные editor кнопки
- Валидацию

### Атрибуты Tri-Inspector

```csharp
// Заголовок секции
[Title("Section Name")]

// Группировка полей
[Group("GroupName")]

// Табы
[Tab("TabName")]

// Обязательное поле
[Required]

// Условное отображение
[ShowIf("condition")]

// Информационный блок
[InfoBox("message")]

// Кнопка в инспекторе
[Button]

// Inline редактор для вложенных SO
[InlineEditor]

// Таблица для списков
[TableList]
```

---

## DataConfigBase

**Путь:** `Config/DataConfigBase.cs`

```csharp
[DeclareBoxGroup("data", Title = "📊 Data")]
[DeclareBoxGroup("references", Title = "🔗 References")]
[DeclareBoxGroup("validation", Title = "✅ Validation")]
public abstract class DataConfigBase : ConfigBase
{
    protected override string ConfigTypeName => $"📦 {base.ConfigTypeName}";

    // Editor buttons
    protected virtual void ReloadData();
    protected virtual void ValidateAll();
}
```

### Назначение

Для статических данных: таблицы, справочники, game data.

### Пример

```csharp
[CreateAssetMenu(fileName = "HeroesConfig", menuName = "Config/Heroes")]
public class HeroesConfig : DataConfigBase
{
    [Group("data")]
    [TableList]
    [SerializeField] private List<HeroData> _heroes;
    
    [Group("references")]
    [Required]
    [SerializeField] private Sprite _defaultAvatar;
    
    public IReadOnlyList<HeroData> Heroes => _heroes;
    
    protected override void ValidateConfig()
    {
        foreach (var hero in _heroes)
        {
            if (string.IsNullOrEmpty(hero.Id))
                Debug.LogError($"Hero has empty ID!");
        }
    }
}
```

---

## ServiceConfigBase

**Путь:** `Config/ServiceConfigBase.cs`

```csharp
[DeclareBoxGroup("service", Title = "⚙️ Service Settings")]
[DeclareBoxGroup("pool", Title = "🏊 Pool Settings")]
[DeclareBoxGroup("cache", Title = "💾 Cache Settings")]
[DeclareBoxGroup("network", Title = "🌐 Network Settings")]
[DeclareBoxGroup("debug", Title = "🐛 Debug")]
public abstract class ServiceConfigBase : ConfigBase
{
    protected override string ConfigTypeName => $"🔧 {base.ConfigTypeName}";
}
```

### Назначение

Для настроек сервисов: endpoints, timeouts, pool sizes.

### Пример

```csharp
[CreateAssetMenu(fileName = "NetworkConfig", menuName = "Config/Network")]
public class NetworkConfig : ServiceConfigBase
{
    [Group("network")]
    [SerializeField] private string _baseUrl = "https://api.example.com";
    
    [Group("network")]
    [Range(5, 60)]
    [SerializeField] private int _timeoutSeconds = 30;
    
    [Group("cache")]
    [Range(10, 1000)]
    [SerializeField] private int _maxCacheSize = 100;
    
    [Group("debug")]
    [SerializeField] private bool _logRequests;
    
    public string BaseUrl => _baseUrl;
    public int TimeoutSeconds => _timeoutSeconds;
    public int MaxCacheSize => _maxCacheSize;
    public bool LogRequests => _logRequests;
}
```

---

## ViewConfigBase

**Путь:** `Config/ViewConfigBase.cs`

```csharp
[DeclareBoxGroup("prefabs", Title = "🎨 Prefabs")]
[DeclareBoxGroup("styles", Title = "🖌️ Styles")]
[DeclareBoxGroup("animations", Title = "✨ Animations")]
[DeclareBoxGroup("sounds", Title = "🔊 Sounds")]
public abstract class ViewConfigBase : ConfigBase
{
    protected override string ConfigTypeName => $"🖼️ {base.ConfigTypeName}";

    // Editor button
    protected virtual void FindMissingReferences();
}
```

### Назначение

Для UI конфигов: prefabs, стили, анимации.

### Пример

```csharp
[CreateAssetMenu(fileName = "PopupConfig", menuName = "Config/UI/Popup")]
public class PopupConfig : ViewConfigBase
{
    [Group("prefabs")]
    [Required]
    [SerializeField] private GameObject _tooltipPrefab;
    
    [Group("prefabs")]
    [Required]
    [SerializeField] private GameObject _confirmDialogPrefab;
    
    [Group("animations")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _showDuration = 0.2f;
    
    [Group("sounds")]
    [SerializeField] private AudioClip _showSound;
    
    public GameObject TooltipPrefab => _tooltipPrefab;
    public GameObject ConfirmDialogPrefab => _confirmDialogPrefab;
    public float ShowDuration => _showDuration;
    public AudioClip ShowSound => _showSound;
}
```

---

## IConfigProvider

**Путь:** `Config/IConfigProvider.cs`

```csharp
public interface IConfigProvider
{
    UniTask<T> GetConfigAsync<T>() where T : ConfigBase;
    T GetConfig<T>() where T : ConfigBase;
    bool IsConfigLoaded<T>() where T : ConfigBase;
    UniTask PreloadAllConfigsAsync();
    void ReleaseConfig<T>() where T : ConfigBase;
}
```

### Назначение

Загрузка конфигов через Addressables с кэшированием.

### Реализация

**Путь:** `Core/Infrastructure/Services/Config/ConfigProvider.cs`

### Пример использования

```csharp
public class ChatService : IChatService
{
    private readonly IConfigProvider _configProvider;
    private NetworkConfig _networkConfig;
    
    public async UniTask InitializeAsync()
    {
        _networkConfig = await _configProvider.GetConfigAsync<NetworkConfig>();
    }
    
    public async UniTask<ChatModel> GetChatAsync(string id)
    {
        var url = $"{_networkConfig.BaseUrl}/chats/{id}";
        // ...
    }
}
```

---

## ConfigEntryStyleAttribute

**Путь:** `Attributes/ConfigEntryStyleAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class ConfigEntryStyleAttribute : Attribute
{
    public string IconEmoji { get; set; }
    public ColorPreset ColorPreset { get; set; } = ColorPreset.Blue;
    public float BorderWidth { get; set; } = 4f;
    public bool ShowStatusIcon { get; set; } = true;
    public string MissingRefFieldName { get; set; }
}

public enum ColorPreset
{
    Blue,
    Green,
    Orange,
    Purple,
    Red,
    Pink,
    Cyan,
    Yellow
}
```

### Назначение

Стилизация entries в TableList для красивого отображения.

### Пример

```csharp
[ConfigEntryStyle(ColorPreset.Green, "⚔️")]
public struct HeroData
{
    public string Id;
    public string Name;
    public int Level;
}

[ConfigEntryStyle(ColorPreset.Purple, "🎨", MissingRefFieldName = "Sprite")]
public struct ItemData
{
    public string Id;
    public string Name;
    public Sprite Sprite; // Подсветит красным если null
}
```

### ColorPresets

```csharp
public static class ColorPresets
{
    public static (Color background, Color border) GetColors(ColorPreset preset, bool isMissing)
    {
        if (isMissing)
            return (Red with alpha, Red);
            
        return preset switch
        {
            ColorPreset.Blue => (Blue with alpha, Blue),
            ColorPreset.Green => (Green with alpha, Green),
            ColorPreset.Orange => (Orange with alpha, Orange),
            ColorPreset.Purple => (Purple with alpha, Purple),
            ColorPreset.Red => (Red with alpha, Red),
            ColorPreset.Pink => (Pink with alpha, Pink),
            ColorPreset.Cyan => (Cyan with alpha, Cyan),
            ColorPreset.Yellow => (Yellow with alpha, Yellow),
            _ => (Blue with alpha, Blue)
        };
    }
}
```

---

## IAssetProvider

**Путь:** `AssetManagement/IAssetProvider.cs`

```csharp
public interface IAssetProvider
{
    // Load
    UniTask<T> LoadAsync<T>(string address) where T : class;
    T LoadSync<T>(string address) where T : class;
    UniTask<IList<T>> LoadAllAsync<T>(string label) where T : class;
    UniTask<IList<T>> LoadAllAsync<T>(IEnumerable<string> keys) where T : class;
    UniTask<T> LoadAsync<T>(AssetReference reference) where T : class;
    UniTask<T> LoadAsync<T>(AssetReferenceT<T> reference) where T : Object;
    
    // Instantiate
    UniTask<GameObject> InstantiateAsync(string address, Transform parent = null);
    UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null);
    UniTask<TComponent> InstantiateAsync<TComponent>(string address, Transform parent = null) 
        where TComponent : Component;
    UniTask<TComponent> InstantiateAsync<TComponent>(AssetReferenceT<GameObject> reference, Transform parent = null) 
        where TComponent : Component;
    
    // Release
    void Release<T>(T asset) where T : class;
    void ReleaseInstance(GameObject instance);
    void ReleaseAllByLabel(string label);
    
    // Preload
    UniTask PreloadAsync<T>(string address) where T : class;
    UniTask PreloadAsync<T>(IEnumerable<string> addresses) where T : class;
}
```

### Назначение

Абстракция над Addressables. Загрузка, инстанциация, release.

### Пример использования

```csharp
public class ViewService : IViewService
{
    private readonly IAssetProvider _assetProvider;
    
    public async UniTask<IView> ShowAsync<TViewModel>()
    {
        var address = _viewConfig.GetViewAddress<TViewModel>();
        var view = await _assetProvider.InstantiateAsync<IView>(address, _canvasTransform);
        // ...
        return view;
    }
    
    public async UniTask HideAsync(IView view)
    {
        await view.HideAsync();
        _assetProvider.ReleaseInstance(view.gameObject);
    }
}
```

---

## ITooltipTarget

**Путь:** `UI/ITooltipTarget.cs`

```csharp
public interface ITooltipTarget
{
    void SetTooltipData(string title, string description = null);
}
```

### Назначение

Маркер для элементов, поддерживающих tooltip.

### Пример

```csharp
public class HeroCard : BindableElement<HeroCardData>, ITooltipTarget
{
    private PopupTrigger _popupTrigger;
    
    public void SetTooltipData(string title, string description = null)
    {
        _popupTrigger.SetTooltipData(title, description);
    }
    
    protected override void OnBind(HeroCardData data)
    {
        SetTooltipData(data.Name, $"Level {data.Level}");
    }
}
```

---

## 🎯 Best Practices

1. **Один конфиг = одна ответственность**
2. **Используй группы** для организации полей
3. **Добавляй валидацию** в `ValidateConfig()`
4. **Required для обязательных** ссылок
5. **Preload конфиги** при старте приложения
6. **Release после использования** через IAssetProvider
