using Cysharp.Threading.Tasks;
using Core.Abstractions.Services.Platform;

namespace Core.Infrastructure.Services.Platform
{
    public class DialogService : IDialogService
    {
        public UniTask<bool> ConfirmAsync(string title, string message, string okText = "OK", string cancelText = "Cancel")
        {
            return UniTask.FromResult(true);
        }
    }
}
