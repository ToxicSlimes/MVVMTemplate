#if UNITY_EDITOR
using UnityEngine;
using Core.Editor.PrefabBuilderSystem.Settings;

namespace Core.Editor.PrefabBuilderSystem.MainView
{
    [CreateAssetMenu(fileName = "MainViewBuilderSettings", menuName = "PrefabBuilder/Settings/MainView")]
    public class MainViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Layout")]
        public float ContentPadding = 40f;
        public float ButtonSpacing = 20f;
        public float ButtonWidth = 300f;
        public float ButtonHeight = 60f;

        [Header("Typography")]
        public float TitleFontSize = 48f;
        public float SubtitleFontSize = 18f;
        public float ButtonFontSize = 24f;

        [Header("Colors")]
        public Color BgColor = new(0.12f, 0.12f, 0.14f, 1f);
        public Color PanelColor = new(0.16f, 0.16f, 0.18f, 1f);
        public Color TitleColor = new(1f, 1f, 1f, 1f);
        public Color SubtitleColor = new(0.6f, 0.6f, 0.65f, 1f);
        public Color ButtonColor = new(0.24f, 0.47f, 0.96f, 1f);
        public Color ButtonTextColor = new(1f, 1f, 1f, 1f);
        public Color ButtonHoverColor = new(0.34f, 0.57f, 1f, 1f);

        public override void ResetToDefaults()
        {
            ContentPadding = 40f;
            ButtonSpacing = 20f;
            ButtonWidth = 300f;
            ButtonHeight = 60f;

            TitleFontSize = 48f;
            SubtitleFontSize = 18f;
            ButtonFontSize = 24f;

            BgColor = new Color(0.12f, 0.12f, 0.14f, 1f);
            PanelColor = new Color(0.16f, 0.16f, 0.18f, 1f);
            TitleColor = new Color(1f, 1f, 1f, 1f);
            SubtitleColor = new Color(0.6f, 0.6f, 0.65f, 1f);
            ButtonColor = new Color(0.24f, 0.47f, 0.96f, 1f);
            ButtonTextColor = new Color(1f, 1f, 1f, 1f);
            ButtonHoverColor = new Color(0.34f, 0.57f, 1f, 1f);
        }
    }
}
#endif
