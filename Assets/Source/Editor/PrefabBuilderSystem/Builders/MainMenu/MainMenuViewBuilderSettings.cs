#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.MainMenu
{
    [CreateAssetMenu(fileName = "MainMenuViewBuilderSettings", menuName = "PrefabBuilder/Settings/MainMenu")]
    public class MainMenuViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Title Block")]
        public Vector2 TitleBlockAnchorMin = new(0f, 0.58f);
        public Vector2 TitleBlockAnchorMax = new(1f, 0.84f);
        public float TitleHeight = 140f;
        public float SubtitleHeight = 36f;

        [Header("Menu Block")]
        public Vector2 MenuBlockAnchorMin = new(0f, 0.10f);
        public Vector2 MenuBlockAnchorMax = new(1f, 0.57f);
        public float ButtonWidth = 400f;
        public float PlayButtonHeight = 72f;
        public float MenuButtonHeight = 60f;

        [Header("Footer")]
        public float FooterHeight = 40f;
        public float FooterSideMargin = 48f;
        public float FooterBottomMargin = 32f;
        public float FooterLabelWidth = 320f;

        [Header("Typography")]
        public float TitleFontSize = 112f;
        public float PlayButtonFontSize = 28f;
        public float MenuButtonFontSize = 22f;

        public override void ResetToDefaults()
        {
            TitleBlockAnchorMin = new Vector2(0f, 0.58f);
            TitleBlockAnchorMax = new Vector2(1f, 0.84f);
            TitleHeight = 140f;
            SubtitleHeight = 36f;

            MenuBlockAnchorMin = new Vector2(0f, 0.10f);
            MenuBlockAnchorMax = new Vector2(1f, 0.57f);
            ButtonWidth = 400f;
            PlayButtonHeight = 72f;
            MenuButtonHeight = 60f;

            FooterHeight = 40f;
            FooterSideMargin = 48f;
            FooterBottomMargin = 32f;
            FooterLabelWidth = 320f;

            TitleFontSize = 112f;
            PlayButtonFontSize = 28f;
            MenuButtonFontSize = 22f;
        }
    }
}
#endif
