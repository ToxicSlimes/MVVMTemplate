#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
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
        public Color DimColor = new(0f, 0f, 0f, 0.6f);
        public Color BackgroundColor = new(0.16f, 0.16f, 0.18f, 1f);
        public Color HeaderColor = new(0.20f, 0.20f, 0.22f, 1f);
        public Color TitleColor = new(1f, 1f, 1f, 1f);
        public Color MessageColor = new(0.75f, 0.75f, 0.78f, 1f);
        public Color ConfirmButtonColor = new(0.85f, 0.25f, 0.25f, 1f);
        public Color CancelButtonColor = new(0.35f, 0.35f, 0.40f, 1f);
        public Color ButtonTextColor = new(1f, 1f, 1f, 1f);

        [Header("Buttons")]
        public float ButtonHeight = 40f;
        public float ButtonSpacing = 12f;

        public override void ResetToDefaults()
        {
            ModalWidth = 380f;
            ModalHeight = 220f;
            Padding = 20f;
            TitleFontSize = 20f;
            MessageFontSize = 14f;
            ButtonFontSize = 16f;
            DimColor = new Color(0f, 0f, 0f, 0.6f);
            BackgroundColor = new Color(0.16f, 0.16f, 0.18f, 1f);
            HeaderColor = new Color(0.20f, 0.20f, 0.22f, 1f);
            TitleColor = new Color(1f, 1f, 1f, 1f);
            MessageColor = new Color(0.75f, 0.75f, 0.78f, 1f);
            ConfirmButtonColor = new Color(0.85f, 0.25f, 0.25f, 1f);
            CancelButtonColor = new Color(0.35f, 0.35f, 0.40f, 1f);
            ButtonTextColor = new Color(1f, 1f, 1f, 1f);
            ButtonHeight = 40f;
            ButtonSpacing = 12f;
        }
    }
}
#endif
