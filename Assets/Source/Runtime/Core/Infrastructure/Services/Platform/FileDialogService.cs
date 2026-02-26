using Cysharp.Threading.Tasks;
using Core.Abstractions.Services.Platform;

namespace Core.Infrastructure.Services.Platform
{
    public class FileDialogService : IFileDialogService
    {
        public UniTask<FileDialogResult> OpenImageFileAsync(string title = "Select Image", string filter = "png,jpg,jpeg,webp")
        {
            return UniTask.FromResult(new FileDialogResult
            {
                Success = false,
                FilePath = string.Empty
            });
        }

        public UniTask<FileDialogResult> SavePngFileAsync(string defaultFilename, byte[] fileData, string title = "Save Image")
        {
            return UniTask.FromResult(new FileDialogResult
            {
                Success = false,
                FilePath = string.Empty
            });
        }
    }
}
