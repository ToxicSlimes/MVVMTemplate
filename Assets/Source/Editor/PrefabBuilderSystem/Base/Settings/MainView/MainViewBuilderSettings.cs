#if UNITY_EDITOR
using UnityEngine;
using EndlessAdventure.Editor.PrefabBuilderSystem.Settings;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.MainView
{
    [CreateAssetMenu(fileName = "MainViewBuilderSettings", menuName = "PrefabBuilder/Settings/MainView")]
    public class MainViewBuilderSettings : BuilderSettingsBase
    {
        public float TopBarHeight = 110f;
        public float BottomBarHeight = 200f;
        public float ButtonSize = 70f;
        public float SlotWidth = 110f;
        public float SlotHeight = 160f;
        public float SlotSpacing = 20f;

        public float HeaderFontSize = 24f;
        public float StatNameFontSize = 20f;
        public float StatValueFontSize = 22f;
        public float ResourceFontSize = 22f;
        public float HeroNameFontSize = 16f;
        public float LevelFontSize = 12f;
        public float HealthFontSize = 14f;

        public float LevelBadgeSize = 36f;
        public float HealthBarHeight = 24f;
        public float HeroIconPadding = 8f;

        public Color BgColor = new(0.92f, 0.92f, 0.92f, 1f);
        public Color PanelColor = new(1f, 1f, 1f, 1f);
        public Color IconPlaceholderColor = new(0.3f, 0.3f, 0.3f, 1f);
        public Color TextColor = new(0.2f, 0.2f, 0.2f, 1f);
        public Color ValueColor = new(0.1f, 0.1f, 0.1f, 1f);
        public Color OutlineColor = new(0.1f, 0.1f, 0.1f, 1f);
        public Color StatsScrollBgColor = new(0.3f, 0.4f, 0.96f, 0.8f);
        
        public Color SlotBackgroundColor = new(0.498f, 0.839f, 0.659f, 1f);
        public Color LevelBadgeColor = new(1f, 1f, 1f, 1f);
        public Color LevelTextColor = new(0.2f, 0.2f, 0.2f, 1f);
        public Color HealthBarColor = new(0.898f, 0.224f, 0.208f, 1f);
        public Color HealthBarBgColor = new(0.4f, 0.4f, 0.4f, 1f);
        public Color HealthTextColor = new(1f, 1f, 1f, 1f);

        public override void ResetToDefaults()
        {
            TopBarHeight = 110f;
            BottomBarHeight = 200f;
            ButtonSize = 70f;
            SlotWidth = 110f;
            SlotHeight = 160f;
            SlotSpacing = 20f;

            HeaderFontSize = 24f;
            StatNameFontSize = 20f;
            StatValueFontSize = 22f;
            ResourceFontSize = 22f;
            HeroNameFontSize = 16f;
            LevelFontSize = 12f;
            HealthFontSize = 14f;

            LevelBadgeSize = 36f;
            HealthBarHeight = 24f;
            HeroIconPadding = 8f;

            BgColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            PanelColor = new Color(1f, 1f, 1f, 1f);
            IconPlaceholderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            TextColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            ValueColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            OutlineColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            StatsScrollBgColor = new Color(0.3f, 0.4f, 0.96f, 0.8f);

            SlotBackgroundColor = new Color(0.498f, 0.839f, 0.659f, 1f);
            LevelBadgeColor = new Color(1f, 1f, 1f, 1f);
            LevelTextColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            HealthBarColor = new Color(0.898f, 0.224f, 0.208f, 1f);
            HealthBarBgColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            HealthTextColor = new Color(1f, 1f, 1f, 1f);
        }
    }
}
#endif
