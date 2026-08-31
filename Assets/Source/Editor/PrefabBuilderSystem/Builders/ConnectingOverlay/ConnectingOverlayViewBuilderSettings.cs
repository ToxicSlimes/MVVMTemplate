#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.ConnectingOverlay
{
    [CreateAssetMenu(fileName = "ConnectingOverlayViewBuilderSettings", menuName = "PrefabBuilder/Settings/ConnectingOverlay")]
    public class ConnectingOverlayViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Dim")]
        public float DimAlpha = 0.78f;

        [Header("Window")]
        public float WindowWidth = 600f;
        public float WindowHeight = 328f;
        public int WindowPadding = 40;
        public float WindowBorderWidth = 1f;

        [Header("Title")]
        public float TitleFontSize = 24f;
        public float TitleHeight = 32f;

        [Header("Status")]
        public float StatusTopSpacing = 12f;
        public float StatusHeight = 22f;

        [Header("Progress")]
        public float ProgressTopSpacing = 32f;
        public float ProgressBarHeight = 8f;

        [Header("Cancel Button")]
        public float CancelButtonHeight = 56f;
        public float CancelButtonFontSize = 16f;

        public override void ResetToDefaults()
        {
            DimAlpha = 0.78f;

            WindowWidth = 600f;
            WindowHeight = 328f;
            WindowPadding = 40;
            WindowBorderWidth = 1f;

            TitleFontSize = 24f;
            TitleHeight = 32f;

            StatusTopSpacing = 12f;
            StatusHeight = 22f;

            ProgressTopSpacing = 32f;
            ProgressBarHeight = 8f;

            CancelButtonHeight = 56f;
            CancelButtonFontSize = 16f;
        }
    }
}
#endif
