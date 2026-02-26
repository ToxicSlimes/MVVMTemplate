# MVVM System Documentation

Полная документация по MVVM системе проекта EndlessAdventure.

---

## Содержание

1. [Архитектура](#архитектура)
2. [Структура папок](#структура-папок)
3. [Компоненты системы](#компоненты-системы)
   - [ViewConfig](#1-viewconfig-scriptableobject)
   - [ViewMappingBase](#2-viewmappingbase)
   - [BindableView](#3-bindableviewtviewmodel)
   - [DisposableObject](#4-disposableobject)
   - [ViewFactory](#5-viewfactory)
   - [ViewService](#6-viewservice)
   - [ViewShowParams](#7-viewshowparams)
   - [ViewAnimator](#8-viewanimator)
   - [CanvasProvider](#9-canvasprovider)
   - [UIElementFactory](#10-uielementfactory) *(NEW)*
   - [EmbeddedPanelService](#11-embeddedpanelservice) *(NEW)*
   - [ViewModel (IViewModel)](#12-viewmodel-iviewmodel)
   - [Model (IModel)](#13-model-imodel)
   - [Element System](#14-element-system) *(NEW)*
     - [IElementData](#ielementdata)
     - [BindableElement](#bindableelement)
     - [ElementConfig](#elementconfig)
     - [ElementMappingBase](#elementmappingbase)
     - [UIElementService](#uielementservice)
4. [Интерфейсы системы](#интерфейсы-системы)
5. [Внутренние детали реализации](#внутренние-детали-реализации)
6. [Поток данных (Data Flow)](#поток-данных-data-flow)
7. [Как создавать новые View/ViewModel](#как-создавать-новые-viewviewmodel)
8. [Примеры использования](#примеры-использования)
9. [Анимации и переходы](#анимации-и-переходы)
10. [Canvas Layers](#canvas-layers)
11. [Best Practices](#best-practices)
12. [Troubleshooting](#troubleshooting)
13. [FAQ](#faq)
14. [Тестирование](#тестирование)
15. [Дополнительные детали реализации](#дополнительные-детали-реализации)

---

## Архитектура

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           MVVM ARCHITECTURE                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                         VIEW LAYER                                   │  │
│   │  ┌──────────────────────────────────────────────────────────────┐   │  │
│   │  │  BindableView<TViewModel> : MonoBehaviour                     │   │  │
│   │  │  - UI элементы (Button, Text, Image и т.д.)                  │   │  │
│   │  │  - Подписки на события UI через R3                          │   │  │
│   │  │  - Наблюдение за изменениями ViewModel                       │   │  │
│   │  └───────────────────────┬────────────────────────────────────────┘   │  │
│   └─────────────────────────┼───────────────────────────────────────────┘  │
│                              │ Binds to                                      │
│   ┌──────────────────────────▼───────────────────────────────────────────┐  │
│   │                    VIEWMODEL LAYER                                   │  │
│   │  ┌──────────────────────────────────────────────────────────────┐   │  │
│   │  │  ViewModel : DisposableObject                                │   │  │
│   │  │  - ReactiveProperty<T> для данных                            │   │  │
│   │  │  - ReactiveCommand для команд                                │   │  │
│   │  │  - Бизнес-логика представления                               │   │  │
│   │  └───────────────────────┬────────────────────────────────────────┘   │  │
│   └─────────────────────────┼───────────────────────────────────────────┘  │
│                              │ Uses                                          │
│   ┌──────────────────────────▼───────────────────────────────────────────┐  │
│   │                      MODEL LAYER                                    │  │
│   │  ┌──────────────────────────────────────────────────────────────┐   │  │
│   │  │  Model (Domain Models)                                       │   │  │
│   │  │  - Состояние данных                                          │   │  │
│   │  │  - События изменений                                         │   │  │
│   │  │  - Валидация                                                 │   │  │
│   │  └──────────────────────────────────────────────────────────────┘   │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                    VIEW SERVICE LAYER                                │  │
│   │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │  │
│   │  │ ViewService  │  │ ViewFactory  │  │ViewAnimator  │              │  │
│   │  │ - Управление│  │ - Создание   │  │ - Анимации   │              │  │
│   │  │   навигацией│  │   View       │  │   переходов  │              │  │
│   │  │ - Кэширование│  │ - Биндинг   │  │              │              │  │
│   │  │ - История    │  │   ViewModel  │  │              │              │  │
│   │  └──────────────┘  └──────────────┘  └──────────────┘              │  │
│   │  ┌──────────────────────────┐  ┌──────────────────────────┐        │  │
│   │  │ EmbeddedPanelService     │  │ UIElementFactory         │        │  │
│   │  │ - Встраиваемые панели    │  │ - Инстанцирование UI     │        │  │
│   │  │ - Ленивый биндинг        │  │ - DI инъекция            │        │  │
│   │  │ - Контекст (IViewContext)│  │                          │        │  │
│   │  └──────────────────────────┘  └──────────────────────────┘        │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                    CONFIGURATION LAYER                               │  │
│   │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │  │
│   │  │ ViewConfig   │  │ViewMapping   │  │CanvasProvider │              │  │
│   │  │ - Маппинг    │  │ - Связь View │  │ - Управление │              │  │
│   │  │   ViewModel  │  │   ↔ ViewModel│  │   Canvas     │              │  │
│   │  │   → Address  │  │              │  │   слоями     │              │  │
│   │  └──────────────┘  └──────────────┘  └──────────────┘              │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Структура папок

```
Assets/Source/Runtime/Core/
│
├── Abstractions/MVVM/
│   ├── Views/
│   │   ├── IView.cs                    # Базовый интерфейс View
│   │   ├── BindableView.cs             # Базовый класс для всех View
│   │   ├── IShowable.cs                # Интерфейс для показа View
│   │   ├── IHideable.cs                # Интерфейс для скрытия View
│   │   ├── IUnbindable.cs              # Интерфейс для отвязки ViewModel
│   │   ├── ViewShowParams.cs           # Параметры показа View
│   │   ├── ViewDisplayMode.cs          # Режимы отображения
│   │   └── ViewTransitionStyle.cs       # Стили анимаций
│   │
│   ├── ViewModels/
│   │   ├── IViewModel.cs                # Интерфейс ViewModel
│   │   └── DisposableObject.cs          # Базовый класс ViewModel
│   │
│   ├── Factory/
│   │   ├── IViewFactory.cs              # Интерфейс фабрики View
│   │   └── IUIElementFactory.cs         # Интерфейс фабрики UI элементов
│   │
│   ├── Services/
│   │   ├── IViewService.cs              # Интерфейс сервиса навигации
│   │   └── IUIElementService.cs         # Интерфейс сервиса UI элементов
│   │
│   ├── Elements/                        # Element System интерфейсы
│   │   ├── IBindableElement.cs          # Базовый интерфейс элемента
│   │   ├── IClickableElement.cs         # Интерфейс кликабельного элемента
│   │   ├── ISelectableElement.cs        # Интерфейс выбираемого элемента
│   │   └── BindableElement.cs           # Базовый класс для элементов
│   │
│   ├── Animation/
│   │   └── IViewAnimator.cs             # Интерфейс аниматора
│   │
│   ├── Config/
│   │   ├── IViewConfig.cs               # Интерфейс конфигурации View
│   │   └── IElementConfig.cs            # Интерфейс конфигурации Element
│   │
│   └── Canvas/
│       ├── ICanvasProvider.cs           # Интерфейс провайдера Canvas
│       └── CanvasLayer.cs                # Enum слоёв Canvas
│
├── MVVM/
│   ├── Hosts/
│   │   └── IEmbeddedPanelHost.cs        # Интерфейс хоста встраиваемых панелей
│   │
│   └── Notifications/
│       └── EmbeddedPanelStateChangedNotification.cs  # Уведомление об изменении состояния панели
│
├── Infrastructure/MVVM/
│   ├── Services/
│   │   ├── ViewService.cs               # Реализация навигации
│   │   ├── EmbeddedPanelService.cs      # Сервис для встраиваемых панелей
│   │   └── UIElementService.cs          # Сервис для UI элементов
│   │
│   ├── ViewFactory.cs                   # Реализация фабрики View
│   ├── UIElementFactory.cs              # Фабрика UI элементов с DI инъекцией
│   │
│   ├── Animation/
│   │   └── ViewAnimator.cs              # Реализация анимаций
│   │
│   ├── Config/
│   │   ├── ViewConfig.cs                # ScriptableObject конфигурации View
│   │   ├── ViewConfigInternals.cs       # InternalsVisibleTo атрибуты для тестов
│   │   ├── ViewMappingBase.cs           # Базовый класс маппинга View
│   │   ├── ElementConfig.cs             # ScriptableObject конфигурации Element
│   │   └── ElementMappingBase.cs        # Базовый класс маппинга Element
│   │
│   └── Canvas/
│       └── CanvasProvider.cs            # Реализация провайдера Canvas
│
├── Data/Domain/Models/Elements/
│   └── IElementData.cs                  # Интерфейс данных элемента
│
├── MVVM/                                # Конкретные реализации
│   ├── Views/
│   │   ├── Settings/
│   │   │   └── SettingsView.cs          # Пример простого View
│   │   └── NetworkSandbox/
│   │       ├── NetworkSandboxView.cs   # Пример сложного View
│   │       ├── WorkflowGraphRenderer.cs # Дополнительные компоненты
│   │       ├── WorkflowNodeView.cs
│   │       ├── WorkflowGraphEdgeView.cs
│   │       └── UILineRenderer.cs
│   │
│   ├── ViewModels/
│   │   ├── Settings/
│   │   │   └── SettingsViewModel.cs    # Пример ViewModel
│   │   └── NetworkSandbox/
│   │       └── NetworkSandboxViewModel.cs
│   │
│   └── Models/
│       ├── Settings/
│       │   └── SettingsModel.cs        # Пример Model
│       └── NetworkSandbox/
│           └── NetworkSandboxModel.cs
│
└── DependencyInjection/
    └── Bindings/
        └── InfrastructureBindings.cs   # Регистрация в DI контейнере

Assets/Source/
│
├── CodeGen/Views/                       # Автогенерируемые ViewMapping классы
│   └── {ViewName}/
│       └── {ViewName}ViewMapping.cs     # Генерируется через Editor Window
│
├── 00. Configs/ViewsMapping/           # ScriptableObject assets маппингов
│   └── {ViewName}ViewMapping.asset     # Создаётся автоматически после компиляции
│
├── Editor/ViewMappingGenerator/        # Editor инструменты (namespace: Core.Editor.ViewMappingGenerator)
│   ├── ViewMappingGeneratorWindow.cs    # Editor Window для генерации маппингов
│   ├── ViewMappingGeneratorPostCompile.cs  # Автоматическое создание assets
│   ├── ViewMappingGeneratorConfig.cs    # Конфигурация путей
│   ├── ViewMappingCodeGenerator.cs      # Генератор кода маппингов
│   └── ViewBindingsGenerator.cs         # Генератор биндингов (не используется в основном workflow)
│
└── Runtime/Tests/MVVM/                 # Unit-тесты системы MVVM
    ├── ViewServiceTests.cs
    ├── ViewFactoryTests.cs
    ├── ViewAnimatorTests.cs
    ├── ViewConfigTests.cs
    ├── ViewShowParamsTests.cs
    ├── CanvasProviderTests.cs
    ├── ViewMappingBaseTests.cs
    └── DisposableObjectTests.cs

Assets/Source/00. Configs/
├── ViewConfig.asset                    # Главный конфиг маппингов (добавлен в Addressables)
└── ViewsMapping/                       # Маппинги View → ViewModel
```

---

## Компоненты системы

### 1. ViewConfig (ScriptableObject)

Хранит маппинг между ViewModel типами и Addressable адресами View префабов.

**Создание:**
**Create → MVVM → View Config**

**Расположение:**
Должен лежать в папке `Assets/Source/00. Configs/` с именем `ViewConfig.asset` (полный путь: `Assets/Source/00. Configs/ViewConfig.asset`)

**Важно:** ViewConfig загружается через `ConfigProvider.GetConfig<ViewConfig>()`, который использует Addressables (`IAssetProvider.LoadSync<T>("ViewConfig")`). Файл должен быть добавлен в Addressables с адресом `ViewConfig`.

```csharp
[CreateAssetMenu(fileName = "ViewConfig", menuName = "MVVM/View Config")]
public class ViewConfig : ConfigBase, IViewConfig
{
    [SerializeField] internal List<ViewMappingBase> _mappings = new();
    internal Dictionary<Type, ViewMappingBase> _typeCache;
    
    public string GetViewAddress<TViewModel>() where TViewModel : DisposableObject;
    public string GetViewAddress(Type viewModelType);
    public AssetReferenceT<GameObject> GetViewReference<TViewModel>() where TViewModel : class;
    public AssetReferenceT<GameObject> GetViewReference(Type viewModelType);
    
    internal void BuildCache();
}
```

**Особенности:**
- Кэширование маппингов в `_typeCache` для быстрого поиска (`Dictionary<Type, ViewMappingBase>`)
- Автоматическое построение кэша в `OnEnable()` через `BuildCache()`
- Пересборка кэша при первом обращении если `_typeCache == null`
- Поддержка поиска по типу через generic и через `Type`
- Пропускает null маппинги и маппинги с null `ViewModelType` при построении кэша

**Метод GetViewAddress:**
- Возвращает `mapping.ViewPrefab.RuntimeKey.ToString()` из `AssetReferenceT<GameObject>`
- Выбрасывает `KeyNotFoundException` если:
  - Маппинг не найден в кэше
  - `ViewPrefab` равен null
  - `RuntimeKey.ToString()` возвращает пустую строку

**Метод GetViewReference:**
- Возвращает `AssetReferenceT<GameObject>` напрямую из маппинга
- Выбрасывает `KeyNotFoundException` если маппинг не найден или `ViewPrefab` равен null

**Использование:**
```csharp
var address = _viewConfig.GetViewAddress<MyViewModel>();
var reference = _viewConfig.GetViewReference<MyViewModel>();

var addressByType = _viewConfig.GetViewAddress(typeof(MyViewModel));
var referenceByType = _viewConfig.GetViewReference(typeof(MyViewModel));
```

**ViewConfigInternals.cs:**

Вспомогательный файл с атрибутами `InternalsVisibleTo` для тестовых сборок:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EditMode")]
[assembly: InternalsVisibleTo("PlayMode")]
[assembly: InternalsVisibleTo("Tests.MVVM")]
```

**Назначение:**
- Позволяет тестовым сборкам (`EditMode`, `PlayMode`, `Tests.MVVM`) видеть internal члены ViewConfig
- Необходимо для тестирования internal методов (например, `BuildCache()`) и полей (`_typeCache`, `_mappings`)
- Файл не содержит логики — только assembly-level атрибуты

---

### 2. ViewMappingBase

Связывает View и ViewModel типы с Addressable префабом.

**Создание через Editor Window:**
**Tools → MVVM → View Mapping Generator**

**Или вручную:**
```csharp
public class SettingsViewMapping : ViewMappingBase<SettingsView, SettingsViewModel>
{
    // ViewPrefab устанавливается в Inspector
}
```

**Структура:**
```csharp
public abstract class ViewMappingBase : ScriptableObject
{
    public AssetReferenceT<GameObject> ViewPrefab;  // Addressable префаб
    public abstract Type ViewType { get; }
    public abstract Type ViewModelType { get; }
}

public abstract class ViewMappingBase<TView, TViewModel> : ViewMappingBase
    where TView : BindableView<TViewModel>, IView
    where TViewModel : DisposableObject
{
    public override Type ViewType => typeof(TView);
    public override Type ViewModelType => typeof(TViewModel);
}
```

**Использование:**
Рекомендуется использовать типизированную версию `ViewMappingBase<TView, TViewModel>` для type-safety:
```csharp
public class SettingsViewMapping : ViewMappingBase<SettingsView, SettingsViewModel>
{
    // ViewPrefab устанавливается в Inspector
}
```

**ViewMappingGeneratorWindow - Детали работы:**

Editor Window автоматизирует создание ViewMapping. Открывается через меню **Tools → MVVM → View Mapping Generator**.

**Расположение:** `Assets/Source/Editor/ViewMappingGenerator/ViewMappingGeneratorWindow.cs` (namespace: `Core.Editor.ViewMappingGenerator`)

**UI компоненты:**
- Поисковая строка для фильтрации ViewModels и Views (case-insensitive, фильтрует по `Name` и `FullName`)
- Два списка: ViewModels (слева) и Views (справа) с чередующимся фоном строк
- Preview секция с путями (CS и Asset) и preview кода (с подсветкой синтаксиса через IMGUIContainer)
- Кнопка "Generate Mapping" для генерации маппинга
- Статусная метка для отображения результата операции
- FAQ кнопка (?) с подсказками по использованию (tooltip с краткими инструкциями на русском языке)
- Минимальный размер окна: 700x420 пикселей

**Tooltip инструкции:**
При наведении на кнопку "?" отображаются краткие инструкции:
1. Создай View и ViewModel в Core
2. Открой это окно, найди ViewModel или View через поиск
3. Нажми Generate Mapping
4. Дождись компиляции: скрипт создаст экземпляр SO в виде .asset и добавит его в ViewConfig
5. При необходимости переоткрой ViewMapping и проверь ссылку на префаб View в поле ViewPrefab

**Метод FindTypeInAssemblies:**
- Ищет тип по полному имени через `asm.GetType(typeName)` во всех загруженных сборках (`AppDomain.CurrentDomain.GetAssemblies()`)
- Если не найден по полному имени, ищет по короткому имени (`t.Name == typeName`) во всех типах всех сборок
- Обрабатывает `ReflectionTypeLoadException` при загрузке типов (использует `e.Types` из исключения)
- Пропускает `null` типы при итерации
- Возвращает `null` если тип не найден
- Используется в `ViewMappingGeneratorWindow` для проверки существования скомпилированного типа маппинга
- Используется в `ViewMappingGeneratorPostCompile` для поиска типа после компиляции скрипта
- **Алгоритм поиска:** Сначала пытается найти тип по полному имени (включая namespace), затем по короткому имени (только имя класса). Это позволяет находить типы даже если namespace не указан или изменился

1. **Сканирование типов:** Автоматически находит все ViewModel и View типы в проекте
2. **Поиск:** Фильтрация по имени для быстрого поиска нужных типов
3. **Генерация кода:** Создаёт класс маппинга в `Assets/Source/CodeGen/Views/{ViewName}/{ViewName}ViewMapping.cs`
4. **Автоматическое создание ScriptableObject:** После компиляции скрипта автоматически создаётся asset в `Assets/Source/00. Configs/ViewsMapping/{ViewName}ViewMapping.asset`
5. **Автоматическое добавление в ViewConfig:** Маппинг автоматически добавляется в список `_mappings` ViewConfig
6. **Автоматическое назначение префаба:** Пытается автоматически найти и назначить Addressable префаб по имени

**Пути (ViewMappingGeneratorConfig):**
- Скрипты маппингов: `Assets/Source/CodeGen/Views/{ViewName}/`
- Asset маппингов: `Assets/Source/00. Configs/ViewsMapping/`
- ViewConfig: `Assets/Source/00. Configs/ViewConfig.asset`

**ViewMappingGeneratorWindow - Детали работы:**

**Сканирование типов:**
- Использует `TypeCache.GetTypesDerivedFrom<IViewModel>()` для поиска ViewModel типов
- Использует `TypeCache.GetTypesDerivedFrom(typeof(BindableView<>))` для поиска View типов
- Пропускает абстрактные типы через проверку `type.IsAbstract`
- Обновляет списки при открытии окна (`OnEnable()`) через метод `ScanTypes()`
- Хранит типы в `List<Type> _viewModelTypes` и `List<Type> _viewTypes`

**Поиск и фильтрация:**
- Поиск работает по имени типа (`Name`) и полному имени (`FullName`)
- Фильтрация case-insensitive (через `ToLowerInvariant()`)
- Обновляет оба списка (ViewModels и Views) одновременно через метод `ApplyFilter()`
- Использует `_filteredViewModelTypes` и `_filteredViewTypes` для отображения отфильтрованных результатов
- Обновляет `ListView.itemsSource` и вызывает `Rebuild()` для обновления UI

**Генерация кода:**
- Определяет базовое имя из выбранного типа (удаляет суффиксы "ViewModel" или "View") через методы `OnViewModelSelectionChanged()` и `OnViewSelectionChanged()`
- Показывает preview кода перед генерацией через `UpdatePreview()` метод
- Генерирует код через `ViewMappingCodeGenerator.GenerateMappingCode(viewName)`
- Генерируемый код создаётся в namespace `Core.Views.{ViewName}` (например, `Core.Views.Settings` для `SettingsViewMapping`)
- Класс маппинга наследуется от `ViewMappingBase<{ViewName}View, {ViewName}ViewModel>`
- Пути определяются через `ViewMappingGeneratorConfig`:
  - Скрипт: `Assets/Source/CodeGen/Views/{ViewName}/{ViewName}ViewMapping.cs` (через `GetMappingScriptPath()`)
  - Asset: `Assets/Source/00. Configs/ViewsMapping/{ViewName}ViewMapping.asset` (через `GetMappingAssetPath()`)
- Показывает пути и preview кода в UI перед генерацией

**Автоматическое назначение префаба:**
- Использует `AssetDatabase.FindAssets("{ViewName}View t:prefab")` для поиска префаба
  - **Формат поиска:** `{viewName}View t:prefab` - ищет префабы с именем, содержащим `{viewName}View` (например, "SettingsView")
- Назначает префаб только если найден ровно один результат (проверка `guids.Length == 1`)
  - **Почему только один:** Если найдено несколько префабов, система не может определить, какой использовать, поэтому требует ручного назначения
- Создаёт `AssetReferenceGameObject` из GUID найденного префаба через `AssetReferenceGameObject(guid)`
- Устанавливает `mappingBase.ViewPrefab = assetReference`
- Помечает asset как изменённый через `EditorUtility.SetDirty()` и сохраняет через `AssetDatabase.SaveAssets()`
- Выполняется через метод `TryAutoAssignViewPrefab()` в `ViewMappingGeneratorWindow` и `ViewMappingGeneratorPostCompile`
- Если префаб не найден или найдено несколько результатов, метод завершается без действий (префаб нужно назначить вручную в Inspector)
- **Важно:** Префаб должен быть помечен как Addressable для корректной работы системы. Если префаб не Addressable, его нужно добавить в Addressables Groups перед использованием

**Автоматическое добавление в ViewConfig:**
- Загружает ViewConfig через `AssetDatabase.LoadAssetAtPath<ViewConfig>(ViewMappingGeneratorConfig.ViewConfigPath)`
- Если ViewConfig не найден, метод завершается без действий (нужно создать ViewConfig вручную через меню **Create → MVVM → View Config**)
- Использует `SerializedObject` и `SerializedProperty` для безопасного добавления маппинга
  - **Почему SerializedObject:** Unity требует использования SerializedObject для изменения ScriptableObject в Editor скриптах, чтобы изменения корректно отслеживались системой Undo/Redo
- Проверяет, что маппинг ещё не добавлен перед вставкой (через итерацию по `mappingsProperty.arraySize` и сравнение `element.objectReferenceValue == mapping`)
  - **Проверка дубликатов:** Использует reference equality для проверки, что один и тот же asset не добавляется дважды
- Если маппинг уже добавлен, метод завершается без действий
- Добавляет маппинг в конец списка через `InsertArrayElementAtIndex()` и устанавливает `objectReferenceValue`
  - **Позиция:** Маппинг всегда добавляется в конец списка `_mappings` в ViewConfig
- Применяет изменения через `serializedObject.ApplyModifiedProperties()`
- Помечает ViewConfig как изменённый через `EditorUtility.SetDirty()` и сохраняет через `AssetDatabase.SaveAssets()`
- Выполняется через метод `TryAddMappingToViewConfig()` в `ViewMappingGeneratorWindow` и `ViewMappingGeneratorPostCompile`
- **Важно:** После добавления маппинга в ViewConfig, кэш маппингов автоматически пересобирается при следующем вызове `GetViewAddress()` или `GetViewReference()` (через проверку `if (_typeCache == null) BuildCache()`)

**Автоматическое создание директорий:**
- Вызывает `ViewMappingGeneratorConfig.EnsureDirectoriesExist(viewName)` перед созданием файлов
- Создаёт директорию для скрипта если её нет: `Assets/Source/CodeGen/Views/{ViewName}/`
- Создаёт директорию для asset если её нет: `Assets/Source/00. Configs/ViewsMapping/`

**Проверка существования:**
- Проверяет существование скрипта через `File.Exists(csPath)` перед генерацией
- Если скрипт существует:
  - Ищет тип маппинга через `FindTypeInAssemblies(mappingTypeName)`
  - Если тип найден:
    - Проверяет существование asset через `AssetDatabase.LoadAssetAtPath()`
    - Если asset не существует, создаёт его автоматически через `ScriptableObject.CreateInstance()` и `AssetDatabase.CreateAsset()`
    - Автоматически назначает префаб через `TryAutoAssignViewPrefab()` (если найден)
    - Автоматически добавляет в ViewConfig через `TryAddMappingToViewConfig()`
    - Показывает статус: "SO instance created for existing mapping script and added to ViewConfig"
  - Если asset уже существует:
    - Пытается автоматически назначить префаб (если ещё не назначен)
    - Пытается добавить в ViewConfig (если ещё не добавлен)
    - Показывает статус: "Mapping already exists"
  - Если тип не найден (скрипт ещё не скомпилирован):
    - Планирует создание asset после компиляции через `Schedule()`
    - Показывает статус: "Mapping script exists, waiting for compilation to create SO instance"

**ViewMappingGeneratorPostCompile:**
Автоматически создаёт ScriptableObject asset после компиляции скрипта через атрибут `[DidReloadScripts]`. Это позволяет избежать ручного создания asset после генерации кода.

**Внутренний класс:**
- Класс помечен как `internal static`, доступен только внутри сборки Editor (namespace: `Core.Editor.ViewMappingGenerator`)
- Использует `EditorPrefs` для хранения имени View между компиляциями (ключ: `"ViewMappingGenerator.PendingViewName"`)
- **Расположение:** `Assets/Source/Editor/ViewMappingGenerator/ViewMappingGeneratorPostCompile.cs`
- **Механизм работы EditorPrefs:** Имя View сохраняется в EditorPrefs перед компиляцией через `Schedule(viewName)`, затем удаляется после обработки через `EditorPrefs.DeleteKey()`. Это гарантирует, что каждый запланированный маппинг обрабатывается только один раз, даже если Unity перезагружает скрипты несколько раз

**Механизм работы:**
- Использует `EditorPrefs` для хранения имени View между компиляциями (ключ: `"ViewMappingGenerator.PendingViewName"`)
- `ViewMappingGeneratorPostCompile.Schedule(viewName)` сохраняет имя View в EditorPrefs перед компиляцией
- После компиляции `[DidReloadScripts]` вызывает `OnScriptsReloaded()`, который:
  - Читает имя View из EditorPrefs
  - Удаляет ключ из EditorPrefs (через `EditorPrefs.DeleteKey()`)
  - Ищет скомпилированный тип маппинга через `ViewMappingGeneratorWindow.FindTypeInAssemblies()`
  - Создаёт необходимые директории через `ViewMappingGeneratorConfig.EnsureDirectoriesExist(viewName)`
  - Проверяет существование asset через `AssetDatabase.LoadAssetAtPath()`
  - Если asset не существует, создаёт новый через `ScriptableObject.CreateInstance()` и `AssetDatabase.CreateAsset()`
  - Если asset уже существует, использует существующий
  - Автоматически назначает префаб через `ViewMappingGeneratorWindow.TryAutoAssignViewPrefab()`
  - Автоматически добавляет маппинг в ViewConfig через `ViewMappingGeneratorWindow.TryAddMappingToViewConfig()`

**Процесс работы:**
1. Пользователь нажимает "Generate Mapping" в Editor Window
2. Проверяется существование скрипта через `File.Exists(csPath)`
3. Если скрипт существует:
   - Ищется тип маппинга через `FindTypeInAssemblies()`
   - Если тип найден и asset не существует → создаётся asset сразу
   - Если тип не найден → планируется создание после компиляции через `Schedule()`
4. Если скрипт не существует:
   - Создаются необходимые директории через `EnsureDirectoriesExist()`
   - Генерируется C# код через `ViewMappingCodeGenerator.GenerateMappingCode(viewName)`
   - Сохраняется файл через `File.WriteAllText()` и `AssetDatabase.ImportAsset()`
   - Вызывается `ViewMappingGeneratorPostCompile.Schedule(viewName)` для сохранения имени в EditorPrefs
5. Unity компилирует скрипт
6. `ViewMappingGeneratorPostCompile` (через `[DidReloadScripts]`) создаёт ScriptableObject asset
7. Автоматически назначает префаб (если найден через `AssetDatabase.FindAssets()`)
8. Автоматически добавляет маппинг в ViewConfig через `SerializedObject` и `SerializedProperty`

**ViewMappingGeneratorConfig:**
Статический класс с конфигурацией путей и вспомогательными методами (namespace: `Core.Editor.ViewMappingGenerator`):
- `MappingScriptsFolder = "Assets/Source/CodeGen/Views"` - папка для скриптов маппингов
- `MappingAssetsFolder = "Assets/Source/00. Configs/ViewsMapping"` - папка для asset маппингов
- `ViewConfigPath = "Assets/Source/00. Configs/ViewConfig.asset"` - путь к ViewConfig
- **Расположение:** `Assets/Source/Editor/ViewMappingGenerator/ViewMappingGeneratorConfig.cs`
- `GetMappingScriptPath(viewName)` - возвращает путь к скрипту маппинга (например, `Assets/Source/CodeGen/Views/Settings/SettingsViewMapping.cs`)
  - **Структура пути:** `{MappingScriptsFolder}/{viewName}/{viewName}ViewMapping.cs` - каждый View имеет свою подпапку
- `GetMappingAssetPath(viewName)` - возвращает путь к asset маппинга (например, `Assets/Source/00. Configs/ViewsMapping/SettingsViewMapping.asset`)
  - **Структура пути:** `{MappingAssetsFolder}/{viewName}ViewMapping.asset` - все assets в одной папке
- `GetMappingTypeName(viewName)` - возвращает имя типа маппинга (например, "SettingsViewMapping")
  - **Формат:** `{viewName}ViewMapping` - всегда добавляет суффикс "ViewMapping"
- `EnsureDirectoriesExist(viewName)` - создаёт необходимые директории если их нет (создаёт папку для скрипта и папку для assets)
  - **Создаёт:** Папку для скрипта (`{MappingScriptsFolder}/{viewName}/`) и папку для assets (`{MappingAssetsFolder}/`) если они не существуют

**Реализация:**
```csharp
public static class ViewMappingGeneratorConfig
{
    public const string MappingScriptsFolder = "Assets/Source/CodeGen/Views";
    public const string MappingAssetsFolder = "Assets/Source/00. Configs/ViewsMapping";
    public const string ViewConfigPath = "Assets/Source/00. Configs/ViewConfig.asset";
    
    public static string GetMappingScriptPath(string viewName)
    {
        return Path.Combine(MappingScriptsFolder, viewName, viewName + "ViewMapping.cs");
    }
    
    public static string GetMappingAssetPath(string viewName)
    {
        return Path.Combine(MappingAssetsFolder, viewName + "ViewMapping.asset");
    }
    
    public static string GetMappingTypeName(string viewName)
    {
        return viewName + "ViewMapping";
    }
    
    public static void EnsureDirectoriesExist(string viewName)
    {
        var scriptDir = Path.GetDirectoryName(GetMappingScriptPath(viewName));
        if (!Directory.Exists(scriptDir))
        {
            Directory.CreateDirectory(scriptDir);
        }
        
        if (!Directory.Exists(MappingAssetsFolder))
        {
            Directory.CreateDirectory(MappingAssetsFolder);
        }
    }
}
```

**ViewMappingCodeGenerator:**
Статический класс для генерации кода маппинга (namespace: `Core.Editor.ViewMappingGenerator`):
- `GenerateMappingCode(viewName)` - генерирует C# код класса маппинга
- Генерируемый код включает:
  - Using директивы: `using Core.Infrastructure.MVVM.Config;`, `using Core.Abstractions.MVVM.Views;`, `using UnityEngine;`
  - Namespace: `Core.Views.{ViewName}` (например, `Core.Views.Settings`)
  - Класс: `{ViewName}ViewMapping : ViewMappingBase<{ViewName}View, {ViewName}ViewModel>`
  - Пустой класс (без дополнительных полей или методов)
- Используется в `ViewMappingGeneratorWindow` для генерации кода перед компиляцией
- **Важно:** Генерируемый код использует типизированную версию `ViewMappingBase<TView, TViewModel>` для type-safety и compile-time проверки соответствия View и ViewModel
- **Расположение:** `Assets/Source/Editor/ViewMappingGenerator/ViewMappingCodeGenerator.cs`

**Пример сгенерированного кода:**
```csharp
using Core.Infrastructure.MVVM.Config;
using Core.Abstractions.MVVM.Views;
using UnityEngine;

namespace Core.Views.Settings
{
    public class SettingsViewMapping : ViewMappingBase<SettingsView, SettingsViewModel>
    {
    }
}
```

**ViewBindingsGenerator:**
Статический класс для генерации биндингов (namespace: `Core.Editor.ViewMappingGenerator`, в настоящее время не используется в основном workflow):
- `GenerateBindings(viewModelType)` - генерирует код биндингов для ReactiveProperty и ReactiveCommand
- Анализирует свойства ViewModel через рефлексию (`GetProperties(BindingFlags.Instance | BindingFlags.Public)`)
- Определяет типы `ReactiveProperty<T>`, `ReadOnlyReactiveProperty<T>` и `ReactiveCommand<T>` через проверку базовых типов
- Генерирует шаблонный код подписок для найденных реактивных свойств и команд
- **Примечание:** Текущая реализация проверяет типы UniRx (например, `UniRx.ReactiveProperty`), но проект использует R3. Класс может быть адаптирован для R3 в будущем
- Может быть использован в будущем для автоматической генерации биндингов в View
- **Расположение:** `Assets/Source/Editor/ViewMappingGenerator/ViewBindingsGenerator.cs`

---

### 3. BindableView<TViewModel>

Базовый класс для всех View. Обеспечивает:
- Автоматический биндинг с ViewModel
- Реактивные подписки через R3
- Анимации показа/скрытия
- Управление жизненным циклом

```csharp
public abstract class BindableView<TViewModel> : MonoBehaviour, IView, IShowable, IHideable, IUnbindable
    where TViewModel : DisposableObject
{
    [SerializeField] private CanvasGroup _canvasGroup;  // Опциональный CanvasGroup для анимаций
    [SerializeField] private bool _isPersistent;         // Флаг персистентности (устанавливается в Inspector)
    
    public TViewModel ViewModel => _viewModel.Value;     // Текущий ViewModel (типизированный)
    IViewModel IView.ViewModel => _viewModel.Value;      // Explicit interface для IView (нетипизированный)
    public bool IsVisible { get; protected set; }        // Видимость View
    public virtual bool IsPersistent => _isPersistent;   // Персистентность (не закрывается автоматически)
    
    public IDisposable BindTo(TViewModel viewModel);     // Привязка ViewModel
    public Observable<TViewModel> ObserveViewModel(bool includeCurrent = true); // Наблюдение за ViewModel
    
    protected abstract void OnBind(ref DisposableBag disposables); // Переопределить для подписок
    
    protected virtual UniTask OnBeforeShow();   // Вызывается перед показом
    protected virtual UniTask OnAfterShow();   // Вызывается после показа
    protected virtual UniTask OnBeforeHide();  // Вызывается перед скрытием
    protected virtual UniTask OnAfterHide();    // Вызывается после скрытия
    
    protected virtual UniTask PlayShowAnimation();  // Виртуальный метод для кастомной анимации показа
    protected virtual UniTask PlayHideAnimation();  // Виртуальный метод для кастомной анимации скрытия
}
```

**Внутренняя реализация:**
- `ViewModel` свойство возвращает значение из внутреннего `ReactiveProperty<TViewModel> _viewModel`
- ViewModel хранится в `ReactiveProperty<TViewModel>`, что позволяет наблюдать за его изменениями
- `BindTo()` очищает предыдущие подписки через `_disposables.Dispose()` перед созданием новых
- `BindTo()` возвращает `IDisposable` (DisposableBag) для возможности ручной очистки подписок
- `IsVisible` - свойство для отслеживания состояния видимости View, устанавливается в `true` при `ShowAsync()` и в `false` при `HideAsync()`. Отличается от `gameObject.activeSelf` тем, что отражает логическое состояние видимости View (устанавливается до/после анимаций), а не физическое состояние GameObject
  - **Установка в ShowAsync:** `IsVisible = true` устанавливается **до** вызова `OnBeforeShow()`, что позволяет использовать это свойство в методах жизненного цикла для проверки состояния View
  - **Установка в HideAsync:** `IsVisible = false` устанавливается **после** вызова `OnAfterHide()`, что гарантирует, что свойство остаётся `true` во время всех методов жизненного цикла скрытия
  - **Отличие от gameObject.activeSelf:** `gameObject.activeSelf` отражает физическое состояние GameObject (активен/неактивен), в то время как `IsVisible` отражает логическое состояние видимости View (устанавливается до/после анимаций). Это позволяет различать состояние "View показывается" (IsVisible = true, но анимация ещё не завершена) и "View полностью показан" (IsVisible = true, анимация завершена, gameObject.activeSelf = true)
  - Может использоваться в наследниках для проверки состояния View без прямого обращения к `gameObject.activeSelf`

**Жизненный цикл:**
1. `BindTo(viewModel)` - привязка ViewModel (очищает предыдущие подписки, устанавливает новое значение в `_viewModel`)
2. `OnBind()` - настройка подписок (вызывается после установки ViewModel)
3. `ShowAsync()` - показ View:
   - `gameObject.SetActive(true)`
   - `IsVisible = true`
   - `OnBeforeShow()`
   - `PlayShowAnimation()` (если не переопределено, использует CanvasGroup fade)
   - `OnAfterShow()`
4. `HideAsync()` - скрытие View:
   - `OnBeforeHide()`
   - `PlayHideAnimation()` (если не переопределено, использует CanvasGroup fade)
   - `OnAfterHide()`
   - `IsVisible = false`
   - `gameObject.SetActive(false)`
5. `Unbind()` - отвязка ViewModel (устанавливает `_viewModel.Value = null`, очищает все подписки)
6. `OnDestroy()` - автоматическая очистка при уничтожении GameObject (вызывает `Unbind()`, затем `_disposables.Dispose()`)
7. `Dispose()` - явная очистка ресурсов (освобождает `ReactiveProperty<TViewModel>` и все подписки)

**Метод Unbind:**
- Сохраняет текущий ViewModel в локальную переменную: `var currentViewModel = _viewModel.Value`
- Устанавливает `_viewModel.Value = null` (очищает ViewModel)
- **Вызывает `currentViewModel?.Dispose()`** — освобождает ViewModel (важно: ViewModel полностью уничтожается!)
- Вызывает `_disposables.Dispose()` для очистки всех подписок
- Создаёт новый `DisposableBag` через `_disposables = new DisposableBag()` для возможности повторного использования
- Виртуальный метод, может быть переопределён в наследниках
- **Важно:** После вызова `Unbind()` ViewModel нельзя использовать, так как он был освобождён через `Dispose()`

**Метод Dispose:**
- Реализует `IDisposable` для явной очистки ресурсов
- Освобождает `ReactiveProperty<TViewModel>` через `_viewModel?.Dispose()`
- Очищает все подписки через `_disposables.Dispose()`
- Может быть вызван вручную для принудительной очистки (например, при смене сцены)
- Автоматически вызывается при уничтожении GameObject через `OnDestroy()`

**Внутреннее хранение ViewModel:**
- ViewModel хранится в `ReactiveProperty<TViewModel> _viewModel` (не в обычном поле)
- Это позволяет наблюдать за изменениями ViewModel через `ObserveViewModel()`
- `ViewModel` свойство возвращает `_viewModel.Value` (текущее значение)
- При `BindTo()` устанавливается `_viewModel.Value = viewModel`, что уведомляет всех подписчиков `ObserveViewModel()`

**Наблюдение за ViewModel:**
```csharp
protected override void OnBind(ref DisposableBag disposables)
{
    ObserveViewModel(includeCurrent: false)  // Только будущие изменения (Skip(1))
        .Subscribe(static viewModel => Debug.Log($"ViewModel changed: {viewModel}"))
        .AddTo(ref disposables);
    
    ObserveViewModel(includeCurrent: true)  // Включая текущее значение
        .Subscribe(static viewModel => Debug.Log($"Current ViewModel: {viewModel}"))
        .AddTo(ref disposables);
}
```

**Детали реализации ObserveViewModel:**
- `ObserveViewModel(includeCurrent: true)` возвращает `_viewModel` напрямую (включает текущее значение)
- `ObserveViewModel(includeCurrent: false)` возвращает `_viewModel.Skip(1)` (пропускает текущее значение, только будущие изменения)
- Полезно для различения начальной инициализации и последующих изменений ViewModel
- Используется для реактивной подписки на смену ViewModel (например, при перебиндинге через `BindTo()`)
- `_viewModel` - это `ReactiveProperty<TViewModel>`, который позволяет наблюдать за изменениями через R3 Observable API

**Дефолтные анимации (fallback методы):**

Эти методы выполняют **мгновенную** установку значений без анимации. Настоящие анимации выполняются через `ViewAnimator` с LitMotion когда View показывается с `ViewShowParams` и `TransitionStyle != None`.

- `PlayShowAnimation()` — если `_canvasGroup` не null:
  - Устанавливает `alpha = 1f`, `interactable = true`, `blocksRaycasts = true` мгновенно
  - Возвращает `UniTask.CompletedTask` (без асинхронной анимации)
- `PlayHideAnimation()` — если `_canvasGroup` не null:
  - Устанавливает `alpha = 0f`, `interactable = false`, `blocksRaycasts = false` мгновенно
  - Возвращает `UniTask.CompletedTask` (без асинхронной анимации)
- Если `_canvasGroup` null, методы возвращают `UniTask.CompletedTask`

**⚠️ Важно:** Для настоящих анимаций используйте `ViewService.ShowAsync()` с `ViewShowParams`:
```csharp
await _viewService.ShowAsync<MyView, MyViewModel>(
    ViewShowParams.Default()
        .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.Scale)
        .SetAnimationDuration(0.3f)
);
```
Тогда `ViewAnimator` выполнит анимацию через LitMotion перед вызовом `ShowAsync()` на View.

---

### 4. DisposableObject

Базовый класс для ViewModel. Управляет подписками через R3 DisposableBag и CancellationTokenSource для асинхронных операций.

```csharp
public class DisposableObject : IViewModel
{
    private DisposableBag _disposables;
    protected CancellationTokenSource _requestCts;  // Для отмены асинхронных запросов
    
    public bool IsDisposed { get; private set; }  // Флаг состояния
    
    protected ref DisposableBag Disposables => ref _disposables;  // Для автоматической очистки подписок
    
    protected void SafeCancelAndDisposeRequestCts();  // Безопасная отмена и освобождение CTS
    protected void CancelRequestCts();               // Только отмена CTS (без Dispose)
    protected CancellationTokenSource CreateNewRequestCts();  // Создать новый CTS, отменив предыдущий
    
    public virtual void Dispose();  // Очистка ресурсов (idempотентный)
}
```

**Особенности:**
- `Dispose()` является idempотентным - можно вызывать несколько раз безопасно (проверка `if (IsDisposed) return;`)
- `IsDisposed` позволяет проверить состояние перед использованием (устанавливается в `true` при первом вызове `Dispose()`)
- Все подписки автоматически очищаются при `Dispose()` через `_disposables.Dispose()`
- `Disposables` возвращает `ref DisposableBag` для использования с `.AddTo(ref Disposables)` в R3
- После `Dispose()` все последующие вызовы методов ViewModel должны проверять `IsDisposed` для предотвращения использования после очистки

**CancellationTokenSource API:**
- `_requestCts` — защищённое поле для управления асинхронными операциями (например, сетевыми запросами)
- `SafeCancelAndDisposeRequestCts()` — безопасно отменяет и освобождает `_requestCts`, обрабатывает `ObjectDisposedException`
- `CancelRequestCts()` — только отменяет `_requestCts` без Dispose (полезно для повторного использования)
- `CreateNewRequestCts()` — создаёт новый `CancellationTokenSource`, предварительно отменив и освободив предыдущий

**Использование:**
```csharp
public class MyViewModel : DisposableObject
{
    public ReactiveProperty<string> Name { get; }
    
    public MyViewModel()
    {
        Name = new ReactiveProperty<string>("");
        Name.AddTo(ref Disposables);  // Автоматическая очистка при Dispose
    }
    
    public async UniTask LoadDataAsync()
    {
        if (IsDisposed) return;  // Защита от использования после Dispose
        
        var cts = CreateNewRequestCts();  // Отменяет предыдущий запрос
        try
        {
            var data = await _api.GetDataAsync(cts.Token);
            Name.Value = data.Name;
        }
        catch (OperationCanceledException)
        {
            // Запрос был отменён (например, при вызове нового LoadDataAsync)
        }
    }
    
    public void DoSomething()
    {
        if (IsDisposed) return;  // Защита от использования после Dispose
        // ...
    }
}
```

---

### 5. ViewFactory

Создаёт View экземпляры, разрешает ViewModel из DI контейнера и связывает их.

```csharp
public interface IViewFactory
{
    UniTask<IView> CreateAsync(Type viewModelType, ViewShowParams showParams = null);
    UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null);
    UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null);
    UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams);
    void Destroy(IView view);
}
```

**Процесс создания:**
1. Получает Addressable адрес:
   - Если используется `CreateAsync<TView, TViewModel>(Transform parent)` → получает адрес из `ViewConfig.GetViewAddress<TViewModel>()`
   - Если используется `CreateAsync<TView, TViewModel>(string address, Transform parent)` → использует переданный адрес напрямую (обход ViewConfig)
   - Если используется `CreateAsync<TView, TViewModel>(ViewShowParams showParams)` → получает адрес из `ViewConfig.GetViewAddress<TViewModel>()`
2. Определяет родительский Transform:
   - Если указан `CustomParent` в `ViewShowParams` → использует его
   - Если указан `TargetCanvas` → получает Transform из `ICanvasProvider`
   - Иначе → создаёт/находит `[UIRoot]` GameObject
3. Загружает и инстанцирует префаб через `IAssetProvider.InstantiateAsync<TView>()`
4. **Выполняет инъекцию зависимостей** через `GameObjectInjector.InjectRecursive(view.gameObject, _container)` (Reflex DI)
5. Разрешает ViewModel из DI контейнера через `_container.Resolve<TViewModel>()`
6. Вызывает `BindTo(viewModel)` на View для привязки ViewModel

**Методы CreateAsync:**
- `CreateAsync(Type viewModelType, ViewShowParams showParams = null)` - non-generic версия, создаёт View по типу ViewModel через рефлексию (используется `ViewService.ShowAsync<TViewModel>()`)
  - Получает адрес из ViewConfig через `GetViewAddress(viewModelType)`
  - **Важно:** Использует `_assetProvider.LoadAsync<GameObject>()` + `Object.Instantiate()` (в отличие от generic версий, которые используют `InstantiateAsync`)
  - Получает `IView` компонент через `GetComponent<IView>()` с проверкой (выбрасывает `InvalidOperationException` если компонент не найден)
  - Вызывает `BindTo()` через рефлексию (`GetMethod("BindTo")?.Invoke()`)
  - Полезно когда тип View неизвестен на этапе компиляции
- `CreateAsync<TView, TViewModel>(Transform parent = null)` - получает адрес из ViewConfig (стандартный способ, используется ViewService)
- `CreateAsync<TView, TViewModel>(string address, Transform parent = null)` - использует переданный адрес напрямую (продвинутый способ, обход ViewConfig)
  - **Когда использовать:** Для динамической загрузки View с разными адресами или для тестирования без настройки ViewConfig
  - **Важно:** Этот метод не используется в стандартном workflow через ViewService, который всегда использует ViewConfig
- `CreateAsync<TView, TViewModel>(ViewShowParams showParams)` - получает адрес из ViewConfig и использует параметры для определения родителя

**Зависимости:**
- `Container` (Reflex) - DI контейнер для:
  - Разрешения ViewModel через `_container.Resolve<TViewModel>()`
  - Инъекции зависимостей в View через `GameObjectInjector.InjectRecursive(view.gameObject, _container)`
- `IAssetProvider` - для загрузки и инстанцирования Addressable префабов (обязательный)
- `IViewConfig` - для получения адреса префаба по типу ViewModel (обязательный)
- `ICanvasProvider` (опционально, может быть `null`) - для получения Transform Canvas слоёв
  - Если `null`, `TargetCanvas` из `ViewShowParams` игнорируется, используется `[UIRoot]`
  - Передаётся в конструктор `ViewFactory` и может быть `null` (см. регистрацию в `InfrastructureBindings.AddInfrastructure()`)

**Особенности разрешения ViewModel:**
- `ViewFactory` получает `Container` в конструкторе — это DI контейнер Reflex для разрешения зависимостей
- Разрешение ViewModel происходит через `_container.Resolve<TViewModel>()` после инстанцирования View префаба
- Инъекция зависимостей в View (и его дочерние объекты) выполняется через `GameObjectInjector.InjectRecursive(view.gameObject, _container)`
- Если ViewModel не зарегистрирован в контейнере, будет выброшено исключение при создании View
- **Регистрация ViewModel:** ViewModel регистрируются через `builder.AddSingleton<TViewModel>()` или `builder.AddTransient<TViewModel>()` в методе регистрации (например, в `InfrastructureBindings` или отдельном `ProjectInstaller`)
- **Порядок операций при создании View:** 
1. Получение адреса из ViewConfig (через `GetViewAddress<TViewModel>()`)
2. Определение родительского Transform (через `ResolveParent()` с приоритетом: `CustomParent` → `TargetCanvas` → `[UIRoot]`)
3. Загрузка и инстанцирование префаба через `IAssetProvider.InstantiateAsync<TView>()` (GameObject создаётся активным)
4. Инъекция зависимостей через `GameObjectInjector.InjectRecursive()` (позволяет View и его дочерним компонентам получать зависимости)
5. Разрешение ViewModel из DI контейнера (через `_container.Resolve<TViewModel>()`)
6. Вызов `BindTo(viewModel)` на View для привязки ViewModel (вызывает `OnBind()` для настройки подписок)
- **Примечание:** ViewModel создаётся один раз при первом создании экземпляра View и переиспользуется при последующих показах этого же View, пока View не будет уничтожен (`HideAsync(nonPersistent)`/`CloseAllNonPersistentAsync()`/`ClearCache()`). Регистрация `Transient` гарантирует создание **нового** ViewModel только в тех сценариях, когда View создаётся заново (кэш отсутствует или очищен); сама по себе она не даёт “новый ViewModel при каждом `ShowAsync`”, если View остаётся в кэше `ViewService`.

**UIRoot:**
- Автоматически создаётся при первом использовании `ViewFactory` (в конструкторе)
- Ищет существующий `[UIRoot]` через `GameObject.Find("[UIRoot]")` перед созданием
- Имеет имя `[UIRoot]` для поиска
- Помечается `DontDestroyOnLoad` только в runtime (`Application.isPlaying`)
- Используется как родитель по умолчанию для всех View

**Приоритет выбора родителя (метод `ResolveParent`):**
1. `CustomParent` из `ViewShowParams` (если установлен) → возвращается напрямую
2. `TargetCanvas` из `ViewShowParams` → `ICanvasProvider.GetCanvasTransform()` (если установлен и `ICanvasProvider` не null)
   - Если `ICanvasProvider` равен `null` или `GetCanvasTransform()` возвращает `null`, используется следующий приоритет
3. `[UIRoot]` (по умолчанию) - создаётся автоматически при первом использовании `ViewFactory`

**IAssetProvider:**
Интерфейс для работы с Addressable ассетами. ViewFactory использует его для:
- `InstantiateAsync<TView>(address, parent)` - загрузка и инстанцирование префаба
- `ReleaseInstance(gameObject)` - освобождение инстанса при уничтожении View

**Метод Destroy:**
- Проверяет, что View является `Component` (через проверку `view is Component`)
- Вызывает `Unbind()` если View реализует `IUnbindable` (через pattern matching `view is IUnbindable unbindable`)
- Вызывает `IAssetProvider.ReleaseInstance(component.gameObject)` для освобождения Addressable инстанса
- **Важно:** Метод не уничтожает GameObject напрямую - это делает `IAssetProvider.ReleaseInstance()`, который управляет жизненным циклом Addressable инстансов

---

### 6. ViewService

Управляет навигацией между View: показ, скрытие, история, кэширование.

```csharp
public interface IViewService
{
    IView CurrentView { get; }
    IReadOnlyCollection<IView> ViewHistory { get; }
    IReadOnlyCollection<IView> ActiveViews { get; }
    
    UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null);  // Показ по типу ViewModel
    UniTask<TView> ShowAsync<TView, TViewModel>(bool hideCurrentView = true);
    UniTask<TView> ShowAsync<TView, TViewModel>(ViewShowParams showParams);
    
    UniTask<bool> HideViewAsync<TViewModel>();  // Скрытие по типу ViewModel
    UniTask HideAsync<TView>();
    UniTask HideAsync(IView view);
    UniTask HideCurrentAsync();
    
    TViewModel GetViewModel<TViewModel>();     // Получить ViewModel активного View
    
    UniTask GoBackAsync();                    // Назад по истории
    UniTask GoToRootAsync();                  // К корневому View
    UniTask CloseAllNonPersistentAsync();     // Закрыть все не-персистентные
    
    void ClearHistory();
    void ClearCache();
    
    bool IsViewActive<TView>();
    TView GetActiveView<TView>();
    
    event Action<IView> ViewShown;
    event Action<IView> ViewHidden;
}
```

**Особенности:**
- **Кэширование:** View создаются и кэшируются в `ConcurrentDictionary<Type, IView>`. **ВАЖНО:** Non-persistent View удаляются из кэша и уничтожаются при скрытии через `HideAsync()`! Только persistent View остаются в кэше для переиспользования
- **История:** Автоматическое сохранение в историю при скрытии (использует `Stack<IView>`)
- **Персистентность:** Persistent View не закрываются автоматически и остаются в кэше после скрытия
- **Потокобезопасность:** Все операции синхронизированы через `lock` и `Interlocked`
- **Параметры показа:** Хранит `ViewShowParams` для каждого View в `Dictionary<IView, ViewShowParams>`
  - `ViewShowParams` сохраняются при показе через `ShowAsync<TView, TViewModel>(ViewShowParams showParams)`
  - Используются при скрытии View для применения тех же анимаций и callback'ов (`OnHidden`)
  - Удаляются из `_viewParams` при скрытии View через `HideAsync()`
- **Активные View:** Управляет стеком активных View через `Stack<IView>`

**Внутренние структуры данных:**
- `_viewCache` - кэш созданных View (`ConcurrentDictionary<Type, IView>` для потокобезопасности)
  - Ключ: тип View (`Type`), значение: экземпляр View (`IView`)
  - Один View на тип (кэширование по типу, не по экземпляру)
- `_activeViews` - стек активных View (`Stack<IView>`) - защищён через `lock(_sync)`
  - Предотвращает дубликаты через проверку `Contains(view)` перед добавлением (использует reference equality через `Stack.Contains()`)
  - Может содержать несколько View одновременно (например, при `DisplayMode.Additive`)
- `_viewHistory` - история View для навигации назад (`Stack<IView>`) - защищён через `lock(_sync)`
  - Предотвращает дубликаты через проверку `Contains(view)` перед добавлением (использует reference equality через `Stack.Contains()`)
  - Используется для навигации назад через `GoBackAsync()`
- `_viewParams` - параметры показа для каждого View (`Dictionary<IView, ViewShowParams>`) - защищён через `lock(_sync)`
  - Ключ: экземпляр View (`IView`), значение: параметры показа (`ViewShowParams`)
  - Используется для применения тех же анимаций при скрытии View
- `_currentView` - текущий активный View (обновляется через `Interlocked.Exchange` для потокобезопасности)
  - Может быть `null` если нет активных View
- `_sync` - объект для синхронизации операций с коллекциями
- `_viewSortingOrders` - словарь для хранения sorting order каждого View (`Dictionary<IView, int>`) - защищён через `lock(_sync)`
  - Ключ: экземпляр View (`IView`), значение: назначенный sorting order (`int`)
  - Используется для правильного порядка отображения View поверх друг друга
- `_currentMaxSortingOrder` - текущий максимальный sorting order (`int`)
  - Инкрементируется при показе каждого View через `AssignSortingOrder()`
  - Сбрасывается при освобождении последнего View

**Управление SortingOrder:**

ViewService автоматически управляет sorting order для правильного наложения View друг на друга.

**Метод AssignSortingOrder:**
- Вызывается автоматически при показе View (в `ShowAsync()`)
- Проверяет, что View является `Component` (через pattern matching `view is not Component`)
- Получает или создаёт `Canvas` компонент на View:
  - Если Canvas отсутствует → создаёт через `AddComponent<Canvas>()`
  - Автоматически добавляет `GraphicRaycaster` для обработки UI событий
- Инкрементирует `_currentMaxSortingOrder++`
- Устанавливает `canvas.overrideSorting = true` для независимой сортировки
- Устанавливает `canvas.sortingOrder = _currentMaxSortingOrder`
- Сохраняет sorting order в `_viewSortingOrders[view] = _currentMaxSortingOrder`
- **Важно:** Каждый новый View получает более высокий sorting order, гарантируя его отображение поверх предыдущих

**Метод ReleaseSortingOrder:**
- Вызывается автоматически при скрытии View (в `HideAsync()` и `GoBackAsync()`)
- Удаляет View из `_viewSortingOrders` через `Remove()`
- Если словарь пуст → сбрасывает `_currentMaxSortingOrder = 0`
- Иначе → пересчитывает `_currentMaxSortingOrder` как максимум оставшихся значений через `Values.Max()`
- **Важно:** Пересчёт максимума предотвращает неограниченный рост sorting order при частом показе/скрытии View

**Пример работы SortingOrder:**
```
1. ShowAsync<MenuView>()     → sortingOrder = 1
2. ShowAsync<SettingsView>() → sortingOrder = 2 (поверх MenuView)
3. ShowAsync<PopupView>()    → sortingOrder = 3 (поверх SettingsView)
4. HideAsync<PopupView>()    → sortingOrder освобождён, max = 2
5. ShowAsync<DialogView>()   → sortingOrder = 3 (используется следующий)
```

**Потокобезопасность свойств:**
- `ViewHistory` возвращает копию стека через `ToArray()` для безопасного чтения из других потоков
- `ActiveViews` возвращает копию стека через `ToArray()` для безопасного чтения из других потоков
- Все операции записи синхронизированы через `lock(_sync)`
- **Важно:** Хотя коллекции защищены через `lock`, асинхронные операции (`ShowAsync`, `HideAsync`) не являются полностью потокобезопасными для параллельных вызовов. Рекомендуется вызывать методы `ViewService` из одного потока (обычно главного потока Unity) или использовать механизмы синхронизации (например, `UniTask.SynchronizationContext`) для последовательного выполнения операций

**Новые методы (ViewModel-centric API):**

- `ShowAsync<TViewModel>(ViewShowParams showParams = null)`:
  - Показывает View по типу ViewModel без явного указания типа View
  - Использует non-generic `ViewFactory.CreateAsync(Type, ViewShowParams)` внутри
  - Кэширует View по типу ViewModel (не по типу View!)
  - Полезно когда тип View неизвестен или неважен на вызывающей стороне
  - Возвращает `IView` вместо конкретного типа
  
- `HideViewAsync<TViewModel>()`:
  - Скрывает активный View по типу ViewModel
  - Возвращает `true` если View был найден и скрыт, `false` если View не найден или не активен
  - Ищет View в `_viewCache` и проверяет наличие в `_activeViews`
  - Вызывает `HideAsync(IView)` для найденного View
  
- `GetViewModel<TViewModel>()`:
  - Возвращает ViewModel активного View по типу
  - Возвращает `null` если View не найден или не активен
  - Использует `view.ViewModel as TViewModel` для приведения типа
  - Полезно для получения ViewModel из других частей приложения без прямой зависимости на View

**Логика навигации:**
- `ShowAsync<TView, TViewModel>(bool hideCurrentView = true)`:
  - Если `hideCurrentView = true` и текущий View не persistent → скрывает его через `HideViewInternal()` с `addToHistory = true` (добавляет в историю)
  - **Важно:** Persistent View не скрываются автоматически, даже если `hideCurrentView = true` (проверка `!_currentView.IsPersistent`)
  - Проверяет кэш через `_viewCache.TryGetValue(viewType, out var cachedView)`, если View уже создан → использует кэшированный
  - Иначе создаёт новый через `ViewFactory.CreateAsync<TView, TViewModel>()` (без `ViewShowParams`, используется дефолтный родитель `[UIRoot]`) и добавляет в кэш через `_viewCache[viewType] = view`
  - **Назначает SortingOrder** через `AssignSortingOrder(view)` (создаёт Canvas если нужно, инкрементирует sorting order)
  - Показывает View через `ShowViewInternal()` (без анимации, просто активирует GameObject через `IShowable.ShowAsync()` или `gameObject.SetActive(true)`)
  - Добавляет View в `_activeViews` через `Push()` (если ещё не добавлен, проверяется через `Contains()` с использованием reference equality)
  - Обновляет `_currentView` через `Interlocked.Exchange(ref _currentView, view)`
  - Вызывает событие `ViewShown?.Invoke(view)`
  - **Важно:** `ShowViewInternal()` не добавляет View в историю автоматически - история заполняется только при скрытии других View
  - **Важно:** Этот метод не использует `ViewShowParams`, поэтому анимации не применяются. Для анимаций используйте перегрузку с `ViewShowParams`
  - **Важно:** Параметры показа не сохраняются для последующего скрытия (в отличие от перегрузки с `ViewShowParams`). При скрытии View через `HideAsync()` анимации не будут применены, даже если View был показан с анимацией ранее
  - **Примечание:** Этот метод предназначен для простого показа View без дополнительных настроек. Для более сложных сценариев (анимации, кастомный родитель, Canvas слои) используйте перегрузку с `ViewShowParams`
  
- `GoBackAsync()`:
  - Проверяет наличие View в `_viewHistory` (если пусто, метод завершается без действий через early return)
  - Извлекает предыдущий View из `_viewHistory` через `Pop()` (без добавления текущего в историю)
  - Скрывает текущий View через `HideViewInternal()` (без добавления в историю, `addToHistory = false`)
  - Вызывает `HideAsync(current)` для полной очистки (удаляет из `_activeViews` и `_viewParams` через пересборку стека с использованием `ReferenceEquals`)
  - Показывает предыдущий View через `ShowViewInternal()` (без анимации, просто активирует GameObject)
  - Добавляет предыдущий View в `_activeViews` (если ещё не добавлен, проверяется через `Contains()` с использованием reference equality)
  - Обновляет `_currentView` через `Interlocked.Exchange`
  - Вызывает событие `ViewShown` для предыдущего View
  - **Важно:** Текущий View не добавляется в историю при вызове `GoBackAsync()`, так как он уже был добавлен при скрытии через `DisplayMode.Single`. История используется только для навигации назад, а не для отслеживания всех переходов
  - **Важно:** Если предыдущий View был уничтожен или удалён из кэша, `GoBackAsync()` может показать уже несуществующий View. Убедитесь, что View в истории не уничтожены
  
- `GoToRootAsync()`:
  - Вызывает `GoBackAsync()` в цикле `while (_viewHistory.Count > 0)` до тех пор, пока история не станет пустой
  - Возвращает пользователя к самому первому View в истории навигации
  - Каждый вызов `GoBackAsync()` извлекает один View из истории и показывает его
  
- `HideAsync<TView>()`:
  - Использует `_activeViews.OfType<TView>().FirstOrDefault()` для поиска View по типу
  - Если View не найден, метод завершается без действий
  - Вызывает `HideAsync(IView view)` для найденного View
  
- `HideAsync(IView view)`:
  - **Проверка на null:** Если `view == null`, метод завершается без действий (early return)
  - Проверяет наличие View в `_activeViews` через `wasActive = _activeViews.Contains(view)` (использует reference equality для сравнения)
  - Если View не активен (`!wasActive`), метод завершается без действий (early return)
  - Получает `ViewShowParams` для View из `_viewParams` (если есть)
  - Если есть `ViewShowParams` → использует их для анимации скрытия через `HideViewInternalWithParams()` с `addToHistory = false`
  - Иначе использует `HideViewInternal()` без анимации с `addToHistory = false`
  - Удаляет View из `_activeViews` через пересборку стека:
  - Создаёт буферный стек `buffer = new Stack<IView>()`
  - Извлекает все элементы из `_activeViews` через `Pop()` в цикле
  - Копирует в буфер только те элементы, которые не равны удаляемому View (проверка через `ReferenceEquals(top, view)`)
  - Восстанавливает стек из буфера, возвращая элементы обратно в `_activeViews` через `Push()`
  - **Примечание:** Этот механизм необходим, так как `Stack<T>` не поддерживает прямое удаление элементов по значению
  - Удаляет `ViewShowParams` для View из `_viewParams`
  - Обновляет `_currentView` на верхний элемент стека (`_activeViews.Peek()`) или `null` если стек пуст
  - **Освобождает SortingOrder** через `ReleaseSortingOrder(view)` (удаляет из словаря, пересчитывает максимум)
  - Вызывает событие `ViewHidden`
  - **🚨 УНИЧТОЖЕНИЕ NON-PERSISTENT VIEW:** Если `!view.IsPersistent`:
    - Удаляет View из кэша через `_viewCache.TryRemove(view.GetType(), out _)`
    - Уничтожает View через `_viewFactory.Destroy(view)` (вызывает `Unbind()` и освобождает Addressable инстанс)
    - **Важно:** ViewModel также уничтожается, так как `Unbind()` вызывает `currentViewModel?.Dispose()`!
    - **Важно:** При следующем `ShowAsync()` для этого типа View будет создан новый экземпляр View и ViewModel
  - **Persistent View:** Если `view.IsPersistent`, View остаётся в кэше и не уничтожается (только скрывается)
  - **Не добавляет View в историю** (история заполняется только при скрытии через `DisplayMode.Single` или явно через `HideViewInternal(view, addToHistory: true)`)
  - **Важно:** Метод синхронизирован через `lock(_sync)` для потокобезопасности операций с коллекциями
  - **Важно:** Все сравнения View используют reference equality (`ReferenceEquals`), а не value equality

- `ClearCache()`:
  - Итерируется по всем View в `_viewCache`
  - Для каждого View: проверяет реализацию `IUnbindable`, если реализует → вызывает `Unbind()` для очистки подписок
  - Вызывает `ViewFactory.Destroy()` для каждого View (освобождает Addressable инстанс)
  - Очищает `_viewCache` через `Clear()`
  - **Важно:** `ClearCache()` не удаляет View из `_activeViews` и `_viewHistory`, только из кэша. Для полной очистки используйте `ClearHistory()` и `CloseAllNonPersistentAsync()`

**Метод HideViewInternalWithParams:**
- Вызывается из `HideAsync(IView view)` если для View есть сохранённые `ViewShowParams`
- Вызывает `showParams.OnHidden?.Invoke()` **ПЕРЕД** анимацией скрытия (если callback установлен)
- Добавляет View в историю (если `addToHistory = true`) через `_viewHistory.Push(view)` (с проверкой на дубликаты через `!_viewHistory.Contains(view)`, использует reference equality)
- Проигрывает анимацию скрытия через `ViewAnimator.AnimateHideAsync()` (если `TransitionStyle != None`)
- Вызывает `IHideable.HideAsync()` или `Component.gameObject.SetActive(false)` для скрытия View
- **Важно:** Callback `OnHidden` вызывается **до** анимации скрытия, что позволяет выполнить действия перед началом анимации (например, сохранить данные, остановить таймеры)
- **Важно:** При `DisplayMode.Replace` используется `showParams` **нового View** для анимации скрытия заменяемого View, что позволяет создать плавный переход между View
- **Важно:** Все проверки на дубликаты используют reference equality (`ReferenceEquals` или `Contains()` для `Stack`, который использует `Equals()` по умолчанию, что для reference types эквивалентно reference equality)
- **Порядок операций:** `OnHidden` callback → добавление в историю (если нужно) → анимация скрытия → `HideAsync()` или `SetActive(false)`. Это гарантирует, что callback выполняется до начала анимации, а View деактивируется после завершения анимации
- **Примечание:** Если `TransitionStyle == None`, анимация не проигрывается, но callback `OnHidden` всё равно вызывается перед деактивацией View

**Метод ShowViewInternal:**
- Статический метод для показа View без анимации
- Проверяет реализацию `IShowable`, если реализует → вызывает `ShowAsync()`
- Иначе использует `Component.gameObject.SetActive(true)` как fallback
- Не добавляет View в историю (история заполняется только при скрытии других View)
- Используется в `ShowAsync<TView, TViewModel>(bool hideCurrentView)` и `GoBackAsync()` для показа View без анимации
- **Примечание:** Этот метод не использует `ViewShowParams`, поэтому анимации не применяются. Для показа с анимацией используйте `ShowViewInternalWithParams()`

**Метод ShowViewInternalWithParams:**
- Вызывается из `ShowAsync<TView, TViewModel>(ViewShowParams showParams)` для показа View с анимацией
- Проигрывает анимацию показа через `ViewAnimator.AnimateShowAsync()` (если `TransitionStyle != None`)
- Вызывает `IShowable.ShowAsync()` или `Component.gameObject.SetActive(true)` для активации View
- Сохраняет `ViewShowParams` в `_viewParams` для последующего использования при скрытии
- **Важно:** Анимация проигрывается **до** вызова `ShowAsync()`, что позволяет View быть видимым во время анимации
- **Примечание:** Для корректной работы анимации GameObject View должен быть активен. Если префаб создан неактивным, `ViewAnimator.AnimateShowAsync()` всё равно выполнится (так как проверяет только наличие Component), но анимация не будет видна до активации GameObject. В стандартном workflow View активируется через `ViewFactory` при инстанцировании через `IAssetProvider.InstantiateAsync()`, который создаёт активный GameObject, поэтому анимация работает корректно
- **Порядок операций:** `AnimateShowAsync()` → `ShowAsync()` (или `SetActive(true)`). Это гарантирует, что анимация начинается сразу после активации GameObject, создавая плавный переход

---

### 7. ViewShowParams

Fluent API для настройки параметров показа View.

```csharp
public class ViewShowParams
{
    public ViewDisplayMode DisplayMode { get; private set; }
    public ViewTransitionStyle TransitionStyle { get; private set; }
    public CanvasLayer? TargetCanvas { get; private set; }
    public Transform CustomParent { get; private set; }
    public Func<UniTask> OnShown { get; private set; }
    public Func<UniTask> OnHidden { get; private set; }
    public float? AnimationDuration { get; private set; }
    
    // Fluent API методы
    public ViewShowParams SetDisplayMode(ViewDisplayMode mode);
    public ViewShowParams SetTransition(ViewTransitionStyle style);
    public ViewShowParams SetCanvas(CanvasLayer layer);
    public ViewShowParams SetCustomParent(Transform parent);
    public ViewShowParams SetOnShown(Func<UniTask> callback);
    public ViewShowParams SetOnHidden(Func<UniTask> callback);
        public ViewShowParams SetAnimationDuration(float duration);  // Валидация: duration >= 0
        
        // Фабричные методы
        public static ViewShowParams Default();    // Single + Fade
        public static ViewShowParams Instant();   // None + 0f duration
        public static ViewShowParams Overlay();   // Additive + Overlay canvas
}
```

**Дефолтные значения:**
- `DisplayMode = ViewDisplayMode.Single`
- `TransitionStyle = ViewTransitionStyle.Fade`
- `TargetCanvas = null`
- `CustomParent = null`
- `OnShown = null`
- `OnHidden = null`
- `AnimationDuration = null` (используются дефолтные значения из ViewAnimator)

**Валидация:**
- `SetAnimationDuration()` выбрасывает `ArgumentException` если duration < 0

**Фабричные методы:**
- `Default()` - создаёт новый экземпляр с дефолтными значениями (`DisplayMode.Single`, `TransitionStyle.Fade`)
- `Instant()` - создаёт экземпляр с `TransitionStyle.None` и `AnimationDuration = 0f` (nullable float, устанавливается в `0f` через `SetAnimationDuration(0f)`)
  - Используется для мгновенного показа/скрытия View без анимации
  - **Важно:** `AnimationDuration` устанавливается в `0f` (nullable float), а не `null`, что гарантирует отсутствие задержек даже если `ViewAnimator` использует дефолтные значения
- `Overlay()` - создаёт экземпляр с `DisplayMode.Additive` и `TargetCanvas = CanvasLayer.Overlay` (через `SetDisplayMode(ViewDisplayMode.Additive)` и `SetCanvas(CanvasLayer.Overlay)`)
  - Используется для показа View поверх существующих (например, попапы, диалоги)

**ViewDisplayMode:**
- `Single` - скрывает все non-persistent View перед показом нового (добавляет их в историю, View остаются закэшированными и могут быть показаны снова)
- `SingleClearAll` - закрывает все non-persistent View через `CloseAllNonPersistentAsync()` перед показом (скрывает и уничтожает non-persistent View через `HideAsync()` и `ViewFactory.Destroy()`)
- `Additive` - показывает поверх существующих View (не скрывает текущие)
- `Replace` - заменяет текущий View, используя `TransitionStyle` нового View для анимации скрытия предыдущего (не добавляет заменяемый View в историю)

**Детали реализации DisplayMode в ViewService:**
- `Single`: 
  - Собирает все non-persistent View из `_activeViews` (создаёт копию массива через `ToArray()` для безопасной итерации)
  - Для каждого View: получает `ViewShowParams` из `_viewParams` (если есть), скрывает через `HideViewInternalWithParams()` с `addToHistory = true`, иначе через `HideViewInternal()` с `addToHistory = true`
  - Удаляет View из `_activeViews` через пересборку стека (создаёт буферный стек, копирует все элементы кроме удаляемых через `ReferenceEquals` для сравнения)
  - Удаляет `ViewShowParams` из `_viewParams` для каждого скрытого View
  - Обновляет `_currentView` если удаляемый View был текущим (проверка через `ReferenceEquals`)
- `SingleClearAll`: 
  - Вызывает `CloseAllNonPersistentAsync()`, который скрывает все non-persistent View через `HideAsync()` (без добавления в историю)
  - `CloseAllNonPersistentAsync()` создаёт копию массива `_activeViews` через `ToArray()` для безопасной итерации
  - **Важно:** View, скрытые через `SingleClearAll`, не добавляются в историю, поэтому навигация назад через `GoBackAsync()` не вернёт к этим View
- `Additive`: 
  - Не выполняет никаких действий перед показом - View просто добавляется в `_activeViews` (если ещё не добавлен, проверяется через `Contains()` с использованием reference equality)
  - Существующие View остаются активными и видимыми
  - Полезно для показа попапов, диалогов поверх основного контента
- `Replace`: 
  - Скрывает текущий View (`_currentView`) через `HideViewInternalWithParams()` с параметрами **нового View** (`showParams`), а не параметрами заменяемого View
  - `addToHistory = false` (без добавления в историю)
  - Удаляет View из `_activeViews` через пересборку стека (использует `ReferenceEquals` для сравнения при удалении)
  - Удаляет `ViewShowParams` заменяемого View из `_viewParams`
  - **Важно:** При `Replace` анимация скрытия использует параметры нового View, что позволяет создать плавный переход
  - **Важно:** Заменяемый View не добавляется в историю, поэтому навигация назад через `GoBackAsync()` не вернёт к заменённому View

**ViewTransitionStyle (Flags):**
- `None = 0` - без анимации (проверяется в начале `AnimateShowAsync`/`AnimateHideAsync`)
- `Fade = 1 << 0` - плавное появление/исчезновение через `CanvasGroup.alpha`
- `SlideUp = 1 << 1` - скольжение снизу вверх
- `SlideDown = 1 << 2` - скольжение сверху вниз
- `SlideLeft = 1 << 3` - скольжение справа налево
- `SlideRight = 1 << 4` - скольжение слева направо
- `Scale = 1 << 5` - масштабирование через `Transform.localScale`
- `Bounce = 1 << 6` - модификатор для Slide анимаций с эффектом "отскока" (использует `Ease.OutBack`/`Ease.InBack`)

**Комбинирование анимаций:**
- Можно комбинировать через bitwise OR: `Fade | SlideUp | Scale`
- Все анимации выполняются параллельно через `UniTask.WhenAll(tasks)`
- `HasSlideFlag()` проверяет наличие любого Slide флага перед выполнением Slide анимации
- Если несколько Slide флагов установлены, все флаги комбинируются - View перемещается по диагонали (например, `SlideUp | SlideRight` создаст диагональное движение вверх-вправо)

---

### 8. ViewAnimator

Реализует анимации показа/скрытия View с использованием **LitMotion** — высокопроизводительной библиотеки для анимаций в Unity (Burst-compiled, zero allocation).

```csharp
public interface IViewAnimator
{
    UniTask AnimateShowAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null);
    UniTask AnimateHideAsync(IView view, ViewTransitionStyle transitionStyle, float? duration = null);
    void StopAnimation(IView view);
}
```

**Поддерживаемые анимации:**
- **Fade** - через `CanvasGroup.alpha` (автоматически создаёт CanvasGroup если отсутствует)
- **Slide** - через `RectTransform.anchoredPosition` (SlideUp/Down/Left/Right)
- **Scale** - через `Transform.localScale`
- Комбинации через bitwise OR

**Дефолтные длительности:**
- `DefaultShowDuration = 0.25f` - для показа
- `DefaultHideDuration = 0.2f` - для скрытия
- Можно переопределить через `ViewShowParams.SetAnimationDuration()`

**LitMotion Ease функции:**
- `Ease.OutQuad` - для Fade показа
- `Ease.InQuad` - для Fade скрытия
- `Ease.OutCubic` - для Slide показа
- `Ease.InCubic` - для Slide скрытия и Scale скрытия
- `Ease.OutBack` - для Scale показа (эффект "отскока")

**Реализация с LitMotion:**

```csharp
// Пример Fade анимации
private static MotionHandle AnimateFade(Component component, float from, float to, float duration, Ease ease)
{
    var canvasGroup = component.GetComponent<CanvasGroup>();
    if (canvasGroup == null)
        canvasGroup = component.gameObject.AddComponent<CanvasGroup>();

    canvasGroup.alpha = from;
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = to > 0.5f;

    return LMotion.Create(from, to, duration)
        .WithEase(ease)
        .WithOnComplete(() =>
        {
            canvasGroup.alpha = to;
            canvasGroup.interactable = to > 0.5f;
            canvasGroup.blocksRaycasts = to > 0.5f;
        })
        .Bind(alpha => canvasGroup.alpha = alpha);
}
```

**Особенности LitMotion реализации:**
- **Zero allocation** — никаких аллокаций во время анимации
- **Burst-compiled** — высокая производительность
- Отслеживает активные анимации через `Dictionary<IView, List<MotionHandle>>`
- `MotionHandle` позволяет управлять каждой анимацией отдельно
- `handle.ToUniTask()` для async/await интеграции
- `handle.IsActive()` для проверки активности
- `handle.Cancel()` для остановки анимации
- Все анимации выполняются параллельно через `UniTask.WhenAll(tasks)`

**Управление анимациями:**
```csharp
public void StopAnimation(IView view)
{
    if (!_activeAnimations.TryGetValue(view, out var handles))
        return;

    foreach (var handle in handles)
    {
        if (handle.IsActive())
            handle.Cancel();
    }
    _activeAnimations.Remove(view);
}
```

**Детали реализации:**

**Fade:**
- Автоматически создаёт `CanvasGroup` если отсутствует
- Устанавливает `blocksRaycasts` в зависимости от направления
- Использует `Ease.OutQuad` для показа, `Ease.InQuad` для скрытия

**Slide:**
- Использует `RectTransform.anchoredPosition`
- `GetOffscreenPosition()` вычисляет позицию за пределами экрана (+100f отступ)
- Использует `Ease.OutCubic` для показа, `Ease.InCubic` для скрытия

**Scale:**
- Использует `Transform.localScale`
- От 0 до 1 для показа с `Ease.OutBack` (эффект "отскока")
- От 1 до 0 для скрытия с `Ease.InCubic`

**⚠️ ВАЖНО: BindableView НЕ анимирует!**

`BindableView.PlayShowAnimation()` и `PlayHideAnimation()` НЕ содержат анимации — только устанавливают финальные значения. Вся анимация происходит через `ViewAnimator`.

---

private static MotionHandle AnimateScale(Component component, float from, float to, float duration, Ease ease)
{
    var transform = component.transform;
    transform.localScale = Vector3.one * from;

    return LMotion.Create(from, to, duration)
        .WithEase(ease)
        .WithOnComplete(() => transform.localScale = Vector3.one * to)
        .Bind(s => transform.localScale = Vector3.one * s);
}
```
- Анимирует uniform scale (одинаковый по всем осям)
- **Показ:** `Ease.OutBack` (0 → 1) — эффект "отскока"
- **Скрытие:** `Ease.InCubic` (1 → 0) — плавное уменьшение

**Вспомогательный метод HasSlideFlag:**
```csharp
private static bool HasSlideFlag(ViewTransitionStyle style)
{
    return style.HasFlag(ViewTransitionStyle.SlideUp) ||
           style.HasFlag(ViewTransitionStyle.SlideDown) ||
           style.HasFlag(ViewTransitionStyle.SlideLeft) ||
           style.HasFlag(ViewTransitionStyle.SlideRight);
}
```
- Проверяет наличие любого Slide флага перед выполнением Slide анимации

---

### 9. CanvasProvider

Управляет Canvas слоями для организации UI иерархии.

```csharp
public interface ICanvasProvider
{
    Canvas GetCanvas(CanvasLayer layer);
    Transform GetCanvasTransform(CanvasLayer layer);
    void RegisterCanvas(CanvasLayer layer, Canvas canvas);
    void UnregisterCanvas(CanvasLayer layer);
    bool HasCanvas(CanvasLayer layer);
}
```

**CanvasLayer enum:**
- `World` - 3D UI в мировом пространстве
- `Screen` - основной UI экрана
- `Overlay` - поверх всего (попапы, диалоги)
- `System` - системный UI (загрузка, ошибки)

**Особенности:**
- Потокобезопасная регистрация через `lock(_lock)` объекта
- Логирование при регистрации/отмене регистрации Canvas через `Debug.Log()` и `Debug.LogWarning()`
- Предупреждение при перезаписи существующего Canvas (проверка `_canvases.ContainsKey(layer)`)
- `HasCanvas(layer)` - проверка наличия зарегистрированного Canvas (потокобезопасная, через `lock`)
- `GetCanvas(layer)` - возвращает Canvas для указанного слоя или `null` если не зарегистрирован (потокобезопасная)
- `GetCanvasTransform(layer)` - возвращает Transform Canvas для указанного слоя или `null` (потокобезопасная)
- `RegisterCanvas(layer, canvas)` - выбрасывает `ArgumentNullException` если canvas равен `null`
- `UnregisterCanvas(layer)` - удаляет Canvas из словаря, логирует предупреждение если Canvas не был зарегистрирован

---

### 10. UIElementFactory

Простая фабрика для создания UI элементов с автоматической инъекцией зависимостей через Reflex DI.

```csharp
public interface IUIElementFactory
{
    GameObject Instantiate(GameObject prefab, Transform parent);
    T Instantiate<T>(T prefab, Transform parent) where T : Component;
}
```

**Реализация:**
```csharp
public class UIElementFactory : IUIElementFactory
{
    private readonly Container _container;

    public UIElementFactory(Container container)
    {
        _container = container;
    }

    public GameObject Instantiate(GameObject prefab, Transform parent)
    {
        var instance = Object.Instantiate(prefab, parent);
        GameObjectInjector.InjectRecursive(instance, _container);
        return instance;
    }

    public T Instantiate<T>(T prefab, Transform parent) where T : Component
    {
        var instance = Object.Instantiate(prefab, parent);
        GameObjectInjector.InjectRecursive(instance.gameObject, _container);
        return instance;
    }
}
```

**Когда использовать:**
- Создание динамических UI элементов (списки, ячейки, панели)
- Когда элементам нужны зависимости из DI контейнера
- Альтернатива ручному вызову `Object.Instantiate()` + `GameObjectInjector.InjectRecursive()`

**Пример использования:**
```csharp
public class ItemListView : MonoBehaviour
{
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private Transform _container;
    
    private IUIElementFactory _uiFactory;
    
    [Inject]
    public void Construct(IUIElementFactory uiFactory)
    {
        _uiFactory = uiFactory;
    }
    
    public void AddItem(ItemModel item)
    {
        var itemView = _uiFactory.Instantiate<ItemView>(_itemPrefab, _container);
        itemView.Setup(item);
    }
}
```

**Отличие от ViewFactory:**
- `ViewFactory` — для View с ViewModel (загрузка через Addressables, биндинг ViewModel)
- `UIElementFactory` — для простых UI элементов (прямое инстанцирование prefab)

---

### 11. EmbeddedPanelService

Сервис для управления встраиваемыми (embedded) панелями — панелями, которые встроены в другой View и показываются/скрываются с анимацией.

**Интерфейс:**
```csharp
public interface IEmbeddedPanelHost
{
    void RegisterPanel<TViewModel>(BindableView<TViewModel> view)
        where TViewModel : DisposableObject;

    UniTask<bool> ShowEmbeddedAsync<TViewModel>(
        IViewContext context,
        CancellationToken ct = default)
        where TViewModel : DisposableObject;

    UniTask HideEmbeddedAsync<TViewModel>(
        CancellationToken ct = default)
        where TViewModel : DisposableObject;

    bool IsEmbeddedVisible<TViewModel>()
        where TViewModel : DisposableObject;

    TViewModel GetEmbeddedViewModel<TViewModel>()
        where TViewModel : DisposableObject;
}
```

**Реализация:**
```csharp
public class EmbeddedPanelService : IEmbeddedPanelHost
{
    private readonly Container _container;
    private readonly IViewAnimator _viewAnimator;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    private readonly Dictionary<Type, EmbeddedPanelInfo> _panels = new();

    private const float AnimationDuration = 0.3f;

    // Маппинг анимаций для разных панелей
    // SlideLeft = появляется справа, уезжает влево
    // SlideRight = появляется слева, уезжает вправо
    // При Hide направление инвертируется
    private static readonly Dictionary<Type, ViewTransitionStyle> PanelTransitions = new()
    {
        { typeof(HeroesGridPanelViewModel), ViewTransitionStyle.SlideLeft | ViewTransitionStyle.Fade },
        { typeof(HeroDetailPanelViewModel), ViewTransitionStyle.SlideRight | ViewTransitionStyle.Fade }
    };

    private static ViewTransitionStyle GetTransitionStyle(Type vmType)
    {
        return PanelTransitions.TryGetValue(vmType, out var style)
            ? style
            : ViewTransitionStyle.SlideLeft | ViewTransitionStyle.Fade; // default
    }
    
    // ...
}
```

**Особенности:**
- Панели регистрируются через `RegisterPanel<TViewModel>()` (обычно при инициализации родительского View)
- ViewModel разрешается из DI контейнера при регистрации
- Биндинг View↔ViewModel происходит лениво при первом показе
- Используется `IViewContext` для передачи контекста в панель (например, `HeroesGridContext`)
- Публикует `EmbeddedPanelStateChangedNotification` через `IMediator` при изменении состояния

**EmbeddedPanelInfo (внутренний класс):**
```csharp
private class EmbeddedPanelInfo
{
    public IView View;
    public GameObject GameObject;
    public DisposableObject ViewModel;
    public bool IsVisible;
    public bool IsBound;  // Флаг, был ли ViewModel привязан к View
}
```

**Жизненный цикл:**
1. `RegisterPanel<TViewModel>(view)` — регистрация панели:
   - Деактивирует GameObject панели
   - Разрешает ViewModel из DI контейнера
   - Сохраняет информацию в `_panels`
2. `ShowEmbeddedAsync<TViewModel>(context)` — показ панели:
   - Если уже видима — применяет контекст и возвращает `true`
   - Биндит View↔ViewModel (если ещё не привязан)
   - Применяет контекст через `ApplyContext()`
   - Активирует GameObject
   - Проигрывает анимацию показа через `IViewAnimator`
   - Публикует `EmbeddedPanelStateChangedNotification(vmType, true)`
3. `HideEmbeddedAsync<TViewModel>()` — скрытие панели:
   - Проигрывает анимацию скрытия
   - Деактивирует GameObject
   - Публикует `EmbeddedPanelStateChangedNotification(vmType, false)`

**Пример использования:**
```csharp
public class MainScreenView : BindableView<MainScreenViewModel>
{
    [SerializeField] private HeroesGridPanelView _heroesPanel;
    
    private IEmbeddedPanelHost _panelHost;
    
    [Inject]
    public void Construct(IEmbeddedPanelHost panelHost)
    {
        _panelHost = panelHost;
    }
    
    protected override void OnBind(ref DisposableBag disposables)
    {
        // Регистрируем встраиваемую панель
        _panelHost.RegisterPanel(_heroesPanel);
        
        // Подписываемся на команду показа панели
        ViewModel.ShowHeroesPanelCommand
            .Subscribe(this, static (_, self) => self.ShowHeroesPanel().Forget())
            .AddTo(ref disposables);
    }
    
    private async UniTaskVoid ShowHeroesPanel()
    {
        var context = new HeroesGridContext(HeroesGridMode.Selection);
        await _panelHost.ShowEmbeddedAsync<HeroesGridPanelViewModel>(context);
    }
}
```

**EmbeddedPanelStateChangedNotification:**
```csharp
public readonly struct EmbeddedPanelStateChangedNotification : INotification
{
    public readonly Type ViewModelType;
    public readonly bool IsVisible;

    public EmbeddedPanelStateChangedNotification(Type vmType, bool isVisible)
    {
        ViewModelType = vmType;
        IsVisible = isVisible;
    }
}
```

**Подписка на изменения:**
```csharp
public class SomeController : INotificationHandler<EmbeddedPanelStateChangedNotification>
{
    public async UniTask Handle(EmbeddedPanelStateChangedNotification notification, CancellationToken ct)
    {
        if (notification.ViewModelType == typeof(HeroesGridPanelViewModel))
        {
            Debug.Log($"Heroes panel is now {(notification.IsVisible ? "visible" : "hidden")}");
        }
    }
}
```

**Отличие от ViewService:**
- `ViewService` — для полноэкранных View с навигацией и историей
- `EmbeddedPanelService` — для панелей, встроенных в другой View (без навигации)

---

### 12. ViewModel (IViewModel)

Интерфейс для всех ViewModel. Должен наследоваться от `DisposableObject`.

```csharp
public interface IViewModel : IDisposable
{
}
```

**Обязанности ViewModel:**
- Хранение состояния через `ReactiveProperty<T>`
- Команды через `ReactiveCommand<T>`
- Бизнес-логика представления
- Взаимодействие с Domain Services
- Управление подписками (через DisposableBag)

**Требования:**
- Все ViewModel должны наследоваться от `DisposableObject`
- Все `ReactiveProperty` и `ReactiveCommand` должны быть добавлены в `Disposables` через `.AddTo(ref Disposables)`
- Все подписки на события должны быть добавлены в `Disposables`
- ViewModel должен быть зарегистрирован в DI контейнере (обычно в `InfrastructureBindings` или `ProjectInstaller`)

---

### 13. Model (IModel)

Domain модели для хранения данных и бизнес-логики.

```csharp
public interface IModel : IDisposable
{
    bool IsValid { get; }      // Валидность данных модели
    void Reset();              // Сброс к начальному состоянию
}
```

**Характеристики:**
- События изменений (`event Action<T> PropertyChanged`)
- Валидация данных через `IsValid`
- Состояние приложения
- Независимость от View/ViewModel
- Метод `Reset()` для возврата к начальному состоянию

**Отношение к ViewModel:**
- Model содержит бизнес-логику и состояние данных
- ViewModel адаптирует Model для View, добавляя реактивные свойства и команды
- ViewModel подписывается на события Model для синхронизации состояния
- Model может использоваться несколькими ViewModel одновременно

---

### 14. Element System

Element System - подсистема MVVM для работы с динамическими UI элементами (ячейки списков, карточки, элементы коллекций). В отличие от View, которые представляют экраны с навигацией, Element - это повторяемые UI компоненты внутри View.

**Архитектура Element System:**

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ELEMENT SYSTEM                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌───────────────────────────────────────────────────────────────────┐    │
│   │                    ELEMENT LAYER                                   │    │
│   │  ┌─────────────────────────────────────────────────────────────┐  │    │
│   │  │  BindableElement<TData> : MonoBehaviour                     │  │    │
│   │  │  - UI представление элемента (Image, Text, Button)          │  │    │
│   │  │  - Bind/Refresh/Unbind жизненный цикл                       │  │    │
│   │  │  - Selection state                                          │  │    │
│   │  │  - Popup интеграция                                         │  │    │
│   │  └─────────────────────────────────────────────────────────────┘  │    │
│   └───────────────────────────────────────────────────────────────────┘    │
│                              │ Binds to                                     │
│   ┌──────────────────────────▼────────────────────────────────────────┐    │
│   │                    DATA LAYER                                     │    │
│   │  ┌─────────────────────────────────────────────────────────────┐  │    │
│   │  │  TData : struct, IElementData                               │  │    │
│   │  │  - Id для идентификации элемента                            │  │    │
│   │  │  - Данные для отображения                                   │  │    │
│   │  │  - Immutable struct для производительности                  │  │    │
│   │  └─────────────────────────────────────────────────────────────┘  │    │
│   └───────────────────────────────────────────────────────────────────┘    │
│                                                                             │
│   ┌───────────────────────────────────────────────────────────────────┐    │
│   │                    SERVICE LAYER                                   │    │
│   │  ┌──────────────────────┐  ┌──────────────────────────────────┐  │    │
│   │  │ UIElementService     │  │ ElementConfig                    │  │    │
│   │  │ - CreateElementAsync │  │ - Element -> Prefab mapping      │  │    │
│   │  │ - DestroyElement     │  │ - Addressable addresses          │  │    │
│   │  │ - ClearContainer     │  │                                  │  │    │
│   │  │ - ReconcileElements  │  │                                  │  │    │
│   │  └──────────────────────┘  └──────────────────────────────────┘  │    │
│   └───────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### IElementData

Базовый интерфейс для данных элемента.

```csharp
namespace Source.Runtime.Core.Data.Domain.Models.Elements
{
    public interface IElementData
    {
        string Id { get; }
    }
}
```

**Характеристики:**
- `Id` - уникальный идентификатор элемента для reconciliation
- Должен быть `struct` для производительности (избежание GC)
- Используется для сравнения элементов при обновлении списков

**Пример реализации:**
```csharp
public readonly struct HeroCardData : IElementData, IEquatable<HeroCardData>
{
    public string Id { get; }
    public string Name { get; }
    public int Level { get; }
    public Sprite Icon { get; }

    public HeroCardData(string id, string name, int level, Sprite icon)
    {
        Id = id;
        Name = name;
        Level = level;
        Icon = icon;
    }

    public bool Equals(HeroCardData other) => Id == other.Id && Level == other.Level;
}
```

#### BindableElement

Базовый класс для всех UI элементов. Аналог `BindableView`, но для элементов внутри View.

```csharp
[RequireComponent(typeof(RectTransform))]
public abstract class BindableElement<TData> : MonoBehaviour, ISelectableElement<TData>
    where TData : struct, IElementData
{
    [Header("Interaction")]
    [SerializeField] protected Button _clickButton;

    [Header("Selection")]
    [SerializeField] protected GameObject _selectionIndicator;

    [Header("Popup")]
    [SerializeField] protected PopupTrigger _popupTrigger;

    public string ElementId { get; }        // Id из текущих данных (null если не привязан)
    public bool IsBound { get; }            // Привязан ли элемент
    public bool IsSelected { get; }         // Выбран ли элемент
    public bool HasPopup { get; }           // Есть ли popup trigger

    public Action<TData> OnClicked { set; } // Callback при клике

    protected TData CurrentData { get; }    // Текущие данные
    protected ref DisposableBag Disposables { get; }  // Для подписок

    public void Bind(TData data);           // Привязать данные
    public void Refresh(TData data);        // Обновить данные
    public void SetSelected(bool selected); // Установить состояние выбора
    public void Unbind();                   // Отвязать данные
    public void Dispose();                  // Освободить ресурсы

    protected abstract void OnBind(TData data);
    protected virtual void OnRefresh(TData oldData, TData newData);
    protected virtual void OnUnbind();
    protected virtual void OnSelectionChanged(bool selected);
    protected virtual void OnDispose();
    protected virtual IPopupData GetPopupData(TData data);
}
```

**Жизненный цикл:**
1. `Awake()` - инициализация DisposableBag, настройка click handler
2. `Bind(data)` - первичная привязка данных, вызывает `OnBind()`
3. `Refresh(data)` - обновление данных, вызывает `OnRefresh()`
4. `SetSelected(selected)` - изменение состояния выбора
5. `Unbind()` - отвязка данных, сброс состояния
6. `Dispose()` - полная очистка ресурсов

**Особенности:**
- `Bind()` автоматически вызывает `Refresh()` если элемент уже привязан
- `Refresh()` автоматически вызывает `Bind()` если элемент не привязан
- `_clickButton` автоматически подписывается на клик при инициализации
- `_selectionIndicator` автоматически показывается/скрывается при `SetSelected()`
- `_popupTrigger` автоматически получает данные через `GetPopupData()`

**Пример реализации:**
```csharp
public class HeroCardElement : BindableElement<HeroCardData>
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _levelText;

    protected override void OnBind(HeroCardData data)
    {
        _iconImage.sprite = data.Icon;
        _nameText.text = data.Name;
        _levelText.text = $"Lv. {data.Level}";
    }

    protected override void OnRefresh(HeroCardData oldData, HeroCardData newData)
    {
        // Оптимизация: обновляем только изменившиеся поля
        if (oldData.Level != newData.Level)
        {
            _levelText.text = $"Lv. {newData.Level}";
        }
        // Имя и иконка обычно не меняются, поэтому не обновляем
    }

    protected override void OnSelectionChanged(bool selected)
    {
        // Дополнительная логика при выборе (анимация, звук)
        if (selected)
        {
            transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
        }
        else
        {
            transform.DOScale(1f, 0.1f);
        }
    }

    protected override IPopupData GetPopupData(HeroCardData data)
    {
        return new HeroPopupData(data.Id, data.Name, data.Level);
    }
}
```

**Иерархия интерфейсов:**
```
IBindableElement
    ├── ElementId: string
    ├── IsBound: bool
    ├── Unbind()
    └── Dispose()
         │
         ▼
IBindableElement<TData>
    ├── Bind(TData data)
    └── Refresh(TData data)
         │
         ▼
IClickableElement<TData>
    └── OnClicked: Action<TData> { set; }
         │
         ▼
ISelectableElement<TData>
    ├── IsSelected: bool
    └── SetSelected(bool selected)
```

#### ElementConfig

ScriptableObject для маппинга Element типов на Addressable префабы. Аналог `ViewConfig` для элементов.

**Создание:**
**Create -> MVVM -> Element Config**

```csharp
[CreateAssetMenu(fileName = "ElementConfig", menuName = "MVVM/Element Config")]
public class ElementConfig : ConfigBase, IElementConfig
{
    [SerializeField] internal List<ElementMappingBase> _mappings;

    internal Dictionary<Type, ElementMappingBase> _elementTypeCache;
    internal Dictionary<Type, ElementMappingBase> _dataTypeCache;

    public string GetElementAddress<TElement, TData>();
    public string GetElementAddress(Type elementType);
    public AssetReferenceT<GameObject> GetElementReference<TElement, TData>();
    public AssetReferenceT<GameObject> GetElementReference(Type elementType);
    public bool TryGetMapping<TElement, TData>(out ElementMappingBase mapping);
    public bool TryGetMapping(Type elementType, out ElementMappingBase mapping);
}
```

**Особенности:**
- Два кэша для поиска: по типу Element и по типу Data
- Автоматическое построение кэша в `OnEnable()` через `BuildCache()`
- Поддержка Editor API для добавления/удаления маппингов

**Editor API (только в Unity Editor):**
```csharp
#if UNITY_EDITOR
public void AddMapping(ElementMappingBase mapping);
public void RemoveMapping(ElementMappingBase mapping);
public IReadOnlyList<ElementMappingBase> GetAllMappings();
#endif
```

#### ElementMappingBase

Связывает Element и Data типы с Addressable префабом. Аналог `ViewMappingBase` для элементов.

```csharp
public abstract class ElementMappingBase : ScriptableObject
{
    public AssetReferenceT<GameObject> ElementPrefab;
    public abstract Type ElementType { get; }
    public abstract Type DataType { get; }
}

public abstract class ElementMappingBase<TElement, TData> : ElementMappingBase
    where TElement : BindableElement<TData>
    where TData : struct, IElementData
{
    public override Type ElementType => typeof(TElement);
    public override Type DataType => typeof(TData);
}
```

**Пример:**
```csharp
public class HeroCardElementMapping : ElementMappingBase<HeroCardElement, HeroCardData>
{
    // ElementPrefab устанавливается в Inspector
}
```

#### UIElementService

Сервис для создания, управления и reconciliation UI элементов. Ключевой компонент для работы с динамическими списками.

```csharp
public interface IUIElementService
{
    UniTask<TElement> CreateElementAsync<TElement, TData>(
        Transform container,
        TData data,
        Action<TData> onClick = null)
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData;

    void DestroyElement(IBindableElement element);

    void ClearContainer(Transform container);

    IReadOnlyList<TElement> GetElementsInContainer<TElement, TData>(Transform container)
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData;

    UniTask ReconcileElementsAsync<TElement, TData>(
        Transform container,
        IReadOnlyList<TData> newData,
        Action<TData> onClick = null)
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData, IEquatable<TData>;
}
```

**Методы:**

**CreateElementAsync:**
- Создает новый элемент из Addressable префаба
- Выполняет DI инъекцию через `GameObjectInjector.InjectRecursive()`
- Устанавливает `OnClicked` callback (если элемент реализует `IClickableElement`)
- Вызывает `Bind(data)` для привязки данных
- Отслеживает элемент в контейнере

**DestroyElement:**
- Вызывает `Dispose()` на элементе
- Освобождает Addressable инстанс через `IAssetProvider.ReleaseInstance()`
- Удаляет из отслеживания

**ClearContainer:**
- Уничтожает все отслеживаемые элементы в контейнере
- Освобождает все Addressable инстансы
- Очищает дочерние объекты контейнера

**GetElementsInContainer:**
- Возвращает список элементов определенного типа в контейнере
- Используется для получения текущего состояния

**ReconcileElementsAsync:**
- Умное обновление списка элементов с минимальными изменениями
- Сравнивает текущие элементы с новыми данными по `Id`
- Удаляет элементы, которых нет в новых данных
- Обновляет существующие элементы через `Refresh()`
- Создает новые элементы для новых данных
- Сохраняет порядок элементов через `SetSiblingIndex()`
- **Важно:** `TData` должен реализовывать `IEquatable<TData>` для reconciliation

**Пример использования:**
```csharp
public class HeroListView : BindableView<HeroListViewModel>
{
    [SerializeField] private Transform _heroContainer;

    private IUIElementService _elementService;

    [Inject]
    public void Construct(IUIElementService elementService)
    {
        _elementService = elementService;
    }

    protected override void OnBind(ref DisposableBag disposables)
    {
        // Подписка на изменение списка героев
        ViewModel.Heroes
            .Subscribe(this, static (heroes, self) => self.UpdateHeroes(heroes).Forget())
            .AddTo(ref disposables);
    }

    private async UniTaskVoid UpdateHeroes(IReadOnlyList<HeroCardData> heroes)
    {
        // Reconciliation автоматически:
        // - Удалит карточки героев, которых больше нет
        // - Обновит существующие карточки через Refresh()
        // - Создаст новые карточки для новых героев
        // - Сохранит порядок
        await _elementService.ReconcileElementsAsync<HeroCardElement, HeroCardData>(
            _heroContainer,
            heroes,
            OnHeroClicked
        );
    }

    private void OnHeroClicked(HeroCardData data)
    {
        ViewModel.SelectHeroCommand.Execute(data.Id);
    }
}
```

**Отличие от UIElementFactory:**
- `UIElementFactory` - простое инстанцирование prefab с DI инъекцией (синхронное)
- `UIElementService` - полное управление элементами через Addressables (асинхронное, с отслеживанием, reconciliation)

---

## Интерфейсы системы

### IView

Базовый интерфейс для всех View.

```csharp
public interface IView : IDisposable
{
    bool IsPersistent { get; }
    IViewModel ViewModel { get; }
}
```

**Особенности:**
- Наследует `IDisposable` для управления ресурсами
- `IsPersistent` определяет, будет ли View закрыт автоматически при показе нового View
- `ViewModel` — доступ к текущему ViewModel через интерфейс `IViewModel` (позволяет получить ViewModel без знания конкретного типа)
- Реализуется в `BindableView<TViewModel>` через explicit interface implementation для `ViewModel`

### IShowable

Интерфейс для View, которые могут быть показаны с анимацией.

```csharp
public interface IShowable
{
    UniTask ShowAsync();
}
```

**Особенности:**
- Используется в `ViewService` для показа View через `ShowViewInternal()` и `ShowViewInternalWithParams()`
- Реализуется в `BindableView<TViewModel>`
- Если View не реализует `IShowable`, используется `Component.gameObject.SetActive(true)` как fallback

### IHideable

Интерфейс для View, которые могут быть скрыты с анимацией.

```csharp
public interface IHideable
{
    UniTask HideAsync();
}
```

**Особенности:**
- Используется в `ViewService` для скрытия View через `HideViewInternal()` и `HideViewInternalWithParams()`
- Реализуется в `BindableView<TViewModel>`
- Если View не реализует `IHideable`, используется `Component.gameObject.SetActive(false)` как fallback

### IUnbindable

Интерфейс для View, которые могут отвязать ViewModel.

```csharp
public interface IUnbindable
{
    void Unbind();
}
```

**Особенности:**
- Используется в `ViewService.ClearCache()` для очистки подписок перед уничтожением View
- Используется в `ViewFactory.Destroy()` для очистки подписок перед освобождением инстанса
- Реализуется в `BindableView<TViewModel>`
- `Unbind()` устанавливает `ViewModel = null` и очищает все подписки через `DisposableBag`

### IUIElementFactory

Интерфейс для фабрики UI элементов с DI инъекцией.

```csharp
public interface IUIElementFactory
{
    GameObject Instantiate(GameObject prefab, Transform parent);
    T Instantiate<T>(T prefab, Transform parent) where T : Component;
}
```

**Особенности:**
- Инстанцирует prefab как дочерний элемент указанного `parent`
- Автоматически выполняет `GameObjectInjector.InjectRecursive()` для инъекции зависимостей
- Generic версия позволяет получить конкретный компонент без приведения типов
- Реализуется в `UIElementFactory`
- **Не путать с `IViewFactory`** — `IUIElementFactory` для простых UI элементов, `IViewFactory` для View с ViewModel

### IEmbeddedPanelHost

Интерфейс для хоста встраиваемых панелей.

```csharp
public interface IEmbeddedPanelHost
{
    void RegisterPanel<TViewModel>(BindableView<TViewModel> view)
        where TViewModel : DisposableObject;

    UniTask<bool> ShowEmbeddedAsync<TViewModel>(
        IViewContext context,
        CancellationToken ct = default)
        where TViewModel : DisposableObject;

    UniTask HideEmbeddedAsync<TViewModel>(
        CancellationToken ct = default)
        where TViewModel : DisposableObject;

    bool IsEmbeddedVisible<TViewModel>()
        where TViewModel : DisposableObject;

    TViewModel GetEmbeddedViewModel<TViewModel>()
        where TViewModel : DisposableObject;
}
```

**Особенности:**
- Управляет встраиваемыми панелями внутри View
- Панели регистрируются при инициализации родительского View
- Поддерживает контекст через `IViewContext` для передачи параметров
- Возвращает `false` если панель не зарегистрирована
- Реализуется в `EmbeddedPanelService`

### IElementConfig

Интерфейс конфигурации для Element System.

```csharp
public interface IElementConfig
{
    string GetElementAddress<TElement, TData>()
        where TElement : MonoBehaviour
        where TData : struct, IElementData;

    string GetElementAddress(Type elementType);

    AssetReferenceT<GameObject> GetElementReference<TElement, TData>()
        where TElement : MonoBehaviour
        where TData : struct, IElementData;

    AssetReferenceT<GameObject> GetElementReference(Type elementType);
}
```

**Особенности:**
- Аналог `IViewConfig` для Element System
- Поддерживает generic и non-generic методы получения адреса/референса
- Реализуется в `ElementConfig`

### IUIElementService

Интерфейс сервиса для управления UI элементами.

```csharp
public interface IUIElementService
{
    UniTask<TElement> CreateElementAsync<TElement, TData>(
        Transform container,
        TData data,
        Action<TData> onClick = null)
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData;

    void DestroyElement(IBindableElement element);

    void ClearContainer(Transform container);

    IReadOnlyList<TElement> GetElementsInContainer<TElement, TData>(Transform container)
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData;

    UniTask ReconcileElementsAsync<TElement, TData>(
        Transform container,
        IReadOnlyList<TData> newData,
        Action<TData> onClick = null)
        where TElement : MonoBehaviour, IBindableElement<TData>
        where TData : struct, IElementData, IEquatable<TData>;
}
```

**Особенности:**
- Создание элементов через Addressables с DI инъекцией
- Reconciliation для оптимального обновления списков
- Отслеживание элементов в контейнерах
- Реализуется в `UIElementService`

### IBindableElement / IBindableElement<TData>

Интерфейсы для биндинг элементов.

```csharp
public interface IBindableElement : IDisposable
{
    string ElementId { get; }
    bool IsBound { get; }
    void Unbind();
}

public interface IBindableElement<TData> : IBindableElement
    where TData : struct
{
    void Bind(TData data);
    void Refresh(TData data);
}
```

**Особенности:**
- `ElementId` возвращает `Id` из текущих данных (или `null` если не привязан)
- `IsBound` указывает, привязаны ли данные к элементу
- `Bind()` для первичной привязки данных
- `Refresh()` для обновления существующих данных
- Реализуется в `BindableElement<TData>`

### IClickableElement<TData>

Интерфейс для кликабельных элементов.

```csharp
public interface IClickableElement<TData> : IBindableElement<TData>
    where TData : struct
{
    Action<TData> OnClicked { set; }
}
```

**Особенности:**
- Добавляет callback для обработки кликов
- Callback получает текущие данные элемента
- Реализуется в `BindableElement<TData>`

### ISelectableElement<TData>

Интерфейс для выбираемых элементов.

```csharp
public interface ISelectableElement<TData> : IClickableElement<TData>
    where TData : struct
{
    bool IsSelected { get; }
    void SetSelected(bool selected);
}
```

**Особенности:**
- Добавляет состояние выбора
- `SetSelected()` управляет визуальным индикатором выбора
- Реализуется в `BindableElement<TData>`

---

## Внутренние детали реализации

### ViewConfigInternals

Файл `ViewConfigInternals.cs` предоставляет доступ к внутренним методам для тестирования через атрибуты `InternalsVisibleTo`:

```csharp
[assembly: InternalsVisibleTo("EditMode")]
[assembly: InternalsVisibleTo("PlayMode")]
[assembly: InternalsVisibleTo("Tests.MVVM")]
```

Это позволяет тестам вызывать внутренние методы, такие как `BuildCache()`.

**Доступные внутренние члены для тестирования:**
- `ViewConfig._mappings` - список всех маппингов
- `ViewConfig._typeCache` - кэш маппингов по типу ViewModel
- `ViewConfig.BuildCache()` - метод для пересборки кэша (полезно в тестах после изменения маппингов)

**Использование в тестах:**
```csharp
[Test]
public void ViewConfig_ShouldBuildCache()
{
    var config = ScriptableObject.CreateInstance<ViewConfig>();
    // Добавляем маппинги...
    config.BuildCache(); // Вызываем внутренний метод
    // Проверяем результат...
}
```

### Загрузка ViewConfig

ViewConfig загружается автоматически при регистрации в DI контейнере в методе `InfrastructureBindings.AddInfrastructure()`:

```csharp
builder.AddSingleton<IViewConfig>(container =>
{
    var configProvider = container.Resolve<IConfigProvider>();
    var config = configProvider.GetConfig<ViewConfig>();
    return config == null 
        ? throw new System.Exception("ViewConfig not found in ConfigProvider") 
        : config;
});
```

**Важно:** 
- Файл должен быть в `Assets/Source/00. Configs/ViewConfig.asset`
- Файл должен быть добавлен в Addressables с адресом `ViewConfig`
- `ConfigProvider` использует `IAssetProvider` для загрузки через Addressables
- Если файл не найден, приложение не запустится (выбрасывается исключение)

**Регистрация всех MVVM компонентов:**
Все компоненты MVVM системы регистрируются в `InfrastructureBindings.AddInfrastructure()`:
- `IAssetProvider` → `AssetProvider` (singleton, создаётся через `new AssetProvider()`)
- `ICanvasProvider` → `CanvasProvider` (singleton, создаётся через `new CanvasProvider()`)
- `IViewAnimator` → `ViewAnimator` (singleton, создаётся через `new ViewAnimator()`)
- `IViewConfig` → `ViewConfig` (singleton, загружается через `ConfigProvider.GetConfig<ViewConfig>()` из Addressables, выбрасывает исключение если не найден)
- `IViewFactory` → `ViewFactory` (singleton, создаётся с зависимостями: `container` (текущий DI контейнер), `IAssetProvider`, `IViewConfig`, `ICanvasProvider`)
- `IViewService` → `ViewService` (singleton, создаётся с зависимостями: `IViewFactory`, `IViewAnimator`)
- `IUIElementFactory` → `UIElementFactory` (singleton, создаётся с зависимостью: `container`)
- `IEmbeddedPanelHost` → `EmbeddedPanelService` (singleton, создаётся с зависимостями: `container`, `IViewAnimator`, `IMediator`, `ILogger`)

**Важно:** Все компоненты регистрируются как singleton, что означает, что они создаются один раз и переиспользуются на протяжении всего жизненного цикла приложения.

### UIRoot создание

`ViewFactory` автоматически создаёт корневой GameObject для UI:

```csharp
private static Transform CreateUIRoot()
{
    var existing = GameObject.Find("[UIRoot]");
    if (existing != null)
    {
        return existing.transform;
    }
    var root = new GameObject("[UIRoot]");
    
    if (Application.isPlaying)
    {
        Object.DontDestroyOnLoad(root);
    }
    
    return root.transform;
}
```

**Особенности:**
- Ищет существующий `[UIRoot]` перед созданием
- Помечается `DontDestroyOnLoad` только в runtime
- Используется как родитель по умолчанию для всех View

---

## Поток данных (Data Flow)

### Показ View

```
1. ViewService.ShowAsync<MyView, MyViewModel>()
   ↓
2. ViewService проверяет кэш
   - Если есть → использует кэшированный View
   - Если нет → создаёт новый через ViewFactory
   ↓
3. ViewFactory.CreateAsync<MyView, MyViewModel>()
   ↓
4. ViewConfig.GetViewAddress<MyViewModel>()
   → Возвращает Addressable адрес префаба
   ↓
5. AssetProvider.InstantiateAsync<MyView>(address, parent)
   → Загружает и инстанцирует префаб
   ↓
6. Container.Resolve<MyViewModel>()
   → Разрешает ViewModel из DI контейнера
   ↓
7. View.BindTo(viewModel)
   ↓
8. View.OnBind(ref disposables)
   → Настройка подписок на ViewModel
   ↓
9. ViewService.AssignSortingOrder(view)
   → Создаёт Canvas + GraphicRaycaster (если нет)
   → Устанавливает sortingOrder для правильного наложения
   ↓
10. ViewService.ShowViewInternalWithParams(view, showParams)
    ↓
11. ViewAnimator.AnimateShowAsync(view, transitionStyle, duration)
    → Проигрывает анимацию показа
    ↓
12. View.ShowAsync()
    → Активирует GameObject, вызывает OnBeforeShow/OnAfterShow
    ↓
13. ViewService добавляет View в ActiveViews
    ↓
14. ViewService.ViewShown?.Invoke(view)
    → Вызывает событие
    ↓
15. showParams.OnShown?.Invoke()
    → Вызывает callback (если указан)
```

### Скрытие View

```
1. ViewService.HideAsync<MyView>()
   ↓
2. ViewService находит View в ActiveViews
   ↓
3. ViewService получает ViewShowParams для View
   ↓
4. showParams.OnHidden?.Invoke()
   → Вызывает callback (если указан) — ПЕРЕД анимацией!
   ↓
5. ViewAnimator.AnimateHideAsync(view, transitionStyle, duration)
   → Проигрывает анимацию скрытия
   ↓
6. View.HideAsync()
   → Деактивирует GameObject, вызывает OnBeforeHide/OnAfterHide
   ↓
7. ViewService удаляет View из ActiveViews
   ↓
8. ViewService.ReleaseSortingOrder(view)
   → Удаляет sorting order из словаря
   → Пересчитывает максимальный sorting order
   ↓
9. ViewService.ViewHidden?.Invoke(view)
   → Вызывает событие
   ↓
10. [ТОЛЬКО ДЛЯ NON-PERSISTENT VIEW]
    ↓
11. ViewService удаляет View из кэша
    → _viewCache.TryRemove(view.GetType())
    ↓
12. ViewFactory.Destroy(view)
    → Вызывает Unbind() (уничтожает ViewModel!)
    → Освобождает Addressable инстанс
```

### Реактивный биндинг (View ↔ ViewModel)

```
ViewModel изменяет свойство:
  ViewModel.Name.Value = "New Name"
  ↓
ReactiveProperty уведомляет подписчиков
  ↓
View подписан через:
  ViewModel.Name.Subscribe(UpdateUI).AddTo(ref disposables)
  ↓
View обновляет UI:
  _nameText.text = name;
```

---

## Как создавать новые View/ViewModel

### Пошаговая инструкция

Допустим, нужно создать экран настроек (Settings). Этот пример демонстрирует создание простого View с базовой функциональностью.

#### Шаг 1: Создать Model

```csharp
// Core/MVVM/Models/Settings/SettingsModel.cs

using System;
using Core.Abstractions.MVVM.Models.Settings;

namespace Core.Models.Settings
{
    public class SettingsModel : ISettingsModel
    {
        private bool _isVisible;

        public event Action<bool> IsVisibleChanged;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                IsVisibleChanged?.Invoke(_isVisible);
            }
        }

        public bool IsValid => true;

        public void Reset()
        {
            IsVisible = false;
        }

        public void Dispose()
        {
            IsVisibleChanged = null;
        }
    }
}
```

**Примечание:** Интерфейс `ISettingsModel` должен быть определён в `Core.Abstractions.MVVM.Models.Settings`:
```csharp
public interface ISettingsModel : IModel
{
    bool IsVisible { get; set; }
    event Action<bool> IsVisibleChanged;
}
```

#### Шаг 2: Создать ViewModel

```csharp
// Core/MVVM/ViewModels/Settings/SettingsViewModel.cs

using Core.Abstractions.MVVM.Models.Settings;
using Core.Abstractions.MVVM.ViewModels;
using R3;

public class SettingsViewModel : DisposableObject
{
    public ReactiveProperty<bool> IsVisible { get; }

    public ReactiveCommand<Unit> ToggleCommand { get; }
    public ReactiveCommand<Unit> CloseCommand { get; }

    private readonly ISettingsModel _model;

    public SettingsViewModel(ISettingsModel model)
    {
        _model = model;

        IsVisible = new ReactiveProperty<bool>(_model.IsVisible);
        IsVisible.AddTo(ref Disposables);

        _model.IsVisibleChanged += OnModelIsVisibleChanged;

        IsVisible.Subscribe(this, static (value, self) => self._model.IsVisible = value)
            .AddTo(ref Disposables);

        ToggleCommand = new ReactiveCommand<Unit>();
        ToggleCommand.Subscribe(this, static (_, self) => self.Toggle())
            .AddTo(ref Disposables);

        CloseCommand = new ReactiveCommand<Unit>();
        CloseCommand.Subscribe(this, static (_, self) => self.Close())
            .AddTo(ref Disposables);
    }

    private void OnModelIsVisibleChanged(bool value) => IsVisible.Value = value;

    private void Toggle() => IsVisible.Value = !IsVisible.Value;

    private void Close() => IsVisible.Value = false;

    public override void Dispose()
    {
        _model.IsVisibleChanged -= OnModelIsVisibleChanged;
        base.Dispose();
        _model.Dispose();
    }
}
```

#### Шаг 3: Создать View Prefab в Unity

1. Создать GameObject в сцене
2. Добавить компонент `SettingsView` (скрипт из следующего шага)
3. Настроить UI элементы (Button, Canvas, Panel и т.д.)
4. Привязать ссылки в Inspector
5. Сохранить как Prefab
6. Добавить в Addressables с адресом (например: `"SettingsView"`)

#### Шаг 4: Создать View Script

```csharp
// Core/MVVM/Views/Settings/SettingsView.cs

using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : BindableView<SettingsViewModel>
{
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _settingsPanel;

    protected override void OnBind(ref DisposableBag disposables)
    {
        if (_canvas != null)
        {
            _canvas.sortingOrder = 1000;
        }

        if (_settingsButton != null)
        {
            _settingsButton.OnClickAsObservable()
                .Subscribe(ViewModel.ToggleCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                .AddTo(ref disposables);
        }

        if (_closeButton != null)
        {
            _closeButton.OnClickAsObservable()
                .Subscribe(ViewModel.CloseCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                .AddTo(ref disposables);
        }

        ViewModel.IsVisible
            .Subscribe(this, static (isVisible, self) =>
            {
                if (self._settingsPanel != null)
                {
                    self._settingsPanel.SetActive(isVisible);
                }
                else if (self._canvas != null)
                {
                    self._canvas.enabled = isVisible;
                }
            })
            .AddTo(ref disposables);
    }

    public override async UniTask ShowAsync()
    {
        gameObject.SetActive(true);
        IsVisible = true;
        if (_settingsButton != null)
        {
            _settingsButton.gameObject.SetActive(true);
        }
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(ViewModel.IsVisible.Value);
        }
        await OnBeforeShow();
        await PlayShowAnimation();
        await OnAfterShow();
    }

    public override async UniTask HideAsync()
    {
        await OnBeforeHide();
        await PlayHideAnimation();
        await OnAfterHide();
        IsVisible = false;
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(false);
        }
        else if (_canvas != null)
        {
            _canvas.enabled = false;
        }
    }
}
```

#### Шаг 5: Создать ViewMapping

**Вариант 1: Через Editor Window (рекомендуется)**

1. Открыть **Tools → MVVM → View Mapping Generator**
2. Использовать поиск для фильтрации списков ViewModels и Views
3. Выбрать `SettingsViewModel` в списке ViewModels (или `SettingsView` в списке Views)
4. Нажать **Generate**

Editor Window автоматически:
- Создаёт класс `SettingsViewMapping : ViewMappingBase<SettingsView, SettingsViewModel>` в `Assets/Source/CodeGen/Views/Settings/`
- После компиляции автоматически создаёт ScriptableObject asset в `Assets/Source/00. Configs/ViewsMapping/SettingsViewMapping.asset`
- Автоматически пытается найти и назначить Addressable префаб по имени
- Автоматически добавляет маппинг в `ViewConfig._mappings`

**Примечание:** Если префаб не был найден автоматически, его нужно назначить вручную в Inspector созданного asset.

**Вариант 2: Вручную**

```csharp
// 1. Создать класс маппинга:
public class SettingsViewMapping : ViewMappingBase<SettingsView, SettingsViewModel>
{
}

// 2. Создать ScriptableObject asset:
// Create → MVVM → View Mapping (если настроено)
// Или создать через Inspector: Create → ScriptableObject → SettingsViewMapping

// 3. В Inspector установить:
// - ViewPrefab: ссылка на Addressable префаб SettingsView

// 4. Добавить маппинг в ViewConfig:
// Открыть ViewConfig → добавить SettingsViewMapping в список _mappings
```

#### Шаг 6: Зарегистрировать в DI контейнере

```csharp
// Core/DependencyInjection/Bindings/InfrastructureBindings.cs

// В методе AddInfrastructure или отдельном методе:

builder.AddSingleton<ISettingsModel>(_ => new SettingsModel());
builder.AddSingleton<SettingsViewModel>(container => new SettingsViewModel(
    container.Resolve<ISettingsModel>()
));
```

**Примечание:** 
- `ViewConfig`, `ViewFactory`, `ViewService`, `ViewAnimator` и `CanvasProvider` уже зарегистрированы в `InfrastructureBindings.cs`
- `ViewConfig` загружается автоматически через `ConfigProvider.GetConfig<ViewConfig>()` (Addressables)
- Если ViewConfig не найден, будет выброшено исключение при старте приложения
- `ViewFactory` использует `Container.ProjectContainer` для разрешения ViewModel (глобальный контейнер проекта)
- `IAssetProvider` также должен быть зарегистрирован в DI контейнере (уже зарегистрирован в `InfrastructureBindings.cs`)

#### Шаг 7: Использовать

```csharp
public class GameManager
{
    private readonly IViewService _viewService;
    
    public GameManager(IViewService viewService)
    {
        _viewService = viewService;
    }
    
    public async UniTask ShowSettings()
    {
        await _viewService.ShowAsync<SettingsView, SettingsViewModel>();
    }
    
    public async UniTask ShowSettingsWithAnimation()
    {
        var showParams = ViewShowParams.Default()
            .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp)
            .SetAnimationDuration(0.3f)
            .SetDisplayMode(ViewDisplayMode.Single);
        
        await _viewService.ShowAsync<SettingsView, SettingsViewModel>(showParams);
    }
}
```

---

## Сложные View с несколькими компонентами

Некоторые View требуют более сложной структуры с несколькими компонентами. Примером такого View является `NetworkSandboxView`, который включает в себя:

- Основной View компонент (`NetworkSandboxView`)
- Графический рендерер (`WorkflowGraphRenderer`)
- Компоненты узлов (`WorkflowNodeView`)
- Компоненты рёбер (`WorkflowGraphEdgeView`)
- Вспомогательные компоненты (`UILineRenderer`)

### Структура сложного View

```
NetworkSandboxView (BindableView<NetworkSandboxViewModel>)
├── WorkflowGraphRenderer (MonoBehaviour)
│   ├── WorkflowNodeView (MonoBehaviour) × N
│   └── WorkflowGraphEdgeView (MonoBehaviour) × M
└── UILineRenderer (MonoBehaviour)
```

### Особенности реализации

#### 1. Dependency Injection для дочерних компонентов

Сложные View могут использовать Dependency Injection для дочерних компонентов:

```csharp
public class NetworkSandboxView : BindableView<NetworkSandboxViewModel>
{
    [SerializeField] private WorkflowGraphRenderer _graphRenderer;
    
    private IWorkflowNodeDefinitionService _definitionService;
    private IWorkflowNodeMapper _nodeMapper;
    
    [Inject]
    public void Construct(
        IWorkflowNodeDefinitionService definitionService,
        IWorkflowNodeMapper nodeMapper)
    {
        _definitionService = definitionService;
        _nodeMapper = nodeMapper;
        
        if (_graphRenderer != null)
        {
            _graphRenderer.Initialize();
        }
    }
    
    protected override void OnBind(ref DisposableBag disposables)
    {
        ViewModel.WorkflowGraph
            .Subscribe(this, static (graph, self) =>
            {
                if (self._graphRenderer != null && graph != null)
                {
                    self._graphRenderer.RenderGraph(graph);
                }
            })
            .AddTo(ref disposables);
    }
}
```

#### 2. Инициализация дочерних компонентов

Дочерние компоненты могут требовать инициализации при создании View:

```csharp
public class WorkflowGraphRenderer : MonoBehaviour
{
    private IWorkflowNodeDefinitionService _definitionService;
    private IWorkflowNodeMapper _nodeMapper;
    
    public void Initialize()
    {
        _definitionService = Container.ProjectContainer.Resolve<IWorkflowNodeDefinitionService>();
        _nodeMapper = Container.ProjectContainer.Resolve<IWorkflowNodeMapper>();
    }
    
    public void RenderGraph(WorkflowGraphModel graph)
    {
        ClearGraph();
        
        foreach (var node in graph.Nodes)
        {
            CreateNodeView(node);
        }
        
        foreach (var edge in graph.Edges)
        {
            CreateEdgeView(edge);
        }
    }
    
    private void CreateNodeView(RuntimeWorkflowNode node)
    {
        var nodeView = Instantiate(_nodePrefab, transform);
        var component = nodeView.GetComponent<WorkflowNodeView>();
        component.Setup(node, _nodeMapper);
    }
    
    private void CreateEdgeView(WorkflowGraphEdge edge)
    {
        var edgeView = Instantiate(_edgePrefab, transform);
        var component = edgeView.GetComponent<WorkflowGraphEdgeView>();
        component.Setup(edge);
    }
}
```

#### 3. Управление жизненным циклом дочерних компонентов

Дочерние компоненты должны правильно управлять своим жизненным циклом:

```csharp
public class WorkflowNodeView : MonoBehaviour
{
    private RuntimeWorkflowNode _node;
    private DisposableBag _disposables = new();
    
    public void Setup(RuntimeWorkflowNode node, IWorkflowNodeMapper mapper)
    {
        _disposables.Dispose();
        _disposables = new DisposableBag();
        
        _node = node;
        
        UpdateVisuals();
        
        _node.PropertyChanged
            .Subscribe(this, static (_, self) => self.UpdateVisuals())
            .AddTo(ref _disposables);
    }
    
    private void UpdateVisuals()
    {
        if (_node == null) return;
        
        _titleText.text = _node.Name;
        _typeText.text = _node.Type;
    }
    
    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
```

#### 4. Реактивная синхронизация данных

Сложные View могут синхронизировать данные между основным View и дочерними компонентами:

```csharp
protected override void OnBind(ref DisposableBag disposables)
{
    ViewModel.WorkflowGraph
        .Subscribe(this, static (graph, self) =>
        {
            if (self._graphRenderer != null)
            {
                self._graphRenderer.RenderGraph(graph);
            }
        })
        .AddTo(ref disposables);
    
    ViewModel.SelectedNode
        .Subscribe(this, static (node, self) =>
        {
            if (self._graphRenderer != null)
            {
                self._graphRenderer.HighlightNode(node);
            }
        })
        .AddTo(ref disposables);
}
```

### Best Practices для сложных View

1. **Разделение ответственности**: Каждый компонент должен иметь чётко определённую роль
2. **Инициализация**: Используйте `[Inject]` методы для DI зависимостей
3. **Управление ресурсами**: Все подписки должны быть добавлены в `DisposableBag`
4. **Очистка**: Дочерние компоненты должны правильно очищать ресурсы в `OnDestroy`
5. **Реактивность**: Используйте ReactiveProperty для синхронизации данных между компонентами

### Пример: NetworkSandboxView

`NetworkSandboxView` демонстрирует все эти принципы:

- Основной View управляет общим состоянием и навигацией
- `WorkflowGraphRenderer` отвечает за визуализацию графа
- `WorkflowNodeView` и `WorkflowGraphEdgeView` управляют отдельными элементами
- Все компоненты синхронизируются через ReactiveProperty из ViewModel

Подробнее о реализации `NetworkSandboxView` см. в `Assets/Source/Runtime/Core/MVVM/Views/NetworkSandbox/NetworkSandbox.README.md`.

---

## Примеры использования

### Простой показ View

```csharp
public class MainMenuController
{
    private readonly IViewService _viewService;
    
    public MainMenuController(IViewService viewService)
    {
        _viewService = viewService;
    }
    
    public async UniTask OpenSettings()
    {
        await _viewService.ShowAsync<SettingsView, SettingsViewModel>();
    }
}
```

### Показ View с анимацией

```csharp
public async UniTask ShowPopup()
{
    var showParams = ViewShowParams.Default()
        .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.Scale)
        .SetAnimationDuration(0.25f)
        .SetDisplayMode(ViewDisplayMode.Additive)
        .SetCanvas(CanvasLayer.Overlay)
        .SetOnShown(async () =>
        {
            Debug.Log("Popup shown!");
            await UniTask.Delay(1000);
        });
    
    await _viewService.ShowAsync<PopupView, PopupViewModel>(showParams);
}
```

### Показ View с callback'ами

```csharp
public async UniTask ShowConfirmationDialog(string message)
{
    bool confirmed = false;
    
    var showParams = ViewShowParams.Default()
        .SetOnShown(async () =>
        {
            Debug.Log("Dialog appeared");
            await UniTask.Yield();
        })
        .SetOnHidden(async () =>
        {
            Debug.Log("Dialog hidden");
            await UniTask.Yield();
        });
    
    var dialog = await _viewService.ShowAsync<ConfirmationDialogView, ConfirmationDialogViewModel>(showParams);
    
    // Ждём результата от ViewModel
    confirmed = await dialog.ViewModel.Result;
    
    if (confirmed)
    {
        Debug.Log("User confirmed");
    }
}
```

### Навигация назад

```csharp
public class NavigationController
{
    private readonly IViewService _viewService;
    
    public async UniTask GoToNextScreen()
    {
        await _viewService.ShowAsync<NextScreenView, NextScreenViewModel>();
    }
    
    public async UniTask GoBack()
    {
        await _viewService.GoBackAsync();
    }
    
    public async UniTask GoToRoot()
    {
        await _viewService.GoToRootAsync();
    }
}
```

### Закрытие View

```csharp
public async UniTask CloseCurrentView()
{
    await _viewService.HideCurrentAsync();
}

public async UniTask CloseSpecificView()
{
    await _viewService.HideAsync<SettingsView>();
}

public async UniTask CloseAllPopups()
{
    await _viewService.CloseAllNonPersistentAsync();
}
```

### Проверка активности View

```csharp
public bool IsSettingsOpen()
{
    return _viewService.IsViewActive<SettingsView>();
}

public SettingsView GetSettingsView()
{
    return _viewService.GetActiveView<SettingsView>();
}
```

### Использование сложного View (NetworkSandbox)

```csharp
public class NetworkSandboxController
{
    private readonly IViewService _viewService;
    
    public NetworkSandboxController(IViewService viewService)
    {
        _viewService = viewService;
    }
    
    public async UniTask OpenNetworkSandbox()
    {
        var showParams = ViewShowParams.Default()
            .SetDisplayMode(ViewDisplayMode.Single)
            .SetTransition(ViewTransitionStyle.Fade)
            .SetAnimationDuration(0.3f);
        
        await _viewService.ShowAsync<NetworkSandboxView, NetworkSandboxViewModel>(showParams);
    }
    
    public bool IsNetworkSandboxOpen()
    {
        return _viewService.IsViewActive<NetworkSandboxView>();
    }
    
    public NetworkSandboxView GetNetworkSandboxView()
    {
        return _viewService.GetActiveView<NetworkSandboxView>();
    }
}
```

### Подписка на события ViewService

```csharp
public class ViewLifecycleTracker
{
    private readonly IViewService _viewService;
    
    public ViewLifecycleTracker(IViewService viewService)
    {
        _viewService = viewService;
        
        _viewService.ViewShown += OnViewShown;
        _viewService.ViewHidden += OnViewHidden;
    }
    
    private void OnViewShown(IView view)
    {
        Debug.Log($"View shown: {view.GetType().Name}");
    }
    
    private void OnViewHidden(IView view)
    {
        Debug.Log($"View hidden: {view.GetType().Name}");
    }
}
```

### Полный пример: Inventory Screen

```csharp
// Model
public interface IInventoryModel : IModel
{
    IReadOnlyList<ItemModel> Items { get; }
    ItemModel SelectedItem { get; set; }
    
    event Action<ItemModel> ItemSelected;
    void UseItem(ItemModel item);  // Метод вместо event для вызова
}

// ViewModel
public class InventoryViewModel : DisposableObject
{
    private readonly IInventoryModel _model;
    
    public ReactiveProperty<IReadOnlyList<ItemModel>> Items { get; }
    public ReactiveProperty<ItemModel> SelectedItem { get; }
    public ReactiveCommand<ItemModel> UseItemCommand { get; }
    public ReactiveCommand<Unit> CloseCommand { get; }
    
    public InventoryViewModel(IInventoryModel model)
    {
        _model = model;
        
        Items = new ReactiveProperty<IReadOnlyList<ItemModel>>(_model.Items);
        Items.AddTo(ref Disposables);
        
        SelectedItem = new ReactiveProperty<ItemModel>(_model.SelectedItem);
        SelectedItem.AddTo(ref Disposables);
        
        UseItemCommand = new ReactiveCommand<ItemModel>(item => item != null);
        UseItemCommand.Subscribe(this, static (item, self) => self._model.UseItem(item))
            .AddTo(ref Disposables);
        
        CloseCommand = new ReactiveCommand<Unit>();
        CloseCommand.AddTo(ref Disposables);
        
        _model.ItemSelected += OnItemSelected;
    }
    
    private void OnItemSelected(ItemModel item) => SelectedItem.Value = item;
    
    public override void Dispose()
    {
        _model.ItemSelected -= OnItemSelected;
        base.Dispose();
    }
}

// View
public class InventoryView : BindableView<InventoryViewModel>
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Transform _itemsContainer;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private ItemView _selectedItemView;
    
    protected override void OnBind(ref DisposableBag disposables)
    {
        _closeButton.OnClickAsObservable()
            .Subscribe(ViewModel.CloseCommand, static (_, cmd) => cmd.Execute(Unit.Default))
            .AddTo(ref disposables);
        
        ViewModel.Items
            .Subscribe(this, static (items, self) => self.UpdateItemsList(items))
            .AddTo(ref disposables);
        
        ViewModel.SelectedItem
            .Subscribe(this, static (item, self) => self.ShowSelectedItem(item))
            .AddTo(ref disposables);
    }
    
    private void UpdateItemsList(IReadOnlyList<ItemModel> items)
    {
        foreach (Transform child in _itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var item in items)
        {
            var itemView = Instantiate(_itemPrefab, _itemsContainer);
            var itemViewComponent = itemView.GetComponent<ItemView>();
            itemViewComponent.Setup(item, ViewModel.UseItemCommand);
        }
    }
    
    private void ShowSelectedItem(ItemModel item)
    {
        if (_selectedItemView != null)
        {
            _selectedItemView.Setup(item);
        }
    }
}
```

---

## Анимации и переходы

### Базовые анимации

```csharp
// Fade (плавное появление)
var fadeParams = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.Fade)
    .SetAnimationDuration(0.3f);

// Slide (скольжение)
var slideParams = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.SlideUp)
    .SetAnimationDuration(0.25f);

// Scale (масштабирование)
var scaleParams = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.Scale)
    .SetAnimationDuration(0.2f);
```

### Комбинированные анимации

```csharp
// Fade + Slide
var combinedParams = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp)
    .SetAnimationDuration(0.3f);

// Fade + Slide + Scale
var allParams = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp | ViewTransitionStyle.Scale)
    .SetAnimationDuration(0.35f);
```

### Мгновенный показ (без анимации)

```csharp
var instantParams = ViewShowParams.Instant();
await _viewService.ShowAsync<MyView, MyViewModel>(instantParams);
```

### Кастомная длительность

```csharp
var slowParams = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.Fade)
    .SetAnimationDuration(1.0f);  // Медленная анимация
```

### Разные анимации для показа и скрытия

Анимация скрытия берётся из `ViewShowParams`, которые были использованы при показе View.

```csharp
// При показе
var showParams = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp);
await _viewService.ShowAsync<MyView, MyViewModel>(showParams);

// При скрытии используется та же анимация
await _viewService.HideAsync<MyView>();
```

---

## Canvas Layers

### Регистрация Canvas

```csharp
public class CanvasSetup : MonoBehaviour
{
    [SerializeField] private Canvas _screenCanvas;
    [SerializeField] private Canvas _overlayCanvas;
    [SerializeField] private Canvas _systemCanvas;
    
    private ICanvasProvider _canvasProvider;
    
    public void Initialize(ICanvasProvider canvasProvider)
    {
        _canvasProvider = canvasProvider;
        
        _canvasProvider.RegisterCanvas(CanvasLayer.Screen, _screenCanvas);
        _canvasProvider.RegisterCanvas(CanvasLayer.Overlay, _overlayCanvas);
        _canvasProvider.RegisterCanvas(CanvasLayer.System, _systemCanvas);
    }
}
```

### Показ View на определённом Canvas слое

```csharp
var overlayParams = ViewShowParams.Default()
    .SetCanvas(CanvasLayer.Overlay)
    .SetDisplayMode(ViewDisplayMode.Additive);

await _viewService.ShowAsync<PopupView, PopupViewModel>(overlayParams);
```

### Использование Custom Parent

```csharp
var customParams = ViewShowParams.Default()
    .SetCustomParent(myCustomTransform)
    .SetDisplayMode(ViewDisplayMode.Additive);

await _viewService.ShowAsync<MyView, MyViewModel>(customParams);
```

**Приоритет:**
1. `CustomParent` (если установлен)
2. `TargetCanvas` (если установлен)
3. `[UIRoot]` (по умолчанию)

---

## Best Practices

### ✅ Рекомендации

#### 1. Всегда используйте ReactiveProperty для данных в ViewModel

```csharp
// ❌ Плохо - нет реактивности
public class BadViewModel : DisposableObject
{
    public string Name { get; set; }  // View не узнает об изменении
}

// ✅ Хорошо - реактивное свойство
public class GoodViewModel : DisposableObject
{
    public ReactiveProperty<string> Name { get; }
    
    public GoodViewModel()
    {
        Name = new ReactiveProperty<string>("");
        Name.AddTo(ref Disposables);
    }
}
```

#### 2. Всегда добавляйте подписки в DisposableBag

```csharp
// ❌ Плохо - утечка памяти
protected override void OnBind(ref DisposableBag disposables)
{
    ViewModel.Name.Subscribe(name => _text.text = name);  // Подписка не очистится, closure!
}

// ✅ Хорошо - автоматическая очистка со static lambda
protected override void OnBind(ref DisposableBag disposables)
{
    ViewModel.Name
        .Subscribe(this, static (name, self) => self._text.text = name)
        .AddTo(ref disposables);  // Очистится при Unbind
}
```

#### 2.1. 🚨 ВСЕГДА используйте static lambdas (NO CLOSURES!)

**Критически важно для R3:** Всегда используйте перегрузку `Subscribe` со state и static lambda чтобы избежать closure allocations и утечек памяти.

```csharp
// ❌ Плохо - closure захватывает `this` и все поля
ViewModel.Items.Subscribe(items => UpdateList(items));

// ❌ Плохо - closure захватывает локальную переменную
var service = _myService;
ViewModel.Data.Subscribe(data => service.Process(data));

// ✅ Хорошо - static lambda со state (NO CLOSURES!)
ViewModel.Items.Subscribe(this, static (items, self) => self.UpdateList(items));

// ✅ Хорошо - tuple state для нескольких зависимостей
ViewModel.Data.Subscribe(
    (Self: this, Service: _myService), 
    static (data, state) => state.Service.Process(data)
);
```

**Паттерн для async операций:**
```csharp
// ❌ Плохо - async lambda это closure
ViewModel.LoadCommand.Subscribe(async _ => await LoadDataAsync());

// ✅ Хорошо - вызов через static lambda + Forget()
ViewModel.LoadCommand.Subscribe(this, static (_, self) => self.LoadDataAsync().Forget());

// Метод возвращает UniTaskVoid
private async UniTaskVoid LoadDataAsync() { /* ... */ }
```

#### 3. Используйте ReactiveCommand для действий

```csharp
// ❌ Плохо - прямое выполнение, closure, нет отписки
protected override void OnBind(ref DisposableBag disposables)
{
    _button.onClick.AddListener(() => DoSomething());  // Сложно тестировать, утечка памяти
}

// ✅ Хорошо - через команду с static lambda
public class MyViewModel : DisposableObject
{
    public ReactiveCommand<Unit> DoSomethingCommand { get; }
    
    public MyViewModel()
    {
        DoSomethingCommand = new ReactiveCommand<Unit>();
        DoSomethingCommand.Subscribe(this, static (_, self) => self.DoSomething())
            .AddTo(ref Disposables);
    }
    
    private void DoSomething() { /* ... */ }
}

// В View:
_button.OnClickAsObservable()
    .Subscribe(ViewModel.DoSomethingCommand, static (_, cmd) => cmd.Execute(Unit.Default))
    .AddTo(ref disposables);
```

#### 4. Не делайте прямые ссылки на View из ViewModel

```csharp
// ❌ Плохо - ViewModel знает о View
public class BadViewModel
{
    public void ShowMessage(MyView view)  // Зависимость от View
    {
        view.ShowMessage("Hello");
    }
}

// ✅ Хорошо - ViewModel только данные и команды
public class GoodViewModel : DisposableObject
{
    public ReactiveProperty<string> Message { get; }
    
    public void SetMessage(string message)
    {
        Message.Value = message;  // View подпишется и обновит UI
    }
}
```

#### 5. Используйте ViewShowParams для сложных сценариев

```csharp
// ❌ Плохо - много параметров в методе
await _viewService.ShowAsync<MyView, MyViewModel>(
    hideCurrent: true,
    animation: ViewTransitionStyle.Fade,
    duration: 0.3f,
    canvas: CanvasLayer.Overlay
);

// ✅ Хорошо - через ViewShowParams
var showParams = ViewShowParams.Default()
    .SetDisplayMode(ViewDisplayMode.Single)
    .SetTransition(ViewTransitionStyle.Fade)
    .SetAnimationDuration(0.3f)
    .SetCanvas(CanvasLayer.Overlay);

await _viewService.ShowAsync<MyView, MyViewModel>(showParams);
```

#### 6. Обрабатывайте null в подписках

```csharp
// ❌ Плохо - может упасть если ViewModel == null, и closure!
ViewModel.Name.Subscribe(name => _text.text = name);

// ✅ Хорошо - проверка на null и static lambda
ViewModel?.Name
    .Where(static _ => true)  // Или другое условие
    .Subscribe(this, static (name, self) => self._text.text = name)
    .AddTo(ref disposables);
```

#### 6.1. Используйте методы жизненного цикла для инициализации

```csharp
public class MyView : BindableView<MyViewModel>
{
    protected override async UniTask OnBeforeShow()
    {
        await LoadDataAsync();
    }
    
    protected override async UniTask OnAfterShow()
    {
        StartAnimations();
    }
    
    protected override async UniTask OnBeforeHide()
    {
        SaveData();
    }
}
```

#### 7. Используйте персистентные View для важных экранов

**🚨 ВАЖНО:** Non-persistent View **уничтожаются** после `HideAsync()`! Это означает:
- View удаляется из кэша
- ViewModel уничтожается (вызывается `Dispose()`)
- При следующем `ShowAsync()` создаётся новый экземпляр View и ViewModel

Для переиспользования View установите `IsPersistent = true`:

**Вариант 1: Через Inspector (рекомендуется)**
```csharp
public class MainMenuView : BindableView<MainMenuViewModel>
{
    // В Inspector установить _isPersistent = true
    // Или переопределить свойство:
    public override bool IsPersistent => true;  // Не закроется автоматически, остаётся в кэше
}
```

**Вариант 2: Через сериализуемое поле**
В Inspector установить флаг `_isPersistent = true` на компоненте View.

**Когда использовать Persistent View:**
- Главное меню, HUD, инвентарь — экраны, которые часто открываются
- View с тяжёлой инициализацией
- View, которые должны сохранять состояние между показами

#### 8. Очищайте кэш при необходимости

```csharp
public class SceneTransitionManager
{
    private readonly IViewService _viewService;
    
    public async UniTask TransitionToNewScene()
    {
        _viewService.ClearCache();  // Очищаем кэш перед переходом
        _viewService.ClearHistory();
        
        await LoadNewScene();
    }
}
```

#### 9. Используйте правильные DisplayMode

```csharp
// Single - для полноэкранных экранов
var fullscreenParams = ViewShowParams.Default()
    .SetDisplayMode(ViewDisplayMode.Single);

// Additive - для попапов и диалогов
var popupParams = ViewShowParams.Default()
    .SetDisplayMode(ViewDisplayMode.Additive)
    .SetCanvas(CanvasLayer.Overlay);

// Replace - для быстрой замены без анимации скрытия
var replaceParams = ViewShowParams.Default()
    .SetDisplayMode(ViewDisplayMode.Replace);
```

#### 10. Тестируйте ViewModel отдельно от View

```csharp
[Test]
public void ViewModel_ShouldUpdateName()
{
    var viewModel = new MyViewModel();
    viewModel.Name.Value = "Test";
    
    Assert.AreEqual("Test", viewModel.Name.Value);
}
```

---

## Troubleshooting

### 🐛 Распространённые проблемы

#### Проблема: View не показывается

**Причина 1:** ViewConfig не найден или не содержит маппинг

```
KeyNotFoundException: View mapping not found for ViewModel: MyViewModel
```

**Решение:**
1. Проверьте, что `ViewConfig` находится в папке `Assets/Source/00. Configs/` с именем `ViewConfig.asset` и добавлен в Addressables
2. Убедитесь, что создан `ViewMapping` для вашего ViewModel
3. Проверьте, что `ViewMapping` добавлен в список `_mappings` в `ViewConfig`
4. Проверьте, что Addressable адрес установлен в маппинге (`ViewPrefab` не null)
5. Убедитесь, что `ViewConfig` загружается через `ConfigProvider.GetConfig<ViewConfig>()` в DI контейнере

**Причина 2:** Префаб не добавлен в Addressables

**Решение:**
1. Откройте Addressables Groups
2. Убедитесь, что префаб View добавлен
3. Проверьте, что адрес совпадает с адресом в ViewMapping
4. Проверьте, что `ViewPrefab.RuntimeKey` не пустой в ViewMapping

---

#### Проблема: ViewModel не разрешается из DI

**Ошибка:**
```
InvalidOperationException: No registration found for MyViewModel
```

**Решение:**
Зарегистрируйте ViewModel в DI контейнере:

```csharp
// В InfrastructureBindings.cs
builder.AddSingleton<MyViewModel>(container => new MyViewModel(
    container.Resolve<IMyModel>()
));
```

---

#### Проблема: Подписки не очищаются (утечка памяти)

**Причина:** Не добавлены в DisposableBag

```csharp
// ❌ Неправильно
ViewModel.Name.Subscribe(UpdateUI);

// ✅ Правильно
ViewModel.Name
    .Subscribe(UpdateUI)
    .AddTo(ref disposables);
```

---

#### Проблема: UI не обновляется при изменении ViewModel

**Причина 1:** Не подписались на ReactiveProperty

```csharp
// ❌ Неправильно - одноразовое чтение
_text.text = ViewModel.Name.Value;

// ✅ Правильно - реактивная подписка с static lambda
ViewModel.Name
    .Subscribe(this, static (name, self) => self._text.text = name)
    .AddTo(ref disposables);
```

**Причина 2:** Изменяете свойство напрямую вместо ReactiveProperty

```csharp
// ❌ Неправильно
ViewModel.Name = "New";  // Если Name - обычное свойство

// ✅ Правильно
ViewModel.Name.Value = "New";  // Если Name - ReactiveProperty
```

---

#### Проблема: Анимация не проигрывается

**Причина 1:** TransitionStyle = None

```csharp
// ❌ Неправильно
var params = ViewShowParams.Instant();  // Нет анимации

// ✅ Правильно
var params = ViewShowParams.Default()
    .SetTransition(ViewTransitionStyle.Fade);
```

**Причина 2:** View не имеет необходимых компонентов

- Для Fade нужен `CanvasGroup` (создаётся автоматически, но лучше добавить вручную)
- Для Slide нужен `RectTransform` (обязателен для UI)
- Для Scale нужен `Transform` (всегда присутствует)

**Решение:** 
- Добавьте `CanvasGroup` компонент на GameObject View для Fade анимаций
- Убедитесь, что View имеет `RectTransform` (автоматически для UI элементов)

**Причина 3:** Анимация была остановлена

```csharp
// Проверьте, не вызывался ли StopAnimation
_viewAnimator.StopAnimation(view);
```

---

#### Проблема: View показывается на неправильном Canvas

**Причина:** Не установлен TargetCanvas или CustomParent

**Решение:**
```csharp
var params = ViewShowParams.Default()
    .SetCanvas(CanvasLayer.Overlay);  // Указываем слой

// Или
var params = ViewShowParams.Default()
    .SetCustomParent(myCanvasTransform);  // Прямой родитель
```

---

#### Проблема: Persistent View закрывается при показе нового

**Причина:** View не помечен как Persistent

**Решение:**
```csharp
public class MyView : BindableView<MyViewModel>
{
    public override bool IsPersistent => true;  // Не закроется автоматически
}
```

---

#### Проблема: View создаётся несколько раз вместо кэширования

**Причина 1:** View не помечен как Persistent

**Решение:**
Non-persistent View уничтожаются после `HideAsync()`. Чтобы View переиспользовался:

```csharp
public class MyView : BindableView<MyViewModel>
{
    public override bool IsPersistent => true;  // Остаётся в кэше после скрытия
}
```

Или установите `_isPersistent = true` в Inspector.

**Причина 2:** View не кэшируется, если используется прямой адрес

**Решение:**
Используйте ViewModel тип вместо адреса:

```csharp
// ❌ Неправильно - не кэшируется
await _viewService.ShowAsync<MyView, MyViewModel>("MyViewAddress");

// ✅ Правильно - кэшируется
await _viewService.ShowAsync<MyView, MyViewModel>();
```

---

#### Проблема: ViewHistory не работает

**Причина:** View скрывается без добавления в историю

**Решение:**
Используйте `DisplayMode.Single` или скрывайте через `HideAsync` с сохранением истории:

```csharp
// View автоматически добавляется в историю при скрытии в режиме Single
var params = ViewShowParams.Default()
    .SetDisplayMode(ViewDisplayMode.Single);
await _viewService.ShowAsync<NextView, NextViewModel>(params);
```

---

## FAQ

### Q: Зачем нужны отдельные Model и ViewModel?

**A:** Разделение ответственности:
- **Model** - состояние данных, бизнес-логика, независим от UI
- **ViewModel** - логика представления, адаптирует Model для View, содержит ReactiveProperty и команды

Если изменится UI - меняется только ViewModel, Model остаётся неизменным.

---

### Q: Можно ли использовать обычные события вместо ReactiveProperty?

**A:** Технически да, но ReactiveProperty предоставляет:
- Автоматическую очистку подписок через DisposableBag
- Операторы LINQ (Where, Select, Skip и т.д.)
- Лучшую интеграцию с R3
- Более декларативный код

Рекомендуется использовать ReactiveProperty.

---

### Q: Как передать параметры в ViewModel при показе View?

**A:** Через DI контейнер. Создайте фабрику или используйте параметризованную регистрацию:

```csharp
// Вариант 1: Фабрика
builder.AddSingleton<MyViewModelFactory>(container => 
    new MyViewModelFactory(container.Resolve<IMyModel>())
);

// Вариант 2: Параметризованная регистрация
public class MyViewModelFactory
{
    private readonly IMyModel _model;
    
    public MyViewModel Create(string parameter)
    {
        return new MyViewModel(_model, parameter);
    }
}
```

---

### Q: Как показать View с параметрами из другого View?

**A:** Через ViewModel. Первый View устанавливает данные в Model, второй View читает:

```csharp
// View 1
public class FirstView : BindableView<FirstViewModel>
{
    protected override void OnBind(ref DisposableBag disposables)
    {
        _button.OnClickAsObservable()
            .Subscribe(ViewModel, static (_, vm) => vm.NavigateToSecond("parameter"))
            .AddTo(ref disposables);
    }
}

// ViewModel 1
public class FirstViewModel : DisposableObject
{
    private readonly IViewService _viewService;
    private readonly ISharedModel _sharedModel;
    
    public void NavigateToSecond(string parameter)
    {
        _sharedModel.SetParameter(parameter);
        _viewService.ShowAsync<SecondView, SecondViewModel>().Forget();
    }
}
```

---

### Q: Можно ли использовать несколько View для одного ViewModel?

**A:** Да, но нужно использовать разные типы ViewModel или разные маппинги. Один ViewModel тип → один маппинг в ViewConfig.

Для разных представлений одного ViewModel создайте разные ViewModel типы-обёртки.

---

### Q: Как обработать ошибки при создании View?

**A:** Оберните вызов в try-catch:

```csharp
try
{
    await _viewService.ShowAsync<MyView, MyViewModel>();
}
catch (KeyNotFoundException ex)
{
    Debug.LogError($"View mapping not found: {ex.Message}");
}
catch (Exception ex)
{
    Debug.LogError($"Failed to show view: {ex.Message}");
}
```

---

### Q: Как очистить все View при переходе между сценами?

**A:**
```csharp
_viewService.ClearCache();
_viewService.ClearHistory();
await _viewService.CloseAllNonPersistentAsync();
```

---

### Q: Можно ли использовать View без ViewModel?

**A:** Нет, архитектура требует ViewModel. Если логика простая, создайте пустой ViewModel:

```csharp
public class SimpleViewModel : DisposableObject
{
    // Минимальная логика
}
```

---

### Q: Как остановить анимацию во время проигрывания?

**A:**
```csharp
_viewAnimator.StopAnimation(view);
```

**Примечание:** Анимация проверяет `IsAnimationActive` на каждом кадре, поэтому остановка происходит безопасно.

---

### Q: Можно ли использовать кастомные анимации?

**A:** Да, есть несколько способов:

**Вариант 1: Переопределить методы в BindableView**
```csharp
public class MyView : BindableView<MyViewModel>
{
    protected override async UniTask PlayShowAnimation()
    {
        await DOTween.Sequence()
            .Append(transform.DOScale(1f, 0.3f))
            .Join(GetComponent<CanvasGroup>().DOFade(1f, 0.3f));
    }
    
    protected override async UniTask PlayHideAnimation()
    {
        await transform.DOScale(0f, 0.2f);
    }
}
```

**Вариант 2: Создать свой IViewAnimator реализацию**
```csharp
public class CustomViewAnimator : IViewAnimator
{
    public async UniTask AnimateShowAsync(IView view, ViewTransitionStyle style, float? duration)
    {
        // Ваша реализация
    }
    
    public async UniTask AnimateHideAsync(IView view, ViewTransitionStyle style, float? duration)
    {
        // Ваша реализация
    }
    
    public void StopAnimation(IView view) { }
}
```

Затем зарегистрируйте в DI контейнере вместо стандартного `ViewAnimator`.

---

### Q: Как проверить, что View активен перед действием?

**A:**
```csharp
if (_viewService.IsViewActive<MyView>())
{
    var view = _viewService.GetActiveView<MyView>();
    // Работаем с view
}
```

---

### Q: ViewConfig не обновляется после изменения маппингов

**A:** ViewConfig кэширует маппинги в `OnEnable()`. Перезапустите Unity или вызовите `BuildCache()` программно (если есть доступ через InternalsVisibleTo).

**Примечание:** `ViewConfigInternals` предоставляет доступ к внутренним методам для тестов через `InternalsVisibleTo` атрибуты.

---

### Q: Как показать View поверх всех остальных?

**A:**
```csharp
var params = ViewShowParams.Default()
    .SetDisplayMode(ViewDisplayMode.Additive)
    .SetCanvas(CanvasLayer.System)  // Самый верхний слой
    .SetTransition(ViewTransitionStyle.Fade);

await _viewService.ShowAsync<MyView, MyViewModel>(params);
```

---

### Q: Можно ли использовать View в разных сценах?

**A:** Да, View создаются через Addressables и могут использоваться в любой сцене. UIRoot создаётся автоматически и помечается `DontDestroyOnLoad`.

---

### Q: Как передать результат из View обратно в вызывающий код?

**A:** Через ViewModel:

```csharp
// ViewModel
public class DialogViewModel : DisposableObject
{
    public ReactiveProperty<bool?> Result { get; }
    
    public void Confirm()
    {
        Result.Value = true;
    }
    
    public void Cancel()
    {
        Result.Value = false;
    }
}

// Использование
var dialog = await _viewService.ShowAsync<DialogView, DialogViewModel>();
var result = await dialog.ViewModel.Result.WaitAsync();  // Ждём результата
```

---

## Чеклист при создании нового View/ViewModel

- [ ] Создан Model (если нужен), реализует `IModel` с `IsValid` и `Reset()`
- [ ] Создан ViewModel, наследуется от `DisposableObject`
- [ ] Все `ReactiveProperty` добавлены в `DisposableBag`
- [ ] Все команды добавлены в `DisposableBag`
- [ ] Проверка `IsDisposed` в публичных методах ViewModel (если нужно)
- [ ] Создан View, наследуется от `BindableView<TViewModel>`
- [ ] Все подписки в `OnBind` добавлены в `DisposableBag`
- [ ] Настроен `CanvasGroup` в Inspector (для Fade анимаций)
- [ ] **🚨 РЕШЕНО:** Установлен флаг `_isPersistent` в Inspector? (без него View уничтожается после HideAsync!)
- [ ] Переопределены методы жизненного цикла (если нужно): `OnBeforeShow`, `OnAfterShow`, `OnBeforeHide`, `OnAfterHide`
- [ ] Создан Prefab View в Unity
- [ ] Prefab добавлен в Addressables с корректным адресом
- [ ] Создан ViewMapping через Editor Window или вручную
- [ ] ViewMapping добавлен в ViewConfig (автоматически или вручную)
- [ ] ViewConfig находится в `Assets/Source/00. Configs/ViewConfig.asset` и добавлен в Addressables
- [ ] ViewModel зарегистрирован в DI контейнере
- [ ] Model зарегистрирован в DI контейнере (если нужен)
- [ ] Написаны тесты (опционально)

---

## Тестирование

Система MVVM имеет полный набор unit-тестов в папке `Assets/Source/Runtime/Tests/MVVM/`:

- **ViewServiceTests.cs** - тесты навигации, кэширования, истории
- **ViewFactoryTests.cs** - тесты создания View и разрешения ViewModel
- **ViewAnimatorTests.cs** - тесты анимаций показа/скрытия
- **ViewConfigTests.cs** - тесты конфигурации и маппингов
- **ViewShowParamsTests.cs** - тесты параметров показа
- **CanvasProviderTests.cs** - тесты управления Canvas слоями
- **ViewMappingBaseTests.cs** - тесты базового класса маппинга
- **DisposableObjectTests.cs** - тесты управления ресурсами

**Запуск тестов:**
- Edit Mode тесты: Unity Test Runner → EditMode
- Play Mode тесты: Unity Test Runner → PlayMode

**Особенности тестирования:**
- Используется `InternalsVisibleTo("Tests.MVVM")` для доступа к внутренним методам
- Тесты используют моки для `IAssetProvider`, `ICanvasProvider` и других зависимостей
- ViewConfig тесты создают временные ScriptableObject для изоляции

---

## Дополнительные детали реализации

### Container (DI контейнер)

`ViewFactory` использует `Container` (Reflex DI контейнер), который передаётся в конструктор при регистрации в DI. Это позволяет разрешать ViewModel и выполнять инъекцию зависимостей в View.

**Внутренняя реализация:**
- `ViewFactory` получает `Container` в конструкторе и сохраняет его в приватное поле `_container`
- При регистрации в DI (в `InfrastructureBindings.AddInfrastructure()`) в конструктор передаётся `container` из lambda-выражения:
  ```csharp
  builder.AddSingleton<IViewFactory>(container => new ViewFactory(
      container,  // DI контейнер
      container.Resolve<IAssetProvider>(),
      container.Resolve<IViewConfig>(),
      container.Resolve<ICanvasProvider>()
  ));
  ```
- Разрешение ViewModel происходит через `_container.Resolve<TViewModel>()`
- Инъекция зависимостей в View выполняется через `GameObjectInjector.InjectRecursive(view.gameObject, _container)`

**Преимущества:**
- ViewModel зарегистрированы в едином DI контейнере
- ViewFactory автоматически получает контейнер при создании
- Упрощает архитектуру - все зависимости управляются через DI
- Позволяет использовать ViewModel в разных сценах (если контейнер не пересоздаётся)
- Гарантирует единый источник истины для всех ViewModel в проекте

**Регистрация ViewModel:**
ViewModel должны быть зарегистрированы в DI контейнере до их использования. Регистрация происходит через `ContainerBuilder` в методе `AddViewModels()` в `InfrastructureBindings`:

```csharp
builder.AddSingleton<MyViewModel>(container => new MyViewModel(
    container.Resolve<IMyModel>()
));
```

После создания контейнера через `builder.Build()`, все зарегистрированные ViewModel становятся доступными для `ViewFactory` через переданный контейнер.

### Автоматическая очистка ресурсов

Все компоненты системы MVVM реализуют `IDisposable` для корректной очистки ресурсов:
- `BindableView` - очищает подписки при `Unbind()` или `Dispose()`
- `DisposableObject` - очищает все подписки при `Dispose()`
- `ViewService` - очищает кэш и историю при `ClearCache()` и `ClearHistory()`

### Потокобезопасность

`ViewService` и `CanvasProvider` используют `lock` для потокобезопасности:
- Все операции с коллекциями синхронизированы через объект `_sync` (в ViewService) и `_lock` (в CanvasProvider)
- `Interlocked.Exchange` используется для атомарного обновления `_currentView` в ViewService
- `ConcurrentDictionary` используется для кэша View (`_viewCache`) - потокобезопасная коллекция
- `Stack<IView>` для `_activeViews` и `_viewHistory` защищены через `lock(_sync)`
- `Dictionary<IView, ViewShowParams>` для `_viewParams` защищён через `lock(_sync)`
- `Dictionary<CanvasLayer, Canvas>` в CanvasProvider защищён через `lock(_lock)`

### Обработка ошибок

Система выбрасывает исключения в следующих случаях:
- `KeyNotFoundException` - если маппинг не найден в ViewConfig или ViewPrefab равен null
- `ArgumentNullException` - если переданы null параметры (например, `ViewShowParams` в `ShowAsync`, `canvas` в `CanvasProvider.RegisterCanvas()`)
- `ArgumentException` - если параметры невалидны (например, отрицательная длительность анимации в `SetAnimationDuration`)
- `InvalidOperationException` - если компонент не найден на инстанцированном префабе (в `IAssetProvider.InstantiateAsync<TComponent>`)
- `System.Exception` - если ViewConfig не найден в ConfigProvider при регистрации в DI

**Рекомендации:**
- Всегда оборачивайте вызовы `ShowAsync` и `HideAsync` в try-catch для обработки ошибок
- Проверяйте наличие ViewConfig перед запуском приложения
- Убедитесь, что все ViewModel зарегистрированы в DI контейнере перед использованием

**Обработка уничтоженных View:**
- Если View уничтожен внешне (например, через `Destroy()` или при смене сцены) пока он находится в кэше или активных View, система не отслеживает это автоматически
- Рекомендуется вызывать `ClearCache()` и `ClearHistory()` перед уничтожением View или при переходе между сценами
- При попытке показать View, который был уничтожен, система попытается создать новый экземпляр через `ViewFactory` (так как кэш проверяет наличие по типу, а не по валидности экземпляра)
- **Важно:** Если View уничтожен, но остался в `_activeViews` или `_viewHistory`, методы `GoBackAsync()` или `GetActiveView<TView>()` могут вернуть null или уничтоженный объект. Всегда проверяйте результат на null перед использованием
- **Проверка валидности View:** Перед использованием View из кэша или истории рекомендуется проверять его валидность через `view != null && view is Component component && component != null`. Это особенно важно при работе с `GoBackAsync()` или `GetActiveView<TView>()`
- **Очистка при смене сцены:** При переходе между сценами рекомендуется вызывать `ClearCache()`, `ClearHistory()` и `CloseAllNonPersistentAsync()` для предотвращения утечек памяти и работы с уничтоженными объектами

---

## Changelog

### 2026-02-05 (Documentation Review)
- **📝 FIXED:** `EmbeddedPanelService` — обновлена секция реализации:
  - Заменена устаревшая константа `PanelTransition` на актуальный словарь `PanelTransitions`
  - Добавлен метод `GetTransitionStyle(Type vmType)` для получения анимации по типу ViewModel
  - Добавлены комментарии о направлениях анимаций (SlideLeft/SlideRight)
  - Код теперь соответствует фактической реализации в `EmbeddedPanelService.cs`

### 2025-01-28 (Documentation Review)
- **📝 FIXED:** `IView` интерфейс — добавлено недокументированное свойство `IViewModel ViewModel { get; }`
- **📝 FIXED:** `BindableView` — добавлен explicit interface implementation `IViewModel IView.ViewModel`
- **📝 FIXED:** `ViewFactory.CreateAsync(Type)` — исправлено описание: non-generic версия использует `LoadAsync` + `Object.Instantiate`, а не `InstantiateAsync`
- **🚨 NEW:** Добавлена секция 2.1 "ВСЕГДА используйте static lambdas (NO CLOSURES!)" в Best Practices
- **📝 FIXED:** Все примеры кода обновлены для соответствия правилам R3:
  - Все `Subscribe` используют static lambdas со state
  - Все closures заменены на static lambdas с передачей `this`/`self`
  - Async subscriptions используют паттерн `static (_, self) => self.Method().Forget()`
  - Event subscriptions с отпиской в `Dispose()`
- **📝 UPDATED:** Примеры SettingsViewModel, InventoryViewModel, WorkflowNodeView, FirstView и другие

### 2025-01-27 (Update 2)
- **✨ NEW:** Добавлена документация для `UIElementFactory` (секция 10)
  - Простая фабрика для UI элементов с DI инъекцией
  - Интерфейс `IUIElementFactory` и реализация
  - Примеры использования
- **✨ NEW:** Добавлена документация для `EmbeddedPanelService` (секция 11)
  - Сервис для встраиваемых панелей
  - Интерфейс `IEmbeddedPanelHost`
  - `EmbeddedPanelStateChangedNotification` для Mediator
  - Жизненный цикл и примеры использования
- **📝 UPDATED:** Обновлена диаграмма архитектуры с новыми компонентами
- **📝 UPDATED:** Обновлена структура папок
- **📝 UPDATED:** Обновлен список DI регистраций
- **📝 UPDATED:** Обновлено оглавление

### 2025-01-27 (Initial)
- **🚨 КРИТИЧЕСКОЕ:** Добавлена документация об уничтожении non-persistent View после `HideAsync()`
  - Секция `HideAsync(IView view)` обновлена с детальным описанием логики уничтожения
  - Секция "Особенности" ViewService обновлена с предупреждением о кэшировании
  - Data Flow диаграмма "Скрытие View" обновлена с шагами уничтожения
  - Best Practices секция "Используйте персистентные View" расширена
  - Troubleshooting секция "View создаётся несколько раз" расширена
  - Чеклист обновлён с акцентом на `_isPersistent` флаг

---

*Документация актуальна на момент создания. При изменении архитектуры - обновить!*
