# ViewMappingGenerator

Editor tool для автоматической генерации ViewMapping классов и ScriptableObject ассетов.

## 📍 Location

```
Assets/Source/Editor/ViewMappingGenerator/
├── ViewMappingGeneratorWindow.cs     # Editor window (Tools menu)
├── ViewMappingCodeGenerator.cs       # Генерация кода ViewMapping классов
├── ViewMappingGeneratorConfig.cs     # Пути и конфигурация
├── ViewMappingGeneratorPostCompile.cs # Post-compile обработчик
└── ViewBindingsGenerator.cs          # (unused) Генерация биндингов для ReactiveProperty
```

## 🎯 Назначение

Связывает **View** и **ViewModel** через типизированный ScriptableObject маппинг:

```
ViewModel (LoginViewModel) ←→ ViewMapping (LoginViewMapping.asset) ←→ View Prefab (LoginView.prefab)
```

ViewService использует эти маппинги для:
1. Резолва View по типу ViewModel
2. Загрузки prefab через Addressables

## 🚀 Как использовать

### Открыть окно

**Tools → MVVM → View Mapping Generator**

### Workflow

1. **Создай View и ViewModel** в Core (например, `SettingsView` + `SettingsViewModel`)
2. **Открой окно генератора**
3. **Найди ViewModel или View** через поиск
4. **Нажми "Generate Mapping"**
5. **Дождись компиляции** — автоматически создастся:
   - `.cs` файл маппинга
   - `.asset` ScriptableObject
   - Запись в `ViewConfig.asset`
6. **Проверь ссылку на prefab** в созданном ViewMapping asset

### Tooltip (?)

Окно содержит желтый "?" с подсказкой:
> 1. Создай View и ViewModel в Core.
> 2. Открой это окно, найди ViewModel или View через поиск.
> 3. Нажми Generate Mapping.
> 4. Дождись компиляции: скрипт создаст экземпляр SO в виде .asset и добавит его в ViewConfig.
> 5. При необходимости переоткрой ViewMapping и проверь ссылку на префаб View в поле ViewPrefab.

## 📂 Что генерируется

### 1. Mapping Script

**Путь:** `Assets/Source/CodeGen/Views/{ViewName}/{ViewName}ViewMapping.cs`

```csharp
using Core.Infrastructure.MVVM.Config;
using Core.Abstractions.MVVM.Views;
using UnityEngine;

namespace Core.Views.Login
{
    public class LoginViewMapping : ViewMappingBase<LoginView, LoginViewModel>
    {
    }
}
```

### 2. Mapping Asset (ScriptableObject)

**Путь:** `Assets/Source/00. Configs/ViewsMapping/{ViewName}ViewMapping.asset`

Содержит:
- `ViewPrefab` — AssetReference на prefab View (автоматически находит если имя совпадает)

### 3. ViewConfig Entry

Автоматически добавляется в `Assets/Source/00. Configs/ViewConfig.asset` → `_mappings[]`

## 🏗️ Архитектура

### ViewMappingBase

```csharp
public abstract class ViewMappingBase<TView, TViewModel> : ViewMappingBase
    where TView : BindableView<TViewModel>, IView
    where TViewModel : DisposableObject
{
    public override Type ViewType => typeof(TView);
    public override Type ViewModelType => typeof(TViewModel);
}
```

### ViewConfig

```csharp
[CreateAssetMenu(fileName = "ViewConfig", menuName = "MVVM/View Config")]
public class ViewConfig : ConfigBase, IViewConfig
{
    [SerializeField] internal List<ViewMappingBase> _mappings = new();
    
    // Кэширует маппинги по типу ViewModel для быстрого lookup
    public string GetViewAddress<TViewModel>() { ... }
    public AssetReferenceT<GameObject> GetViewReference<TViewModel>() { ... }
}
```

## 📁 Конфигурация путей

Определены в `ViewMappingGeneratorConfig.cs`:

| Константа | Путь |
|-----------|------|
| `MappingScriptsFolder` | `Assets/Source/CodeGen/Views` |
| `MappingAssetsFolder` | `Assets/Source/00. Configs/ViewsMapping` |
| `ViewConfigPath` | `Assets/Source/00. Configs/ViewConfig.asset` |

## ⚙️ Механизм работы

### 1. Сканирование типов

```csharp
// Находит все IViewModel реализации
TypeCache.GetTypesDerivedFrom<IViewModel>()

// Находит все BindableView<> реализации
TypeCache.GetTypesDerivedFrom(typeof(BindableView<>))
```

### 2. Извлечение имени

```csharp
// "LoginViewModel" → "Login"
name.Substring(0, name.Length - "ViewModel".Length)

// "LoginView" → "Login"  
name.Substring(0, name.Length - "View".Length)
```

### 3. Post-compile callback

Используется `[DidReloadScripts]` для создания SO после компиляции нового скрипта:

```csharp
[DidReloadScripts]
private static void OnScriptsReloaded()
{
    // Создаёт .asset если был запланирован viewName
}
```

### 4. Auto-assign prefab

```csharp
// Ищет prefab по имени "{ViewName}View t:prefab"
AssetDatabase.FindAssets(searchName)
```

## 📋 Существующие маппинги

| View | Script | Asset |
|------|--------|-------|
| Chat | `CodeGen/Views/Chat/ChatViewMapping.cs` | `ViewsMapping/ChatViewMapping.asset` |
| HeroDetailPanel | `CodeGen/Views/HeroDetailPanel/HeroDetailPanelViewMapping.cs` | `ViewsMapping/HeroDetailPanelViewMapping.asset` |
| HeroesGridPanel | `CodeGen/Views/HeroesGridPanel/HeroesGridPanelViewMapping.cs` | `ViewsMapping/HeroesGridPanelViewMapping.asset` |
| ImageGen | `CodeGen/Views/ImageGen/ImageGenViewMapping.cs` | `ViewsMapping/ImageGenViewMapping.asset` |
| Login | `CodeGen/Views/Login/LoginViewMapping.cs` | `ViewsMapping/LoginViewMapping.asset` |
| Main | `CodeGen/Views/Main/MainViewMapping.cs` | `ViewsMapping/MainViewMapping.asset` |
| MenuSelection | `CodeGen/Views/MenuSelection/MenuSelectionViewMapping.cs` | `ViewsMapping/MenuSelectionViewMapping.asset` |
| NetworkSandbox | `CodeGen/Views/NetworkSandbox/NetworkSandboxViewMapping.cs` | `ViewsMapping/NetworkSandboxViewMapping.asset` |
| PerformanceTest | `CodeGen/Views/PerformanceTest/PerformanceTestViewMapping.cs` | `ViewsMapping/PerformanceTestViewMapping.asset` |
| Settings | `CodeGen/Views/Settings/SettingsViewMapping.cs` | `ViewsMapping/SettingsViewMapping.asset` |
| StandaloneWorkflow | `CodeGen/Views/StandaloneWorkflow/StandaloneWorkflowViewMapping.cs` | `ViewsMapping/StandaloneWorkflowViewMapping.asset` |
| UIConstructor | `CodeGen/Views/UIConstructor/UIConstructorViewMapping.cs` | `ViewsMapping/UIConstructorViewMapping.asset` |

## ⚠️ Important Notes

1. **НЕ редактируй** сгенерированные `.cs` файлы в `CodeGen/Views/` — они перезаписываются
2. **Проверяй ViewPrefab** — автоматический поиск работает только если имя prefab = `{ViewName}View`
3. **Addressables** — prefab должен быть помечен как Addressable для работы ViewService
4. **ViewConfig** — если маппинг не добавился автоматически, добавь вручную

## 🔧 Troubleshooting

### SO не создаётся после генерации скрипта
- Дождись полной компиляции (следи за спиннером в Unity)
- Переоткрой окно генератора и нажми "Generate Mapping" повторно

### Prefab не привязался автоматически
- Prefab должен называться точно `{ViewName}View` (например, `LoginView`)
- Если имя отличается — привяжи вручную в Inspector

### Маппинг не добавился в ViewConfig
- Открой `Assets/Source/00. Configs/ViewConfig.asset`
- Добавь маппинг в массив `_mappings` вручную

### View/ViewModel не отображается в списке
- Класс не должен быть abstract
- View должен наследовать `BindableView<TViewModel>`
- ViewModel должен реализовывать `IViewModel`

## 🔗 Related

- [MVVM.README.md](../../Runtime/Core/Infrastructure/MVVM/MVVM.README.md) — MVVM архитектура
- [ViewService](../../Runtime/Core/Infrastructure/MVVM/Services/ViewService.cs) — навигация/жизненный цикл View
- [ViewConfig.asset](../../00.%20Configs/ViewConfig.asset) — конфигурация маппингов
