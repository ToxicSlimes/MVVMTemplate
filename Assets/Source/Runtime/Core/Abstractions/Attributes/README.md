# Attributes Abstractions

Кастомные атрибуты для Unity и конфигов.

## Типы

| Тип | Категория | Описание |
|-----|-----------|----------|
| `ConfigEntryStyleAttribute` | Attribute | Стилизация config entries в Inspector |
| `ColorPreset` | Enum | Цветовые пресеты (Blue, Green, Orange, Purple, Red, Pink, Cyan, Yellow) |
| `ColorPresets` | Static class | Хелпер для получения цветов из пресета |

## ConfigEntryStyleAttribute

Атрибут для стилизации записей в ScriptableObject конфигах.

### Параметры

| Параметр | Тип | По умолчанию | Описание |
|----------|-----|--------------|----------|
| `IconEmoji` | string | null | Emoji иконка для записи |
| `ColorPreset` | ColorPreset | Blue | Цветовая схема |
| `BorderWidth` | float | 4f | Ширина бордера |
| `ShowStatusIcon` | bool | true | Показывать статус-иконку |
| `MissingRefFieldName` | string | null | Имя поля для проверки missing ref |

### Использование

```csharp
using Core.Abstractions.Attributes;

[ConfigEntryStyle(ColorPreset.Green, "🎮")]
public class EnemyConfig
{
    public string Name;
    public int Health;
    public float Speed;
}

[ConfigEntryStyle(ColorPreset.Orange, "⚔️")]
public class WeaponConfig
{
    public string Name;
    public int Damage;
}
```

## ColorPreset

```csharp
public enum ColorPreset
{
    Blue,    // Синий — default
    Green,   // Зелёный — успех, готовность
    Orange,  // Оранжевый — предупреждение
    Purple,  // Фиолетовый — магия, особые
    Red,     // Красный — ошибка, опасность
    Pink,    // Розовый — UI, косметика
    Cyan,    // Бирюзовый — информация
    Yellow   // Жёлтый — награды, лут
}
```

## ColorPresets Helper

```csharp
// Получить цвета для пресета
(Color background, Color border) = ColorPresets.GetColors(ColorPreset.Green, isMissing: false);

// isMissing = true возвращает красные цвета (для подсветки missing refs)
```

## Реализация

Drawer находится в `Editor/` папке проекта.
