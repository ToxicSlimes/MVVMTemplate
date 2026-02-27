using System;

namespace Core.Abstractions.MVVM.Models.MainMenu
{
    public interface IMainMenuModel : IDisposable
    {
        bool IsVisible { get; set; }
        event Action<bool> IsVisibleChanged;
    }
}
