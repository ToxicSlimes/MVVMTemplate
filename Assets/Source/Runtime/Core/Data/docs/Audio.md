# Audio Data

Audio data structures для работы со звуком в игре.

**Путь:** `Domain/Audio/`  
**Namespace:** `Core.Data.Configs.Audio`

---

## AudioClipEntry

**Путь:** `Domain/Audio/AudioClipEntry.cs`

Struct для хранения аудио клипа с весом (для рандомизации).

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| ClipReference | `AssetReferenceT<AudioClip>` | Addressable референс на клип |
| LegacyClip | `AudioClip` | Прямая ссылка на клип (legacy) |
| Weight | `int` | Вес для рандомизации (0-50) |

### Конструктор

```csharp
public AudioClipEntry(AudioClip clip, int weight = 5)
```

### Computed Properties

| Property | Тип | Описание |
|----------|-----|----------|
| HasClip | `bool` | Есть ли валидный клип |

### Использование

```csharp
var entry = new AudioClipEntry(myClip, weight: 10);
if (entry.HasClip)
{
    // Play clip
}
```

---

## AudioMixerGroupSetup

**Путь:** `Domain/Audio/AudioMixerGroupSetup.cs`

Class для настройки mixer group.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Id | `AudioMixerGroupType` | ID mixer group (enum) |
| MixerGroup | `AudioMixerGroup` | Unity mixer group |
| Cooldown | `float` | Минимальное время между звуками (0-10s) |
| PoolSize | `int` | Размер пула audio sources (1-50) |
| Loop | `bool` | Зациклить звук |

### Использование

```csharp
var setup = new AudioMixerGroupSetup
{
    Id = AudioMixerGroupType.SoundEffects,
    MixerGroup = sfxMixerGroup,
    Cooldown = 0.1f,
    PoolSize = 10,
    Loop = false
};
```

---

## SoundSetup

**Путь:** `Domain/Audio/SoundSetup.cs`

Class для настройки конкретного звука.

### Поля

| Поле | Тип | Default | Описание |
|------|-----|---------|----------|
| Id | `SoundType` | — | ID звука (enum) |
| MixerGroup | `AudioMixerGroupType` | `SoundEffects` | Mixer group |
| PlaybackScope | `SoundPlaybackScope` | `Global` | Scope воспроизведения |
| Volume | `float` | `1.0` | Громкость (0-1) |
| MinDistance | `float` | `1.0` | Мин. дистанция (0.1-20) |
| MaxDistance | `float` | `25.0` | Макс. дистанция (1-200) |
| OverrideSpatialSettings | `bool` | `false` | Переопределить spatial настройки |
| SpatialBlend | `float` | `1.0` | 2D/3D blend (0-1) |
| DopplerLevel | `float` | `1.0` | Уровень эффекта Doppler (0-5) |
| Spread | `float` | `0.0` | Spread (0-360) |
| RolloffMode | `AudioRolloffMode` | `Logarithmic` | Режим rolloff |
| ReverbLevel | `float` | `0.0` | Уровень reverb (0-1) |
| CustomRolloffCurve | `AnimationCurve` | Linear | Кастомная кривая rolloff |
| Clips | `List<AudioClipEntry>` | Empty | Список клипов с весами |

### Использование

```csharp
var soundSetup = new SoundSetup
{
    Id = SoundType.UI_ButtonClick,
    MixerGroup = AudioMixerGroupType.UserInterface,
    Volume = 0.8f,
    Clips = new List<AudioClipEntry>
    {
        new(clickClip1, weight: 5),
        new(clickClip2, weight: 3)
    }
};
```

---

## Enums

### AudioMixerGroupType

**Путь:** `Domain/Enums/AudioEnums.cs`

```csharp
public enum AudioMixerGroupType
{
    Unknown = 0,
    Master = 1,
    Music = 2,
    SoundEffects = 3,
    UserInterface = 4,
    Environment = 5,
    Voice = 6
}
```

### SoundPlaybackScope

**Путь:** `Domain/Enums/AudioEnums.cs`

```csharp
public enum SoundPlaybackScope
{
    Global = 0,  // Слышно везде (UI, музыка)
    Local = 1    // Зависит от позиции (SFX, environment)
}
```

### SoundType

**Путь:** `Domain/Enums/SoundType.cs`

```csharp
public enum SoundType
{
    Unknown = 0,

    // Music (1-99)
    Music_MainMenu = 1,
    Music_Gameplay = 2,
    Music_Battle = 3,
    Music_Victory = 4,
    Music_Defeat = 5,

    // UI (100-199)
    UI_ButtonClick = 100,
    UI_ButtonHover = 101,
    UI_MenuOpen = 102,
    UI_MenuClose = 103,
    UI_Notification = 104,
    UI_Purchase = 105,
    UI_Error = 106,
    UI_Success = 107,

    // SFX (200-299)
    SFX_Hit = 200,
    SFX_Miss = 201,
    SFX_Critical = 202,
    SFX_Death = 203,
    SFX_LevelUp = 204,
    SFX_ItemPickup = 205,
    SFX_ItemDrop = 206,
    SFX_Equip = 207,
    SFX_Unequip = 208,

    // Environment (300-399)
    Environment_Ambient = 300,
    Environment_Weather = 301,
    Environment_Footstep = 302
}
```

**Группировка по диапазонам:**
- `1-99` — Музыка
- `100-199` — UI звуки
- `200-299` — Игровые SFX
- `300-399` — Environment звуки

---

## Архитектура

### Audio Service Pattern

```csharp
public interface IAudioService
{
    void PlaySound(SoundType soundType, Vector3? position = null);
    void PlayMusic(SoundType musicType);
    void StopMusic();
    void SetVolume(AudioMixerGroupType groupType, float volume);
}
```

### Рандомизация клипов

```csharp
// SoundSetup содержит List<AudioClipEntry> с весами
// При воспроизведении выбирается случайный клип с учётом весов
var totalWeight = clips.Sum(c => c.Weight);
var randomValue = Random.Range(0, totalWeight);
var selectedClip = SelectClipByWeight(clips, randomValue);
```

### Pooling

AudioService использует object pooling для audio sources на основе `AudioMixerGroupSetup.PoolSize`.
