using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services.Platform
{
    public class FileDialogResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; }
    }

    public interface IFileDialogService
    {
        UniTask<FileDialogResult> OpenImageFileAsync(string title = "Select Image", string filter = "png,jpg,jpeg,webp");
        UniTask<FileDialogResult> SavePngFileAsync(string defaultFilename, byte[] fileData, string title = "Save Image");
    }
}
