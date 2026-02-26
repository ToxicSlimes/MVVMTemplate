# ChatView

## Назначение

AI-чат с персонажами. Поддерживает Procedural UI расширения, workflow триггеры, отображение истории сообщений.

## Файлы

| Тип | Путь |
|-----|------|
| View | `Views/Chat/ChatView.cs` |
| ViewModel | `ViewModels/Chat/ChatViewModel.cs` |
| Model | `IChatModel` (Abstractions) |

---

## ViewModel Properties

| Property | Тип | Описание |
|----------|-----|----------|
| `StatusText` | `ReactiveProperty<string>` | Статус операции |
| `ResponseText` | `ReactiveProperty<string>` | JSON ответ (debug) |
| `IsLoading` | `ReactiveProperty<bool>` | Индикатор загрузки |
| `Character` | `ReactiveProperty<CharacterModel>` | Текущий персонаж |
| `CurrentChatId` | `ReactiveProperty<string>` | ID активного чата |
| `UiExtensions` | `ReactiveProperty<UiExtensionsModel>` | UI расширения от workflow |
| `ChatMessages` | `ReactiveProperty<List<ChatMessage>>` | История сообщений |
| `InputMessage` | `ReactiveProperty<string>` | Текст ввода |
| `DesignSystemSettings` | `DesignSystemSettings` | Цвета и стили |

---

## ViewModel Commands

| Command | Параметр | Описание |
|---------|----------|----------|
| `RunWorkflowCommand` | `Unit` | Запустить полный workflow (create chat + load extensions) |
| `SendMessageCommand` | `string` | Отправить сообщение в чат |
| `DeleteChatCommand` | `Unit` | Удалить текущий чат |
| `TriggerUiActionCommand` | `ChatUiActionRequest` | Триггер UI action |
| `GoBackCommand` | `Unit` | Вернуться в MenuSelection |

---

## UI Элементы

### Chat Area

| Поле | Тип | Описание |
|------|-----|----------|
| `_chatScrollView` | `ScrollRect` | Скролл чата |
| `_chatContent` | `RectTransform` | Контейнер сообщений |

### Input

| Поле | Тип | Описание |
|------|-----|----------|
| `_inputMessageField` | `TMP_InputField` | Поле ввода |
| `_sendMessageButton` | `Button` | Отправить |
| `_deleteChatButton` | `Button` | Удалить чат |
| `_goBackButton` | `Button` | Назад |

### Workflow

| Поле | Тип | Описание |
|------|-----|----------|
| `_runWorkflowButton` | `Button` | Запустить workflow |

### Status

| Поле | Тип | Описание |
|------|-----|----------|
| `_statusText` | `TMP_Text` | Статус |
| `_responseText` | `TMP_Text` | Debug response |
| `_chatIdText` | `TMP_Text` | ID чата |
| `_loadingIndicator` | `GameObject` | Spinner |

### Character Header

| Поле | Тип | Описание |
|------|-----|----------|
| `_characterNameText` | `TMP_Text` | Имя персонажа |
| `_characterAvatarImage` | `Image` | Аватар |

### Procedural UI Containers

| Поле | Тип | Описание |
|------|-----|----------|
| `_aboveInputExtensionsContainer` | `Transform` | Над полем ввода |
| `_belowChatExtensionsContainer` | `Transform` | Под чатом |
| `_sidebarExtensionsContainer` | `Transform` | Боковая панель |
| `_sidebarDisplayText` | `TMP_Text` | Текст sidebar |

---

## Биндинги

```csharp
// Статус
ViewModel.StatusText
    .Subscribe(this, (text, self) => self.UpdateStatusText())
    .AddTo(ref disposables);

// Сообщения
ViewModel.ChatMessages
    .Subscribe(this, (messages, self) => self.DisplayChatMessages(messages))
    .AddTo(ref disposables);

// Персонаж
ViewModel.Character
    .Subscribe(this, (character, self) => self.UpdateCharacterHeader(character))
    .AddTo(ref disposables);

// Send button (interactable depends on chat + loading + text)
ViewModel.CurrentChatId.CombineLatest(
    ViewModel.IsLoading,
    ViewModel.InputMessage,
    (chatId, isLoading, inputMessage) => ...)
```

---

## Связанные сервисы

| Сервис/Handler | Использование |
|----------------|---------------|
| `GetCharacterQuery` | Получить персонажа |
| `GetUserPersonasQuery` | Получить персоны пользователя |
| `CreateChatCommand` | Создать чат |
| `SendChatMessageCommand` | Отправить сообщение |
| `DeleteChatCommand` | Удалить чат |
| `GetChatHistoryQuery` | Получить историю |
| `GetChatExtensionsQuery` | Получить UI расширения |
| `SetChatContextCommand` | Установить контекст чата |
| `RenderUiComponentsCommand` | Отрендерить Procedural UI |
| `ExecuteWorkflowCommand` | Выполнить workflow |

---

## Notifications

ViewModel подписывается на:

| Notification | Реакция |
|--------------|---------|
| `UiActionNotification` | OnProceduralUiAction |
| `UiErrorNotification` | OnProceduralUiError |
| `SendMessageNotification` | OnUiSendMessage |
| `InsertTextNotification` | OnUiInsertText |
| `WorkflowTriggeredNotification` | RefreshUiExtensionsAsync |

---

## Схема UI

```
┌────────────────────────────────────────────────────────────────┐
│ [←] Chat with Character                        [Run Workflow]  │
├────────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ [Avatar] Character Name                                   │  │
│  │ Status: [OK] Connected   Chat: abc-123                    │  │
│  └──────────────────────────────────────────────────────────┘  │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  ┌────────────────────────────────────────────────────────┐    │
│  │                                  ┌──────────────────┐  │    │
│  │  AI Message                      │  User Message    │  │    │
│  │  Lorem ipsum...                  │  Hello there!    │  │    │
│  │                                  └──────────────────┘  │    │
│  │  ┌───────────────────────┐                             │    │
│  │  │  AI Message           │                             │    │
│  │  │  Response...          │                             │    │
│  │  └───────────────────────┘                             │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                │
├────────────────────────────────────────────────────────────────┤
│  [Procedural UI - Above Input]                                 │
├────────────────────────────────────────────────────────────────┤
│  [Enter message...]                            [Send] [Delete] │
└────────────────────────────────────────────────────────────────┘
```

---

## Особенности

- Динамический рендеринг сообщений (без prefab)
- Цвета из DesignSystemSettings
- Загрузка аватара через UnityWebRequest
- ProceduralUiContainers для workflow-generated UI
- Auto-scroll к последнему сообщению
