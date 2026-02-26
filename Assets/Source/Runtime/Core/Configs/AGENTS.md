# AGENTS.md - Core.Configs

## 🎛️ Что это

`Core.Configs` содержит **классы ScriptableObject-конфигов**. Сами `.asset`-инстансы хранятся в `Assets/Source/00. Configs/`.

## 📂 Structure

```
Configs/
├── DesignSystem/
│   ├── DesignSystemSettings.cs
│   ├── ColorPalette.cs
│   ├── TypographySettings.cs
│   └── SpacingSettings.cs
├── Services/
│   ├── NetworkConfig.cs
│   ├── ProceduralUiConfig.cs
│   └── Audio/
│       ├── AudioConfig.cs
│       ├── AudioMixerGroupConfig.cs
│       └── SoundConfig.cs
└── Views/                     # ViewConfig и связанные с layout/view settings (если добавятся)
    └── ViewConfig.cs         # ScriptableObject с маппингами ViewModel → Addressable View
```

## 🔌 Runtime Loading (актуально)

Конфиги загружаются через `IConfigProvider` (Addressables):

- **Адрес** по умолчанию: `typeof(T).Name` (например `NetworkConfig`, `ProceduralUiConfig`, `DesignSystemSettings`, `ViewConfig`) — это значение используется, если не задано иное имя/label
- **Preload**: `ConfigProvider.PreloadAllConfigsAsync()` прогревает фиксированный список конфигов
- **Label fallback**: для отдельных конфигов используется label (пример: `AudioConfig` → label `Audio`), который переопределяет поиск по имени типа

## 🧩 Как используются в DI

В `InfrastructureBindings` конфиги берутся так:

- `NetworkConfig` обязателен (если не найден — исключение)
- `ViewConfig` обязателен (если не найден — исключение)
- `AudioConfig` опционален: если не найден, `IAudioService` вернёт `null` и будет выведен warning
- `ProceduralUiConfig` опционален: если не найден, `UiComponentFactory`/`UiComponentPool` работают с дефолтами (без регистрации префабов/без override pool size)

## ✅ Добавление нового конфига (правильный путь)

1. Создай класс в `Core/Configs/{Category}/` и укажи корректный namespace `Core.Configs.{Category}`
2. Создай `.asset` в `Assets/Source/00. Configs/{Category}/`
3. Добавь asset в **Addressables** с адресом **равным имени типа** (по умолчанию `MyNewConfig`)
4. Если конфиг должен грузиться на старте — добавь имя типа в список `ConfigProvider.PreloadAllConfigsAsync()`

## 📚 Links

- `docs/config-system.md`
- `Assets/Source/Runtime/Core/Infrastructure/Services/Config/ConfigProvider.cs`

