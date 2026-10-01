using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit
{
    public static class ToolkitPanelSettingsBootstrap
    {
        private const string DefaultPanelSettingsPath = "Assets/Source/00. Configs/UI/DefaultPanelSettings.asset";
        private static PanelSettings _cachedSettings;

        public static void Apply(UIDocument document)
        {
            if (document == null || document.panelSettings != null)
                return;

            var settings = ResolveSettings();
            if (settings != null)
                document.panelSettings = settings;
        }

        private static PanelSettings ResolveSettings()
        {
            if (_cachedSettings != null)
                return _cachedSettings;

            _cachedSettings = Resources.Load<PanelSettings>("DefaultPanelSettings");

#if UNITY_EDITOR
            if (_cachedSettings == null)
                _cachedSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(DefaultPanelSettingsPath);
#endif

            return _cachedSettings;
        }
    }
}
