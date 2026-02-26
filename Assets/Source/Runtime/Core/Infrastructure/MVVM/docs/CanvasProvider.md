# CanvasProvider

Сервис управления Canvas слоями для MVVM системы.

---

## Назначение

`CanvasProvider` отвечает за:
- Регистрацию Canvas по слоям (CanvasLayer)
- Получение Canvas и Transform для размещения View
- Thread-safe управление маппингом слоёв

---

## Интерфейс ICanvasProvider

```csharp
public interface ICanvasProvider
{
    UnityEngine.Canvas GetCanvas(CanvasLayer layer);
    Transform GetCanvasTransform(CanvasLayer layer);
    void RegisterCanvas(CanvasLayer layer, UnityEngine.Canvas canvas);
    void UnregisterCanvas(CanvasLayer layer);
    bool HasCanvas(CanvasLayer layer);
}
```

---

## CanvasLayer Enum

```csharp
public enum CanvasLayer
{
    World,    // 3D UI в мире (Screen Space - Camera)
    Screen,   // Основной UI (Screen Space - Overlay)
    Overlay,  // Поверх всего — диалоги, попапы (высокий sorting order)
    System    // Системные сообщения, дебаг (самый верхний)
}
```

### Визуализация слоёв

```
┌──────────────────────────────────────────────┐
│             System Layer (Order 1000)        │ ← Debug overlay
├──────────────────────────────────────────────┤
│             Overlay Layer (Order 100)        │ ← Dialogs, popups
├──────────────────────────────────────────────┤
│             Screen Layer (Order 0)           │ ← Main UI
├──────────────────────────────────────────────┤
│             World Layer (Camera space)       │ ← 3D UI
└──────────────────────────────────────────────┘
```

---

## Реализация

```csharp
public class CanvasProvider : ICanvasProvider
{
    private readonly Dictionary<CanvasLayer, UnityEngine.Canvas> _canvases = new();
    private readonly object _lock = new();

    public UnityEngine.Canvas GetCanvas(CanvasLayer layer)
    {
        lock (_lock)
        {
            return _canvases.TryGetValue(layer, out var canvas) ? canvas : null;
        }
    }

    public Transform GetCanvasTransform(CanvasLayer layer)
    {
        var canvas = GetCanvas(layer);
        return canvas != null ? canvas.transform : null;
    }

    public void RegisterCanvas(CanvasLayer layer, UnityEngine.Canvas canvas)
    {
        if (canvas == null)
            throw new ArgumentNullException(nameof(canvas), 
                $"Cannot register null canvas for layer {layer}");

        lock (_lock)
        {
            if (_canvases.ContainsKey(layer))
            {
                Debug.LogWarning($"Canvas for layer {layer} is already registered. Overwriting.");
            }

            _canvases[layer] = canvas;
            Debug.Log($"Canvas registered for layer {layer}: {canvas.name}");
        }
    }

    public void UnregisterCanvas(CanvasLayer layer)
    {
        lock (_lock)
        {
            if (_canvases.Remove(layer))
                Debug.Log($"Canvas unregistered for layer {layer}");
            else
                Debug.LogWarning($"No canvas registered for layer {layer}");
        }
    }

    public bool HasCanvas(CanvasLayer layer)
    {
        lock (_lock)
        {
            return _canvases.ContainsKey(layer);
        }
    }
}
```

---

## Настройка Canvas иерархии

### В сцене

Создай GameObject'ы для каждого слоя:

```
[UI Root]
├── WorldCanvas      (Canvas, CanvasLayer.World)
│   └── [3D UI элементы]
├── ScreenCanvas     (Canvas, CanvasLayer.Screen)
│   └── [Основной UI]
├── OverlayCanvas    (Canvas, CanvasLayer.Overlay)
│   └── [Диалоги, попапы]
└── SystemCanvas     (Canvas, CanvasLayer.System)
    └── [Debug UI]
```

### MonoBehaviour регистратор

```csharp
public class CanvasLayerRegistry : MonoBehaviour
{
    [SerializeField] private UnityEngine.Canvas _worldCanvas;
    [SerializeField] private UnityEngine.Canvas _screenCanvas;
    [SerializeField] private UnityEngine.Canvas _overlayCanvas;
    [SerializeField] private UnityEngine.Canvas _systemCanvas;

    [Inject] private ICanvasProvider _canvasProvider;

    private void Start()
    {
        if (_worldCanvas != null)
            _canvasProvider.RegisterCanvas(CanvasLayer.World, _worldCanvas);
        
        if (_screenCanvas != null)
            _canvasProvider.RegisterCanvas(CanvasLayer.Screen, _screenCanvas);
        
        if (_overlayCanvas != null)
            _canvasProvider.RegisterCanvas(CanvasLayer.Overlay, _overlayCanvas);
        
        if (_systemCanvas != null)
            _canvasProvider.RegisterCanvas(CanvasLayer.System, _systemCanvas);
    }

    private void OnDestroy()
    {
        _canvasProvider.UnregisterCanvas(CanvasLayer.World);
        _canvasProvider.UnregisterCanvas(CanvasLayer.Screen);
        _canvasProvider.UnregisterCanvas(CanvasLayer.Overlay);
        _canvasProvider.UnregisterCanvas(CanvasLayer.System);
    }
}
```

---

## Использование

### В ViewShowParams

```csharp
// Показать View на Overlay слое
await _viewService.ShowAsync<DialogView, DialogViewModel>(
    new ViewShowParams()
        .SetCanvas(CanvasLayer.Overlay)
);
```

### В ViewFactory

```csharp
private Transform ResolveParent(ViewShowParams showParams)
{
    // Проверка CanvasLayer
    if (showParams.TargetCanvas.HasValue && _canvasProvider != null)
    {
        var canvasTransform = _canvasProvider.GetCanvasTransform(
            showParams.TargetCanvas.Value);
        
        if (canvasTransform != null)
            return canvasTransform;
    }

    return _uiRoot; // Fallback
}
```

### Программная регистрация

```csharp
public class UIBootstrap : MonoBehaviour
{
    [Inject] private ICanvasProvider _canvasProvider;

    private void Awake()
    {
        CreateAndRegisterCanvases();
    }

    private void CreateAndRegisterCanvases()
    {
        var screenCanvas = CreateCanvas("ScreenCanvas", 0);
        _canvasProvider.RegisterCanvas(CanvasLayer.Screen, screenCanvas);

        var overlayCanvas = CreateCanvas("OverlayCanvas", 100);
        _canvasProvider.RegisterCanvas(CanvasLayer.Overlay, overlayCanvas);
    }

    private UnityEngine.Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name);
        DontDestroyOnLoad(go);

        var canvas = go.AddComponent<UnityEngine.Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        go.AddComponent<UnityEngine.UI.CanvasScaler>();
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        return canvas;
    }
}
```

---

## Thread Safety

CanvasProvider использует `lock` для thread-safe операций:

```csharp
private readonly object _lock = new();

lock (_lock)
{
    // Safe dictionary access
}
```

---

## Рекомендуемые sorting orders

| Layer | Sorting Order | Использование |
|-------|---------------|---------------|
| World | -100 | 3D UI элементы в мире |
| Screen | 0 | Основной игровой UI |
| Overlay | 100 | Диалоги, уведомления |
| System | 1000 | Debug overlay, FPS counter |

---

## Регистрация в DI

```csharp
// В InfrastructureBindings
container.AddSingleton<ICanvasProvider, CanvasProvider>();
```

---

## Best Practices

### ✅ DO

1. **Регистрируй Canvas в Start** через MonoBehaviour
2. **Используй DontDestroyOnLoad** для persistent Canvas
3. **Unregister в OnDestroy** для избежания dangling references
4. **Используй CanvasLayer.Overlay** для модальных окон

### ❌ DON'T

1. **Не регистрируй null Canvas**
2. **Не забывай Unregister** при уничтожении Canvas
3. **Не используй FindObjectOfType** — кешируй ссылки

---

## См. также

- [ViewFactory.md](ViewFactory.md) — использование CanvasProvider
- [ViewService.md](ViewService.md) — показ View на слоях
- [README.md](README.md) — общая архитектура
