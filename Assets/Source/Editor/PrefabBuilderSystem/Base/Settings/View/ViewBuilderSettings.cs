#if UNITY_EDITOR
using UnityEngine;
using Core.Editor.PrefabBuilderSystem.Settings;

namespace Core.Editor.PrefabBuilderSystem.View
{
    [CreateAssetMenu(fileName = "ViewBuilderSettings", menuName = "PrefabBuilder/Settings/View")]
    public class ViewBuilderSettings : BuilderSettingsBase
    {
        public float PanelHeight = 100f;
        public float Padding = 10f;
        public float Spacing = 5f;

        public Color PanelColor = new(0.15f, 0.15f, 0.15f, 0.9f);
        public Color BackgroundColor = new(0.2f, 0.2f, 0.2f, 0.8f);
        public Color TextColor = Color.white;

        public override void ResetToDefaults()
        {
            PanelHeight = 100f;
            Padding = 10f;
            Spacing = 5f;
            PanelColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            TextColor = Color.white;
        }
    }
}
#endif
