# Infrastructure — Техдолг и Улучшения

*Обновлено: 2026-02-11*

---

## 🔴 Высокий приоритет

### 1. StateMachines/States — пустая папка
**Где:** `Infrastructure/StateMachines/States/`
**Проблема:** Папка пустая, States перенесены в `Presentation/States/`. Мёртвый код / мёртвая структура.
**Решение:** Удалить пустую папку или задокументировать почему оставлена.

### 2. UI/Components — пустая заготовка
**Где:** `Infrastructure/UI/`
**Проблема:** Пустая папка-заготовка без реализации.
**Решение:** Либо реализовать (если планируется), либо удалить.

### 3. ConfigProvider — PreloadAllConfigsAsync в конструкторе
**Где:** `Services/Config/ConfigProvider.cs`
**Проблема:** `PreloadAllConfigsAsync().Forget()` вызывается в конструкторе. Если загрузка упадёт — ошибку проглотит.
**Решение:** Вынести preload в явный `InitializeAsync()` метод, вызываемый из bootstrap.

---

## 🟡 Средний приоритет

### 4. Отсутствие единого error handling паттерна
**Проблема:** Часть сервисов логирует ошибки, часть бросает, часть возвращает null. Нет единого подхода.
**Решение:** Ввести `Result<T>` паттерн или единый error handling middleware.

### 5. ProceduralUI — большой набор компонентов без абстракций
**Где:** `Services/ProceduralUI/Components/`
**Проблема:** 12+ Procedural-компонентов с возможной дупликацией логики. Базовый класс `ProceduralUiComponentBase` есть, но coverage не ясен.
**Решение:** Ревью на общие паттерны, вынос в shared utility.

### 6. Network — SessionStore без интерфейса
**Где:** `Services/Network/SessionStore.cs`
**Проблема:** Если `SessionStore` не имеет интерфейса в Abstractions — нарушает паттерн.
**Решение:** Добавить `ISessionStore` в Abstractions.

---

## 🟢 Низкий приоритет

### 7. Extensions — один файл
**Где:** `Extensions/UnityObjectExtensions.cs`
**Проблема:** Единственный extension-файл. При росте может стать God-файлом.
**Решение:** При добавлении новых extensions — группировать по доменам.

### 8. Документация сервисов
**Проблема:** Не все папки `Services/*` имеют docs.
**Решение:** Создать минимальную документацию для каждого домена сервисов.

### 9. InputSystem_Actions — сгенерированный файл
**Где:** `Services/Input/InputSystem_Actions.cs`
**Проблема:** Сгенерированный Unity Input System файл лежит рядом с `InputService.cs`. Может путать.
**Решение:** Пометить комментарием `// Auto-generated` или вынести в отдельную папку.
