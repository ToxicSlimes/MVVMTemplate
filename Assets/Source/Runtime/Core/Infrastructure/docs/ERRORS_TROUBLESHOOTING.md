# Infrastructure — Ошибки и Troubleshooting

*Обновлено: 2026-02-11*

---

## Ошибка: Сервис не зарезолвлен (Reflex DI)

**Симптом:** `ReflexException: No binding found for IMyService`

**Причина:** Сервис не зарегистрирован в Installer.

**Решение:**
```csharp
// В соответствующем Installer
builder.AddSingleton<IMyService, MyService>();
```

**Проверь:**
- Installer добавлен на сцене (MonoBehaviour с `IInstaller`)
- Правильный lifetime (Singleton/Transient)
- Интерфейс и реализация совпадают

---

## Ошибка: NullReferenceException в конструкторе сервиса

**Симптом:** `NullReferenceException` при создании сервиса через DI.

**Причина:** Одна из зависимостей сервиса не зарегистрирована или зарегистрирована позже.

**Решение:**
1. Проверь что ВСЕ зависимости конструктора зарегистрированы
2. Проверь порядок регистрации — зависимость должна быть зарегистрирована раньше
3. Добавь `?? throw new ArgumentNullException()` в конструктор для раннего обнаружения

---

## Ошибка: UniTask was already completed / ObjectDisposedException

**Симптом:** Async операция отменена или объект уже disposed.

**Причина:** `CancellationToken` отменён (сцена выгружена, объект уничтожен).

**Решение:**
```csharp
public async UniTask DoAsync(CancellationToken ct)
{
    ct.ThrowIfCancellationRequested();  // Проверка в начале
    
    var result = await _someService.GetAsync(ct);
    
    ct.ThrowIfCancellationRequested();  // Проверка после await
    // использовать result
}
```

---

## Ошибка: HTTP запрос failed / timeout

**Симптом:** `HttpResponse.IsSuccess == false`, timeout exception.

**Причина:** Сервер недоступен, неправильный endpoint, сетевая ошибка.

**Решение:**
1. Проверь URL через `IUrlBuilder` — логируется в `ApiClient` (`HTTP GET/POST {url}`)
2. Проверь `HttpResponse.StatusCode` и `HttpResponse.Error`
3. Проверь что сервер запущен и доступен
4. Увеличь timeout при необходимости

---

## Ошибка: Asset не найден (Addressables)

**Симптом:** `InvalidKeyException` или `null` при загрузке через `IAssetProvider`.

**Причина:** Addressable адрес не настроен или ассет не в билде.

**Решение:**
1. Проверь адрес ассета в Addressables Groups
2. Убедись что ассет помечен как Addressable
3. Ребилд Addressables: `Window > Asset Management > Addressables > Build`

---

## Ошибка: ConfigProvider — конфиг не загружен

**Симптом:** `GetConfigAsync<T>()` возвращает null или бросает исключение.

**Причина:** ScriptableObject конфиг не найден по Addressable адресу.

**Решение:**
1. Проверь что конфиг наследуется от `ConfigBase`
2. Проверь Addressable label (см. `LabelByConfigName` в `ConfigProvider`)
3. Проверь что ScriptableObject asset существует в проекте

---

## Ошибка: Circular dependency

**Симптом:** `StackOverflowException` или DI контейнер не может создать сервис.

**Причина:** Сервис A зависит от B, B зависит от A.

**Решение:**
- Используй `Lazy<T>` или интерфейс-медиатор
- Разбей на два сервиса
- Используй Mediator для косвенного взаимодействия (Send/Publish вместо прямого вызова)
