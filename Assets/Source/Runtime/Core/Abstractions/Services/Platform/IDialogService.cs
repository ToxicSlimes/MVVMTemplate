using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services.Platform
{
    public interface IDialogService
    {
        UniTask<bool> ConfirmAsync(string title, string message, string okText = "OK", string cancelText = "Cancel");
    }
}
