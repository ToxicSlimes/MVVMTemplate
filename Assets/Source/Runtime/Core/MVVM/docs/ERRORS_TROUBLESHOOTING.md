# MVVM — Ошибки и Troubleshooting

*Обновлено: 2026-02-11*

---

## 1. Утечки подписок (Memory Leak)

### Симптомы
- Рост памяти при открытии/закрытии view
- Callbacks продолжают срабатывать после закрытия view
- `ObjectDisposedException` при обращении к уничтоженному GameObject

### Причины
- Подписка без `.AddTo(ref disposables)` в `OnBind()`
- Подписка в ViewModel без `.AddTo(ref Disposables)`
- Event-подписка на Model (`+=`) без отписки в `Dispose()`

### Решение
```csharp
// ❌ Утечка — нет AddTo
ViewModel.Title.Subscribe(this, static (v, self) => self.UpdateTitle(v));

// ✅ Правильно
ViewModel.Title.Subscribe(this, static (v, self) => self.UpdateTitle(v))
    .AddTo(ref disposables);
```

Для event'ов Model в ViewModel:
```csharp
// В конструкторе:
_model.TitleChanged += OnTitleChanged;

// В Dispose():
public override void Dispose()
{
    _model.TitleChanged -= OnTitleChanged;
    base.Dispose();
}
```

---

## 2. Pool reuse — старые данные видны

### Симптомы
- При скролле списка элемент показывает данные от предыдущего item
- Текстура от предыдущего элемента мелькает перед загрузкой новой

### Причины
- `OnUnbind()` не очищает все поля
- Нет null-check на `OnBind()` — поле не обновляется если null

### Решение
```csharp
protected override void OnUnbind()
{
    // Очистить ВСЁ
    if (_icon != null) _icon.sprite = null;
    if (_name != null) _name.text = string.Empty;
    if (_healthBar != null) _healthBar.fillAmount = 1f;
    if (_overlay != null) _overlay.SetActive(false);
}
```

---

## 3. ViewModel not resolved from DI

### Симптомы
- `NullReferenceException` при обращении к `ViewModel` во View
- `ReflexException: Unable to resolve type MyFeatureViewModel`

### Причины
- ViewModel не зарегистрирован в DI-контейнере (Reflex)
- Model-интерфейс не зарегистрирован (`IMyModel → MyModel`)
- Зависимость ViewModel'а не зарегистрирована (сервис, другая model)

### Решение
Проверь регистрацию в Installer/Scope:
```csharp
container.AddTransient<MyFeatureViewModel>();
container.AddSingleton<IMyFeatureModel, MyFeatureModel>();
```

Проверь все зависимости конструктора ViewModel — каждая должна быть в DI.

---

## 4. View не привязывается к ViewModel

### Симптомы
- View отображается, но данные пустые
- `OnBind()` не вызывается

### Причины
- `ViewService` не находит View на сцене (нет GameObject с компонентом)
- View на неактивном GameObject (`SetActive(false)` до binding)
- Неправильный generic-тип: `BindableView<WrongViewModel>`

### Решение
1. Убедись, что View-компонент есть на сцене (или в prefab, загружаемом ViewService)
2. Проверь что `BindableView<TViewModel>` указывает на правильный ViewModel
3. Проверь что `ViewService.ShowAsync<TViewModel>()` вызывается из правильного State

---

## 5. ReactiveCommand не реагирует на нажатие

### Симптомы
- Кнопка нажимается, но ничего не происходит
- Command создан, но `Execute` не вызывается

### Причины
- Command не инициализирован (`null`)
- Command создан после `OnBind()` — подписка на null
- CanvasGroup.interactable = false

### Решение
- Инициализируй все Commands в конструкторе ViewModel (до `OnBind`)
- Проверь `CanvasGroup` на View — `interactable` и `blocksRaycasts` должны быть `true`
- Добавь `.AddTo(ref Disposables)` на каждый ReactiveCommand

---

## 6. Подписка срабатывает на старом значении

### Симптомы
- При открытии view callback срабатывает с начальным значением ReactiveProperty

### Причина
- `ReactiveProperty` при подписке эмитит текущее значение

### Решение
Если не нужно начальное значение:
```csharp
property.Skip(1)
    .Subscribe(this, static (v, self) => self.Handle(v))
    .AddTo(ref disposables);
```

---

## 7. MissingReferenceException после переключения сцен

### Симптомы
- `MissingReferenceException: The object of type 'X' has been destroyed`

### Причины
- Подписка держит ссылку на уничтоженный GameObject
- `IsPersistent = false`, но подписки не отписались

### Решение
- Убедись что `Unbind()` вызывается перед сменой сцены
- Для persistent view: `override bool IsPersistent => true`
- `OnDestroy()` в `BindableView` автоматически вызывает `Unbind()` — не блокируй его
