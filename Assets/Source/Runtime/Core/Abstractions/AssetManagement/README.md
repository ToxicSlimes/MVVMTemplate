# AssetManagement Abstractions

Контракты для загрузки ассетов через Addressables.

## Типы

| Тип | Описание |
|-----|----------|
| `IAssetProvider` | Обёртка над Addressables для загрузки/инстанцирования ассетов |

## IAssetProvider API

```csharp
public interface IAssetProvider
{
    // === Load Single Asset ===
    UniTask<T> LoadAsync<T>(string address) where T : class;
    T LoadSync<T>(string address) where T : class;
    UniTask<T> LoadAsync<T>(AssetReference reference) where T : class;
    UniTask<T> LoadAsync<T>(AssetReferenceT<T> reference) where T : Object;

    // === Load Multiple Assets ===
    UniTask<IList<T>> LoadAllAsync<T>(string label) where T : class;
    UniTask<IList<T>> LoadAllAsync<T>(IEnumerable<string> keys) where T : class;

    // === Instantiate ===
    UniTask<GameObject> InstantiateAsync(string address, Transform parent = null);
    UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null);
    UniTask<TComponent> InstantiateAsync<TComponent>(string address, Transform parent = null) 
        where TComponent : Component;
    UniTask<TComponent> InstantiateAsync<TComponent>(AssetReferenceT<GameObject> reference, Transform parent = null) 
        where TComponent : Component;

    // === Release ===
    void Release<T>(T asset) where T : class;
    void ReleaseInstance(GameObject instance);
    void ReleaseAllByLabel(string label);

    // === Preload ===
    UniTask PreloadAsync<T>(string address) where T : class;
    UniTask PreloadAsync<T>(IEnumerable<string> addresses) where T : class;
}
```

## Использование

### Загрузка ассета

```csharp
// По адресу
var prefab = await _assetProvider.LoadAsync<GameObject>("Prefabs/Player");

// По AssetReference
[SerializeField] private AssetReference _playerRef;
var prefab = await _assetProvider.LoadAsync<GameObject>(_playerRef);

// Типизированная ссылка
[SerializeField] private AssetReferenceT<Sprite> _iconRef;
var icon = await _assetProvider.LoadAsync(_iconRef);

// Синхронная загрузка (только для критичных случаев!)
var config = _assetProvider.LoadSync<GameConfig>("Configs/GameConfig");
```

### Загрузка нескольких ассетов

```csharp
// По label
var enemies = await _assetProvider.LoadAllAsync<EnemyConfig>("EnemyConfigs");

// По нескольким ключам
var addresses = new[] { "Prefabs/Enemy1", "Prefabs/Enemy2", "Prefabs/Enemy3" };
var prefabs = await _assetProvider.LoadAllAsync<GameObject>(addresses);
```

### Инстанцирование

```csharp
// GameObject
var instance = await _assetProvider.InstantiateAsync("Prefabs/Enemy", _container);

// С получением компонента
var enemy = await _assetProvider.InstantiateAsync<EnemyController>("Prefabs/Enemy", _container);

// Из AssetReferenceT
[SerializeField] private AssetReferenceT<GameObject> _enemyRef;
var enemy = await _assetProvider.InstantiateAsync<EnemyController>(_enemyRef, _container);
```

### Освобождение ресурсов

```csharp
// Освободить загруженный ассет
_assetProvider.Release(prefab);

// Уничтожить инстанс и освободить ресурсы
_assetProvider.ReleaseInstance(instance);

// Освободить все ассеты по label
_assetProvider.ReleaseAllByLabel("EnemyConfigs");
```

### Предзагрузка

```csharp
// Предзагрузить один ассет
await _assetProvider.PreloadAsync<GameObject>("Prefabs/HeavyPrefab");

// Предзагрузить несколько
await _assetProvider.PreloadAsync<Sprite>(new[] { "Icons/Icon1", "Icons/Icon2" });
```

## Важно

- **Release обязателен** — не забывай освобождать ресурсы после использования
- **LoadSync** — использовать только в крайних случаях (блокирует main thread)
- **Preload** — используй для предзагрузки тяжёлых ассетов перед использованием

## Реализация

`Infrastructure/AssetManagement/AssetProvider.cs`
