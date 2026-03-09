using Core.Data.Domain.Enums;

namespace Core.Abstractions.UI
{
    public interface IUIControlService
    {
        IThemeProvider CurrentTheme { get; }
        void Register(IThemeable control);
        void Unregister(IThemeable control);
        void ApplyThemeToAll();
        void PlaySound(SoundType sound);
    }
}
