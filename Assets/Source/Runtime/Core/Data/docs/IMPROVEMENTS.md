# Техдолг и Улучшения — Data Layer

> Что можно улучшить в слое Data. Приоритезировано по влиянию.

*Обновлено: 2026-02-11*

---

## 🔴 Высокий приоритет

### 1. Domain Models — мутабельные классы

**Проблема:** `ChatModel`, `CharacterModel`, `MessageModel` — обычные классы с `{ get; set; }`. Любой может изменить данные после маппинга.

**Где:** `Domain/Models/Chat/ChatModel.cs`, `Domain/Models/Character/CharacterModel.cs`, `Domain/Models/Message/MessageModel.cs`

**Решение:** Перевести на `init`-сеттеры или конструкторы. Или `record`:
```csharp
// Сейчас
public class ChatModel
{
    public string Id { get; set; }  // мутабельно
}

// Целевое
public class ChatModel
{
    public string Id { get; init; }  // immutable после создания
}
```

### 2. Дублирование маппинг-логики для dynamic JSON

**Проблема:** `WorkflowMapper` и `StandaloneUiMapper` оба содержат практически идентичные helper-методы: `GetStringFromConfig`, `GetIntFromConfig`, `GetFloatFromConfig`, `GetBoolFromConfig`.

**Где:** `Core.Domain.Mappers.WorkflowMapper`, `Core.Domain.Mappers.StandaloneUiMapper`

**Решение:** Вынести в shared `ConfigHelper` utility:
```csharp
public static class ConfigHelper
{
    public static string GetString(Dictionary<string, object> config, string key, string def = null) { ... }
    public static int GetInt(Dictionary<string, object> config, string key, int def = 0) { ... }
}
```

### 3. Inconsistent namespaces для Mappers

**Проблема:** Mappers разбросаны по двум namespace-ам:
- `Core.Data.Domain.Mappers` — ChatMapper, CharacterMapper, MessageMapper
- `Core.Domain.Mappers` — WorkflowMapper, ChatExtensionsUiMapper, StandaloneUiMapper

**Решение:** Унифицировать namespace. Предпочтительно `Core.Data.Domain.Mappers`.

---

## 🟡 Средний приоритет

### 4. `PerformanceMetrics` — не настоящий Value Object

**Проблема:** `PerformanceMetrics` в `Domain/ValueObjects/` — мутабельный класс с `{ get; set; }`. Это не VO по определению.

**Решение:** `record` или `readonly` свойства:
```csharp
public record PerformanceMetrics(
    string RequestName,
    long DurationMs,
    long ResponseSizeBytes,
    DateTime Timestamp,
    bool IsSuccess,
    string ErrorMessage = null)
{
    public float DurationSeconds => DurationMs / 1000f;
}
```

### 5. Parse-методы для enum дублируются

**Проблема:** `ParsePlacement`, `ParseComponentType`, `ParseButtonVariant`, `ParseActionType` — дублируются между `ChatExtensionsUiMapper` и `StandaloneUiMapper` с небольшими различиями (разные default-ы, разные наборы значений).

**Решение:** Консолидировать в один набор Parse-методов или использовать extension methods на enum-ах.

### 6. Нет валидации в Domain Models

**Проблема:** Модели не проверяют инварианты. `CharacterModel.Id` может быть null после маппинга.

**Решение:** Добавить Guard-проверки хотя бы для ключевых полей (Id).

---

## 🟢 Низкий приоритет

### 7. UI Data structures — отсутствие `IEquatable` у некоторых

**Проблема:** Не все Element Data structs реализуют `IEquatable<T>`. Некоторые могут использовать boxing при сравнении.

### 8. `ApiEndpoints` — magic strings в format patterns

**Проблема:** `"/api/inventory/items/{0}"` — format-строки с индексами.

**Решение:** Рассмотреть typed builder:
```csharp
public static string ById(string id) => $"/api/inventory/items/{id}";
```

### 9. Нет README или summary для каждой DTO-категории

**Проблема:** 7 папок DTO (Auth, Chat, Character, Message, Workflow, Persona, Gemini) без inline-документации о том, какие endpoints они покрывают.

---

## 📊 Сводка

| # | Задача | Приоритет | Сложность |
|---|--------|-----------|-----------|
| 1 | Immutable Domain Models | 🔴 | Средняя |
| 2 | Shared ConfigHelper | 🔴 | Лёгкая |
| 3 | Unified Mapper namespaces | 🔴 | Лёгкая |
| 4 | PerformanceMetrics → record | 🟡 | Лёгкая |
| 5 | Consolidate Parse methods | 🟡 | Средняя |
| 6 | Domain Model validation | 🟡 | Средняя |
| 7 | IEquatable everywhere | 🟢 | Лёгкая |
| 8 | Typed ApiEndpoints | 🟢 | Лёгкая |
| 9 | DTO category docs | 🟢 | Лёгкая |
