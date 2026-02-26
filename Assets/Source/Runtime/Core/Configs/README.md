# Core.Configs

Модуль с **классами ScriptableObject-конфигов** (только `.cs`), которые используются рантайм-сервисами через `IConfigProvider`.

## Что где лежит

- `Assets/Source/Runtime/Core/Configs/` — **типы конфигов** (код, `.cs`)
- `Assets/Source/00. Configs/` — **инстансы конфигов** (данные, `.asset`)

## Как конфиги загружаются в рантайме

- В DI регистрируется `IConfigProvider` (`Core.Infrastructure.Services.Config.ConfigProvider`)
- `ConfigProvider` грузит конфиги из Addressables по **адресу = имени типа** (например `NetworkConfig`, `ViewConfig`, `DesignSystemSettings`)
- Для части конфигов возможен fallback-загрузчик через label (пример: `AudioConfig` → label `Audio`)

## Смотри также

- `docs/config-system.md` — правила хранения/создания конфигов
- `Assets/Source/Runtime/Core/Infrastructure/Services/Config/ConfigProvider.cs` — фактическая логика загрузки/кэша

