# AGENTS.md - ViewMappingGenerator

## 🎯 Назначение

Editor tool для автоматической генерации ViewMapping классов и ScriptableObject ассетов, связывающих View и ViewModel через типизированный маппинг.

## 🧭 Точки входа

- **Window**: `ViewMappingGeneratorWindow`  
  Меню: `Tools → MVVM → View Mapping Generator`  
  Сканирует `IViewModel` и `BindableView<>` реализации, позволяет генерировать маппинги.

- **Post-compile handler**: `ViewMappingGeneratorPostCompile`  
  Автоматически создаёт SO asset после компиляции нового mapping скрипта.

## 📁 Структура файлов

| Файл | Назначение |
|------|------------|
| `ViewMappingGeneratorWindow.cs` | Editor window (UI Toolkit) |
| `ViewMappingCodeGenerator.cs` | Генерация кода mapping классов |
| `ViewMappingGeneratorConfig.cs` | Константы путей и утилиты |
| `ViewMappingGeneratorPostCompile.cs` | `[DidReloadScripts]` callback |
| `ViewBindingsGenerator.cs` | (unused) Генерация ReactiveProperty биндингов |

## 📂 Пути генерации

| Константа | Путь |
|-----------|------|
| `MappingScriptsFolder` | `Assets/Source/CodeGen/Views` |
| `MappingAssetsFolder` | `Assets/Source/00. Configs/ViewsMapping` |
| `ViewConfigPath` | `Assets/Source/00. Configs/ViewConfig.asset` |

## 🔄 Workflow генерации

1. **Сканирование типов**
   - `TypeCache.GetTypesDerivedFrom<IViewModel>()` — все ViewModel
   - `TypeCache.GetTypesDerivedFrom(typeof(BindableView<>))` — все View

2. **Извлечение имени**
   - `LoginViewModel` → `Login`
   - `LoginView` → `Login`

3. **Генерация скрипта**
   - Создаёт `{ViewName}ViewMapping.cs` в `CodeGen/Views/{ViewName}/`
   - Класс наследует `ViewMappingBase<TView, TViewModel>`

4. **Post-compile создание SO**
   - После компиляции скрипта создаётся `.asset` в `ViewsMapping/`
   - Автоматически ищет prefab `{ViewName}View` и привязывает

5. **Добавление в ViewConfig**
   - Маппинг добавляется в `ViewConfig._mappings[]`

## 📋 Сгенерированный код

```csharp
using Core.Infrastructure.MVVM.Config;
using Core.Abstractions.MVVM.Views;
using UnityEngine;

namespace Core.Views.{ViewName}
{
    public class {ViewName}ViewMapping : ViewMappingBase<{ViewName}View, {ViewName}ViewModel>
    {
    }
}
```

## ⚠️ Правила для изменений

- **НЕ редактируй** сгенерированные файлы в `CodeGen/Views/` — перезаписываются
- **Editor-only**: код под `#if UNITY_EDITOR`
- **Именование**: prefab должен называться `{ViewName}View` для auto-assign
- **Addressables**: prefab должен быть помечен Addressable для работы ViewService

## 🔧 Troubleshooting

| Проблема | Решение |
|----------|---------|
| SO не создаётся | Дождись компиляции, повтори Generate |
| Prefab не привязался | Проверь имя prefab = `{ViewName}View` |
| Не добавился в ViewConfig | Добавь вручную в `_mappings[]` |
| View/ViewModel не в списке | Проверь: не abstract, наследует правильный базовый класс |

## 🔗 Связанные файлы

- `README.md` — документация для пользователей
- `Core/Infrastructure/MVVM/Config/ViewMappingBase.cs` — базовый класс маппинга
- `Core/Infrastructure/MVVM/Config/ViewConfig.cs` — конфигурация всех маппингов
- `Core/Infrastructure/MVVM/Services/ViewService.cs` — использует маппинги для навигации
