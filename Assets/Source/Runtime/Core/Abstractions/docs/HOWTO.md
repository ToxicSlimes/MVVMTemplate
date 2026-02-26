# HOWTO — Abstractions

Рецепты для типовых задач в слое контрактов и базовых классов.

---

## Добавить новый интерфейс сервиса

### Когда нужен

- Новая внешняя зависимость (API, платформа, файловая система)
- Новый домен-сервис (персонажи, чат, workflow)
- Существующий интерфейс слишком раздут

### Шаги

1. **Определи категорию** — выбери папку:
   - `Services/Platform/` — платформенные сервисы (clipboard, file dialog, file system)
   - `Services/Network/` — сетевые (HTTP, auth, session)
   - `Services/Chat/` — чат-домен
   - `Services/Workflow/` — workflow-домен
   - `Services/ProceduralUI/` — procedural UI система
   - `Services/` — общие (logger, audio, prefs)

2. **Создай интерфейс**:

```csharp
// Services/Inventory/IInventoryService.cs
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services.Inventory
{
    public interface IInventoryService
    {
        UniTask<List<InventoryItem>> GetItemsAsync(CancellationToken ct = default);
        UniTask<bool> UseItemAsync(string itemId, CancellationToken ct = default);
    }
}
```

3. **Реализуй в Infrastructure** (не в Abstractions!)

4. **Зарегистрируй в DI** через Reflex

### Чеклист

- [ ] Интерфейс в правильной подпапке `Services/`
- [ ] Все async методы с `CancellationToken ct = default`
- [ ] Возвращает `UniTask<T>`, не `Task<T>`
- [ ] Нет зависимостей на Infrastructure
- [ ] ISP — один интерфейс = одна ответственность
- [ ] `IDisposable` если сервис владеет ресурсами

---

## Добавить новый NavigationTarget

### Когда нужен

- Новый экран/view в приложении

### Шаги

1. **Добавь struct в `Navigation/NavigationTargets.cs`**:

```csharp
namespace Core.Abstractions.Navigation
{
    // Существующие:
    public readonly struct LoginNavigation : INavigationTarget { }
    public readonly struct ChatNavigation : INavigationTarget { }
    
    // Новый:
    public readonly struct InventoryNavigation : INavigationTarget { }
}
```

2. **Создай ViewModel** (в MVVM слое, не в Abstractions)

3. **Зарегистрируй маппинг** NavigationTarget → ViewModel в state machine

### Чеклист

- [ ] `readonly struct` (не class!)
- [ ] Implements `INavigationTarget`
- [ ] Добавлен в `NavigationTargets.cs` (не отдельный файл)
- [ ] Маппинг зарегистрирован

---

## Добавить базовый класс (BindableView / BindableElement)

### BindableView — для новых экранов

Каждый экран — наследник `BindableView<TViewModel>`:

```csharp
// В MVVM слое (не в Abstractions!):
public class InventoryView : BindableView<InventoryViewModel>
{
    [SerializeField] private Transform _itemsContainer;
    
    protected override void OnBind(ref DisposableBag disposables)
    {
        // Подписки на ViewModel
        ViewModel.ObserveItems()
            .Subscribe(items => RefreshItems(items))
            .AddTo(ref disposables);
    }
}
```

`BindableView` уже предоставляет:
- `ViewModel` property
- `ShowAsync()` / `HideAsync()` lifecycle
- `CanvasGroup` управление
- `DisposableBag` для подписок
- `IsPersistent` флаг

### BindableElement — для элементов списков

Каждый элемент списка — наследник `BindableElement<TData>`:

```csharp
// 1. Определи data struct (в Data слое):
public struct InventoryItemData : IElementData
{
    public string Id { get; set; }
    public string Name;
    public int Count;
    public Sprite Icon;
}

// 2. Создай элемент (в MVVM слое):
public class InventoryItemElement : BindableElement<InventoryItemData>
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private Image _icon;
    
    protected override void OnBind(InventoryItemData data)
    {
        _nameText.text = data.Name;
        _countText.text = data.Count.ToString();
        _icon.sprite = data.Icon;
    }
}
```

`BindableElement` предоставляет:
- `Bind(data)` / `Refresh(data)` / `Unbind()`
- Click handling через `_clickButton`
- Selection через `_selectionIndicator`
- Popup integration через `_popupTrigger`

### Чеклист

- [ ] Data struct implements `IElementData` (имеет `Id`)
- [ ] Override `OnBind()` — обязательно
- [ ] Override `OnRefresh()` — если нужна оптимизация (по умолчанию вызывает OnBind)
- [ ] Override `OnUnbind()` — если нужна очистка
- [ ] `OnDispose()` — для освобождения ресурсов

---

## Добавить новый Factory интерфейс

### Когда нужен

- Создание сложных объектов с зависимостями (DI)
- Фабрика для пулов или lazy-создания

### Шаги

1. **Создай интерфейс в `Factories/`**:

```csharp
// Factories/IInventoryFactory.cs
namespace Core.Abstractions.Factories
{
    public interface IInventoryFactory
    {
        IInventoryPanel Create(InventoryConfig config);
    }
}
```

Существующий пример:

```csharp
public interface IGameStateMachineFactory
{
    IGameStateMachine Create();
}
```

### Чеклист

- [ ] Интерфейс в `Factories/`
- [ ] Метод `Create()` возвращает **интерфейс**, не конкретный тип
- [ ] Параметры — конфиги или value types

---

## Добавить новый Model интерфейс (MVVM)

### Когда нужен

- Новый экран с данными и состоянием

### Шаги

1. **Создай в `MVVM/Models/{ScreenName}/`**:

```csharp
// MVVM/Models/Inventory/IInventoryModel.cs
using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models;

namespace Core.Abstractions.MVVM.Models.Inventory
{
    public interface IInventoryModel : IModel
    {
        event Action<List<InventoryItem>> ItemsChanged;
        event Action<string> SelectedItemIdChanged;
        event Action<bool> IsLoadingChanged;
        
        List<InventoryItem> Items { get; set; }
        string SelectedItemId { get; set; }
        bool IsLoading { get; set; }
        
        void LoadData();
        void SelectItem(string itemId);
    }
}
```

### Паттерн Model интерфейса

```
IModel (базовый: IsValid, Reset, Dispose)
  └── I{Screen}Model
        ├── event Action<T> {Property}Changed  — уведомления
        ├── T Property { get; set; }            — данные
        └── void Method()                       — команды
```

### Чеклист

- [ ] Extends `IModel`
- [ ] Event для каждого observable property: `event Action<T> {Name}Changed`
- [ ] Properties с get/set
- [ ] Папка `MVVM/Models/{ScreenName}/`

---

## Добавить новый Config базовый класс

### Иерархия

```
ConfigBase (ScriptableObject + Tri-Inspector)
  ├── ServiceConfigBase  — ⚙️ для сервисов (pool, cache, network, debug секции)
  ├── DataConfigBase     — 📊 для данных (data, references, validation секции)
  └── ViewConfigBase     — 🖼️ для UI (prefabs, styles, animations, sounds секции)
```

### Создание нового конфига

```csharp
// Наследуй от нужного базового:
[CreateAssetMenu(fileName = "InventoryConfig", menuName = "Configs/Inventory")]
public class InventoryConfig : ServiceConfigBase
{
    [Group("service")]
    [SerializeField] private int _maxSlots = 50;
    
    [Group("cache")]
    [SerializeField] private float _cacheTtlSeconds = 300f;
    
    public int MaxSlots => _maxSlots;
    public float CacheTtlSeconds => _cacheTtlSeconds;
    
    protected override void ValidateConfig()
    {
        if (_maxSlots <= 0)
            Debug.LogWarning($"[{name}] MaxSlots must be positive");
    }
}
```

---

*Обновлено: 2026-02-11*
