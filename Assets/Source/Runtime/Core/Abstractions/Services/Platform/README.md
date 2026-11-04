# Platform Service Abstractions

Контракты для платформенных сервисов (файловая система, диалоги, буфер обмена).

## Типы

| Тип | Описание |
|-----|----------|
| `IFileDialogService` | Диалоги выбора файлов (Open/Save) |
| `IFileSystemService` | Операции с файловой системой |
| `IClipboardService` | Буфер обмена |
| `IDialogService` | Системные диалоговые окна |
| `IResourceService` | Доступ к Unity Resources |
| `IPlatformService` | Платформа (`GamePlatform`), тип устройства (`DeviceKind`), язык, сигналы ready/gameplay |

## IFileDialogService

```csharp
public interface IFileDialogService
{
    UniTask<string> OpenFileAsync(string title, string[] extensions);
    UniTask<string[]> OpenFilesAsync(string title, string[] extensions);
    UniTask<string> SaveFileAsync(string title, string defaultName, string[] extensions);
    UniTask<string> OpenFolderAsync(string title);
}
```

## IFileSystemService

```csharp
public interface IFileSystemService
{
    UniTask<byte[]> ReadAllBytesAsync(string path);
    UniTask WriteAllBytesAsync(string path, byte[] data);
    UniTask<string> ReadAllTextAsync(string path);
    UniTask WriteAllTextAsync(string path, string content);
    bool Exists(string path);
    void Delete(string path);
    string[] GetFiles(string directory, string pattern);
}
```

## IClipboardService

```csharp
public interface IClipboardService
{
    void SetText(string text);
    string GetText();
    void SetImage(Texture2D texture);
}
```

## IDialogService

```csharp
public interface IDialogService
{
    UniTask<bool> ConfirmAsync(string title, string message);
    UniTask AlertAsync(string title, string message);
    UniTask<string> PromptAsync(string title, string message, string defaultValue = "");
}
```

## IResourceService

```csharp
public interface IResourceService
{
    T Load<T>(string path) where T : Object;
    UniTask<T> LoadAsync<T>(string path) where T : Object;
}
```

## IPlatformService

```csharp
public interface IPlatformService
{
    GamePlatform Platform { get; }
    DeviceKind Device { get; }
    string Language { get; }
    void MarkReady();
    void SetGameplayActive(bool active);
}
```

`UnityPlatformService` (Infrastructure) не зарегистрирован по умолчанию: игра регистрирует реализацию сама. `IAdsService` (`Services/Ads`) по умолчанию `NullAdsService`, игра переопределяет.

## Использование

```csharp
// Выбор файла
var path = await _fileDialog.OpenFileAsync("Select Image", new[] { "png", "jpg" });
if (!string.IsNullOrEmpty(path))
{
    var data = await _fileSystem.ReadAllBytesAsync(path);
}

// Буфер обмена
_clipboard.SetText("Copied text!");

// Диалог подтверждения
if (await _dialog.ConfirmAsync("Delete?", "Are you sure?"))
{
    // delete...
}
```

## Реализация

`Infrastructure/Services/Platform/`
