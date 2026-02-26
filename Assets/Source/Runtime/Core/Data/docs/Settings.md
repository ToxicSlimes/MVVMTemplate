# Settings Data

Settings data structures для хранения настроек игры.

**Путь:** `Domain/Models/`  
**Namespace:** `Core.Data.Domain.Models`

---

## AudioSettingsData

**Путь:** `Domain/Models/AudioSettingsData.cs`

Настройки аудио.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| MasterVolume | `float` | `1.0` | Общая громкость (0-1) |
| MusicVolume | `float` | `0.7` | Громкость музыки (0-1) |
| SfxVolume | `float` | `0.8` | Громкость SFX (0-1) |
| IsMasterMuted | `bool` | `false` | Заглушен ли общий звук |
| IsMusicMuted | `bool` | `false` | Заглушена ли музыка |
| IsSfxMuted | `bool` | `false` | Заглушены ли SFX |

### Factory Method

```csharp
public static AudioSettingsData Default => new()
{
    MasterVolume = 1f,
    MusicVolume = 0.7f,
    SfxVolume = 0.8f,
    IsMasterMuted = false,
    IsMusicMuted = false,
    IsSfxMuted = false
};
```

### Использование

```csharp
var audioSettings = AudioSettingsData.Default;
audioService.SetVolume(AudioMixerGroupType.Music, audioSettings.MusicVolume);
```

---

## VideoSettingsData

**Путь:** `Domain/Models/SettingsData.cs`

Настройки видео/графики.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| ResolutionIndex | `int` | — | Индекс разрешения из Screen.resolutions |
| Quality | `QualityPreset` | `High` | Пресет качества (enum) |
| IsFullscreen | `bool` | `true` | Полноэкранный режим |
| IsVSync | `bool` | `true` | Вертикальная синхронизация |
| ShowFps | `bool` | `false` | Показывать FPS |
| TargetFps | `TargetFpsOption` | `Fps60` | Целевой FPS (enum) |

---

## DisplaySettingsData

**Путь:** `Domain/Models/SettingsData.cs`

Настройки отображения.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| Brightness | `float` | `0.5` | Яркость (0-1) |
| Gamma | `float` | `1.0` | Гамма (0-2) |
| UiScale | `float` | `1.0` | Масштаб UI (0.5-2) |
| IsColorblindMode | `bool` | `false` | Режим для дальтоников |
| ColorblindType | `ColorblindType` | — | Тип дальтонизма (enum) |

---

## ControlsSettingsData

**Путь:** `Domain/Models/SettingsData.cs`

Настройки управления.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| MouseSensitivity | `float` | `5.0` | Чувствительность мыши (1-10) |
| InvertYAxis | `bool` | `false` | Инвертировать Y ось |

---

## GameSettingsData

**Путь:** `Domain/Models/SettingsData.cs`

Игровые настройки.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| Language | `GameLanguage` | `English` | Язык игры (enum) |
| IsAutoSave | `bool` | `true` | Автосохранение |
| ShowTutorials | `bool` | `true` | Показывать туториалы |
| ScreenShake | `bool` | `true` | Эффект тряски экрана |
| MatureContent | `bool` | `false` | Контент для взрослых |

---

## Settings Enums

### SettingsTab

**Путь:** `Domain/Enums/SettingsEnums.cs`

```csharp
public enum SettingsTab
{
    Audio,
    Video,
    Display,
    Controls,
    Game
}
```

### QualityPreset

**Путь:** `Domain/Enums/SettingsEnums.cs`

```csharp
public enum QualityPreset
{
    Low,
    Medium,
    High,
    Ultra
}
```

### ColorblindType

**Путь:** `Domain/Enums/SettingsEnums.cs`

```csharp
public enum ColorblindType
{
    None,
    Deuteranopia,  // Красно-зелёный (зелёная слепота)
    Protanopia,    // Красно-зелёный (красная слепота)
    Tritanopia     // Сине-жёлтый
}
```

### GameLanguage

**Путь:** `Domain/Enums/SettingsEnums.cs`

```csharp
public enum GameLanguage
{
    English,
    Russian,
    German,
    French,
    Spanish,
    Chinese,
    Japanese,
    Korean
}
```

### TargetFpsOption

**Путь:** `Domain/Enums/SettingsEnums.cs`

```csharp
public enum TargetFpsOption
{
    Fps30 = 30,
    Fps60 = 60,
    Fps120 = 120,
    Unlimited = -1
}
```

---

## Архитектура

### Settings Service Pattern

```csharp
public interface ISettingsService
{
    AudioSettingsData Audio { get; set; }
    VideoSettingsData Video { get; set; }
    DisplaySettingsData Display { get; set; }
    ControlsSettingsData Controls { get; set; }
    GameSettingsData Game { get; set; }
    
    void LoadSettings();
    void SaveSettings();
    void ResetToDefaults();
    void ApplySettings();
}
```

### Persistence

Settings хранятся в PlayerPrefs или в JSON файле:

```csharp
var json = JsonConvert.SerializeObject(audioSettings);
PlayerPrefs.SetString("Settings.Audio", json);
```

### Apply Pipeline

```csharp
public void ApplySettings()
{
    ApplyAudioSettings(Audio);
    ApplyVideoSettings(Video);
    ApplyDisplaySettings(Display);
    ApplyControlsSettings(Controls);
    ApplyGameSettings(Game);
}
```
