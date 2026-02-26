# Network Layer

Утилиты для работы с сетью — API endpoints и HTTP response wrapper.

**Путь:** `Data/Network/`  
**Namespace:** `Core.Data.Network`

---

## ApiEndpoints

**Путь:** `Network/ApiEndpoints.cs`

Статический класс с константами API endpoints. Все endpoints организованы по категориям.

### Категории

#### App

```csharp
public static class App
{
    public const string Root = "";
    public const string Protected = "protected";
    public const string Health = "health";
}
```

#### Characters

```csharp
public static class Characters
{
    public const string Base = "characters";
    public const string ById = "characters/{0}";                    // GET /characters/{id}
    public const string BySlug = "characters/{0}/{1}";              // GET /characters/{userSlug}/{slug}
    public const string Report = "characters/{0}/report";           // POST
    public const string Like = "characters/{0}/like";               // POST
    public const string Dislike = "characters/{0}/dislike";         // POST
    public const string Favorite = "characters/{0}/favorite";       // POST
    public const string Photo = "characters/{0}/photo";             // POST
    public const string Similar = "characters/{0}/similar";         // GET
    public const string LoreEntries = "characters/{0}/lore-entries"; // GET
    public const string LoreStats = "characters/{0}/lore-stats";    // GET
}
```

#### Chats

```csharp
public static class Chats
{
    public const string Base = "chats";                             // GET (list), POST (create)
    public const string ById = "chats/{0}";                         // GET, PATCH, DELETE
    public const string Stream = "chats/stream";                    // POST (SSE)
    public const string Generate = "chats/generate";                // POST
    public const string Regenerate = "chats/regenerate";            // POST
    public const string EstimateCost = "chats/estimate-cost";       // POST
    public const string Starter = "chats/starter/{0}";              // GET
    public const string Pin = "chats/{0}/pin";                      // POST
    public const string Clone = "chats/{0}/clone";                  // POST
    public const string Encrypt = "chats/{0}/encrypt";              // POST
    public const string Decrypt = "chats/{0}/decrypt";              // POST
    public const string ChangePassword = "chats/{0}/change-password"; // POST
    public const string BulkChangePassword = "chats/bulk/change-password"; // POST
    public const string ExtensionsUi = "chats/{0}/extensions/ui";   // GET
    public const string RegenerateMessage = "chats/messages/{0}/regenerate"; // POST
}
```

#### Workflow

```csharp
public static class Workflow
{
    public const string Generate = "workflow/generate";                         // POST
    public const string GenerateReset = "workflow/generate/reset";              // POST
    public const string Entrypoints = "workflow/entrypoints";                   // GET
    public const string WorkflowEntrypoints = "workflow/{0}/entrypoints";       // GET
    public const string Trigger = "workflow/{0}/trigger";                       // POST
    public const string RunEntrypoint = "workflow/run-entrypoint";              // POST
    public const string StandaloneExecute = "workflow/standalone/{0}/execute";  // POST
    public const string StandaloneUi = "workflow/standalone/{0}/ui";            // GET
    public const string StandaloneUiAction = "workflow/standalone/{0}/ui/action"; // POST
}
```

#### WorkflowCrud

```csharp
public static class WorkflowCrud
{
    public const string Base = "workflow-crud";                                 // GET (list), POST (create)
    public const string ById = "workflow-crud/{0}";                             // GET, PATCH, DELETE
    public const string Helper = "workflow-crud/helper";                        // GET
    public const string Duplicate = "workflow-crud/{0}/duplicate";              // POST
    public const string NewVersion = "workflow-crud/{0}/new-version";           // POST
    public const string Test = "workflow-crud/{0}/test";                        // POST
    public const string Versions = "workflow-crud/{0}/versions";                // GET
    public const string Runs = "workflow-crud/{0}/runs";                        // GET
    public const string RunById = "workflow-crud/{0}/runs/{1}";                 // GET
    public const string ChatActive = "workflow-crud/chat/{0}/active";           // GET
    public const string ChatAttach = "workflow-crud/chat/{0}/attach";           // POST
    public const string ChatDetach = "workflow-crud/chat/{0}/detach";           // POST
    public const string ChatToggleActive = "workflow-crud/chat/{0}/toggle-active"; // POST
    public const string ChatWorkflowStorage = "workflow-crud/chat/{0}/workflow/{1}/storage"; // GET, POST
}
```

#### Messages, Models, Personas, Profile, Finance, etc.

Полный список в файле ApiEndpoints.cs — всего **20+ категорий**.

### Использование

```csharp
// Простой endpoint
var url = ApiEndpoints.Characters.Base;
var response = await apiClient.Get<CharacterListResponseDto>(url);

// Endpoint с параметрами (string.Format)
var characterId = "char_123";
var url = string.Format(ApiEndpoints.Characters.ById, characterId);
var response = await apiClient.Get<CharacterResponseDto>(url);

// Несколько параметров
var userSlug = "john";
var slug = "warrior";
var url = string.Format(ApiEndpoints.Characters.BySlug, userSlug, slug);
```

---

## HttpResponse<T>

**Путь:** `Network/HttpResponse.cs`

Generic wrapper для HTTP ответов.

### Поля

| Поле | Тип | Описание |
|------|-----|----------|
| IsSuccess | `bool` | Успешен ли запрос |
| StatusCode | `long` | HTTP status code |
| Data | `T` | Десериализованные данные (null при ошибке) |
| Error | `string` | Сообщение об ошибке (null при успехе) |
| Headers | `Dictionary<string, string>` | HTTP headers |

### Конструктор

```csharp
public HttpResponse(
    bool isSuccess, 
    long statusCode, 
    T data, 
    string error, 
    Dictionary<string, string> headers)
```

### Factory Methods

#### Success

```csharp
public static HttpResponse<T> Success(
    T data, 
    long statusCode, 
    Dictionary<string, string> headers = null)
```

#### Failure

```csharp
public static HttpResponse<T> Failure(
    string error, 
    long statusCode, 
    Dictionary<string, string> headers = null)
```

### Использование

#### API Client

```csharp
public async Task<HttpResponse<CharacterModel>> GetCharacter(string id)
{
    try
    {
        var url = string.Format(ApiEndpoints.Characters.ById, id);
        var dto = await _httpClient.Get<CharacterResponseDto>(url);
        
        if (dto != null)
        {
            var model = CharacterMapper.ToModel(dto);
            return HttpResponse<CharacterModel>.Success(model, 200);
        }
        
        return HttpResponse<CharacterModel>.Failure("Character not found", 404);
    }
    catch (HttpException ex)
    {
        return HttpResponse<CharacterModel>.Failure(ex.Message, ex.StatusCode);
    }
}
```

#### Service Layer

```csharp
public async Task<CharacterModel> LoadCharacter(string id)
{
    var response = await _apiClient.GetCharacter(id);
    
    if (response.IsSuccess)
    {
        return response.Data;
    }
    
    // Error handling
    if (response.StatusCode == 404)
    {
        throw new CharacterNotFoundException(id);
    }
    else if (response.StatusCode >= 500)
    {
        throw new ServerException(response.Error);
    }
    else
    {
        throw new ApiException(response.Error);
    }
}
```

#### View Model

```csharp
public async void LoadCharacter(string id)
{
    IsLoading = true;
    
    var response = await _characterService.GetCharacter(id);
    
    IsLoading = false;
    
    if (response.IsSuccess)
    {
        Character = response.Data;
    }
    else
    {
        // Show error
        ShowError(response.Error);
    }
}
```

---

## Архитектура

### Typed Endpoints

Вместо magic strings используй константы:

```csharp
// ❌ Bad
var url = "characters/" + id;

// ✅ Good
var url = string.Format(ApiEndpoints.Characters.ById, id);
```

### Error Handling Strategy

```csharp
public class ApiClient
{
    private async Task<HttpResponse<T>> ExecuteRequest<T>(string url)
    {
        try
        {
            var result = await _httpClient.Get<T>(url);
            return HttpResponse<T>.Success(result, 200);
        }
        catch (UnityWebRequestException ex) when (ex.ResponseCode == 401)
        {
            // Token expired → refresh
            await _authService.RefreshToken();
            return await ExecuteRequest<T>(url);
        }
        catch (UnityWebRequestException ex) when (ex.ResponseCode >= 500)
        {
            // Server error → retry with backoff
            await Task.Delay(1000);
            return await ExecuteRequest<T>(url);
        }
        catch (Exception ex)
        {
            return HttpResponse<T>.Failure(ex.Message, 0);
        }
    }
}
```

### Response Headers

```csharp
var response = await apiClient.GetCharacter(id);

if (response.IsSuccess && response.Headers != null)
{
    if (response.Headers.TryGetValue("X-RateLimit-Remaining", out var remaining))
    {
        Debug.Log($"API calls remaining: {remaining}");
    }
}
```
