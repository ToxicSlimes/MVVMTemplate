# ViewMappingGenerator

Editor tool для автоматической генерации ViewMapping классов в MVVM-архитектуре шаблона.

## Назначение

Генератор создает связку между View и ViewModel через типизированный ViewMapping класс и ScriptableObject asset. Это позволяет ViewService корректно инстанцировать View с нужным ViewModel.

## Быстрый старт

1. Создай `{Name}View` (наследуется от `BindableView<{Name}ViewModel>`)
2. Создай `{Name}ViewModel` (реализует `IViewModel`)
3. Открой `Tools > MVVM > View Mapping Generator`
4. Найди нужный ViewModel или View в списке
5. Нажми **Generate Mapping**
6. Дождись компиляции Unity

После компиляции автоматически:
- Создастся `{Name}ViewMapping.cs` в `CodeGen/Views/{Name}/`
- Создастся `{Name}ViewMapping.asset` в `00. Configs/ViewsMapping/`
- Asset добавится в `ViewConfig._mappings[]`
- Если есть prefab `{Name}View` - он привяжется автоматически

## Структура модуля

| Файл | Назначение |
|------|------------|
| `ViewMappingGeneratorWindow.cs` | UI окно генератора (UI Toolkit) |
| `ViewMappingCodeGenerator.cs` | Генерация C# кода mapping класса |
| `ViewMappingGeneratorConfig.cs` | Константы путей и утилиты |
| `ViewMappingGeneratorPostCompile.cs` | Post-compile callback для создания SO |
| `ViewBindingsGenerator.cs` | Утилита для генерации биндингов (не используется в UI) |

## Пути генерации

| Что | Куда |
|-----|------|
| Mapping скрипты | `Assets/Source/CodeGen/Views/{Name}/` |
| Mapping assets | `Assets/Source/00. Configs/ViewsMapping/` |
| ViewConfig | `Assets/Source/00. Configs/ViewConfig.asset` |

## Сгенерированный код

```csharp
using Core.Infrastructure.MVVM.Config;
using Core.Abstractions.MVVM.Views;
using UnityEngine;

namespace Core.Views.{Name}
{
    public class {Name}ViewMapping : ViewMappingBase<{Name}View, {Name}ViewModel>
    {
    }
}
```

## Требования к именованию

- ViewModel: `{Name}ViewModel` (например: `LoginViewModel`)
- View: `{Name}View` (например: `LoginView`)
- Prefab: `{Name}View` (для auto-assign)
- Mapping: `{Name}ViewMapping` (генерируется автоматически)

## Troubleshooting

| Проблема | Решение |
|----------|---------|
| SO не создается после генерации | Дождись полной компиляции Unity |
| Prefab не привязался | Проверь что prefab называется `{Name}View` |
| View/ViewModel не отображается в списке | Убедись что класс не abstract и наследует правильный базовый класс |
| Mapping не добавился в ViewConfig | Добавь вручную через инспектор |

## Связанные модули

- `Core.Infrastructure.MVVM.Config.ViewMappingBase` - базовый класс маппинга
- `Core.Infrastructure.MVVM.Config.ViewConfig` - конфигурация всех маппингов
- `Core.Infrastructure.MVVM.Services.ViewService` - использует маппинги для навигации

## Ограничения

- Сгенерированные файлы в `CodeGen/Views/` перезаписываются при повторной генерации
- Prefab должен быть помечен как Addressable для работы ViewService
