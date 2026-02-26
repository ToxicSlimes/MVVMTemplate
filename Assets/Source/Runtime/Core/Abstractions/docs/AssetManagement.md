# AssetManagement — Загрузка ассетов

> Абстракция над Addressables для загрузки, инстанциации и управления ресурсами.

## 📋 Обзор интерфейсов

| Интерфейс | Назначение | Путь |
|-----------|------------|------|
| `IAssetProvider` | Загрузчик ассетов | `AssetManagement/IAssetProvider.cs` |

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

Единый интерфейс для работы с Addressables:
- **Загрузка** ассетов (async/sync)
- **Инстанциация** prefabs с parent
- **Release** для освобождения памяти
- **Preload** для предзагрузки при старте

### Реализация

**Путь:** `Core/Infrastructure/Services/AssetManagement/AssetProvider.cs`

---

## 📖 Примеры использования

### Загрузка ScriptableObject

```csharp
public class ConfigLoader
{
    private readonly IAssetProvider _assetProvider;
    
    public async UniTask<GameConfig> LoadConfigAsync()
    {
        var config = await _assetProvider.LoadAsync<GameConfig>("Configs/GameConfig");
        return config;
    }
    
    public void ReleaseConfig(GameConfig config)
    {
        _assetProvider.Release(config);
    }
}
```

### Загрузка по Label

```csharp
// Загрузить все префабы героев
var heroPrefabs = await _assetProvider.LoadAllAsync<GameObject>("Heroes");

// Загрузить по списку ключей
var keys = new[] { "Hero_Warrior", "Hero_Mage", "Hero_Archer" };
var heroes = await _assetProvider.LoadAllAsync<GameObject>(keys);

// Освободить все по label
_assetProvider.ReleaseAllByLabel("Heroes");
```

### Инстанциация Prefabs

```csharp
public class ViewFactory : IViewFactory
{
    private readonly IAssetProvider _assetProvider;
    
    public async UniTask<LoginView> CreateLoginViewAsync(Transform parent)
    {
        // Инстанцировать по адресу
        var view = await _assetProvider.InstantiateAsync<LoginView>(
            "UI/Views/LoginView",
            parent
        );
        
        return view;
    }
    
    public void DestroyView(GameObject viewObject)
    {
        _assetProvider.ReleaseInstance(viewObject);
    }
}
```

### Использование AssetReference

```csharp
[Serializable]
public class ViewConfig
{
    [SerializeField] private AssetReferenceT<GameObject> _viewPrefab;
    
    public async UniTask<GameObject> InstantiateAsync(
        IAssetProvider assetProvider, 
        Transform parent)
    {
        return await assetProvider.InstantiateAsync(_viewPrefab, parent);
    }
}
```

### Preload при старте

```csharp
public class DataPreloadService : IDataPreloadService
{
    private readonly IAssetProvider _assetProvider;
    
    public async UniTask PreloadDataAsync(
        IProgress<float> progress = null, 
        CancellationToken cancellationToken = default)
    {
        // Предзагрузить критичные конфиги
        await _assetProvider.PreloadAsync<ConfigBase>(new[]
        {
            "Configs/GameConfig",
            "Configs/NetworkConfig",
            "Configs/AudioConfig"
        });
        
        // Предзагрузить View prefabs
        await _assetProvider.PreloadAsync<GameObject>(new[]
        {
            "UI/Views/LoginView",
            "UI/Views/MenuView"
        });
        
        progress?.Report(1f);
    }
}
```

### Синхронная загрузка (для Editor)

```csharp
#if UNITY_EDITOR
public T LoadConfigSync<T>() where T : ConfigBase
{
    return _assetProvider.LoadSync<T>($"Configs/{typeof(T).Name}");
}
#endif
```

---

## 🔄 Lifecycle ассетов

```
┌─────────────────────────────────────────────────────┐
│                  Load/Instantiate                    │
├─────────────────────────────────────────────────────┤
│                                                      │
│  LoadAsync("address")                                │
│         │                                            │
│         ▼                                            │
│  Addressables.LoadAssetAsync()                       │
│         │                                            │
│         ▼                                            │
│  ✅ Asset loaded and cached                          │
│                                                      │
│  InstantiateAsync("address", parent)                 │
│         │                                            │
│         ▼                                            │
│  LoadAsync<GameObject>("address")                    │
│         │                                            │
│         ▼                                            │
│  Instantiate(prefab, parent)                         │
│         │                                            │
│         ▼                                            │
│  ✅ Instance created                                 │
│                                                      │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                    Release                           │
├─────────────────────────────────────────────────────┤
│                                                      │
│  Release(asset)                                      │
│         │                                            │
│         ▼                                            │
│  Addressables.Release(asset)                         │
│         │                                            │
│         ▼                                            │
│  ✅ Reference count decreased                        │
│     (asset unloaded if refs = 0)                     │
│                                                      │
│  ReleaseInstance(gameObject)                         │
│         │                                            │
│         ▼                                            │
│  Destroy(gameObject)                                 │
│         │                                            │
│         ▼                                            │
│  Addressables.ReleaseInstance()                      │
│         │                                            │
│         ▼                                            │
│  ✅ Instance destroyed, prefab ref decreased         │
│                                                      │
└─────────────────────────────────────────────────────┘
```

---

## ⚠️ Важные замечания

### 1. **Всегда Release после использования**

```csharp
// ❌ BAD — утечка памяти
var config = await _assetProvider.LoadAsync<GameConfig>("Configs/GameConfig");
// config используется, но никогда не освобождается

// ✅ GOOD
var config = await _assetProvider.LoadAsync<GameConfig>("Configs/GameConfig");
try
{
    // use config
}
finally
{
    _assetProvider.Release(config);
}
```

### 2. **Используй ReleaseInstance для GameObject**

```csharp
// ❌ BAD
var obj = await _assetProvider.InstantiateAsync("Prefabs/Enemy");
Destroy(obj); // Addressables ссылка не освобождена!

// ✅ GOOD
var obj = await _assetProvider.InstantiateAsync("Prefabs/Enemy");
_assetProvider.ReleaseInstance(obj);
```

### 3. **Preload критичных ассетов**

```csharp
// При старте приложения
await _assetProvider.PreloadAsync<GameObject>(new[]
{
    "UI/Views/LoginView",
    "UI/Views/LoadingView",
    "UI/Elements/LoadingSpinner"
});

// Теперь InstantiateAsync будет мгновенным (ассет уже в памяти)
var view = await _assetProvider.InstantiateAsync("UI/Views/LoginView", parent);
```

### 4. **ReleaseAllByLabel для массовой очистки**

```csharp
// Загрузили все ассеты уровня
var levelAssets = await _assetProvider.LoadAllAsync<GameObject>("Level_1");

// После завершения уровня — освободить все разом
_assetProvider.ReleaseAllByLabel("Level_1");
```

---

## 🎯 Best Practices

1. **Используй label** для группировки связанных ассетов
2. **Preload** UI и критичные ассеты при старте
3. **Release** сразу после использования или в Dispose()
4. **Избегай LoadSync** в runtime (только для Editor tools)
5. **Кэширование** — IAssetProvider уже кэширует, не дублируй
6. **Cancellation** — передавай CancellationToken для длительных операций

---

## 🔗 Связанные интерфейсы

- `IConfigProvider` — использует IAssetProvider для загрузки конфигов
- `IViewFactory` — использует для инстанциации View prefabs
- `IPopupService` — использует для Popup prefabs
- `IUiComponentFactory` — использует для ProceduralUI prefabs
