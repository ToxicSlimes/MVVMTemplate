# SettingsView

## Назначение

Модальное окно настроек игры с табами: Audio, Video, Display, Controls, Game.

## Файлы

| Тип | Путь |
|-----|------|
| View | `Views/Settings/SettingsView.cs` |
| ViewModel | `ViewModels/Settings/SettingsViewModel.cs` |
| Model | `ISettingsModel` (Abstractions) |

---

## ViewModel Properties

### Общие

| Property | Тип | Описание |
|----------|-----|----------|
| `IsVisible` | `ReactiveProperty<bool>` | Видимость окна настроек |
| `CurrentTab` | `ReactiveProperty<SettingsTab>` | Текущий активный таб |

### Audio Settings

| Property | Тип | Описание |
|----------|-----|----------|
| `MasterVolume` | `ReactiveProperty<float>` | Общая громкость (0-1) |
| `MusicVolume` | `ReactiveProperty<float>` | Громкость музыки |
| `SfxVolume` | `ReactiveProperty<float>` | Громкость эффектов |
| `VoiceVolume` | `ReactiveProperty<float>` | Громкость голоса |
| `IsMuteAll` | `ReactiveProperty<bool>` | Отключить весь звук |
| `IsMuteMusic` | `ReactiveProperty<bool>` | Отключить музыку |
| `IsMuteSfx` | `ReactiveProperty<bool>` | Отключить эффекты |

### Video Settings

| Property | Тип | Описание |
|----------|-----|----------|
| `ResolutionIndex` | `ReactiveProperty<int>` | Индекс разрешения |
| `Quality` | `ReactiveProperty<QualityPreset>` | Пресет качества |
| `IsFullscreen` | `ReactiveProperty<bool>` | Полноэкранный режим |
| `IsVSync` | `ReactiveProperty<bool>` | Вертикальная синхронизация |
| `ShowFps` | `ReactiveProperty<bool>` | Показывать FPS |
| `TargetFps` | `ReactiveProperty<TargetFpsOption>` | Целевой FPS |

### Display Settings

| Property | Тип | Описание |
|----------|-----|----------|
| `Brightness` | `ReactiveProperty<float>` | Яркость |
| `Gamma` | `ReactiveProperty<float>` | Гамма (0.5-2.0) |
| `UiScale` | `ReactiveProperty<float>` | Масштаб UI (75%-150%) |
| `IsColorblindMode` | `ReactiveProperty<bool>` | Режим дальтоника |
| `ColorblindType` | `ReactiveProperty<ColorblindType>` | Тип дальтонизма |

### Controls Settings

| Property | Тип | Описание |
|----------|-----|----------|
| `MouseSensitivity` | `ReactiveProperty<float>` | Чувствительность мыши |
| `InvertYAxis` | `ReactiveProperty<bool>` | Инвертировать ось Y |

### Game Settings

| Property | Тип | Описание |
|----------|-----|----------|
| `Language` | `ReactiveProperty<GameLanguage>` | Язык игры |
| `IsAutoSave` | `ReactiveProperty<bool>` | Автосохранение |
| `ShowTutorials` | `ReactiveProperty<bool>` | Показывать туториалы |
| `ScreenShake` | `ReactiveProperty<bool>` | Тряска экрана |
| `MatureContent` | `ReactiveProperty<bool>` | Контент 18+ |

---

## ViewModel Commands

| Command | Параметр | Описание |
|---------|----------|----------|
| `ToggleCommand` | `Unit` | Переключить видимость |
| `CloseCommand` | `Unit` | Закрыть и скрыть |
| `ApplyCommand` | `Unit` | Применить настройки |
| `ResetCommand` | `Unit` | Сбросить к дефолтам |
| `CancelCommand` | `Unit` | Отмена (загрузить из prefs) |
| `SelectTabCommand` | `SettingsTab` | Выбрать таб |
| `NextTabCommand` | `Unit` | Следующий таб |
| `PreviousTabCommand` | `Unit` | Предыдущий таб |

---

## UI Элементы

### Modal

| Поле | Тип | Описание |
|------|-----|----------|
| `_dimBackground` | `Button` | Затемнённый фон (клик = закрыть) |
| `_closeButton` | `Button` | Кнопка закрытия |
| `_modalWindow` | `GameObject` | Контейнер окна |

### Tabs

| Поле | Тип | Описание |
|------|-----|----------|
| `_tabAudio` | `Toggle` | Таб Audio |
| `_tabVideo` | `Toggle` | Таб Video |
| `_tabDisplay` | `Toggle` | Таб Display |
| `_tabControls` | `Toggle` | Таб Controls |
| `_tabGame` | `Toggle` | Таб Game |
| `_audioContent` | `GameObject` | Контент Audio |
| `_videoContent` | `GameObject` | Контент Video |
| `_displayContent` | `GameObject` | Контент Display |
| `_controlsContent` | `GameObject` | Контент Controls |
| `_gameContent` | `GameObject` | Контент Game |

### Footer

| Поле | Тип | Описание |
|------|-----|----------|
| `_applyButton` | `Button` | Применить |
| `_resetButton` | `Button` | Сбросить |
| `_cancelButton` | `Button` | Отмена |

---

## Биндинги

```csharp
// Слайдер громкости
BindSlider(_masterVolumeSlider, _masterVolumeValue, ViewModel.MasterVolume, ref disposables);

// Toggle mute
BindToggle(_muteAllToggle, ViewModel.IsMuteAll, ref disposables);

// Dropdown quality
_qualityDropdown.onValueChanged.AsObservable()
    .Subscribe(ViewModel.Quality, (v, prop) => prop.Value = (QualityPreset)v)
    .AddTo(ref disposables);

// Tab selection
ViewModel.CurrentTab
    .Subscribe(this, (tab, self) => self.UpdateTabVisuals(tab))
    .AddTo(ref disposables);
```

---

## Связанные сервисы

| Сервис | Использование |
|--------|---------------|
| `ISettingsModel` | Хранение и персистенция настроек |
| `IMediator` | HideViewRequest |
| `IInputService` | Keyboard navigation (Q/E tabs, Escape) |

---

## Схема UI

```
┌──────────────────────────────────────────────────┐
│ Settings                                    [X]  │
├──────────────────────────────────────────────────┤
│ [Audio] [Video] [Display] [Controls] [Game]      │
├──────────────────────────────────────────────────┤
│                                                  │
│  Master Volume  ═══════════○═══════════  85%     │
│  Music Volume   ═══════════════○═══════  100%    │
│  SFX Volume     ══════○════════════════  50%     │
│  Voice Volume   ═══════════════════○═══  90%     │
│                                                  │
│  [✓] Mute All   [ ] Mute Music   [ ] Mute SFX    │
│                                                  │
├──────────────────────────────────────────────────┤
│           [Apply]  [Reset]  [Cancel]             │
└──────────────────────────────────────────────────┘
```

---

## Особенности

- `IsPersistent = true` — View не уничтожается при скрытии
- Keyboard navigation: Q/E для переключения табов
- Escape закрывает окно
- Автосохранение в PlayerPrefs при Apply
