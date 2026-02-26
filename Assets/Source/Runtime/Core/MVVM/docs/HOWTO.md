# MVVM — HOWTO (Рецепты)

*Обновлено: 2026-02-11*

---

## 1. Как создать новый View + ViewModel

### Шаг 1: Model (интерфейс + реализация)

```csharp
// Abstractions/MVVM/Models/MyFeature/IMyFeatureModel.cs
public interface IMyFeatureModel : IModel
{
    event Action<string> TitleChanged;
    string Title { get; set; }
}

// MVVM/Models/MyFeature/MyFeatureModel.cs
public class MyFeatureModel : IMyFeatureModel
{
    public event Action<string> TitleChanged;
    private string _title;

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            TitleChanged?.Invoke(value);
        }
    }

    public bool IsValid => true;
    public void Reset() => Title = null;
    public void Dispose() => TitleChanged = null;
}
```

### Шаг 2: ViewModel

```csharp
// MVVM/ViewModels/MyFeature/MyFeatureViewModel.cs
[GenerateViewCommands]
public class MyFeatureViewModel : DisposableObject
{
    private readonly IMyFeatureModel _model;
    private readonly IMediator _mediator;

    public ReactiveProperty<string> Title { get; }
    public ReactiveCommand<Unit> CloseCommand { get; set; }

    public MyFeatureViewModel(IMyFeatureModel model, IMediator mediator)
    {
        _model = model;
        _mediator = mediator;
        Title = new ReactiveProperty<string>(model.Title);

        // Model → ViewModel sync
        _model.TitleChanged += v => Title.Value = v;

        // ViewModel → Model sync
        Title.Subscribe(this, static (v, s) => s._model.Title = v)
            .AddTo(ref Disposables);

        CloseCommand = new ReactiveCommand<Unit>(_ => Close());
        CloseCommand.AddTo(ref Disposables);
    }

    private async void Close()
    {
        await _mediator.Send(new HideViewRequest<MyFeatureView, MyFeatureViewModel>());
    }

    public override void Dispose()
    {
        _model.TitleChanged -= /* handler */;
        _model.Dispose();
        base.Dispose();
    }
}
```

### Шаг 3: View

```csharp
// MVVM/Views/MyFeature/MyFeatureView.cs
public class MyFeatureView : BindableView<MyFeatureViewModel>
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Button _closeButton;

    protected override void OnBind(ref DisposableBag disposables)
    {
        ViewModel.Title
            .Subscribe(_titleText, static (v, txt) => txt.text = v)
            .AddTo(ref disposables);

        _closeButton.OnClickAsObservable()
            .Subscribe(ViewModel.CloseCommand, static (_, cmd) => cmd.Execute(Unit.Default))
            .AddTo(ref disposables);
    }
}
```

### Шаг 4: Регистрация в DI

Зарегистрируй `IMyFeatureModel → MyFeatureModel` и `MyFeatureViewModel` в DI-контейнере (Reflex).

---

## 2. Как привязать ReactiveProperty к UI

### Slider ↔ ReactiveProperty<float>

```csharp
// ViewModel → UI
property.Subscribe((slider, valueText), static (v, state) =>
{
    state.slider.value = v;
    if (state.valueText != null)
        state.valueText.text = $"{(int)(v * 100)}%";
}).AddTo(ref disposables);

// UI → ViewModel
slider.onValueChanged.AsObservable()
    .Subscribe(property, static (v, prop) => prop.Value = v)
    .AddTo(ref disposables);
```

### Toggle ↔ ReactiveProperty<bool>

```csharp
property.Subscribe(toggle, static (v, t) => t.isOn = v)
    .AddTo(ref disposables);

toggle.onValueChanged.AsObservable()
    .Subscribe(property, static (v, prop) => prop.Value = v)
    .AddTo(ref disposables);
```

### Button → ReactiveCommand

```csharp
button.OnClickAsObservable()
    .Subscribe(viewModel.SomeCommand, static (_, cmd) => cmd.Execute(Unit.Default))
    .AddTo(ref disposables);
```

### Dropdown ↔ ReactiveProperty<int> / enum

```csharp
// Для enum:
property.Subscribe(dropdown, static (v, dd) => dd.value = (int)v)
    .AddTo(ref disposables);

dropdown.onValueChanged.AsObservable()
    .Subscribe(property, static (v, prop) => prop.Value = (MyEnum)v)
    .AddTo(ref disposables);
```

---

## 3. Как создать Element (poolable UI component)

```csharp
// Data struct
public struct MyItemData : IElementData
{
    public string Id { get; set; }
    public string Name;
    public Sprite Icon;
    public bool IsLocked;
}

// Element
public class MyItemElement : BindableElement<MyItemData>
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private GameObject _lockOverlay;

    protected override void OnBind(MyItemData data)
    {
        if (_icon != null) _icon.sprite = data.Icon;
        if (_name != null) _name.text = data.Name;
        if (_lockOverlay != null) _lockOverlay.SetActive(data.IsLocked);
    }

    protected override void OnUnbind()
    {
        if (_icon != null) _icon.sprite = null;
        if (_name != null) _name.text = string.Empty;
        if (_lockOverlay != null) _lockOverlay.SetActive(false);
    }

    protected override IPopupData GetPopupData(MyItemData data)
    {
        return new TooltipData(data.Name, data.IsLocked ? "Заблокировано" : "");
    }
}
```

---

## 4. Как использовать готовый Element во View

Когда Element уже создан, можно использовать его напрямую через SerializeField и вызвать `.Bind(data)`:

```csharp
// View.cs
using HeroSlotElementData = Core.Data.Domain.Models.Elements.HeroSlotData;

public class HeroDetailPanelView : BindableView<HeroDetailPanelViewModel>
{
    [SerializeField] private HeroSlotElement _heroSlotElement;

    protected override void OnBind(ref DisposableBag disposables)
    {
        ViewModel.HeroData
            .Subscribe(this, static (data, self) =>
            {
                if (self._heroSlotElement != null && !string.IsNullOrEmpty(data.Id))
                {
                    var slotData = new HeroSlotElementData(
                        heroId: data.Id,
                        name: data.Name,
                        portrait: null,
                        level: data.Level,
                        healthPercent: 1f);

                    self._heroSlotElement.Bind(slotData);
                }
            })
            .AddTo(ref disposables);
    }
}
```

**Ключевые моменты:**
- Element уже на сцене (SerializeField) — НЕ создаётся через Mediator
- Вызов `.Bind(data)` обновляет визуал
- Using alias для краткости: `using HeroSlotElementData = Core.Data.Domain.Models.Elements.HeroSlotData`

---

## 5. Как настроить навигацию к новому View

### Через State Machine (глобальная навигация)

1. Создай `State` в `Presentation/States/`:
   ```csharp
   public class MyFeatureState : GameStateBase<MyFeatureState>, IEnterState
   {
       private readonly IViewService _viewService;
       public MyFeatureState(IViewService viewService) => _viewService = viewService;
       public void Enter() => _viewService.ShowAsync<MyFeatureViewModel>().Forget();
       public override void Exit() { }
   }
   ```

2. Создай navigation target и handler в `Presentation/Handlers/`:
   ```csharp
   public struct MyFeatureNavigation : INavigationTarget { }

   public sealed class NavigateToMyFeatureHandler
       : NavigateHandlerBase<MyFeatureNavigation, MyFeatureState>
   {
       public NavigateToMyFeatureHandler(IGameStateMachine sm) : base(sm) { }
   }
   ```

3. Из ViewModel инициируй навигацию:
   ```csharp
   await _mediator.Send(new NavigateCommand<MyFeatureNavigation>());
   ```

### Внутри View (показ/скрытие без смены state)

```csharp
await _mediator.Send(new ShowViewRequest<MyPanelView, MyPanelViewModel>());
await _mediator.Send(new HideViewRequest<MyPanelView, MyPanelViewModel>());
```

---

## 6. Lifecycle

```
┌─────────────────────────────────────────────────┐
│                  View Lifecycle                  │
├─────────────────────────────────────────────────┤
│ 1. DI создаёт ViewModel (конструктор)           │
│ 2. ViewService находит View (MonoBehaviour)      │
│ 3. view.BindTo(viewModel)                       │
│    └─ OnBind(ref disposables) — подписки        │
│ 4. view.ShowAsync()                             │
│    ├─ OnBeforeShow()                            │
│    ├─ PlayShowAnimation()                       │
│    └─ OnAfterShow()                             │
│ 5. ... пользователь взаимодействует ...         │
│ 6. view.HideAsync()                             │
│    ├─ OnBeforeHide()                            │
│    ├─ PlayHideAnimation()                       │
│    └─ OnAfterHide()                             │
│ 7. view.Unbind()                                │
│    ├─ disposables.Dispose() — отписка           │
│    └─ viewModel.Dispose()                       │
└─────────────────────────────────────────────────┘
```

**Ключевые моменты:**
- `OnBind` вызывается до `ShowAsync` — подписки уже активны
- `Unbind()` dispose'ит и ViewModel, и все подписки
- Если `IsPersistent = true`, View не уничтожается при переходе между state'ами
- `CanvasGroup` управляет видимостью через alpha/interactable/blocksRaycasts
