# Config System — Abstractions

> ⚠️ **Полная документация:** `docs/config-system.md`

Базовые классы для ScriptableObject конфигов с **Tri-Inspector**.

## Базовые классы

| Класс | Использование | Иконка |
|-------|---------------|--------|
| `ConfigBase` | Любой конфиг | 📋 |
| `ServiceConfigBase` | Конфиги сервисов | 🔧 |
| `DataConfigBase` | Данные, таблицы | 📦 |
| `ViewConfigBase` | UI, prefabs, стили | 🖼️ |

## Быстрый старт

```csharp
using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MyConfig", menuName = "Game/My Config")]
public class MyConfig : ServiceConfigBase
{
    [Group("service")]
    [Required]
    [SerializeField] private string _apiUrl;

    [Group("pool")]
    [Range(1, 100)]
    [SerializeField] private int _poolSize = 10;

    [Group("debug")]
    [SerializeField] private bool _enableLogs;
}
```

## Популярные атрибуты Tri-Inspector

### Группировка

```csharp
// Box группа
[DeclareBoxGroup("settings", Title = "⚙️ Settings")]
[Group("settings")]
public string field;

// Foldout группа (сворачиваемая)
[DeclareFoldoutGroup("advanced", Title = "🔧 Advanced")]
[Group("advanced")]
public int value;

// Табы
[DeclareTabGroup("tabs")]
[Tab("tabs", "General")]
public string general;
[Tab("tabs", "Advanced")]
public int advanced;

// Горизонтальная группа
[DeclareHorizontalGroup("row")]
[Group("row")]
public float x, y, z;
```

### Валидация

```csharp
[Required]                      // Обязательное поле
[ValidateInput(nameof(Validate))] // Кастомная валидация
[InfoBox("Важная информация")]  // Информационный блок
```

### Условное отображение

```csharp
[ShowIf(nameof(_enabled))]      // Показать если true
[HideIf(nameof(_disabled))]     // Скрыть если true
[EnableIf(nameof(_active))]     // Активно если true
[DisableIf(nameof(_locked))]    // Заблокировано если true
```

### Визуализация

```csharp
[Title("Section Title")]        // Заголовок
[LabelText("Custom Label")]     // Кастомный label
[HideLabel]                     // Без label
[ReadOnly]                      // Только чтение
[PropertyOrder(10)]             // Порядок
[PropertySpace(10)]             // Отступ
```

### Коллекции

```csharp
[TableList]                     // Таблица
[ListDrawerSettings]            // Настройки списка
[InlineEditor]                  // Inline редактор для SO
```

### Кнопки

```csharp
[Button]                        // Простая кнопка (имя из метода)
[Button(ButtonSizes.Large)]     // Большая кнопка
[Button(ButtonSizes.Medium)]    // Средняя кнопка
[ButtonGroup("actions")]        // Группа кнопок
// Название кнопки = имя метода, поэтому называй методы красиво!
```

## Где хранить конфиги

Все конфиги должны храниться в:
```
Assets/Source/00. Configs/
├── Services/
│   ├── NetworkConfig.asset
│   └── ProceduralUiConfig.asset
├── Data/
│   └── GameData.asset
└── Views/
    └── DesignSystemSettings.asset
```

## Пример полного конфига

```csharp
using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
[DeclareTabGroup("tabs")]
[DeclareBoxGroup("tabs/General", Title = "🎮 General")]
[DeclareBoxGroup("tabs/Balance", Title = "⚖️ Balance")]
[DeclareBoxGroup("tabs/Debug", Title = "🐛 Debug")]
public class GameConfig : DataConfigBase
{
    [Tab("tabs", "General")]
    [Group("tabs/General")]
    [Required]
    [SerializeField] private string _gameName = "My Game";

    [Group("tabs/General")]
    [SerializeField] private Sprite _gameIcon;

    [Tab("tabs", "Balance")]
    [Group("tabs/Balance")]
    [Range(0.1f, 10f)]
    [SerializeField] private float _difficultyMultiplier = 1f;

    [Group("tabs/Balance")]
    [TableList]
    [SerializeField] private List<LevelData> _levels;

    [Tab("tabs", "Debug")]
    [Group("tabs/Debug")]
    [SerializeField] private bool _godMode;

    [Group("tabs/Debug")]
    [ShowIf(nameof(_godMode))]
    [SerializeField] private int _startGold = 9999;

    [System.Serializable]
    public class LevelData
    {
        [TableColumnWidth(60)]
        public int Level;
        
        [TableColumnWidth(100)]
        public string Name;
        
        public int RequiredXP;
    }

    protected override void ValidateConfig()
    {
        base.ValidateConfig();
        
        if (_levels != null)
        {
            for (int i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].Level != i + 1)
                {
                    Debug.LogWarning($"Level {i} has incorrect level number!");
                }
            }
        }
    }
}
```

## Документация

- [Tri-Inspector GitHub](https://github.com/codewriter-packages/Tri-Inspector)
- [Tri-Inspector Wiki](https://github.com/codewriter-packages/Tri-Inspector/wiki)
