# UIElementFactory

Простая фабрика для создания UI элементов с DI инъекцией.

---

## Назначение

`UIElementFactory` предоставляет упрощённый API для:
- Инстанцирования GameObject или Component
- Автоматической DI инъекции в созданный объект
- Использования в сценариях, где не нужна асинхронная загрузка (Addressables)

---

## Интерфейс IUIElementFactory

```csharp
public interface IUIElementFactory
{
    GameObject Instantiate(GameObject prefab, Transform parent);
    T Instantiate<T>(T prefab, Transform parent) where T : Component;
}
```

---

## Реализация

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

---

## Использование

### Создание GameObject

```csharp
[Inject] private IUIElementFactory _uiFactory;
[SerializeField] private GameObject _buttonPrefab;
[SerializeField] private Transform _container;

private void CreateButton()
{
    var button = _uiFactory.Instantiate(_buttonPrefab, _container);
    // DI injection уже выполнен
}
```

### Создание типизированного Component

```csharp
[Inject] private IUIElementFactory _uiFactory;
[SerializeField] private Button _buttonPrefab;
[SerializeField] private Transform _container;

private void CreateButton()
{
    var button = _uiFactory.Instantiate(_buttonPrefab, _container);
    button.onClick.AddListener(OnButtonClick);
}
```

---

## Отличия от других фабрик

| Фабрика | Загрузка | DI | Async | Использование |
|---------|----------|----|----|---------------|
| **UIElementFactory** | Object.Instantiate | ✅ | ❌ | Простые префабы в сцене |
| **ViewFactory** | Addressables | ✅ | ✅ | View с динамической загрузкой |
| **UIElementService** | Addressables | ✅ | ✅ | Списки, reconciliation |

---

## Когда использовать

### ✅ Используй UIElementFactory

- Префаб уже в памяти (SerializeField)
- Не нужна асинхронная загрузка
- Простой UI (кнопки, иконки, метки)
- Быстрое прототипирование

### ❌ Не используй UIElementFactory

- Нужна загрузка через Addressables
- Требуется reconciliation (списки)
- Сложные View с ViewModel
- Кеширование и pooling

---

## Регистрация в DI

```csharp
container.AddSingleton<IUIElementFactory, UIElementFactory>();
```

---

## Пример в ViewModel

```csharp
public class NotificationViewModel : DisposableObject
{
    private readonly IUIElementFactory _uiFactory;
    private readonly Transform _notificationContainer;
    private readonly GameObject _notificationPrefab;

    public NotificationViewModel(
        IUIElementFactory uiFactory,
        Transform notificationContainer,
        GameObject notificationPrefab)
    {
        _uiFactory = uiFactory;
        _notificationContainer = notificationContainer;
        _notificationPrefab = notificationPrefab;
    }

    public void ShowNotification(string message)
    {
        var notification = _uiFactory.Instantiate(_notificationPrefab, _notificationContainer);
        var text = notification.GetComponentInChildren<TMP_Text>();
        text.text = message;

        // Auto-destroy после анимации
        _ = DestroyAfterDelay(notification, 3f);
    }

    private async UniTaskVoid DestroyAfterDelay(GameObject obj, float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        if (obj != null)
            Object.Destroy(obj);
    }
}
```

---

## Best Practices

### ✅ DO

1. **Используй для простых префабов** без сложной логики
2. **Кешируй prefab references** через SerializeField
3. **Очищай созданные объекты** через Object.Destroy

### ❌ DON'T

1. **Не используй для View** — используй ViewFactory
2. **Не используй для списков** — используй UIElementService
3. **Не загружай Addressables** — это sync операция

---

## См. также

- [ViewFactory.md](ViewFactory.md) — создание View
- [ElementSystem.md](ElementSystem.md) — UIElementService для списков
- [Config.md](Config.md) — конфигурация маппингов
