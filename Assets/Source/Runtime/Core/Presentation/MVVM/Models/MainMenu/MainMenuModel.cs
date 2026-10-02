using System;
using Core.Abstractions.MVVM.Models.MainMenu;

namespace Core.Presentation.MVVM.Models.MainMenu
{
    public class MainMenuModel : IMainMenuModel
    {
        private bool _isVisible;

        public event Action<bool> IsVisibleChanged;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                IsVisibleChanged?.Invoke(_isVisible);
            }
        }

        public void Dispose()
        {
            IsVisibleChanged = null;
        }
    }
}
