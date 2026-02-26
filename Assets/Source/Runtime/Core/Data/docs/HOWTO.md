# HOWTO — Рецепты для Data Layer

> Практические рецепты: как добавлять DTO, модели, маппинг и enum-ы в слой Data.

*Обновлено: 2026-02-11*

---

## 📚 Содержание

- [1. Как добавить новый DTO](#1-как-добавить-новый-dto)
- [2. Как добавить новый Domain Model](#2-как-добавить-новый-domain-model)
- [3. Как создать Mapper](#3-как-создать-mapper)
- [4. Как добавить новый Enum](#4-как-добавить-новый-enum)
- [5. Как добавить Value Object](#5-как-добавить-value-object)
- [6. Как добавить Element Data (readonly struct для UI)](#6-как-добавить-element-data)

---

## 1. Как добавить новый DTO

**Сценарий:** API возвращает новый endpoint `/api/inventory/items`.

### Шаг 1. Создай файл в `Network/DTO/{Category}/`

```
Network/DTO/Inventory/InventoryItemDto.cs
```

### Шаг 2. Напиши DTO-класс

Ориентируйся на `CharacterResponseDto` или `ChatResponseDto`:

```csharp
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Core.Data.Network.DTO.Inventory
{
    public class InventoryItemDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
```

### Правила:
- Каждое поле — `[JsonProperty("camelCaseName")]`
- Все свойства — `{ get; set; }` (Newtonsoft.Json требует сеттер)
- Nullable типы для опциональных полей (`DateTime?`, `int?`, `string`)
- Никакой логики — только данные

### Шаг 3. Для списочного endpoint — создай Response wrapper

```csharp
public class InventoryListResponseDto
{
    [JsonProperty("items")]
    public List<InventoryItemDto> Items { get; set; }

    [JsonProperty("pagination")]
    public PaginationDto Pagination { get; set; }
}
```

Используй существующий `PaginationDto` из `Network/DTO/PaginationDto.cs`.

### Шаг 4. Добавь endpoint в `ApiEndpoints.cs`

```csharp
// Network/ApiEndpoints.cs
public static class Inventory
{
    public const string List = "/api/inventory/items";
    public const string ById = "/api/inventory/items/{0}";
}
```

---

## 2. Как добавить новый Domain Model

**Сценарий:** нужна бизнес-модель для инвентарного предмета.

### Шаг 1. Создай файл в `Domain/Models/{Category}/`

```
Domain/Models/Inventory/InventoryItem.cs
```

### Шаг 2. Напиши модель

Ориентируйся на `CharacterModel`, `ChatModel`:

```csharp
using System.Collections.Generic;

namespace Core.Data.Domain.Models.Inventory
{
    public class InventoryItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public ItemRarity Rarity { get; set; }
        public string IconUrl { get; set; }

        // Computed properties
        public bool IsStackable => Quantity > 1;
        public string GetDisplayName() => string.IsNullOrEmpty(Name) ? "Unknown Item" : Name;
    }
}
```

### Правила:
- **Без** `[JsonProperty]` — модель не знает о сериализации
- Только нужные поля (не все из DTO)
- Computed properties и helper-методы — ОК
- Enum-ы вместо строк где возможно (`Rarity` → `ItemRarity`)

---

## 3. Как создать Mapper

**Сценарий:** связать `InventoryItemDto` → `InventoryItem`.

### Шаг 1. Создай маппер в `Domain/Mappers/`

```
Domain/Mappers/InventoryMapper.cs
```

### Шаг 2. Напиши маппер

Ориентируйся на `CharacterMapper`, `ChatMapper`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Core.Data.Domain.Models.Inventory;
using Core.Data.Network.DTO.Inventory;

namespace Core.Data.Domain.Mappers
{
    public static class InventoryMapper
    {
        public static InventoryItem ToModel(InventoryItemDto dto)
        {
            if (dto == null) return null;

            return new InventoryItem
            {
                Id = dto.Id,
                Name = dto.Name,
                Quantity = dto.Quantity,
                Rarity = ParseRarity(dto.Rarity),
                IconUrl = dto.IconUrl
            };
        }

        public static List<InventoryItem> ToModels(IEnumerable<InventoryItemDto> dtos)
        {
            return dtos?.Select(ToModel).ToList() ?? new List<InventoryItem>();
        }

        private static ItemRarity ParseRarity(string value)
        {
            return value?.ToLowerInvariant() switch
            {
                "common" => ItemRarity.Common,
                "rare" => ItemRarity.Rare,
                "epic" => ItemRarity.Epic,
                "legendary" => ItemRarity.Legendary,
                _ => ItemRarity.Common
            };
        }
    }
}
```

### Чеклист маппера:
- [ ] `if (dto == null) return null;` — первая строка
- [ ] Коллекции: `?.Select(ToModel).ToList() ?? new List<T>()`
- [ ] String → Enum через `private static` Parse-метод
- [ ] `ToModel` + `ToModels` в каждом маппере
- [ ] Namespace: `Core.Data.Domain.Mappers`

---

## 4. Как добавить новый Enum

### Шаг 1. Определи, куда добавить

- **Новая категория** → новый файл в `Domain/Enums/`
- **Расширение существующего** → в существующий файл (например, `SettingsEnums.cs`)

### Шаг 2. Напиши enum

```csharp
// Domain/Enums/InventoryEnums.cs
namespace Core.Data.Domain.Enums
{
    public enum ItemRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum ItemCategory
    {
        Weapon,
        Armor,
        Consumable,
        Material
    }
}
```

### Правила:
- Числовые значения — если нужна сериализация или порядок (см. `SoundType.cs`: `Music_ = 1`, `UI_ = 100`, `SFX_ = 200`)
- Группировка по категории с помощью числовых диапазонов (паттерн из `SoundType`)
- Один namespace: `Core.Data.Domain.Enums`

---

## 5. Как добавить Value Object

### Вариант A: `readonly struct` (для лёгких, stack-allocated значений)

Ориентируйся на `SceneLoadPayload`:

```csharp
public readonly struct CurrencyAmount
{
    public readonly string Currency;
    public readonly int Amount;

    public CurrencyAmount(string currency, int amount)
    {
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Amount = amount;
    }

    public string GetFormatted() => $"{Amount} {Currency}";
}
```

### Вариант B: `class` с computed properties (для сложных VO)

Ориентируйся на `PerformanceMetrics`:

```csharp
public class PerformanceMetrics
{
    public long DurationMs { get; set; }
    public long ResponseSizeBytes { get; set; }

    // Computed
    public float DurationSeconds => DurationMs / 1000f;
    public float ResponseSizeKB => ResponseSizeBytes / 1024f;
}
```

### Размещение:
- `Domain/ValueObjects/` — для доменных VO
- `ValueObjects/` (корень Data) — для инфраструктурных VO (SceneLoadPayload)

---

## 6. Как добавить Element Data

**Сценарий:** новый UI-элемент (слот, карточка, строка).

Ориентируйся на `HeroSlotData`, `EquipmentSlotData`:

```csharp
using System;
using Source.Runtime.Core.Data.Domain.Models.Elements;

namespace Core.Data.Domain.Models.Elements
{
    public readonly struct QuestSlotData : IElementData, IEquatable<QuestSlotData>
    {
        public readonly string QuestId;
        public readonly string Title;
        public readonly float Progress;

        public string Id => QuestId;

        public QuestSlotData(string questId, string title, float progress)
        {
            QuestId = questId ?? throw new ArgumentNullException(nameof(questId));
            Title = title ?? string.Empty;
            Progress = Mathf.Clamp01(progress);
        }

        public bool Equals(QuestSlotData other) => QuestId == other.QuestId;
        public override bool Equals(object obj) => obj is QuestSlotData other && Equals(other);
        public override int GetHashCode() => QuestId?.GetHashCode() ?? 0;

        public static bool operator ==(QuestSlotData left, QuestSlotData right) => left.Equals(right);
        public static bool operator !=(QuestSlotData left, QuestSlotData right) => !left.Equals(right);
    }
}
```

### Чеклист:
- [ ] `readonly struct`
- [ ] Реализует `IElementData` (свойство `Id`)
- [ ] Реализует `IEquatable<T>` (по Id)
- [ ] Все поля `readonly`
- [ ] `ArgumentNullException` для обязательных полей
- [ ] `==` и `!=` операторы
