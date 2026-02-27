#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Counter
{
    [CreateAssetMenu(fileName = "CounterViewBuilderSettings", menuName = "PrefabBuilder/Settings/Counter")]
    public class CounterViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Layout")]
        public float ModalWidth = 400f;
        public float ModalHeight = 350f;
        public float ButtonSpacing = 15f;
        public float ButtonHeight = 50f;

        [Header("Typography")]
        public float CounterFontSize = 72f;
        public float IncrementsFontSize = 16f;
        public float ButtonFontSize = 20f;
        public float TitleFontSize = 24f;

        [Header("Colors")]
        public Color BgDimColor = new(0f, 0f, 0f, 0.7f);
        public Color ModalBgColor = new(0.14f, 0.14f, 0.16f, 1f);
        public Color HeaderColor = new(0.18f, 0.18f, 0.20f, 1f);
        public Color CounterTextColor = new(1f, 1f, 1f, 1f);
        public Color IncrementsTextColor = new(0.5f, 0.5f, 0.55f, 1f);
        public Color IncrementButtonColor = new(0.24f, 0.47f, 0.96f, 1f);
        public Color DecrementButtonColor = new(0.96f, 0.47f, 0.24f, 1f);
        public Color ResetButtonColor = new(0.4f, 0.4f, 0.45f, 1f);
        public Color ButtonTextColor = new(1f, 1f, 1f, 1f);

        public override void ResetToDefaults()
        {
            ModalWidth = 400f;
            ModalHeight = 350f;
            ButtonSpacing = 15f;
            ButtonHeight = 50f;
            CounterFontSize = 72f;
            IncrementsFontSize = 16f;
            ButtonFontSize = 20f;
            TitleFontSize = 24f;
            BgDimColor = new Color(0f, 0f, 0f, 0.7f);
            ModalBgColor = new Color(0.14f, 0.14f, 0.16f, 1f);
            HeaderColor = new Color(0.18f, 0.18f, 0.20f, 1f);
            CounterTextColor = new Color(1f, 1f, 1f, 1f);
            IncrementsTextColor = new Color(0.5f, 0.5f, 0.55f, 1f);
            IncrementButtonColor = new Color(0.24f, 0.47f, 0.96f, 1f);
            DecrementButtonColor = new Color(0.96f, 0.47f, 0.24f, 1f);
            ResetButtonColor = new Color(0.4f, 0.4f, 0.45f, 1f);
            ButtonTextColor = new Color(1f, 1f, 1f, 1f);
        }
    }
}
#endif
