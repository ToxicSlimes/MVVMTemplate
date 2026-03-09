using Core.Data.Domain.Enums;
using UnityEngine;

namespace Core.Abstractions.UI
{
    public interface IThemeProvider
    {
        Color GetColor(ThemeTag tag);
        Color GetHoverColor(ThemeTag tag);
        Color GetPressedColor(ThemeTag tag);
        Color GetDisabledColor(ThemeTag tag);
        Color GetTextColor(ThemeTag tag);
    }
}
