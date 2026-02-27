#if UNITY_EDITOR
using UnityEngine;
using Core.Editor.PrefabBuilderSystem.Settings;

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
        public Color DimBackgroundColor = new(0f, 0f, 0f, 0.7f);
        public Color ModalBackgroundColor = new(0.15f, 0.15f, 0.2f, 1f);
        public Color HeaderColor = new(0.1f, 0.1f, 0.15f, 1f);
        public Color TabBarColor = new(0.12f, 0.12f, 0.17f, 1f);
        public Color ContentColor = new(0.18f, 0.18f, 0.23f, 1f);
        public Color FooterColor = new(0.1f, 0.1f, 0.15f, 1f);

        [Header("Colors - Tab")]
        public Color TabNormalColor = new(0.25f, 0.25f, 0.3f, 1f);
        public Color TabSelectedColor = new(0.4f, 0.6f, 1f, 1f);
        public Color TabHoverColor = new(0.35f, 0.35f, 0.4f, 1f);

        [Header("Colors - Controls")]
        public Color SliderFillColor = new(0.4f, 0.6f, 1f, 1f);
        public Color SliderBackgroundColor = new(0.3f, 0.3f, 0.35f, 1f);
        public Color ToggleCheckColor = new(0.4f, 0.8f, 0.4f, 1f);
        public Color DropdownColor = new(0.25f, 0.25f, 0.3f, 1f);

        [Header("Colors - Buttons")]
        public Color ApplyButtonColor = new(0.2f, 0.7f, 0.3f, 1f);
        public Color ResetButtonColor = new(0.7f, 0.5f, 0.2f, 1f);
        public Color CancelButtonColor = new(0.5f, 0.5f, 0.55f, 1f);
        public Color CloseButtonColor = new(0.7f, 0.2f, 0.2f, 1f);

        [Header("Colors - Text")]
        public Color TextColor = new(0.9f, 0.9f, 0.9f, 1f);
        public Color LabelColor = new(0.7f, 0.7f, 0.75f, 1f);
        public Color ValueColor = new(0.9f, 0.9f, 0.9f, 1f);
        public Color ButtonTextColor = new(1f, 1f, 1f, 1f);

        [Header("Colors - Misc")]
        public Color DividerColor = new(0.3f, 0.3f, 0.35f, 1f);
        public Color OutlineColor = new(0.25f, 0.25f, 0.3f, 1f);

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

            DimBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
            ModalBackgroundColor = new Color(0.15f, 0.15f, 0.2f, 1f);
            HeaderColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            TabBarColor = new Color(0.12f, 0.12f, 0.17f, 1f);
            ContentColor = new Color(0.18f, 0.18f, 0.23f, 1f);
            FooterColor = new Color(0.1f, 0.1f, 0.15f, 1f);

            TabNormalColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            TabSelectedColor = new Color(0.4f, 0.6f, 1f, 1f);
            TabHoverColor = new Color(0.35f, 0.35f, 0.4f, 1f);

            SliderFillColor = new Color(0.4f, 0.6f, 1f, 1f);
            SliderBackgroundColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            ToggleCheckColor = new Color(0.4f, 0.8f, 0.4f, 1f);
            DropdownColor = new Color(0.25f, 0.25f, 0.3f, 1f);

            ApplyButtonColor = new Color(0.2f, 0.7f, 0.3f, 1f);
            ResetButtonColor = new Color(0.7f, 0.5f, 0.2f, 1f);
            CancelButtonColor = new Color(0.5f, 0.5f, 0.55f, 1f);
            CloseButtonColor = new Color(0.7f, 0.2f, 0.2f, 1f);

            TextColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            LabelColor = new Color(0.7f, 0.7f, 0.75f, 1f);
            ValueColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            ButtonTextColor = new Color(1f, 1f, 1f, 1f);

            DividerColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            OutlineColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        }
    }
}
#endif
