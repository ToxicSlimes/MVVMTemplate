# PropertyDrawer System - Примеры использования

## Базовая архитектура

Система состоит из:
1. **ConfigEntryStyleAttribute** - атрибут для стилизации (цвета, иконки, настройки)
2. **BaseMissingRefsPropertyDrawer** - базовый PropertyDrawer с автоматической подсветкой missing refs
3. **ColorPresets** - готовые цветовые схемы (Blue, Green, Orange, Purple, Red, Pink, Cyan, Yellow)

## Пример 1: Простой Reference Entry

```csharp
// Runtime класс
[Serializable]
[ConfigEntryStyle(ColorPreset.Green, MissingRefFieldName = "Asset")]
public class AssetReference
{
    public string Name;
    public UnityEngine.Object Asset;
}

// Editor drawer
[CustomPropertyDrawer(typeof(AssetReference))]
public class AssetReferenceDrawer : BaseMissingRefsPropertyDrawer
{
    protected override void DrawPropertyContent(Rect contentRect, SerializedProperty property, GUIContent label, bool hasMissingRef)
    {
        var nameProp = property.FindPropertyRelative("Name");
        var assetProp = property.FindPropertyRelative("Asset");

        var nameRect = new Rect(contentRect.x, contentRect.y, 120f, contentRect.height);
        var assetRect = new Rect(nameRect.xMax + 4f, contentRect.y, contentRect.width - 124f - 24f, contentRect.height);

        EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
        EditorGUI.PropertyField(assetRect, assetProp, GUIContent.none);
    }
}
```

## Пример 2: Key-Value Entry

```csharp
// Runtime класс
[Serializable]
[ConfigEntryStyle(ColorPreset.Purple, ShowStatusIcon = false)]
public class StringKeyValue
{
    public string Key;
    public string Value;
}

// Editor drawer
[CustomPropertyDrawer(typeof(StringKeyValue))]
public class StringKeyValueDrawer : BaseMissingRefsPropertyDrawer
{
    protected override void DrawPropertyContent(Rect contentRect, SerializedProperty property, GUIContent label, bool hasMissingRef)
    {
        var keyProp = property.FindPropertyRelative("Key");
        var valueProp = property.FindPropertyRelative("Value");

        var keyRect = new Rect(contentRect.x, contentRect.y, contentRect.width * 0.35f, contentRect.height);
        var arrowRect = new Rect(keyRect.xMax + 4f, contentRect.y, 20f, contentRect.height);
        var valueRect = new Rect(arrowRect.xMax + 4f, contentRect.y, contentRect.width - keyRect.width - 28f, contentRect.height);

        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
        EditorGUI.LabelField(arrowRect, "→", EditorStyles.boldLabel);
        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
    }
}
```

## Пример 3: Enum + Reference Entry

```csharp
// Runtime класс
public enum SoundType { Music, SFX, Voice, Ambient }

[Serializable]
[ConfigEntryStyle(ColorPreset.Orange, IconEmoji = "🔊", MissingRefFieldName = "Clip")]
public class SoundEntry
{
    public SoundType Type;
    public AudioClip Clip;
}

// Editor drawer
[CustomPropertyDrawer(typeof(SoundEntry))]
public class SoundEntryDrawer : BaseMissingRefsPropertyDrawer
{
    protected override void DrawPropertyContent(Rect contentRect, SerializedProperty property, GUIContent label, bool hasMissingRef)
    {
        var typeProp = property.FindPropertyRelative("Type");
        var clipProp = property.FindPropertyRelative("Clip");

        var typeRect = new Rect(contentRect.x, contentRect.y, 80f, contentRect.height);
        var clipRect = new Rect(typeRect.xMax + 6f, contentRect.y, contentRect.width - 86f - 24f, contentRect.height);

        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);
        EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);
    }
}
```

## Пример 4: Nested Object Entry

```csharp
// Runtime класс
[Serializable]
[ConfigEntryStyle(ColorPreset.Cyan, BorderWidth = 6f)]
public class NestedConfigEntry
{
    public string Id;
    [Min(0)] public int Value;
    public bool Enabled;
}

// Editor drawer
[CustomPropertyDrawer(typeof(NestedConfigEntry))]
public class NestedConfigEntryDrawer : BaseMissingRefsPropertyDrawer
{
    protected override float ExtraHeight => 8f; // Больше высоты для вложенного объекта

    protected override void DrawPropertyContent(Rect contentRect, SerializedProperty property, GUIContent label, bool hasMissingRef)
    {
        var idProp = property.FindPropertyRelative("Id");
        var valueProp = property.FindPropertyRelative("Value");
        var enabledProp = property.FindPropertyRelative("Enabled");

        var idRect = new Rect(contentRect.x, contentRect.y, contentRect.width * 0.4f, contentRect.height);
        var valueRect = new Rect(idRect.xMax + 4f, contentRect.y, 60f, contentRect.height);
        var enabledRect = new Rect(valueRect.xMax + 4f, contentRect.y, 20f, contentRect.height);

        EditorGUI.PropertyField(idRect, idProp, GUIContent.none);
        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
        EditorGUI.PropertyField(enabledRect, enabledProp, GUIContent.none);
    }
}
```

## Доступные ColorPresets

```csharp
ColorPreset.Blue     // Синий (по умолчанию)
ColorPreset.Green    // Зеленый
ColorPreset.Orange   // Оранжевый
ColorPreset.Purple   // Фиолетовый
ColorPreset.Red      // Красный
ColorPreset.Pink     // Розовый
ColorPreset.Cyan     // Бирюзовый
ColorPreset.Yellow   // Желтый
```

## Настройки ConfigEntryStyleAttribute

```csharp
[ConfigEntryStyle(
    ColorPreset.Blue,                    // Цветовая схема
    IconEmoji = "🎨",                    // Emoji иконка (опционально)
    MissingRefFieldName = "MyField",     // Имя поля для проверки missing ref
    BorderWidth = 4f,                    // Ширина бордера (default: 4f)
    ShowStatusIcon = true                // Показывать ли иконки статуса (default: true)
)]
```

## Как создать новый PropertyDrawer

1. Создайте класс данных с атрибутом `[ConfigEntryStyle]`
2. Создайте PropertyDrawer, наследующий `BaseMissingRefsPropertyDrawer`
3. Переопределите `DrawPropertyContent()` для кастомной отрисовки
4. (Опционально) Переопределите `GetMissingRefProperty()` если нужна кастомная логика

Система автоматически:
- Применит цвета из атрибута
- Проверит missing references
- Нарисует warning/check иконки
- Подсветит missing refs красным цветом
