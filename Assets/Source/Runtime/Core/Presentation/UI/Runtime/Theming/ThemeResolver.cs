using Core.Abstractions.UI;
using Core.Presentation.UI.DesignSystem;
using Core.Domain.Enums;
using UnityEngine;

namespace Core.Presentation.UI.Theming
{
  public class ThemeResolver : IThemeProvider
  {
    private readonly DesignSystemSettings _settings;

    public ThemeResolver(DesignSystemSettings settings)
    {
      _settings = settings;
    }

    public DesignSystemSettings Settings => _settings;

    public Color GetColor(ThemeTag tag) => tag switch
    {
      ThemeTag.Primary => _settings.colors != null ? _settings.colors.indigo : Color.white,
      ThemeTag.Secondary => _settings.colors != null ? _settings.colors.purple : Color.gray,
      ThemeTag.Accent => _settings.colors != null ? _settings.colors.cyan : Color.cyan,
      ThemeTag.Danger => _settings.colors != null ? _settings.colors.errorText : Color.red,
      ThemeTag.Success => _settings.colors != null ? new Color(0.2f, 0.8f, 0.3f, 1f) : Color.green,
      ThemeTag.Ghost => Color.clear,
      _ => Color.white
    };

    public Color GetHoverColor(ThemeTag tag)
    {
      var baseColor = GetColor(tag);
      return tag == ThemeTag.Ghost
        ? new Color(1f, 1f, 1f, 0.1f)
        : Color.Lerp(baseColor, Color.white, 0.15f);
    }

    public Color GetPressedColor(ThemeTag tag)
    {
      var baseColor = GetColor(tag);
      return Color.Lerp(baseColor, Color.black, 0.15f);
    }

    public Color GetDisabledColor(ThemeTag tag)
    {
      var baseColor = GetColor(tag);
      baseColor.a = 0.4f;
      return baseColor;
    }

    public Color GetTextColor(ThemeTag tag)
    {
      if (_settings.colors == null) return Color.white;
      return tag == ThemeTag.Ghost ? _settings.colors.textPrimary : Color.white;
    }
  }
}