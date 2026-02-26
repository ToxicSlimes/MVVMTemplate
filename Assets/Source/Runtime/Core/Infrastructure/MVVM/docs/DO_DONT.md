# MVVM Infrastructure — DO / DON'T

*Обновлено: 2026-02-11*

---

## ✅ DO

### ViewService
- **Используй `ShowAsync<TView, TViewModel>()`** для показа View — он управляет кешем, историей, sorting order
- **Подписывайся на `ViewShown` / `ViewHidden`** events если нужно реагировать на навигацию
- **Используй `ViewShowParams`** для настройки DisplayMode, Transition, Canvas layer

```csharp
await _viewService.ShowAsync<SettingsView, SettingsViewModel>();
```

### ViewFactory
- **Полагайся на ViewFactory для DI-инъекции** — он вызывает `GameObjectInjector.InjectRecursive` автоматически
- **Используй `ViewConfig`** для маппинга ViewModel → Prefab адрес — не хардкодь адреса
- **Передавай parent Transform** если нужен конкретный Canvas layer

### CanvasProvider
- **Регистрируй Canvas через `RegisterCanvas(CanvasLayer, Canvas)`** в bootstrap/сцене
- **Получай Canvas через `GetCanvas(CanvasLayer)`** — не ищи через `Find`
- **Используй `CanvasLayer` enum** — `Background`, `Main`, `Overlay`, etc.

### UIElementFactory / UIElementService
- **Используй `UIElementService.CreateElementAsync<TElement, TData>()`** для создания элементов с DI
- **Используй `ReconcileElementsAsync`** для обновления списков — не пересоздавай элементы вручную
- **Трекай элементы через container** — `UIElementService` автоматически отслеживает по parent Transform

### Pool Management
- **Переиспользуй View** — `ViewService` кеширует View в `_viewCache` по типу
- **Для списков используй reconciliation** — `UIElementService` сравнивает и обновляет, а не destroy/create

### View Lifecycle
- **Подписки в `OnBind(ref DisposableBag)`** — они автоматически dispose при Unbind
- **Используй static lambdas** в Subscribe для zero-alloc:

```csharp
property.Subscribe(this, static (val, self) => self.UpdateUI(val))
    .AddTo(ref disposables);
```

- **Порядок:** Create → BindTo → OnBind → Show (animate) → Hide (animate) → Unbind → Destroy

---

## ❌ DON'T

### ViewService
- **НЕ показывай View напрямую** через `gameObject.SetActive(true)` — используй ViewService
- **НЕ управляй sorting order вручную** — ViewService автоматически назначает `_currentMaxSortingOrder`
- **НЕ кешируй View самостоятельно** — ViewService уже делает это через `ConcurrentDictionary`

### ViewFactory
- **НЕ создавай View через `Instantiate` вручную** — DI инъекция не произойдёт
- **НЕ резолви ViewModel вне ViewFactory** (для View) — ViewFactory связывает View и ViewModel через `BindTo`

```csharp
// ❌ Нельзя
var view = Object.Instantiate(prefab);  // Нет DI!
```

### CanvasProvider
- **НЕ храни ссылки на Canvas напрямую** — используй `ICanvasProvider`
- **НЕ забудь `UnregisterCanvas`** при выгрузке сцены — иначе ссылка на destroyed объект

### UIElementFactory
- **НЕ создавай UI элементы через `new GameObject()`** — используй фабрику
- **НЕ забывай Dispose/Unbind** элементов — утечки памяти

### Общее
- **НЕ храни ссылку на View в ViewModel** — однонаправленный поток данных
- **НЕ вызывай Unity API из ViewModel** — ViewModel должен быть testable
- **НЕ используй `Find` / `GetComponent` в Update** — кешируй в `OnBind`
