using System;
using UnityEngine;

namespace Core.Abstractions.Attributes
{
    // NOTE: Cannot wrap in #if UNITY_EDITOR — attribute is used on runtime configs
    // (TypographySettings, ProceduralUiConfig). ColorPresets.GetColors() is only called from Editor code.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class ConfigEntryStyleAttribute : Attribute
    {
        public string IconEmoji { get; set; }
        public ColorPreset ColorPreset { get; set; } = ColorPreset.Blue;
        public float BorderWidth { get; set; } = 4f;
        public bool ShowStatusIcon { get; set; } = true;
        public string MissingRefFieldName { get; set; }

        public ConfigEntryStyleAttribute()
        {
        }

        public ConfigEntryStyleAttribute(ColorPreset colorPreset, string iconEmoji = null)
        {
            ColorPreset = colorPreset;
            IconEmoji = iconEmoji;
        }
    }

    public enum ColorPreset
    {
        Blue,
        Green,
        Orange,
        Purple,
        Red,
        Pink,
        Cyan,
        Yellow
    }

    public static class ColorPresets
    {
        public static (Color background, Color border) GetColors(ColorPreset preset, bool isMissing)
        {
            if (isMissing)
            {
                return (
                    new Color(1f, 0.3f, 0.3f, 0.2f),
                    new Color(1f, 0f, 0f, 0.8f)
                );
            }

            return preset switch
            {
                ColorPreset.Blue => (new Color(0.2f, 0.6f, 1f, 0.12f), new Color(0.2f, 0.6f, 1f, 0.5f)),
                ColorPreset.Green => (new Color(0.2f, 1f, 0.4f, 0.12f), new Color(0.2f, 1f, 0.4f, 0.5f)),
                ColorPreset.Orange => (new Color(1f, 0.6f, 0.2f, 0.12f), new Color(1f, 0.6f, 0.2f, 0.5f)),
                ColorPreset.Purple => (new Color(0.7f, 0.3f, 1f, 0.12f), new Color(0.7f, 0.3f, 1f, 0.5f)),
                ColorPreset.Red => (new Color(1f, 0.3f, 0.3f, 0.12f), new Color(1f, 0.3f, 0.3f, 0.5f)),
                ColorPreset.Pink => (new Color(1f, 0.4f, 0.7f, 0.12f), new Color(1f, 0.4f, 0.7f, 0.5f)),
                ColorPreset.Cyan => (new Color(0.2f, 0.9f, 1f, 0.12f), new Color(0.2f, 0.9f, 1f, 0.5f)),
                ColorPreset.Yellow => (new Color(1f, 0.9f, 0.2f, 0.12f), new Color(1f, 0.9f, 0.2f, 0.5f)),
                _ => (new Color(0.2f, 0.6f, 1f, 0.12f), new Color(0.2f, 0.6f, 1f, 0.5f))
            };
        }
    }
}
