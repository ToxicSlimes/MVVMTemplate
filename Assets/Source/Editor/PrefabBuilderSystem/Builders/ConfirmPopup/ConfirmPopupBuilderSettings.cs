#if UNITY_EDITOR
using Core.Presentation.UI.DesignSystem;
using Core.Editor.PrefabBuilderSystem.Settings;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.ConfirmPopup
{
    [CreateAssetMenu(fileName = "ConfirmPopupBuilderSettings", menuName = "PrefabBuilder/Settings/ConfirmPopup")]
    public class ConfirmPopupBuilderSettings : BuilderSettingsBase
    {
        [Header("Size")]
        public float ModalWidth = 380f;
        public float ModalHeight = 220f;
        public float Padding = 20f;

        [Header("Typography")]
        public float TitleFontSize = 20f;
        public float MessageFontSize = 14f;
        public float ButtonFontSize = 16f;

        [Header("Colors")]
        public Color DimColor = new(0.024f, 0.043f, 0.102f, 0.6f);
        public Color BackgroundColor = new(0.051f, 0.090f, 0.188f, 1f);
        public Color HeaderColor = new(0.086f, 0.149f, 0.290f, 0.8f);
        public Color TitleColor = new(0.973f, 0.980f, 0.988f, 1f);
        public Color MessageColor = new(0.639f, 0.706f, 0.800f, 1f);
        public Color ConfirmButtonColor = new(0.902f, 0.224f, 0.275f, 0.45f);
        public Color CancelButtonColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color ButtonTextColor = new(0.973f, 0.980f, 0.988f, 1f);

        [Header("Buttons")]
        public float ButtonHeight = 40f;
        public float ButtonSpacing = 12f;

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
            ModalWidth = 380f;
            ModalHeight = 220f;
            Padding = 20f;
            TitleFontSize = 20f;
            MessageFontSize = 14f;
            ButtonFontSize = 16f;
            DimColor = Dim(0.6f);
            BackgroundColor = Palette.darkSurface;
            HeaderColor = Palette.elevatedSurface;
            TitleColor = Palette.textPrimary;
            MessageColor = Palette.textSecondary;
            ConfirmButtonColor = Palette.errorBorder;
            CancelButtonColor = Palette.cardBackground;
            ButtonTextColor = Palette.textPrimary;
            ButtonHeight = 40f;
            ButtonSpacing = 12f;
        }
    }
}
#endif
