# Services — Сервисные интерфейсы

> Все сервисные контракты системы EndlessAdventure.

## 📋 Сводная таблица

| Категория | Интерфейс | Путь | Реализация |
|-----------|-----------|------|------------|
| **Network** | `IApiClient` | `Services/Network/` | `ApiClient` |
| | `IHttpClient` | `Services/Network/` | `UnityHttpClient` |
| | `IHttpTransportService` | `Services/Network/` | `HttpTransportService` |
| | `IAuthService` | `Services/Network/` | `AuthService` |
| | `ISessionStore` | `Services/Network/` | `SessionStore` |
| | `IUrlBuilder` | `Services/Network/` | `UrlBuilder` |
| **Chat** | `IChatService` | `Services/Chat/` | `ChatService` |
| **Workflow** | `IWorkflowService` | `Services/Workflow/` | `WorkflowService` |
| | `IWorkflowNodeDefinitionService` | `Services/Workflow/` | `WorkflowNodeDefinitionService` |
| | `IWorkflowNodeMapper` | `Services/Workflow/` | `WorkflowNodeMapper` |
| **Message** | `IMessageService` | `Services/Message/` | `MessageService` |
| **Character** | `ICharacterService` | `Services/Character/` | `CharacterService` |
| **Platform** | `IFileDialogService` | `Services/Platform/` | `FileDialogService` |
| | `IClipboardService` | `Services/Platform/` | `ClipboardService` |
| | `IDialogService` | `Services/Platform/` | `DialogService` |
| | `IFileSystemService` | `Services/Platform/` | `FileSystemService` |
| | `IResourceService` | `Services/Platform/` | `ResourceService` |
| **Input** | `IInputService` | `Services/Input/` | `InputService` |
| **Audio** | `IAudioService` | `Services/` | `AudioService` |
| **ImageGen** | `IGeminiImageService` | `Services/ImageGen/` | `GeminiImageService` |
| **UIGen** | `IUIGenerationPipeline` | `Services/UIGeneration/` | `UIGenerationPipeline` |
| | `IUIPromptBuilder` | `Services/UIGeneration/` | `UIPromptBuilder` |
| | `IBackgroundRemovalService` | `Services/UIGeneration/` | `BackgroundRemovalService` |
| **Media** | `IImageCodec` | `Services/Media/` | `ImageCodec` |
| **Logging** | `ILogger` | `Services/` | `Logger` |
| | `ILogFormatter` | `Services/` | `LogFormatter` |
| | `ILogInterceptor` | `Services/` | `LogInterceptor` |
| | `ILoadSceneService` | `Services/` | `LoadSceneService` |
| | `IDataPreloadService` | `Services/` | `DataPreloadService` |
| | `IPrefsService` | `Services/` | `PrefsService` |

---

## Network

### IApiClient

**Путь:** `Services/Network/IApiClient.cs`

```csharp
public interface IApiClient
{
    UniTask<HttpResponse<T>> GetAsync<T>(
        string endpoint, 
        Dictionary<string, string> queryParams = null, 
        CancellationToken cancellationToken = default, 
        bool useApiPrefix = true);
    
    UniTask<HttpResponse<T>> PostAsync<T>(
        string endpoint, 
        object body = null, 
        CancellationToken cancellationToken = default, 
        bool useApiPrefix = true);
    
    UniTask<HttpResponse<T>> PutAsync<T>(
        string endpoint, 
        object body = null, 
        CancellationToken cancellationToken = default, 
        bool useApiPrefix = true);
    
    UniTask<HttpResponse<T>> PatchAsync<T>(
        string endpoint, 
        object body = null, 
        CancellationToken cancellationToken = default, 
        bool useApiPrefix = true);
    
    UniTask<HttpResponse<T>> DeleteAsync<T>(
        string endpoint, 
        CancellationToken cancellationToken = default, 
        bool useApiPrefix = true);
    
    void SetAuthToken(string token);
    void ClearAuthToken();
}
```

**Назначение:** High-level HTTP клиент с авторизацией, JSON сериализацией, retry логикой.

**Пример:**
```csharp
var response = await _apiClient.GetAsync<UserDto>("/users/me");
if (response.IsSuccess)
{
    var user = response.Data;
}
```

---

### IHttpClient

**Путь:** `Services/Network/IHttpClient.cs`

```csharp
public interface IHttpClient
{
    UniTask<RawHttpResponse> SendAsync(RawHttpRequest request, CancellationToken cancellationToken = default);
}

public sealed class RawHttpRequest
{
    public string Method { get; set; }
    public string Url { get; set; }
    public byte[] Body { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public int TimeoutSeconds { get; set; }
}

public sealed class RawHttpResponse
{
    public bool IsNetworkError { get; set; }
    public bool IsHttpError { get; set; }
    public long StatusCode { get; set; }
    public string ResponseBody { get; set; }
    public string Error { get; set; }
    public Dictionary<string, string> Headers { get; set; }
}
```

**Назначение:** Low-level HTTP транспорт. Обёртка над UnityWebRequest.

---

### IHttpTransportService

**Путь:** `Services/Network/IHttpTransportService.cs`

```csharp
public interface IHttpTransportService
{
    UniTask<HttpResponse<T>> GetAsync<T>(string url, Dictionary<string, string> headers = null, CancellationToken ct = default);
    UniTask<HttpResponse<T>> PostAsync<T>(string url, object body = null, Dictionary<string, string> headers = null, CancellationToken ct = default);
    UniTask<HttpResponse<T>> PutAsync<T>(string url, object body = null, Dictionary<string, string> headers = null, CancellationToken ct = default);
    UniTask<HttpResponse<T>> PatchAsync<T>(string url, object body = null, Dictionary<string, string> headers = null, CancellationToken ct = default);
    UniTask<HttpResponse<T>> DeleteAsync<T>(string url, Dictionary<string, string> headers = null, CancellationToken ct = default);
    
    void SetDefaultHeader(string key, string value);
    void RemoveDefaultHeader(string key);
    void SetAuthToken(string token);
    void ClearAuthToken();
    void SetCookie(string cookieHeader);
    string GetCookie();
}
```

**Назначение:** Средний уровень — JSON сериализация, headers, cookies.

---

### IAuthService

**Путь:** `Services/Network/IAuthService.cs`

```csharp
public interface IAuthService
{
    UniTask<bool> SignInEmailAsync(string email, string password, bool rememberMe, CancellationToken ct = default);
    UniTask<bool> RefreshSessionAsync(CancellationToken ct = default);
    string CurrentCookie { get; }
    string CurrentToken { get; }
}
```

**Назначение:** Авторизация и управление сессией.

**Пример:**
```csharp
bool success = await _authService.SignInEmailAsync("user@mail.com", "password", true);
if (success)
{
    string token = _authService.CurrentToken;
}
```

---

### ISessionStore

**Путь:** `Services/Network/ISessionStore.cs`

```csharp
public interface ISessionStore
{
    string AuthToken { get; set; }
    string SessionCookie { get; set; }
}
```

**Назначение:** Хранилище для токена авторизации и session cookie. Используется IAuthService и IApiClient для персистентного хранения credentials между сессиями.

**Реализация:** `Core/Infrastructure/Services/Network/SessionStore.cs`

**Пример:**
```csharp
// Сохранение после успешного логина
_sessionStore.AuthToken = response.Token;
_sessionStore.SessionCookie = response.Cookie;

// Восстановление при старте приложения
if (!string.IsNullOrEmpty(_sessionStore.AuthToken))
{
    _apiClient.SetAuthToken(_sessionStore.AuthToken);
}
```

---

### IUrlBuilder

**Путь:** `Services/Network/IUrlBuilder.cs`

```csharp
public interface IUrlBuilder
{
    IUrlBuilder Api();
    IUrlBuilder SetEndpoint(string endpoint);
    IUrlBuilder AddPath(string path);
    IUrlBuilder AddPath(params string[] paths);
    IUrlBuilder AddQueryParam(string key, string value);
    IUrlBuilder AddQueryParams(Dictionary<string, string> parameters);
    string Build();
    void Reset();
}
```

**Назначение:** Fluent builder для URL.

**Пример:**
```csharp
string url = _urlBuilder
    .Api()
    .AddPath("users", userId, "chats")
    .AddQueryParam("page", "1")
    .Build();
// https://api.example.com/api/users/123/chats?page=1
```

---

## Chat

### IChatService

**Путь:** `Services/Chat/IChatService.cs`

```csharp
public interface IChatService
{
    UniTask<ChatModel> CreateChatAsync(CreateChatRequestDto dto, CancellationToken ct = default);
    UniTask<ChatModel> GetChatHistoryAsync(string chatId, CancellationToken ct = default);
    UniTask<bool> DeleteChatAsync(string chatId, CancellationToken ct = default);
    UniTask<UiExtensionsModel> GetChatExtensionsUiAsync(string chatId, CancellationToken ct = default);
    UniTask<List<ChatPersonaDto>> GetMyPersonasAsync(CancellationToken ct = default);
}
```

**Реализация:** `Core/Infrastructure/Services/Chat/ChatService.cs`

**Пример:**
```csharp
var chat = await _chatService.CreateChatAsync(new CreateChatRequestDto 
{
    CharacterId = characterId
});
```

---

## Workflow

### IWorkflowService

**Путь:** `Services/Workflow/IWorkflowService.cs`

```csharp
public interface IWorkflowService
{
    // CRUD
    UniTask<WorkflowModel> GetWorkflowAsync(string id, CancellationToken ct = default);
    UniTask<List<WorkflowModel>> GetWorkflowsAsync(WorkflowListQuery query = null, CancellationToken ct = default);
    UniTask<List<WorkflowModel>> GetMyWorkflowsAsync(int page = 1, int limit = 20, CancellationToken ct = default);
    UniTask<WorkflowModel> CreateWorkflowAsync(CreateWorkflowDto dto, CancellationToken ct = default);
    UniTask<WorkflowModel> UpdateWorkflowAsync(string id, UpdateWorkflowDto dto, CancellationToken ct = default);
    UniTask<bool> DeleteWorkflowAsync(string id, CancellationToken ct = default);
    UniTask<WorkflowModel> DuplicateWorkflowAsync(string id, string newName = null, CancellationToken ct = default);
    
    // Versioning
    UniTask<WorkflowModel> CreateNewVersionAsync(string id, UpdateWorkflowDto dto, CancellationToken ct = default);
    UniTask<List<WorkflowModel>> GetVersionsAsync(string id, CancellationToken ct = default);
    
    // Execution (legacy)
    [System.Obsolete("Use ExecuteAsync instead")]
    UniTask<WorkflowTriggerResult> TriggerWorkflowAsync(string workflowId, WorkflowTriggerDto dto, CancellationToken ct = default);
    
    // Execution
    UniTask<WorkflowExecutionResult> ExecuteAsync(
        string workflowId,
        Dictionary<string, object> payload = null,
        StorageContext storageContext = null,
        WorkflowExecutionOptions options = null,
        CancellationToken ct = default);
    
    // UI
    UniTask<StandaloneUiResult> GetUiAsync(
        string workflowId,
        StorageContext storageContext = null,
        CancellationToken ct = default);
    
    UniTask<StandaloneUiResult> ExecuteUiActionAsync(
        string workflowId,
        Dictionary<string, object> actionPayload,
        StorageContext storageContext = null,
        CancellationToken ct = default);
    
    // Runs
    UniTask<List<WorkflowRunModel>> GetRunsAsync(string workflowId, WorkflowRunStatus? status = null, int limit = 50, CancellationToken ct = default);
    UniTask<WorkflowRunModel> GetRunAsync(string workflowId, string runId, CancellationToken ct = default);
    UniTask<bool> DeleteRunAsync(string workflowId, string runId, CancellationToken ct = default);
    
    // Generation
    UniTask<WorkflowGenerationResultDto> GenerateWorkflowAsync(WorkflowGenerationRequestDto dto, CancellationToken ct = default);
    UniTask<bool> ResetWorkflowGenerationAsync(string sessionId, CancellationToken ct = default);
    
    // Entrypoints
    UniTask<List<WorkflowEntrypointDto>> GetEntrypointsAsync(CancellationToken ct = default);
    UniTask<List<WorkflowEntrypointDto>> GetWorkflowEntrypointsAsync(string workflowId, CancellationToken ct = default);
    UniTask<WorkflowHelperResponseDto> GetHelperAsync(CancellationToken ct = default);
    
    // Chat Integration (NEW 2026-02-11)
    UniTask<List<ChatWorkflowModel>> GetActiveWorkflowsAsync(string chatId, CancellationToken ct = default);
    UniTask<bool> AttachWorkflowToChatAsync(string chatId, string workflowId, CancellationToken ct = default);
    UniTask<bool> DetachWorkflowFromChatAsync(string chatId, string workflowId, CancellationToken ct = default);
    UniTask<bool> ToggleWorkflowActiveAsync(string chatId, string workflowId, bool isActive, CancellationToken ct = default);
    UniTask<bool> PatchWorkflowStorageAsync(string chatId, string workflowId, List<StoragePatchOperation> operations, CancellationToken ct = default);
    UniTask<WorkflowExecutionResult> RunEntrypointAsync(string entrypoint, string chatId, Dictionary<string, object> data = null, CancellationToken ct = default);
    
    // Testing
    UniTask<WorkflowExecutionResult> TestWorkflowAsync(string workflowId, Dictionary<string, object> payload = null, CancellationToken ct = default);
}

public class WorkflowListQuery
{
    public bool? MyWorkflows { get; set; }
    public string UserId { get; set; }
    public WorkflowVisibility? Visibility { get; set; }
    public string Search { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
```

**Назначение:** Полный API для workflow — создание, выполнение, UI.

---

### IWorkflowNodeDefinitionService

```csharp
public interface IWorkflowNodeDefinitionService
{
    WorkflowNodeTypeDefinition GetNodeDefinition(string nodeType);
    Dictionary<string, WorkflowNodeTypeDefinition> GetAllDefinitions();
    void LoadDefinitions();
}
```

---

### IWorkflowNodeMapper

```csharp
public interface IWorkflowNodeMapper
{
    List<RuntimeWorkflowNode> ToRuntimeNodes(JArray nodesArray);
}
```

---

## Message

### IMessageService

**Путь:** `Services/Message/IMessageService.cs`

```csharp
public interface IMessageService
{
    UniTask<MessageModel> SendMessageAsync(SendMessageRequestDto dto, CancellationToken ct = default);
}
```

---

## Character

### ICharacterService

**Путь:** `Services/Character/ICharacterService.cs`

```csharp
public interface ICharacterService
{
    UniTask<List<CharacterModel>> GetCharactersAsync(CharacterListQuery query = null, CancellationToken ct = default);
    UniTask<CharacterModel> GetCharacterBySlugAsync(string userSlug, string characterSlug, CancellationToken ct = default);
}

public class CharacterListQuery
{
    public string Search { get; set; }
    public List<string> Tags { get; set; }
    public string SortBy { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
    public string Gender { get; set; }
}
```

**Пример:**
```csharp
var characters = await _characterService.GetCharactersAsync(new CharacterListQuery
{
    Search = "warrior",
    Tags = new List<string> { "fantasy" },
    Limit = 10
});
```

---

## Platform

### IFileDialogService

```csharp
public interface IFileDialogService
{
    UniTask<FileDialogResult> OpenImageFileAsync(string title = "Select Image", string filter = "png,jpg,jpeg,webp");
    UniTask<FileDialogResult> SavePngFileAsync(string defaultFilename, byte[] fileData, string title = "Save Image");
}

public class FileDialogResult
{
    public bool Success { get; set; }
    public string FilePath { get; set; }
}
```

### IClipboardService

```csharp
public interface IClipboardService
{
    void SetText(string text);
    string GetText();
}
```

### IDialogService

```csharp
public interface IDialogService
{
    UniTask<bool> ConfirmAsync(string title, string message, string okText = "OK", string cancelText = "Cancel");
}
```

### IFileSystemService

```csharp
public interface IFileSystemService
{
    byte[] ReadAllBytes(string path);
    void WriteAllBytes(string path, byte[] bytes);
    bool FileExists(string path);
}
```

### IResourceService

```csharp
public interface IResourceService
{
    void ReleaseImage(IImageHandle image);
    void ReleaseImageImmediate(IImageHandle image);
}
```

---

## Input

### IInputService

**Путь:** `Services/Input/IInputService.cs`

```csharp
public interface IInputService : IDisposable
{
    // UI Events
    Observable<Unit> OnCancel { get; }
    Observable<Unit> OnSubmit { get; }
    Observable<Vector2> OnNavigate { get; }
    Observable<Vector2> OnPoint { get; }
    Observable<Unit> OnClick { get; }
    Observable<Unit> OnRightClick { get; }
    Observable<Vector2> OnScroll { get; }

    // Player Events
    Observable<Vector2> OnMove { get; }
    Observable<Vector2> OnLook { get; }
    Observable<Unit> OnAttack { get; }
    Observable<Unit> OnInteract { get; }
    Observable<Unit> OnJump { get; }
    Observable<Unit> OnSprint { get; }
    Observable<Unit> OnCrouch { get; }
    Observable<Unit> OnNext { get; }
    Observable<Unit> OnPrevious { get; }

    // Control
    void EnablePlayerActions();
    void DisablePlayerActions();
    void EnableUIActions();
    void DisableUIActions();

    // State
    Vector2 MoveVector { get; }
    Vector2 LookVector { get; }
    Vector2 MouseScreenPosition { get; }
    bool IsAttackPressed { get; }
    bool IsJumpPressed { get; }
    bool IsSprintPressed { get; }
}
```

**Назначение:** Абстракция над Input System с R3 observables.

---

## Audio

### IAudioService

**Путь:** `Services/IAudioService.cs`

```csharp
public interface IAudioService
{
    float MasterVolume { get; }
    float MusicVolume { get; }
    float SfxVolume { get; }
    bool IsMasterMuted { get; }
    bool IsMusicMuted { get; }
    bool IsSfxMuted { get; }
    bool IsMusicPlaying { get; }

    event Action<float> MasterVolumeChanged;
    event Action<float> MusicVolumeChanged;
    event Action<float> SfxVolumeChanged;
    event Action<bool> MasterMutedChanged;
    event Action<bool> MusicMutedChanged;
    event Action<bool> SfxMutedChanged;

    void Play(SoundType sound, bool randomPitch = true, Vector3 position = default, Transform followTarget = null);
    void PlayMusic(SoundType music);
    void StopMusic();
    void StopAll();

    void SetMasterVolume(float value);
    void SetMusicVolume(float value, bool save = true);
    void SetSfxVolume(float value);

    void MuteMaster(bool muted);
    void MuteMusic(bool muted);
    void MuteSfx(bool muted);

    void SaveSettings();
    void LoadSettings();
    void ResetToDefaults();
}
```

---

## ImageGen

### IGeminiImageService

**Путь:** `Services/ImageGen/IGeminiImageService.cs`

```csharp
public interface IGeminiImageService
{
    UniTask<ImageGenResult> GenerateImageAsync(
        string prompt,
        ImageGenSettings settings = null,
        CancellationToken ct = default);

    UniTask<ImageGenResult> EditImageAsync(
        string prompt,
        IImageHandle sourceImage,
        ImageGenSettings settings = null,
        CancellationToken ct = default);

    UniTask<ImageGenResult> EditImageAsync(
        string prompt,
        string base64Image,
        string mimeType = "image/png",
        ImageGenSettings settings = null,
        CancellationToken ct = default);

    UniTask<ImageGenResult> ChatAsync(
        string prompt,
        List<IImageHandle> attachedImages,
        List<ImageGenMessage> history,
        ImageGenSettings settings = null,
        CancellationToken ct = default);
}
```

---

## UIGeneration

### IUIGenerationPipeline

```csharp
public interface IUIGenerationPipeline
{
    UniTask<UIGenerationResult> GenerateAsync(
        UIGenerationRequest request,
        IProgress<UIGenerationProgress> progress = null,
        CancellationToken ct = default);
    
    UniTask<List<UIGenerationResult>> GenerateBatchAsync(
        List<UIGenerationRequest> requests,
        IProgress<UIBatchGenerationProgress> progress = null,
        CancellationToken ct = default);
}

public enum UIGenerationStage
{
    BuildingPrompt,
    GeneratingImage,
    RemovingBackground,
    PostProcessing,
    Saving,
    Complete
}
```

### IUIPromptBuilder

```csharp
public interface IUIPromptBuilder
{
    List<UIPromptBlock> GetSchemaForElementType(UIElementType elementType);
    List<UIStylePreset> GetAvailableStylePresets();
    void ApplyStylePreset(List<UIPromptBlock> blocks, UIStylePreset preset);
    (bool IsValid, List<string> Errors) ValidateRequest(UIGenerationRequest request);
    string BuildPromptForApi(UIGenerationRequest request);
}
```

### IBackgroundRemovalService

```csharp
public interface IBackgroundRemovalService
{
    UniTask<bool> IsAvailableAsync();
    
    UniTask<IImageHandle> RemoveBackgroundAsync(
        IImageHandle source, 
        bool autoTrim = true,
        int padding = 4,
        CancellationToken ct = default);
    
    UniTask<List<IImageHandle>> ProcessBatchAsync(
        List<IImageHandle> sources,
        bool autoTrim = true,
        int padding = 4,
        IProgress<float> progress = null,
        CancellationToken ct = default);
}
```

---

## Logging

### ILogger

**Путь:** `Services/ILogger.cs`

```csharp
public interface ILogger
{
    LogLevel MinimumLevel { get; set; }
    void Log(LogLevel level, string messageTemplate, params object[] args);
    void Debug(string messageTemplate, params object[] args);
    void Info(string messageTemplate, params object[] args);
    void Warn(string messageTemplate, params object[] args);
    void Error(string messageTemplate, params object[] args);
    void Error(Exception exception, string messageTemplate = "", params object[] args);
}

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    None = 99
}
```

### ILogFormatter

```csharp
public interface ILogFormatter
{
    string Format(LogLevel level, string template, params object[] args);
    string Render(string template, params object[] args);
    string ColorizeValue(object value);
}
```

### ILogInterceptor

```csharp
public interface ILogInterceptor
{
    void Ensure();
}
```

---

## Infrastructure

### StateEnum

```csharp
public enum StateEnum
{
    Login,
    MenuSelection,
    Chat,
    ImageGen,
    StandaloneWorkflow,
    Main
}
```

### ILoadSceneService

```csharp
public interface ILoadSceneService
{
    UniTask LoadAsync(SceneLoadPayload payload, CancellationToken cancellationToken = default);
}
```

### IDataPreloadService

```csharp
public interface IDataPreloadService
{
    UniTask PreloadDataAsync(IProgress<float> progress = null, CancellationToken cancellationToken = default);
}
```

### IPrefsService

```csharp
public interface IPrefsService
{
    void Save<T>(string key, T data);
    T Load<T>(string key);
    bool HasKey(string key);
    void Delete(string key);
}
```

---

## Media

### IImageCodec

```csharp
public interface IImageCodec
{
    byte[] Encode(IImageHandle image, ImageFormat format);
    IImageHandle Decode(byte[] data, ImageFormat format);
}
```
