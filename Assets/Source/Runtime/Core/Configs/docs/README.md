# Configs — Система конфигурации

## Обзор

Все конфиги в проекте — **ScriptableObject**, наследующие `ConfigBase` или `ServiceConfigBase`.

### Иерархия
```
ConfigBase (ScriptableObject)
├── ServiceConfigBase          — для сервисных конфигов (Network, Audio, ProceduralUI)
├── ColorPalette               — цветовая палитра
├── TypographySettings         — типографика
├── SpacingSettings            — отступы
└── ...
```

### Структура папок
```
Configs/
├── DesignSystem/
│   ├── DesignSystemSettings.cs   — корневой конфиг дизайн-системы
│   ├── ColorPalette.cs           — цвета (background, accent, text, buttons)
│   ├── TypographySettings.cs     — шрифты и размеры
│   └── SpacingSettings.cs        — отступы и spacing
├── Services/
│   ├── NetworkConfig.cs          — API URLs, auth, timeout, Gemini key
│   ├── ProceduralUiConfig.cs     — prefabs компонентов, pool, анимации
│   └── Audio/
│       ├── AudioConfig.cs        — AudioMixer, Sound, Prefabs
│       ├── SoundConfig.cs        — настройки звуков
│       └── AudioMixerGroupConfig.cs
└── docs/
```

### Базовые классы

**ConfigBase** — общий базовый класс:
- Inspector через **Tri-Inspector** (Title, Group, Required, Button)
- `ValidateConfig()` — переопредели для валидации
- Кнопки `PingInProject` и `OpenScript` в Editor

**ServiceConfigBase** — для сервисных конфигов:
- Наследует ConfigBase
- Готовые группы: `service`, `pool`, `cache`, `network`, `debug`

### Как конфиги попадают в DI

Через `IConfigProvider`:
```csharp
builder.AddSingleton<IConfigProvider>(c => {
    var provider = new ConfigProvider(c.Resolve<IAssetProvider>(), c.Resolve<ILogger>());
    provider.PreloadAllConfigsAsync().Forget();
    return provider;
});

// Использование в Bindings:
var configProvider = container.Resolve<IConfigProvider>();
var config = configProvider.GetConfig<NetworkConfig>();
```

### Создание ассетов
Конфиги создаются через Unity меню:
- `Assets → Create → EndlessAdventure → Network Config`
- `Assets → Create → EndlessAdventure → Design System → Color Palette`
- `Assets → Create → Configs/Services → Audio Config`

Размещение: `Assets/Source/00. Configs/`

---

*Обновлено: 2026-02-11*
