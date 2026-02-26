# Testing Guide — Data Layer

> Как тестировать маппинг, сериализацию и модели в слое Data.

*Обновлено: 2026-02-11*

---

## 1. Что тестировать

| Что | Зачем | Приоритет |
|-----|-------|-----------|
| Mapper (DTO → Model) | Корректность преобразования, null-safety | 🔴 Высокий |
| JSON → DTO десериализация | Совместимость с API | 🔴 Высокий |
| Domain Model computed properties | Бизнес-логика | 🟡 Средний |
| Value Objects | Equality, immutability | 🟢 Низкий |

---

## 2. Тесты маппинга (Mapper Tests)

### Базовый шаблон

```csharp
using NUnit.Framework;
using Core.Data.Domain.Mappers;
using Core.Data.Network.DTO.Character;

[TestFixture]
public class CharacterMapperTests
{
    [Test]
    public void ToModel_NullDto_ReturnsNull()
    {
        var result = CharacterMapper.ToModel((CharacterResponseDto)null);
        Assert.IsNull(result);
    }

    [Test]
    public void ToModel_ValidDto_MapsAllFields()
    {
        var dto = new CharacterResponseDto
        {
            Id = "char-123",
            Name = "Test Hero",
            Slug = "test-hero",
            Visibility = "public",
            ChatCount = 42,
            Tags = new() { "fantasy", "warrior" },
            Greetings = new()
            {
                new CharacterGreetingDto
                {
                    Id = "greet-1",
                    Title = "Default",
                    Content = "Hello!",
                    IsDefault = true
                }
            }
        };

        var model = CharacterMapper.ToModel(dto);

        Assert.AreEqual("char-123", model.Id);
        Assert.AreEqual("Test Hero", model.Name);
        Assert.AreEqual(42, model.ChatCount);
        Assert.AreEqual(2, model.Tags.Count);
        Assert.AreEqual(1, model.Greetings.Count);
        Assert.AreEqual("Hello!", model.Greetings[0].Content);
    }

    [Test]
    public void ToModel_NullCollections_ReturnEmptyLists()
    {
        var dto = new CharacterResponseDto
        {
            Id = "char-123",
            Tags = null,
            Greetings = null
        };

        var model = CharacterMapper.ToModel(dto);

        Assert.IsNotNull(model.Tags);
        Assert.IsNotNull(model.Greetings);
        Assert.AreEqual(0, model.Tags.Count);
        Assert.AreEqual(0, model.Greetings.Count);
    }

    [Test]
    public void ToModels_NullEnumerable_ReturnsEmptyList()
    {
        var result = CharacterMapper.ToModels((IEnumerable<CharacterResponseDto>)null);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void ToModel_Descriptions_MapsFirstValue()
    {
        var dto = new CharacterResponseDto
        {
            Id = "char-123",
            Descriptions = new() { { "en", "A brave warrior" } }
        };

        var model = CharacterMapper.ToModel(dto);

        Assert.AreEqual("A brave warrior", model.Description);
    }

    [Test]
    public void ToModel_EmptyDescriptions_ReturnsNull()
    {
        var dto = new CharacterResponseDto
        {
            Id = "char-123",
            Descriptions = new()
        };

        var model = CharacterMapper.ToModel(dto);

        Assert.IsNull(model.Description);
    }
}
```

### Маппер с enum-парсингом

```csharp
[TestFixture]
public class ChatExtensionsUiMapperTests
{
    [TestCase("above_input", UiPlacement.AboveInput)]
    [TestCase("below_chat", UiPlacement.BelowChat)]
    [TestCase("sidebar", UiPlacement.Sidebar)]
    [TestCase("unknown_value", UiPlacement.Unknown)]
    [TestCase(null, UiPlacement.Unknown)]
    public void ToModel_ParsesPlacement(string input, UiPlacement expected)
    {
        var dto = new ChatUiComponentDto
        {
            Id = "comp-1",
            Placement = input,
            Type = "display"
        };

        var model = ChatExtensionsUiMapper.ToModel(dto);

        Assert.AreEqual(expected, model.Placement);
    }
}
```

---

## 3. Тесты сериализации (JSON → DTO)

### Проверка что JSON правильно десериализуется

```csharp
using Newtonsoft.Json;
using NUnit.Framework;

[TestFixture]
public class ChatResponseDtoSerializationTests
{
    [Test]
    public void Deserialize_ValidJson_AllFieldsMapped()
    {
        var json = @"{
            ""id"": ""chat-456"",
            ""title"": ""Adventure Chat"",
            ""createdAt"": ""2026-01-15T10:30:00Z"",
            ""messageCount"": 15,
            ""pinnedStatus"": true,
            ""messages"": [
                {
                    ""id"": ""msg-1"",
                    ""role"": ""user"",
                    ""content"": ""Hello""
                }
            ]
        }";

        var dto = JsonConvert.DeserializeObject<ChatResponseDto>(json);

        Assert.AreEqual("chat-456", dto.Id);
        Assert.AreEqual("Adventure Chat", dto.Title);
        Assert.AreEqual(15, dto.MessageCount);
        Assert.IsTrue(dto.PinnedStatus);
        Assert.AreEqual(1, dto.Messages.Count);
    }

    [Test]
    public void Deserialize_MissingOptionalFields_DefaultValues()
    {
        var json = @"{ ""id"": ""chat-789"" }";

        var dto = JsonConvert.DeserializeObject<ChatResponseDto>(json);

        Assert.AreEqual("chat-789", dto.Id);
        Assert.IsNull(dto.Title);
        Assert.IsNull(dto.Messages);
        Assert.IsNull(dto.LastMessageAt);
        Assert.AreEqual(0, dto.MessageCount);
    }

    [Test]
    public void Deserialize_NullableFields_HandledCorrectly()
    {
        var json = @"{
            ""id"": ""chat-1"",
            ""lastUsedTemperature"": 0.7,
            ""lastUsedTopK"": null,
            ""deletedAt"": null
        }";

        var dto = JsonConvert.DeserializeObject<ChatResponseDto>(json);

        Assert.AreEqual(0.7f, dto.LastUsedTemperature);
        Assert.IsNull(dto.LastUsedTopK);
        Assert.IsNull(dto.DeletedAt);
    }
}
```

---

## 4. Тесты Domain Model (computed properties)

```csharp
[TestFixture]
public class ChatModelTests
{
    [Test]
    public void HasMessages_WithMessages_ReturnsTrue()
    {
        var model = new ChatModel
        {
            Messages = new() { new ChatMessage { Id = "1" } }
        };

        Assert.IsTrue(model.HasMessages);
    }

    [Test]
    public void HasMessages_NullMessages_ReturnsFalse()
    {
        var model = new ChatModel { Messages = null };
        Assert.IsFalse(model.HasMessages);
    }

    [Test]
    public void GetLastMessage_ReturnsLatestByDate()
    {
        var model = new ChatModel
        {
            Messages = new()
            {
                new ChatMessage { Id = "old", CreatedAt = DateTime.Parse("2026-01-01") },
                new ChatMessage { Id = "new", CreatedAt = DateTime.Parse("2026-01-15") }
            }
        };

        var last = model.GetLastMessage();
        Assert.AreEqual("new", last.Id);
    }
}

[TestFixture]
public class CharacterModelTests
{
    [Test]
    public void GetDisplayName_EmptyName_ReturnsDefault()
    {
        var model = new CharacterModel { Name = "" };
        Assert.AreEqual("Unnamed Character", model.GetDisplayName());
    }

    [Test]
    public void GetDefaultGreeting_ReturnsDefaultOrFirst()
    {
        var model = new CharacterModel
        {
            Greetings = new()
            {
                new CharacterGreeting { Id = "1", IsDefault = false },
                new CharacterGreeting { Id = "2", IsDefault = true }
            }
        };

        var greeting = model.GetDefaultGreeting();
        Assert.AreEqual("2", greeting.Id);
    }
}
```

---

## 5. Тесты Element Data (readonly struct)

```csharp
[TestFixture]
public class HeroSlotDataTests
{
    [Test]
    public void Equality_SameId_AreEqual()
    {
        var a = new HeroSlotData("hero-1", "Name A", null, 5);
        var b = new HeroSlotData("hero-1", "Name B", null, 10);

        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
    }

    [Test]
    public void Equality_DifferentId_AreNotEqual()
    {
        var a = new HeroSlotData("hero-1", "Name", null, 5);
        var b = new HeroSlotData("hero-2", "Name", null, 5);

        Assert.AreNotEqual(a, b);
    }

    [Test]
    public void Constructor_NullId_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new HeroSlotData(null, "Name", null, 1));
    }

    [Test]
    public void HealthPercent_Clamped()
    {
        var data = new HeroSlotData("hero-1", "Name", null, 1, healthPercent: 1.5f);
        Assert.AreEqual(1f, data.HealthPercent);
    }
}
```

---

## 6. Организация тестов

```
Tests/
├── Data/
│   ├── Mappers/
│   │   ├── ChatMapperTests.cs
│   │   ├── CharacterMapperTests.cs
│   │   ├── MessageMapperTests.cs
│   │   ├── WorkflowMapperTests.cs
│   │   └── ChatExtensionsUiMapperTests.cs
│   ├── Serialization/
│   │   ├── ChatResponseDtoSerializationTests.cs
│   │   ├── CharacterResponseDtoSerializationTests.cs
│   │   └── WorkflowResponseDtoSerializationTests.cs
│   ├── Models/
│   │   ├── ChatModelTests.cs
│   │   └── CharacterModelTests.cs
│   └── Elements/
│       ├── HeroSlotDataTests.cs
│       └── EquipmentSlotDataTests.cs
```

### Naming convention
- `{Class}Tests.cs`
- `{Method}_{Scenario}_{Expected}` — имя теста
- Пример: `ToModel_NullDto_ReturnsNull`
