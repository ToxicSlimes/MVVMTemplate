# Data Layer — DO / DON'T

> Свод правил для слоя Data в EndlessAdventure. Что делать МОЖНО, что НЕЛЬЗЯ, и какие паттерны считаем каноничными.

*Обновлено: 2026-02-11*

---

## ✅ DO — что делать надо

### 1. Разделять DTO и Domain Model

DTO отражает JSON с сервера. Domain Model — чистая бизнес-модель для приложения.

```
Network/DTO/Character/CharacterResponseDto.cs   ← JSON-маппинг
Domain/Models/Character/CharacterModel.cs        ← бизнес-логика
```

DTO содержит `[JsonProperty]` и все поля API. Domain Model содержит только нужные поля + computed properties.

### 2. Маппинг через статические Mapper-классы

Все преобразования DTO → Model выполняются через `static class` в `Domain/Mappers/`:

```csharp
// ChatMapper.cs
public static class ChatMapper
{
    public static ChatModel ToModel(ChatResponseDto dto)
    {
        if (dto == null) return null;
        return new ChatModel { Id = dto.Id, Title = dto.Title, ... };
    }

    public static List<ChatModel> ToModels(IEnumerable<ChatResponseDto> dtos)
    {
        return dtos?.Select(ToModel).ToList() ?? new List<ChatModel>();
    }
}
```

**Паттерн:** `ToModel(dto)` для единичного, `ToModels(dtos)` для коллекции.

### 3. Null-safety в каждом маппере

Каждый `ToModel` начинается с null-check. Коллекции — всегда fallback на пустой список:

```csharp
Messages = dto.Messages?.Select(ToModel).ToList() ?? new List<ChatMessage>();
Greetings = dto.Greetings?.Select(ToModel).ToList() ?? new List<CharacterGreeting>();
```

### 4. `readonly struct` для UI Element Data

Данные для UI элементов (слоты, строки, ресурсы) — `readonly struct` с `IEquatable<T>`:

```csharp
public readonly struct HeroSlotData : IElementData, IEquatable<HeroSlotData>
{
    public readonly string HeroId;
    public readonly string Name;
    public readonly Texture2D Portrait;
    // ...
    public bool Equals(HeroSlotData other) => HeroId == other.HeroId;
}
```

### 5. Computed properties в Domain Models

Domain Model может содержать helper-методы и computed properties:

```csharp
// CharacterModel.cs
public CharacterGreeting GetDefaultGreeting() { ... }
public string GetDisplayName() => string.IsNullOrEmpty(Name) ? "Unnamed Character" : Name;

// ChatModel.cs
public bool HasMessages => Messages != null && Messages.Count > 0;
public ChatMessage GetLastMessage() { ... }
```

### 6. Enum-ы в `Domain/Enums/`

Все перечисления — в отдельных файлах с логической группировкой:

```
Domain/Enums/
├── SoundType.cs         # Все звуки: Music_, UI_, SFX_
├── AudioEnums.cs        # AudioMixerGroupType, SoundPlaybackScope
└── SettingsEnums.cs     # QualityPreset, ColorblindType, GameLanguage, TargetFpsOption
```

### 7. Value Objects — immutable, с computed properties

```csharp
// SceneLoadPayload — readonly struct
public readonly struct SceneLoadPayload { ... }

// PerformanceMetrics — computed helpers
public float DurationSeconds => DurationMs / 1000f;
public string GetFormattedSize() { ... }
```

### 8. `HttpResponse<T>` — generic wrapper для сетевых ответов

```csharp
public static HttpResponse<T> Success(T data, long statusCode, ...);
public static HttpResponse<T> Failure(string error, long statusCode, ...);
```

---

## ❌ DON'T — что делать нельзя

### 1. НЕ класть бизнес-логику в DTO

DTO — **только** данные для сериализации. Никаких методов, computed properties, валидации.

```csharp
// ❌ ПЛОХО
public class ChatResponseDto
{
    public bool HasMessages => Messages?.Count > 0;  // НЕТ!
}

// ✅ Это — в ChatModel
public class ChatModel
{
    public bool HasMessages => Messages != null && Messages.Count > 0;
}
```

### 2. НЕ использовать DTO напрямую в MVVM/UI

DTO — промежуточный формат. В ViewModel/View — только Domain Model.

```csharp
// ❌ ПЛОХО
viewModel.SetChat(chatResponseDto);

// ✅ ХОРОШО
var model = ChatMapper.ToModel(chatResponseDto);
viewModel.SetChat(model);
```

### 3. НЕ мутировать `readonly struct` Element Data

`HeroSlotData`, `EquipmentSlotData` — immutable. Пересоздавай через конструктор.

### 4. НЕ дублировать маппинг-логику вне Mappers

Весь маппинг DTO → Model — только в `Domain/Mappers/` или `Core.Domain.Mappers`. Не писать ad-hoc конвертацию в сервисах.

### 5. НЕ забывать fallback для коллекций

```csharp
// ❌ NullReferenceException
Tags = dto.Tags,

// ✅ Всегда fallback
Tags = dto.Tags ?? new List<string>(),
```

### 6. НЕ смешивать namespace-ы DTO и Domain

```
Core.Data.Network.DTO.*    ← DTO
Core.Data.Domain.Models.*  ← Domain Models
Core.Data.Domain.Mappers   ← Mappers
```

### 7. НЕ использовать `[JsonProperty]` в Domain Models

`[JsonProperty]` — только в DTO. Domain Model не зависит от формата сериализации.
