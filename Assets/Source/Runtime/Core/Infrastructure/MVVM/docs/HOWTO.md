# MVVM Infrastructure — HOWTO

*Обновлено: 2026-02-11*

---

## Как зарегистрировать View в ViewService

### 1. Создай ViewModel

```csharp
public class MyPanelViewModel : DisposableObject
{
    public ReactiveProperty<string> Title { get; } = new("Default");

    public MyPanelViewModel(IMediator mediator) { }
}
```

### 2. Создай View (BindableView)

```csharp
public class MyPanelView : BindableView<MyPanelViewModel>
{
    [SerializeField] private TMP_Text _title;

    protected override void OnBind(ref DisposableBag disposables)
    {
        ViewModel.Title
            .Subscribe(_title, static (t, text) => text.text = t)
            .AddTo(ref disposables);
    }
}
```

### 3. Настрой ViewConfig (ScriptableObject)

В `ViewConfig` ScriptableObject добавь маппинг:
- **ViewModel Type:** `MyPanelViewModel`
- **View Prefab Address:** Addressable адрес prefab'а с `MyPanelView`

`ViewConfig` использует `ViewMappingBase` для связи типа ViewModel → адрес prefab.

### 4. Зарегистрируй ViewModel в DI

```csharp
// В Installer
builder.AddTransient<MyPanelViewModel>();
```

### 5. Покажи View

```csharp
var view = await _viewService.ShowAsync<MyPanelView, MyPanelViewModel>();
```

С параметрами:
```csharp
var view = await _viewService.ShowAsync<MyPanelView, MyPanelViewModel>(
    new ViewShowParams()
        .SetDisplayMode(ViewDisplayMode.Additive)
        .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.Scale)
        .SetCanvas(CanvasLayer.Overlay)
);
```

---

## Как настроить Canvas

### Регистрация Canvas в сцене

В bootstrap MonoBehaviour (или SceneScope):

```csharp
[SerializeField] private Canvas _mainCanvas;
[SerializeField] private Canvas _overlayCanvas;

void Start()
{
    _canvasProvider.RegisterCanvas(CanvasLayer.Main, _mainCanvas);
    _canvasProvider.RegisterCanvas(CanvasLayer.Overlay, _overlayCanvas);
}

void OnDestroy()
{
    _canvasProvider.UnregisterCanvas(CanvasLayer.Main);
    _canvasProvider.UnregisterCanvas(CanvasLayer.Overlay);
}
```

### Использование Canvas в ViewFactory

`ViewFactory` автоматически резолвит parent через `ICanvasProvider`:
- Если `ViewShowParams` указывает `CanvasLayer` — View создаётся в соответствующем Canvas
- Если не указан — используется `_uiRoot` (автоматически создаётся ViewFactory)

### Получение Canvas Transform

```csharp
var mainTransform = _canvasProvider.GetCanvasTransform(CanvasLayer.Main);
```

---

## Как создать UIElement через Factory

### Простой элемент (UIElementFactory)

`UIElementFactory` — базовая фабрика с DI инъекцией:

```csharp
// Instantiate prefab с автоматической DI инъекцией
var instance = _uiElementFactory.Instantiate(prefab, parentTransform);

// Generic версия
var button = _uiElementFactory.Instantiate<MyButton>(buttonPrefab, container);
```

### Bindable элемент (UIElementService)

`UIElementService` — полноценная система с биндингом данных:

```csharp
// Создание с данными
var element = await _elementService.CreateElementAsync<HeroSlot, HeroSlotData>(
    container: _slotsParent,
    data: heroData,
    onClick: data => OnHeroSelected(data)
);
```

### Reconciliation списка

Обновление списка без пересоздания:

```csharp
// UIElementService сравнивает текущие элементы с новыми данными
await _elementService.ReconcileElementsAsync<HeroSlot, HeroSlotData>(
    container: _slotsParent,
    newData: updatedHeroes
);
```

### Настройка ElementConfig

В `ElementConfig` ScriptableObject:
- **Element Type:** `HeroSlot`
- **Prefab Address:** Addressable адрес prefab'а

`ElementConfig` использует `ElementMappingBase` для маппинга Type → Address.

---

## Как зарегистрировать Embedded Panel

`EmbeddedPanelService` управляет панелями, уже размещёнными в сцене (не динамическими):

```csharp
// В сцене — MonoBehaviour регистрирует панель
_embeddedPanelService.RegisterPanel<MyPanelViewModel>(myPanelView);

// Показать
await _embeddedPanelService.ShowAsync<MyPanelViewModel>(ct);

// Скрыть
await _embeddedPanelService.HideAsync<MyPanelViewModel>(ct);
```

Панель при регистрации автоматически деактивируется (`SetActive(false)`), ViewModel резолвится из контейнера и биндится.

---

## Как добавить анимацию View

`ViewAnimator` (LitMotion) автоматически анимирует show/hide. Настраивается через `ViewShowParams`:

```csharp
new ViewShowParams()
    .SetTransition(ViewTransitionStyle.Fade)        // Только fade
    .SetTransition(ViewTransitionStyle.Scale)        // Только scale
    .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.Scale)  // Оба
```
