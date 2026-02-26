# MVVM — Техдолг и Улучшения

*Обновлено: 2026-02-11*

---

## 🔴 Высокий приоритет

### 1. Model event'ы → ReactiveProperty
**Проблема:** Model'и используют C# event'ы (`event Action<T>`) для нотификации. ViewModel подписывается через `+=` и должен вручную отписываться в `Dispose()`. Легко забыть отписку → утечка.

**Решение:** Перевести Model на `ReactiveProperty<T>` вместо event'ов. Тогда подписка через `.AddTo(ref Disposables)` автоматически чистится.

**Затронуто:** Все Model'и (`SettingsModel`, `ChatModel`, `LoginModel` и т.д.)

### 2. BattleState — пустая реализация
**Проблема:** `BattleState.Enter()` и `Exit()` пустые — state зарегистрирован, но ничего не делает.

**Решение:** Реализовать или удалить, если не используется.

---

## 🟡 Средний приоритет

### 3. Helper-методы BindSlider/BindToggle/BindDropdown → Extension или базовый класс
**Проблема:** `SettingsView` содержит static helper'ы `BindSlider`, `BindToggle`, `BindDropdown`. Они полезны для всех view, но лежат в конкретном View.

**Решение:** Вынести в `BindingExtensions` или `BindableViewExtensions` для переиспользования.

### 4. VoiceVolume — placeholder в SettingsViewModel
**Проблема:** `VoiceVolume` помечен комментарием "placeholder, not in IAudioService yet". Свойство существует, но не подключено к реальному сервису.

**Решение:** Подключить к `IAudioService` или убрать из UI до реализации.

### 5. Отсутствие валидации при Bind
**Проблема:** `BindableView.BindTo()` не проверяет что ViewModel валиден (не disposed). Если передать disposed ViewModel — подписки создадутся на мёртвый объект.

**Решение:** Добавить guard `if (viewModel.IsDisposed) throw` в `BindTo()`.

---

## 🟢 Низкий приоритет

### 6. Element popup → generic popup system
**Проблема:** `BindableElement` хардкодит `PopupTrigger` и `TooltipData`. Разные типы popup (tooltip, context menu) требуют разного подхода.

**Решение:** Абстрагировать popup систему, сделать `IPopupProvider` extensible.

### 7. Анимации Show/Hide — только CanvasGroup alpha
**Проблема:** `PlayShowAnimation()` / `PlayHideAnimation()` только переключают alpha. Нет поддержки scale, slide, fade с duration.

**Решение:** Подключить `IViewAnimator` (интерфейс уже есть в Abstractions) для кастомных анимаций через DOTween/LeanTween.

### 8. Standardize namespace'ы
**Проблема:** Некоторые файлы в `Source.Runtime.Core.MVVM`, другие в `Core.Abstractions.MVVM`. State'ы вообще без namespace'ов (global namespace).

**Решение:** Единый namespace convention для всего MVVM модуля.
