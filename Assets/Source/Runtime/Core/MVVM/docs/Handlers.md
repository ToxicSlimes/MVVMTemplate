# Handlers — Каталог

## Обзор

Handlers — обработчики Queries и Commands. Содержат бизнес-логику и взаимодействуют с сервисами.

## Структура Handlers

```
Handlers/
├── Auth/
│   ├── LoginHandler.cs
│   ├── LogoutHandler.cs
│   └── GetStoredCredentialsHandler.cs
├── Chat/
│   ├── GetCharacterHandler.cs
│   ├── GetChatHistoryHandler.cs
│   ├── GetChatExtensionsHandler.cs
│   ├── GetUserPersonasHandler.cs
│   ├── CreateChatHandler.cs
│   ├── SendChatMessageHandler.cs
│   ├── DeleteChatHandler.cs
│   ├── SetChatContextHandler.cs
│   ├── RenderUiComponentsHandler.cs
│   ├── ClearUiComponentsHandler.cs
│   ├── DisposeProceduralUiHandler.cs
│   └── ExecuteWorkflowHandler.cs
├── Common/
│   ├── GetConfigHandler.cs
│   ├── LoadPrefsHandler.cs
│   ├── SavePrefsHandler.cs
│   ├── NavigateHandler.cs
│   ├── OpenSettingsHandler.cs
│   ├── ShowViewHandler.cs
│   ├── HideViewHandler.cs
│   ├── ConfirmDialogHandler.cs
│   └── ReleaseImageHandler.cs
├── Elements/
│   ├── CreateElementHandler.cs
│   ├── DestroyElementHandler.cs
│   ├── ClearContainerHandler.cs
│   └── ReconcileElementsHandler.cs
├── ImageGen/
│   ├── LoadImageHandler.cs
│   ├── OpenImagePickerHandler.cs
│   ├── GenerateImageHandler.cs
│   ├── RemoveBackgroundHandler.cs
│   └── SaveImageHandler.cs
├── Platform/
│   └── OpenImageFileHandler.cs
├── Views/
│   ├── ShowViewRequestHandler.cs
│   ├── HideViewRequestHandler.cs
│   ├── EmbeddedViewRequestHandler.cs
│   └── HideEmbeddedViewHandler.cs
└── Workflow/
    ├── GetWorkflowUiHandler.cs
    ├── GetWorkflowsHandler.cs
    └── ExecuteWorkflowUiActionHandler.cs
```

---

## Каталог Handlers

### Auth Handlers

| Handler | Request | Response | Dependencies |
|---------|---------|----------|--------------|
| `LoginHandler` | `LoginCommand` | `AuthResult` | IAuthService, IPrefsService, IPublisher, ILogger |
| `LogoutHandler` | `LogoutCommand` | `Nothing` | IPrefsService, IPublisher |
| `GetStoredCredentialsHandler` | `GetStoredCredentialsQuery` | `StoredCredentials` | IPrefsService |

### Chat Handlers

| Handler | Request | Response | Dependencies |
|---------|---------|----------|--------------|
| `GetCharacterHandler` | `GetCharacterQuery` | `CharacterModel` | ICharacterService, ILogger |
| `GetChatHistoryHandler` | `GetChatHistoryQuery` | `ChatModel` | IChatService, ILogger |
| `GetChatExtensionsHandler` | `GetChatExtensionsQuery` | `UiExtensionsModel` | IChatService, ILogger |
| `GetUserPersonasHandler` | `GetUserPersonasQuery` | `List<ChatPersonaDto>` | IChatService, ILogger |
| `CreateChatHandler` | `CreateChatCommand` | `CreateChatResult` | IChatService, ILogger |
| `SendChatMessageHandler` | `SendChatMessageCommand` | `SendChatMessageResult` | IMessageService, ILogger |
| `DeleteChatHandler` | `DeleteChatCommand` | `bool` | IChatService, ILogger |
| `SetChatContextHandler` | `SetChatContextCommand` | `Nothing` | IProceduralUiService |
| `RenderUiComponentsHandler` | `RenderUiComponentsCommand` | `Nothing` | IProceduralUiService, ILogger |
| `ClearUiComponentsHandler` | `ClearUiComponentsCommand` | `Nothing` | IProceduralUiService |
| `DisposeProceduralUiHandler` | `DisposeProceduralUiCommand` | `Nothing` | IProceduralUiService |
| `ExecuteWorkflowHandler` | `ExecuteWorkflowCommand` | `ExecuteWorkflowResult` | IWorkflowService, ILogger |

### Common Handlers

| Handler | Request | Response | Dependencies |
|---------|---------|----------|--------------|
| `GetConfigHandler<T>` | `GetConfigQuery<T>` | `TConfig` | IConfigProvider |
| `LoadPrefsHandler<T>` | `LoadPrefsQuery<T>` | `T` | IPrefsService |
| `SavePrefsHandler<T>` | `SavePrefsCommand<T>` | `Nothing` | IPrefsService |
| `NavigateHandler` | `NavigateCommand` | `Nothing` | IGameStateMachine |
| `OpenSettingsHandler` | `OpenSettingsCommand` | `bool` | IViewService |
| `ShowViewHandler<T>` | `ShowViewCommand<T>` | `bool` | IViewService |
| `HideViewHandler<T>` | `HideViewCommand<T>` | `bool` | IViewService |
| `ConfirmDialogHandler` | `ConfirmDialogQuery` | `bool` | IDialogService |
| `ReleaseImageHandler` | `ReleaseImageCommand` | `Nothing` | IResourceService, ILogger |

### Elements Handlers

| Handler | Request | Response | Dependencies |
|---------|---------|----------|--------------|
| `CreateElementHandler<T,D>` | `CreateElementRequest<T,D>` | `TElement` | IUIElementService |
| `DestroyElementHandler` | `DestroyElementRequest` | `Nothing` | IUIElementService |
| `ClearContainerHandler` | `ClearContainerRequest` | `Nothing` | IUIElementService |
| `ReconcileElementsHandler<T,D>` | `ReconcileElementsRequest<T,D>` | `Nothing` | IUIElementService |

### ImageGen Handlers

| Handler | Request | Response | Dependencies |
|---------|---------|----------|--------------|
| `LoadImageHandler` | `LoadImageQuery` | `LoadImageResult` | IFileSystemService, IImageCodec, ILogger |
| `OpenImagePickerHandler` | `OpenImagePickerQuery` | `OpenImagePickerResult` | IFileDialogService, IFileSystemService, IImageCodec, ILogger |
| `GenerateImageHandler` | `GenerateImageCommand` | `GenerateImageResult` | IGeminiImageService, ILogger |
| `RemoveBackgroundHandler` | `RemoveBackgroundCommand` | `RemoveBackgroundResult` | IBackgroundRemovalService, ILogger |
| `SaveImageHandler` | `SaveImageCommand` | `SaveImageResult` | IFileDialogService, IImageCodec, ILogger |

### Views Handlers

| Handler | Request | Response | Dependencies |
|---------|---------|----------|--------------|
| `ShowViewRequestHandler<V,VM>` | `ShowViewRequest<V,VM>` | `bool` | IViewService |
| `HideViewRequestHandler<V,VM>` | `HideViewRequest<V,VM>` | `bool` | IViewService |
| `EmbeddedViewRequestHandler<VM>` | `ShowEmbeddedViewRequest<VM>` | `bool` | IEmbeddedPanelHost |
| `HideEmbeddedViewHandler<VM>` | `HideEmbeddedViewRequest<VM>` | `Nothing` | IEmbeddedPanelHost |

### Workflow Handlers

| Handler | Request | Response | Dependencies |
|---------|---------|----------|--------------|
| `GetWorkflowUiHandler` | `GetWorkflowUiQuery` | `StandaloneUiResult` | IWorkflowService |
| `GetWorkflowsHandler` | `GetWorkflowsQuery` | `List<WorkflowModel>` | IWorkflowService |
| `ExecuteWorkflowUiActionHandler` | `ExecuteWorkflowUiActionCommand` | `StandaloneUiResult` | IWorkflowService |

---

## Примеры Handlers

### Query Handler

```csharp
// Handlers/Chat/GetCharacterHandler.cs
public sealed class GetCharacterHandler : IRequestHandler<GetCharacterQuery, CharacterModel>
{
    private readonly ICharacterService _characterService;
    private readonly ILogger _logger;

    public GetCharacterHandler(ICharacterService characterService, ILogger logger)
    {
        _characterService = characterService;
        _logger = logger;
    }

    public async UniTask<CharacterModel> Handle(GetCharacterQuery query, CancellationToken ct)
    {
        try
        {
            var character = await _characterService.GetCharacterBySlugAsync(
                query.UserSlug, query.CharacterSlug, ct);
            
            if (character == null)
            {
                _logger.Warn("[GetCharacterHandler] Character not found: {0}/{1}", 
                    query.UserSlug, query.CharacterSlug);
            }
            
            return character;
        }
        catch (OperationCanceledException)
        {
            throw; // Пробросить cancellation
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[GetCharacterHandler] Failed to get character");
            return null;
        }
    }
}
```

### Command Handler с Result

```csharp
// Handlers/Chat/CreateChatHandler.cs
public sealed class CreateChatHandler : IRequestHandler<CreateChatCommand, CreateChatResult>
{
    private readonly IChatService _chatService;
    private readonly ILogger _logger;

    public CreateChatHandler(IChatService chatService, ILogger logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async UniTask<CreateChatResult> Handle(CreateChatCommand command, CancellationToken ct)
    {
        try
        {
            var dto = new CreateChatRequestDto
            {
                CharacterId = command.CharacterId,
                GreetingId = command.GreetingId,
                PromptId = command.PromptId,
                ModelId = command.ModelId,
                PersonaId = command.PersonaId,
                IsEncrypted = false,
                MaxTokens = command.MaxTokens
            };

            var chat = await _chatService.CreateChatAsync(dto, ct);

            if (chat == null)
            {
                _logger.Error("[CreateChatHandler] Chat creation failed - null response");
                return CreateChatResult.Failure("Chat creation failed");
            }

            _logger.Info("[CreateChatHandler] Chat created: {0}", chat.Id);
            return CreateChatResult.Success(chat.Id);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[CreateChatHandler] Failed to create chat");
            return CreateChatResult.Failure(ex.Message);
        }
    }
}
```

### Command Handler без результата

```csharp
// Handlers/Chat/SetChatContextHandler.cs
public sealed class SetChatContextHandler : IRequestHandler<SetChatContextCommand>
{
    private readonly IProceduralUiService _proceduralUiService;

    public SetChatContextHandler(IProceduralUiService proceduralUiService)
    {
        _proceduralUiService = proceduralUiService;
    }

    public UniTask<Nothing> Handle(SetChatContextCommand command, CancellationToken ct)
    {
        _proceduralUiService.SetChatContext(command.ChatId);
        return UniTask.FromResult(Nothing.Value);
    }
}
```

### Handler с публикацией Notification

```csharp
// Handlers/Auth/LoginHandler.cs
public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _authService;
    private readonly IPrefsService _prefsService;
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;

    public async UniTask<AuthResult> Handle(LoginCommand request, CancellationToken ct)
    {
        // ... validation и login logic

        var token = _authService.CurrentToken;

        // Публикация notification о успешном логине
        await _publisher.Publish(
            new UserLoggedInNotification(request.Email, token), ct);

        return AuthResult.Success(token);
    }
}
```

### Generic Handler

```csharp
// Handlers/Common/GetConfigHandler.cs
public sealed class GetConfigHandler<TConfig> : IRequestHandler<GetConfigQuery<TConfig>, TConfig>
    where TConfig : ConfigBase
{
    private readonly IConfigProvider _configProvider;

    public GetConfigHandler(IConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public UniTask<TConfig> Handle(GetConfigQuery<TConfig> query, CancellationToken ct)
    {
        var config = _configProvider.GetConfig<TConfig>();
        return UniTask.FromResult(config);
    }
}
```

---

## Handler Interfaces

```csharp
// Query/Command с результатом
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    UniTask<TResponse> Handle(TRequest request, CancellationToken ct);
}

// Command без результата
public interface IRequestHandler<TRequest>
    where TRequest : IRequest
{
    UniTask<Nothing> Handle(TRequest request, CancellationToken ct);
}

// Notification handler
public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    UniTask Handle(TNotification notification, CancellationToken ct);
}
```

---

## Регистрация Handlers

```csharp
// Registration/ViewHandlerRegistration.cs
public static class ViewHandlerRegistration
{
    public static void RegisterHandlers(IContainerBuilder builder)
    {
        // Chat handlers
        builder.Register<GetCharacterHandler>(Lifetime.Transient).AsImplementedInterfaces();
        builder.Register<GetChatHistoryHandler>(Lifetime.Transient).AsImplementedInterfaces();
        builder.Register<CreateChatHandler>(Lifetime.Transient).AsImplementedInterfaces();
        // ...

        // Auth handlers
        builder.Register<LoginHandler>(Lifetime.Transient).AsImplementedInterfaces();
        builder.Register<LogoutHandler>(Lifetime.Transient).AsImplementedInterfaces();
        // ...

        // Generic handlers (require special registration)
        builder.RegisterGeneric(typeof(GetConfigHandler<>), Lifetime.Transient)
            .AsImplementedInterfaces();
    }
}
```

---

## Best Practices

1. **Один handler — один request** — не объединять логику
2. **Dependency Injection** — все зависимости через конструктор
3. **Logging** — логировать ошибки и важные события
4. **CancellationToken** — всегда проверять и передавать
5. **OperationCanceledException** — пробрасывать, не ловить
6. **Result pattern** — возвращать структуры с IsSuccess
7. **Transient lifetime** — handlers должны быть stateless
8. **No side effects в Query handlers** — только чтение данных
