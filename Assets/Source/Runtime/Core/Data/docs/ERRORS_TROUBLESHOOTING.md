# Типичные ошибки и Troubleshooting — Data Layer

> Частые проблемы при работе с DTO, маппингом и сериализацией.

*Обновлено: 2026-02-11*

---

## 1. NullReferenceException при маппинге коллекций

### Симптом
```
NullReferenceException: Object reference not set to an instance of an object
  at ChatMapper.ToModel(ChatResponseDto dto)
```

### Причина
API вернул `null` вместо массива (например, `messages: null`).

### Решение
Всегда используй null-coalescing для коллекций:

```csharp
// ❌ Упадёт если dto.Messages == null
Messages = dto.Messages.Select(ToModel).ToList(),

// ✅ Правильно
Messages = dto.Messages?.Select(ToModel).ToList() ?? new List<ChatMessage>(),
```

Проверяй все маппинг-строки с `.Select()`.

---

## 2. Несовпадение DTO с API response

### Симптом
Поле в модели всегда `null` / `0` / `default`, хотя API возвращает данные.

### Причина
Имя в `[JsonProperty]` не совпадает с JSON-ключом от API.

### Диагностика
1. Логируй raw JSON response:
```csharp
Debug.Log($"Raw response: {rawJson}");
```
2. Сравни ключи в JSON с `[JsonProperty("...")]` в DTO

### Типичные ловушки
```csharp
// API: "gmtCreate" → DTO:
[JsonProperty("gmtCreate")]       // ✅
[JsonProperty("createdAt")]       // ❌ не то имя

// API: "descriptions" (Dictionary) → не "description" (string)
[JsonProperty("descriptions")]
public Dictionary<string, string> Descriptions { get; set; }  // ✅
```

Пример из проекта — `CharacterResponseDto.Descriptions` это `Dictionary<string, string>`, а `CharacterModel.Description` — уже `string`. Маппинг в `CharacterMapper`:
```csharp
Description = dto.Descriptions != null && dto.Descriptions.Count > 0
    ? dto.Descriptions.FirstOrDefault().Value
    : null,
```

---

## 3. Missing Mapper — забыли создать маппер

### Симптом
Новый DTO создан, но в сервисе работают напрямую с DTO вместо Domain Model.

### Чеклист при добавлении нового endpoint
1. DTO в `Network/DTO/{Category}/`
2. Domain Model в `Domain/Models/{Category}/`
3. **Mapper** в `Domain/Mappers/{Category}Mapper.cs`
4. Endpoint в `ApiEndpoints.cs`

### Как найти DTO без маппера
```bash
# Все DTO файлы
find Data/Network/DTO -name "*Dto.cs" | sed 's/.*\///' | sed 's/Dto.cs//'

# Все маппер-файлы
find Data/Domain/Mappers -name "*Mapper.cs"

# Сравни вручную
```

---

## 4. Serialization проблемы с Newtonsoft.Json

### 4.1 Enum десериализуется как 0

**Причина:** JSON содержит строку (`"rarity": "epic"`), а DTO — `enum` без `[JsonConverter]`.

**Решение:** Держи строку в DTO, парси в маппере:
```csharp
// DTO
[JsonProperty("rarity")]
public string Rarity { get; set; }

// Mapper
Rarity = ParseRarity(dto.Rarity),

private static ItemRarity ParseRarity(string value) =>
    value?.ToLowerInvariant() switch
    {
        "common" => ItemRarity.Common,
        "rare" => ItemRarity.Rare,
        _ => ItemRarity.Common
    };
```

### 4.2 DateTime парсится неправильно

**Причина:** API возвращает ISO 8601 или Unix timestamp, а Newtonsoft ожидает другой формат.

**Решение из проекта** (`ChatExtensionsUiMapper`):
```csharp
private static DateTime ParseTimestamp(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return DateTime.MinValue;

    return DateTime.TryParse(
        value,
        CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
        out var parsed) ? parsed : DateTime.MinValue;
}
```

### 4.3 JObject / JArray вместо конкретного типа

**Причина:** API возвращает dynamic JSON (разные форматы в одном поле).

**Решение из проекта** (`WorkflowMapper.ToModel(WorkflowTriggerResponseDto)`):
```csharp
var rawOutput = dto.Output ?? dto.Result;

var output = rawOutput switch
{
    JObject jobj => jobj.ToObject<Dictionary<string, object>>(),
    JArray jarr => new Dictionary<string, object>
    {
        { "components", jarr.ToObject<List<object>>() }
    },
    Dictionary<string, object> dict => dict,
    _ => null
};
```

### 4.4 `object` поле теряет тип при десериализации

**Причина:** `Dictionary<string, object>` — Newtonsoft десериализует числа как `long`, а не `int`.

**Решение из проекта** (`StandaloneUiMapper`):
```csharp
private static int GetIntFromConfig(Dictionary<string, object> config, string key, int defaultValue = 0)
{
    if (config == null || !config.TryGetValue(key, out var value))
        return defaultValue;
    
    if (value is int intVal) return intVal;
    if (value is long longVal) return (int)longVal;
    if (value is double doubleVal) return (int)doubleVal;
    if (int.TryParse(value?.ToString(), out var parsed)) return parsed;
    return defaultValue;
}
```

---

## 5. `readonly struct` не сериализуется

### Симптом
`HeroSlotData` или другой `readonly struct` невозможно сериализовать в JSON.

### Причина
`readonly struct` без сеттеров — Newtonsoft.Json не может записать поля.

### Решение
`readonly struct` — **не для сериализации**. Это UI data. Если нужен JSON — используй DTO (class с `[JsonProperty]`).

---

## 6. `HttpResponse<T>` — проверка ошибок

### Симптом
Данные null, но ошибка не обработана.

### Правильная обработка
```csharp
var response = await apiClient.GetAsync<CharacterResponseDto>(url);

if (!response.IsSuccess)
{
    Debug.LogError($"API error {response.StatusCode}: {response.Error}");
    return;
}

var model = CharacterMapper.ToModel(response.Data);
```
