#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Tooltip
{
    [CreateAssetMenu(fileName = "TooltipBuilderSettings", menuName = "PrefabBuilder/Settings/Tooltip")]
    public class TooltipBuilderSettings : BuilderSettingsBase
    {
        [Header("Size")]
        public float MaxWidth = 280f;
        public float Padding = 12f;
        public float Spacing = 4f;

        [Header("Typography")]
        public float TitleFontSize = 14f;
        public float DescriptionFontSize = 12f;

        [Header("Colors")]
        public Color BackgroundColor = new(0.12f, 0.12f, 0.14f, 0.95f);
        public Color TitleColor = new(1f, 1f, 1f, 1f);
        public Color DescriptionColor = new(0.7f, 0.7f, 0.75f, 1f);
        public Color BorderColor = new(0.3f, 0.3f, 0.35f, 1f);

        [Header("Animation")]
        public float BorderWidth = 1f;
        public float CornerRadius = 6f;

        public override void ResetToDefaults()
        {
            MaxWidth = 280f;
            Padding = 12f;
            Spacing = 4f;
            TitleFontSize = 14f;
            DescriptionFontSize = 12f;
            BackgroundColor = new Color(0.12f, 0.12f, 0.14f, 0.95f);
            TitleColor = new Color(1f, 1f, 1f, 1f);
            DescriptionColor = new Color(0.7f, 0.7f, 0.75f, 1f);
            BorderColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            BorderWidth = 1f;
            CornerRadius = 6f;
        }
    }
}
#endif
