# Расширяемая система PropertyDrawers для конфигов

## 🎯 Что это?

Универсальная система для создания красивых PropertyDrawers с автоматической подсветкой missing references, цветовыми схемами и визуальными индикаторами.

## 🏗️ Архитектура

```
ConfigEntryStyleAttribute ←─ Атрибут для стилизации (цвета, настройки)
        │
        ↓
BaseMissingRefsPropertyDrawer ←─ Базовый класс с логикой подсветки
        │
        ├─ ComponentPrefabEntryDrawer  ←─ ProceduralUI конфиг
        ├─ AssetReferenceDrawer        ←─ Пример: референсы на assets
        ├─ StringKeyValueDrawer        ←─ Пример: key-value pairs
        └─ [YourCustomDrawer]          ←─ Ваши custom drawers!
```

## ✨ Возможности

1. **Автоматическая подсветка missing refs** - красный фон + warning иконка
2. **Цветовые схемы** - 8 готовых presets (Blue, Green, Orange, Purple, Red, Pink, Cyan, Yellow)
3. **Визуальные индикаторы** - warning/check иконки, цветные borders
4. **Декларативная стилизация** - атрибуты вместо кода
5. **Переиспользование** - один базовый класс для всех drawers

## 🚀 Быстрый старт

### 1. Создайте класс данных

```csharp
using System;
using Core.Abstractions.Attributes;
using UnityEngine;

[Serializable]
[ConfigEntryStyle(ColorPreset.Blue, MissingRefFieldName = "Prefab")]
public class MyConfigEntry
{
    public string Name;
    public GameObject Prefab;
}
```

### 2. Создайте PropertyDrawer

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MissingReferencesVisualizer;

[CustomPropertyDrawer(typeof(MyConfigEntry))]
public class MyConfigEntryDrawer : BaseMissingRefsPropertyDrawer
{
    protected override void DrawPropertyContent(Rect contentRect, SerializedProperty property, GUIContent label, bool hasMissingRef)
    {
        var nameProp = property.FindPropertyRelative("Name");
        var prefabProp = property.FindPropertyRelative("Prefab");

        var nameRect = new Rect(contentRect.x, contentRect.y, 120f, contentRect.height);
        var prefabRect = new Rect(nameRect.xMax + 4f, contentRect.y, contentRect.width - 124f - 24f, contentRect.height);

        EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
        EditorGUI.PropertyField(prefabRect, prefabProp, GUIContent.none);
    }
}
#endif
```

### 3. Используйте в конфиге

```csharp
public class MyConfig : ScriptableObject
{
    [SerializeField]
    private MyConfigEntry[] _entries;
}
```

**Готово!** ✅ Система автоматически:
- Проверит missing references
- Применит цветовую схему
- Нарисует warning/check иконки
- Подсветит проблемные элементы

## 📖 Примеры использования

См. [PropertyDrawer.EXAMPLES.md](PropertyDrawer.EXAMPLES.md) для подробных примеров:
- Простой Reference Entry
- Key-Value Entry
- Enum + Reference Entry
- Nested Object Entry

## 🎨 Цветовые схемы

| Preset | Описание | Применение |
|--------|----------|------------|
| `Blue` | Синий (default) | Общие конфиги, prefabs |
| `Green` | Зеленый | Assets, ресурсы |
| `Orange` | Оранжевый | Audio, sound |
| `Purple` | Фиолетовый | Settings, params |
| `Red` | Красный | Critical, errors |
| `Pink` | Розовый | UI, visual |
| `Cyan` | Бирюзовый | Network, data |
| `Yellow` | Желтый | Warnings, temp |

## ⚙️ Настройки атрибута

```csharp
[ConfigEntryStyle(
    ColorPreset.Blue,                    // Цветовая схема
    IconEmoji = "🎨",                    // Emoji иконка (не используется в текущей реализации)
    MissingRefFieldName = "MyField",     // Имя поля для автоматической проверки missing ref
    BorderWidth = 4f,                    // Ширина бордера слева (default: 4f)
    ShowStatusIcon = true                // Показывать ли warning/check иконки (default: true)
)]
```

## 🔧 Расширение системы

### Переопределение логики missing ref

```csharp
protected override SerializedProperty GetMissingRefProperty(SerializedProperty property)
{
    // Кастомная логика для поиска missing ref property
    return property.FindPropertyRelative("MyCustomField");
}
```

### Кастомные цвета

```csharp
protected override (Color background, Color border) GetColors(bool isMissing)
{
    if (isMissing)
        return (Color.red, Color.red);

    return (new Color(1f, 0.5f, 0f, 0.1f), new Color(1f, 0.5f, 0f, 0.5f));
}
```

### Изменение высоты элемента

```csharp
protected override float ExtraHeight => 8f; // Больше места для контента
```

### Отключение иконок статуса

```csharp
[ConfigEntryStyle(ColorPreset.Blue, ShowStatusIcon = false)]
```

## 📋 Checklist для нового PropertyDrawer

- [ ] Создал класс данных с `[Serializable]`
- [ ] Добавил атрибут `[ConfigEntryStyle(...)]` с настройками
- [ ] Указал `MissingRefFieldName` если нужна проверка missing ref
- [ ] Создал PropertyDrawer наследующий `BaseMissingRefsPropertyDrawer`
- [ ] Переопределил `DrawPropertyContent()` для кастомной отрисовки
- [ ] Протестировал с missing references
- [ ] Проверил что Tri-Inspector кнопки работают

## ⚠️ Важные замечания

1. **PropertyDrawer vs Tri-Inspector**: PropertyDrawer применяется к **конкретному типу** (например, ComponentPrefabEntry), не конфликтует с Tri-Inspector
2. **Missing Refs Validation**: Работает только если включен MissingReferencesVisualizer (Tools → Missing References → Enable/Disable)
3. **Tri-Inspector кнопки**: Стилизация кнопок Tri-Inspector требует custom processor (выходит за рамки PropertyDrawer системы)
4. **Performance**: Атрибуты кэшируются, нет overhead на каждый OnGUI

## 📁 Структура файлов

```
Assets/Source/
├── Runtime/Core/Abstractions/Attributes/
│   └── ConfigEntryStyleAttribute.cs          ← Атрибут стилизации
├── Editor/MissingReferencesVisualizer/
│   ├── BaseMissingRefsPropertyDrawer.cs      ← Базовый класс
│   ├── PropertyDrawer.README.md              ← Эта документация
│   └── PropertyDrawer.EXAMPLES.md            ← Примеры
└── Editor/ProceduralUI/
    └── ComponentPrefabEntryDrawer.cs         ← Пример реализации
```

## 🎯 Примеры в проекте

- **ComponentPrefabEntryDrawer** - ProceduralUI prefabs с emoji иконками
- См. `PropertyDrawer.EXAMPLES.md` для дополнительных примеров

## 🐛 Troubleshooting

**PropertyDrawer не применяется:**
- Проверьте что атрибут `[CustomPropertyDrawer(typeof(YourType))]` указан правильно
- Unity Editor может требовать перекомпиляции (Ctrl+R)

**Missing refs не подсвечиваются:**
- Включите MissingReferencesVisualizer: `Tools → Missing References`
- Убедитесь что `MissingRefFieldName` указывает на правильное поле

**Tri-Inspector кнопки не видны:**
- Удалите любые custom Editor для ServiceConfigBase/ProceduralUiConfig
- PropertyDrawer не должен перехватывать весь рендеринг ScriptableObject

## 📚 Дополнительная документация

- [MissingReferencesVisualizer](../MissingReferencesVisualizer/README.md) - Система подсветки missing refs
- [Tri-Inspector GitHub](https://github.com/codewriter-packages/Tri-Inspector) - Документация Tri-Inspector

---

**Версия**: 1.0
**Создано**: 2026-01-21
**Автор**: EndlessAdventure Team
