# MVVM Infrastructure — Ошибки и Troubleshooting

*Обновлено: 2026-02-11*

---

## Ошибка: View не найден — "Prefab at {address} does not have IView component"

**Симптом:** `InvalidOperationException` из `ViewFactory.CreateAsync()`.

**Причина:** Prefab по Addressable адресу не содержит компонент `IView` (BindableView).

**Решение:**
1. Проверь что на корневом GameObject prefab'а есть `BindableView<TViewModel>` компонент
2. Проверь что адрес в `ViewConfig` указывает на правильный prefab
3. Проверь что prefab помечен как Addressable с правильным адресом

---

## Ошибка: Canvas not registered — GetCanvas возвращает null

**Симптом:** `GetCanvas(CanvasLayer.Main)` возвращает `null`, View создаётся в `_uiRoot` вместо нужного Canvas.

**Причина:** Canvas не зарегистрирован через `CanvasProvider.RegisterCanvas()`.

**Решение:**
1. Убедись что в сцене есть bootstrap-скрипт с вызовом `_canvasProvider.RegisterCanvas(layer, canvas)`
2. Проверь что Canvas зарегистрирован ДО показа View
3. Проверь что `UnregisterCanvas` не вызван преждевременно

**Лог-подсказка:** `CanvasProvider` логирует регистрацию: `"Canvas registered for layer {layer}: {name}"`

---

## Ошибка: "Canvas for layer {layer} is already registered. Overwriting..."

**Симптом:** Warning в логе при `RegisterCanvas()`.

**Причина:** Повторная регистрация Canvas для того же `CanvasLayer` (например, при перезагрузке сцены без UnregisterCanvas).

**Решение:**
1. Вызывай `UnregisterCanvas(layer)` в `OnDestroy()` сцены
2. Или проверяй `GetCanvas(layer) == null` перед регистрацией

---

## Ошибка: ViewModel не зарезолвлен — Reflex binding not found

**Симптом:** `ReflexException` при создании View — ViewModel не найден в контейнере.

**Причина:** ViewModel не зарегистрирован в DI.

**Решение:**
```csharp
// В Installer
builder.AddTransient<MyPanelViewModel>();
```

---

## Ошибка: Утечка View — View не уничтожается

**Симптом:** View остаётся в памяти/иерархии после скрытия.

**Причина:** ViewService кеширует View в `_viewCache`. Это by design — View переиспользуется.

**Решение (если нужно уничтожить):**
- Для одноразовых View — не используй кеш (проверь `hideCurrentView` параметр)
- Для принудительного уничтожения — вызови `ViewFactory.Destroy(view)`

---

## Ошибка: Element address not found — "[UIElementService] Element address not found for {Type}"

**Симптом:** `CreateElementAsync` логирует ошибку и возвращает `null`.

**Причина:** Тип элемента не настроен в `ElementConfig`.

**Решение:**
1. Открой `ElementConfig` ScriptableObject
2. Добавь маппинг: тип элемента → Addressable адрес prefab'а
3. Убедись что prefab помечен как Addressable

---

## Ошибка: EmbeddedPanelService — "Panel already registered"

**Симптом:** Warning `[EmbeddedPanelService] Panel already registered: {Type}`.

**Причина:** `RegisterPanel<TViewModel>()` вызван дважды для одного типа.

**Решение:**
- Регистрируй панель один раз (обычно в `Start()` или `Awake()` сцены)
- Проверь что сцена не загружается повторно без выгрузки

---

## Ошибка: Sorting order некорректный — View перекрывается

**Симптом:** Новый View отображается ПОД предыдущим.

**Причина:** `ViewService` автоматически инкрементирует `_currentMaxSortingOrder`, но Canvas override sorting может конфликтовать.

**Решение:**
1. Проверь что View prefab не имеет hardcoded sorting order в Canvas компоненте
2. ViewService автоматически назначает sorting — не ставь его вручную

---

## Ошибка: Подписки не dispose — ReactiveProperty продолжает обновлять UI

**Симптом:** После скрытия View — UI всё ещё обновляется, логи продолжают идти.

**Причина:** Подписка не добавлена в `DisposableBag`.

**Решение:**
```csharp
// ✅ Правильно — AddTo гарантирует dispose при Unbind
observable.Subscribe(...).AddTo(ref disposables);

// ❌ Неправильно — подписка никогда не dispose
observable.Subscribe(...);
```
