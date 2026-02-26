using System;
using Core.Abstractions.MVVM.Models;

namespace Core.Abstractions.MVVM.Models.Login
{
    public interface ILoginModel : IModel
    {
        event Action<string> EmailChanged;
        event Action<bool> RememberMeChanged;
        event Action<string> StatusTextChanged;
        event Action<bool> IsLoadingChanged;
        event Action<bool> IsAuthorizedChanged;

        string Email { get; set; }
        bool RememberMe { get; set; }
        string StatusText { get; set; }
        bool IsLoading { get; set; }
        bool IsAuthorized { get; set; }
    }
}
