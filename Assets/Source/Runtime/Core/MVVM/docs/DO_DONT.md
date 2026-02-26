# MVVM — DO / DON'T

*Обновлено: 2026-02-11*

---

## ✅ DO

### Views
- **Наследуй от `BindableView<TViewModel>`** для всех view-компонентов
- **Все подписки в `OnBind(ref DisposableBag disposables)`** — это единственная точка входа для binding
- **`.AddTo(ref disposables)`** на каждую подписку — без исключений
- **Static lambda** в `.Subscribe()` для избежания аллокаций замыканий:
  ```csharp
  property.Subscribe(this, static (v, self) => self.UpdateUI(v))
      .AddTo(ref disposables);
  ```
- **Null-check на SerializeField** перед использованием (`if (_button != null)`)
- **Группируй binding** по логическим блокам в отдельные методы (`BindAudioSettings`, `BindTabs` и т.д.)
- **`IsPersistent => true`** для view, которые не должны пересоздаваться (Settings, Main)

### ViewModels
- **Наследуй от `DisposableObject`** — он даёт `ref Disposables` и `CancellationTokenSource`
- **`ReactiveProperty<T>`** для всех данных, к которым привязывается UI
- **`ReactiveCommand<T>`** для действий пользователя (кнопки, навигация)
- **Подписки ViewModel → Model** через `.AddTo(ref Disposables)`
- **Атрибут `[GenerateViewCommands]`** на ViewModel для автогенерации command-обвязки
- **Override `Dispose()`** для отписки от event'ов Model (не забывай `base.Dispose()`)

### Models
- **Реализуй интерфейс `IModel`** (или специфичный `ISettingsModel`, `IChatModel`)
- **Event'ы (`Action<T>`)** для нотификации ViewModel об изменениях данных
- **Setter с проверкой `if (old == value) return`** — не стреляй event без изменения
- **`Dispose()` обнуляет event-делегаты** (`IsVisibleChanged = null`)

### Elements
- **Наследуй от `BindableElement<TData>`** где `TData : struct, IElementData`
- **`OnBind(TData data)`** — основная логика отображения
- **`OnUnbind()`** — очистка (null для текстур, empty для текстов, reset fill)
- **`GetPopupData()`** — tooltip данные, если нужны
- **`OnRefresh()`** — переопределяй для оптимизации частых обновлений

### Навигация
- **Через `IMediator`** — `mediator.Send(new HideViewRequest<V, VM>())` / `ShowViewRequest`
- **`NavigateCommand<TTarget>`** для переходов между state'ами

---

## ❌ DON'T

### Views
- **НЕ пиши бизнес-логику во View** — View только отображает и пробрасывает user input в ViewModel
- **НЕ вызывай сервисы напрямую из View** — только через ViewModel
- **НЕ забывай `.AddTo(ref disposables)`** — утечка подписок = утечка памяти
- **НЕ используй лямбды с замыканиями** в горячих путях — только `static` lambda с state
- **НЕ храни состояние во View** — всё состояние в ViewModel/Model

### ViewModels
- **НЕ ссылайся на Unity-типы** (`MonoBehaviour`, `GameObject`, `Transform`) — ViewModel чистый C#
- **НЕ создавай `ReactiveProperty` без `AddTo`** в подписках на них
- **НЕ забывай отписку от Model event'ов** в `Dispose()` — иначе ViewModel будет жить после закрытия view

### Models
- **НЕ ссылайся на ViewModel или View из Model** — Model ничего не знает о presentation layer
- **НЕ стреляй event без реального изменения** — проверяй `if (value == old) return`

### Elements
- **НЕ подписывайся на Observable из Element** без `AddTo(ref Disposables)` — Element может быть в пуле
- **НЕ забывай `OnUnbind()`** — иначе при переиспользовании из пула будут видны старые данные
- **НЕ аллоцируй в `OnBind()`** — этот метод вызывается часто при скролле/пуле

---

## 🔄 Pool Pattern (Elements)

```
BindableElement lifecycle в пуле:
  Pool.Get() → Bind(data) → [OnBind] → ... → Unbind() → [OnUnbind] → Pool.Return()
  Pool.Get() → Bind(newData) → [OnBind] → ...  (повторное использование)
```

- **`OnBind()`** — полная инициализация визуала
- **`OnUnbind()`** — полная очистка (null текстуры, пустые строки, reset fill)
- **`Refresh(data)`** вызывает `OnRefresh()` если уже bound — оптимизация для обновления без rebind
