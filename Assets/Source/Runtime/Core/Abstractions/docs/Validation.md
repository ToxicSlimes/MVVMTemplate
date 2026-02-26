# Validation — Валидация и атрибуты

> Система валидации для Editor Inspector.

## 📋 Обзор атрибутов

| Атрибут | Назначение | Путь |
|---------|------------|------|
| `ValidateMissingRefsAttribute` | Валидация missing references | `Validation/ValidateMissingRefsAttribute.cs` |

---

## ValidateMissingRefsAttribute

**Путь:** `Validation/ValidateMissingRefsAttribute.cs`

```csharp
using System;
using UnityEngine;

namespace Core.Abstractions.Validation
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ValidateMissingRefsAttribute : PropertyAttribute
    {
        public string ErrorMessage { get; }
        public bool ShowWarningIcon { get; }

        public ValidateMissingRefsAttribute(string errorMessage = null, bool showWarningIcon = true)
        {
            ErrorMessage = errorMessage ?? "Missing reference detected!";
            ShowWarningIcon = showWarningIcon;
        }
    }
}
```

### Назначение

PropertyAttribute для отображения warning в Inspector когда ссылка на объект null.
Используется в кастомных PropertyDrawer для визуальной индикации проблем.

---

## 📖 Примеры использования

### Базовый пример

```csharp
public class PlayerController : MonoBehaviour
{
    [ValidateMissingRefs]
    [SerializeField] private Rigidbody _rigidbody;
    
    [ValidateMissingRefs("Animator component is required for animations")]
    [SerializeField] private Animator _animator;
    
    [ValidateMissingRefs(showWarningIcon: false)]
    [SerializeField] private AudioSource _audioSource;
}
```

### В конфигах

```csharp
[CreateAssetMenu(fileName = "ViewConfig", menuName = "Config/View")]
public class ViewConfig : ViewConfigBase
{
    [Group("prefabs")]
    [ValidateMissingRefs("Login view prefab is required")]
    [SerializeField] private GameObject _loginViewPrefab;
    
    [Group("prefabs")]
    [ValidateMissingRefs("Chat view prefab is required")]
    [SerializeField] private GameObject _chatViewPrefab;
    
    public GameObject LoginViewPrefab => _loginViewPrefab;
    public GameObject ChatViewPrefab => _chatViewPrefab;
}
```

### Кастомный PropertyDrawer

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Core.Abstractions.Validation;

[CustomPropertyDrawer(typeof(ValidateMissingRefsAttribute))]
public class ValidateMissingRefsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = attribute as ValidateMissingRefsAttribute;
        
        // Check if reference is null
        bool isMissing = property.objectReferenceValue == null;
        
        if (isMissing)
        {
            // Draw warning background
            var bgRect = position;
            bgRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.DrawRect(bgRect, new Color(1f, 0.5f, 0.5f, 0.2f));
            
            // Draw warning icon
            if (attr.ShowWarningIcon)
            {
                var iconRect = new Rect(position.x, position.y, 20, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("console.warnicon.sml"));
                position.x += 22;
                position.width -= 22;
            }
        }
        
        // Draw property field
        EditorGUI.PropertyField(position, property, label);
        
        // Draw error message if missing
        if (isMissing && !string.IsNullOrEmpty(attr.ErrorMessage))
        {
            var helpRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(helpRect, attr.ErrorMessage, MessageType.Warning);
        }
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var attr = attribute as ValidateMissingRefsAttribute;
        bool isMissing = property.objectReferenceValue == null;
        
        float height = EditorGUIUtility.singleLineHeight;
        
        if (isMissing && !string.IsNullOrEmpty(attr.ErrorMessage))
        {
            height += EditorGUIUtility.singleLineHeight + 2;
        }
        
        return height;
    }
}
#endif
```

---

## 🎨 Визуальное отображение

### Нормальное состояние (reference назначен)

```
[✓] Rigidbody    [ Rigidbody Component ]
```

### Warning состояние (reference null)

```
┌─────────────────────────────────────────┐
│ ⚠️  Rigidbody    [ None (Rigidbody) ]   │  <-- Красный фон
│     ⚠️ Rigidbody component is required  │  <-- Warning message
└─────────────────────────────────────────┘
```

---

## 🔧 Интеграция с ConfigBase

### ViewConfigBase.FindMissingReferences()

Базовый класс `ViewConfigBase` уже включает метод поиска missing references:

```csharp
[Button(ButtonSizes.Large), PropertyOrder(50)]
protected virtual void FindMissingReferences()
{
    var fields = GetType().GetFields(
        System.Reflection.BindingFlags.Instance | 
        System.Reflection.BindingFlags.NonPublic | 
        System.Reflection.BindingFlags.Public);
    
    int missing = 0;
    foreach (var field in fields)
    {
        if (typeof(Object).IsAssignableFrom(field.FieldType))
        {
            var value = field.GetValue(this) as Object;
            if (value == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Missing reference: {field.Name}");
                missing++;
            }
        }
    }
    
    if (missing == 0)
    {
        Debug.Log($"[{GetType().Name}] ✅ All references assigned!");
    }
    else
    {
        Debug.LogWarning($"[{GetType().Name}] ⚠️ {missing} missing references found");
    }
}
```

### Использование кнопки в Inspector

```csharp
[CreateAssetMenu(fileName = "HeroConfig", menuName = "Config/Hero")]
public class HeroConfig : ViewConfigBase
{
    [Group("prefabs")]
    [ValidateMissingRefs]
    [SerializeField] private GameObject _heroCardPrefab;
    
    [Group("prefabs")]
    [ValidateMissingRefs]
    [SerializeField] private GameObject _heroDetailsPrefab;
    
    // Кнопка "Find Missing References" автоматически доступна в Inspector
}
```

---

## 🏗️ Связь с ConfigEntryStyleAttribute

**Путь:** `Attributes/ConfigEntryStyleAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class ConfigEntryStyleAttribute : Attribute
{
    public string IconEmoji { get; set; }
    public ColorPreset ColorPreset { get; set; } = ColorPreset.Blue;
    public float BorderWidth { get; set; } = 4f;
    public bool ShowStatusIcon { get; set; } = true;
    public string MissingRefFieldName { get; set; }  // <-- Указывает какое поле проверять
}
```

### Пример: Подсветка missing references в TableList

```csharp
[ConfigEntryStyle(ColorPreset.Purple, "🎨", MissingRefFieldName = "Sprite")]
public struct ItemData
{
    public string Id;
    public string Name;
    public Sprite Sprite;  // Если null — entry подсветится красным
    public int Price;
}

[CreateAssetMenu(fileName = "ItemsConfig", menuName = "Config/Items")]
public class ItemsConfig : DataConfigBase
{
    [Group("data")]
    [TableList]
    [SerializeField] private List<ItemData> _items;
    
    public IReadOnlyList<ItemData> Items => _items;
}
```

**В Inspector:**
- Entries с `Sprite != null` → Purple border
- Entries с `Sprite == null` → Red border + warning

---

## 🎯 Best Practices

1. **Используй на критичных references** — не на всех полях, только на обязательных
2. **Добавляй понятные ErrorMessage** — объясни почему поле обязательно
3. **Комбинируй с Required** (Tri-Inspector) для двойной защиты
4. **Find Missing References** — добавляй кнопку в конфиги для batch check
5. **Build validation** — интегрируй с build pipeline для проверки перед сборкой

---

## 🔗 Связанные компоненты

- `ConfigBase` — базовый класс с встроенной валидацией
- `ViewConfigBase` — автоматический поиск missing references
- `ConfigEntryStyleAttribute` — визуальная индикация в TableList
- Tri-Inspector `[Required]` — альтернативная валидация
