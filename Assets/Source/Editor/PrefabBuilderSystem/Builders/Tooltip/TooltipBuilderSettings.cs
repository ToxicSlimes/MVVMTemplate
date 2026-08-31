#if UNITY_EDITOR
using Core.Configs.DesignSystem;
using Core.Editor.PrefabBuilderSystem.Settings;
using Core.Editor.PrefabBuilderSystem.Theming;
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
        public Color BackgroundColor = new(0.051f, 0.090f, 0.188f, 1f);
        public Color TitleColor = new(0.973f, 0.980f, 0.988f, 1f);
        public Color DescriptionColor = new(0.639f, 0.706f, 0.800f, 1f);
        public Color BorderColor = new(0.639f, 0.706f, 0.800f, 0.12f);

        [Header("Animation")]
        public float BorderWidth = 1f;
        public float CornerRadius = 6f;

        /// <summary>
        /// Палитра проекта. Цвета этих экранов раньше стояли числами и темы не знали вовсе —
        /// после перекраски интерфейса они остались бы серыми от прежнего проекта.
        /// </summary>
        private static ColorPalette Palette => DesignSystemProvider.Colors;

        /// <summary>
        /// Затемнение под модальным окном. Берётся от фона ночи, а не чёрным: чистый чёрный на
        /// тёмно-синих панелях читается дырой, а не тенью.
        /// </summary>
        private static Color Dim(float alpha)
        {
            var color = Palette.deepSpace;
            color.a = alpha;

            return color;
        }


        public override void ResetToDefaults()
        {
            MaxWidth = 280f;
            Padding = 12f;
            Spacing = 4f;
            TitleFontSize = 14f;
            DescriptionFontSize = 12f;
            BackgroundColor = Palette.darkSurface;
            TitleColor = Palette.textPrimary;
            DescriptionColor = Palette.textSecondary;
            BorderColor = Palette.cardBorder;
            BorderWidth = 1f;
            CornerRadius = 6f;
        }
    }
}
#endif
