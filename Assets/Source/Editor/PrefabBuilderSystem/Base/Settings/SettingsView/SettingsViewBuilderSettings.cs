#if UNITY_EDITOR
using UnityEngine;
using Core.Presentation.UI.DesignSystem;
using Core.Editor.PrefabBuilderSystem.Settings;
using Core.Editor.PrefabBuilderSystem.Theming;

namespace Core.Editor.PrefabBuilderSystem.SettingsView
{
    [CreateAssetMenu(fileName = "SettingsViewBuilderSettings", menuName = "PrefabBuilder/Settings/SettingsView")]
    public class SettingsViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Modal Window")]
        public float ModalWidth = 900f;
        public float ModalHeight = 650f;
        public float CornerRadius = 10f;

        [Header("Header")]
        public float HeaderHeight = 50f;
        public float TitleFontSize = 24f;
        public float CloseButtonSize = 40f;

        [Header("Tab Bar")]
        public float TabBarHeight = 44f;
        public float TabButtonMinWidth = 90f;
        public float TabButtonHeight = 38f;
        public float TabSpacing = 4f;

        [Header("Content Area")]
        public float ContentPadding = 15f;
        public float RowHeight = 44f;
        public float RowSpacing = 8f;
        public float LabelWidth = 180f;

        [Header("Slider")]
        public float SliderHeight = 30f;
        public float SliderHandleSize = 24f;

        [Header("Toggle")]
        public float ToggleSize = 24f;

        [Header("Dropdown")]
        public float DropdownWidth = 250f;
        public float DropdownHeight = 36f;

        [Header("Footer")]
        public float FooterHeight = 60f;
        public float FooterButtonWidth = 120f;
        public float FooterButtonHeight = 40f;
        public float FooterButtonSpacing = 20f;

        [Header("Font Sizes")]
        public float LabelFontSize = 18f;
        public float ValueFontSize = 16f;
        public float ButtonFontSize = 18f;
        public float TabFontSize = 16f;

        [Header("Colors - Background")]
        public Color DimBackgroundColor = new(0.024f, 0.043f, 0.102f, 0.7f);
        public Color ModalBackgroundColor = new(0.051f, 0.090f, 0.188f, 1f);
        public Color HeaderColor = new(0.086f, 0.149f, 0.290f, 0.8f);
        public Color TabBarColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color ContentColor = new(0.051f, 0.090f, 0.188f, 1f);
        public Color FooterColor = new(0.086f, 0.149f, 0.290f, 0.8f);

        [Header("Colors - Tab")]
        public Color TabNormalColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color TabSelectedColor = new(0.184f, 0.816f, 0.451f, 1f);
        public Color TabHoverColor = new(0.086f, 0.149f, 0.290f, 0.6f);

        [Header("Colors - Controls")]
        public Color SliderFillColor = new(0.184f, 0.816f, 0.451f, 1f);
        public Color SliderBackgroundColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color ToggleCheckColor = new(0.184f, 0.816f, 0.451f, 1f);
        public Color DropdownColor = new(0.086f, 0.149f, 0.290f, 0.4f);

        [Header("Colors - Buttons")]
        public Color ApplyButtonColor = new(0.047f, 0.478f, 0.294f, 1f);
        public Color ResetButtonColor = new(1f, 0.827f, 0.306f, 1f);
        public Color CancelButtonColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color CloseButtonColor = new(0.902f, 0.224f, 0.275f, 0.45f);

        [Header("Colors - Text")]
        public Color TextColor = new(0.973f, 0.980f, 0.988f, 1f);
        public Color LabelColor = new(0.639f, 0.706f, 0.800f, 1f);
        public Color ValueColor = new(0.973f, 0.980f, 0.988f, 1f);
        public Color ButtonTextColor = new(0.973f, 0.980f, 0.988f, 1f);

        [Header("Colors - Misc")]
        public Color DividerColor = new(0.639f, 0.706f, 0.800f, 0.12f);
        public Color OutlineColor = new(0.639f, 0.706f, 0.800f, 0.12f);

        /// <summary>
        /// Палитра проекта. Цвета этого экрана стояли числами и темы не знали: после перекраски
        /// интерфейса он остался бы серым от прежнего проекта.
        /// </summary>
        private static ColorPalette Palette => DesignSystemProvider.Colors;

        /// <summary>
        /// Затемнение под модальным окном. От фона ночи, а не чёрное: чистый чёрный на тёмно-синих
        /// панелях читается дырой, а не тенью.
        /// </summary>
        private static Color Dim(float alpha)
        {
            var color = Palette.deepSpace;
            color.a = alpha;

            return color;
        }


        public override void ResetToDefaults()
        {
            ModalWidth = 900f;
            ModalHeight = 650f;
            CornerRadius = 10f;

            HeaderHeight = 50f;
            TitleFontSize = 24f;
            CloseButtonSize = 40f;

            TabBarHeight = 44f;
            TabButtonMinWidth = 90f;
            TabButtonHeight = 38f;
            TabSpacing = 4f;

            ContentPadding = 15f;
            RowHeight = 44f;
            RowSpacing = 8f;
            LabelWidth = 180f;

            SliderHeight = 30f;
            SliderHandleSize = 24f;

            ToggleSize = 24f;

            DropdownWidth = 250f;
            DropdownHeight = 36f;

            FooterHeight = 60f;
            FooterButtonWidth = 120f;
            FooterButtonHeight = 40f;
            FooterButtonSpacing = 20f;

            LabelFontSize = 18f;
            ValueFontSize = 16f;
            ButtonFontSize = 18f;
            TabFontSize = 16f;

            DimBackgroundColor = Dim(0.7f);
            ModalBackgroundColor = Palette.darkSurface;
            HeaderColor = Palette.elevatedSurface;
            TabBarColor = Palette.cardBackground;
            ContentColor = Palette.darkSurface;
            FooterColor = Palette.elevatedSurface;

            TabNormalColor = Palette.cardBackground;
            TabSelectedColor = Palette.indigo;
            TabHoverColor = Palette.cardHoverBackground;

            SliderFillColor = Palette.indigo;
            SliderBackgroundColor = Palette.cardBackground;
            ToggleCheckColor = Palette.indigo;
            DropdownColor = Palette.cardBackground;

            ApplyButtonColor = Palette.primaryButtonGradientStart;
            ResetButtonColor = Palette.purple;
            CancelButtonColor = Palette.cardBackground;
            CloseButtonColor = Palette.errorBorder;

            TextColor = Palette.textPrimary;
            LabelColor = Palette.textSecondary;
            ValueColor = Palette.textPrimary;
            ButtonTextColor = Palette.textPrimary;

            DividerColor = Palette.cardBorder;
            OutlineColor = Palette.cardBorder;
        }
    }
}
#endif
