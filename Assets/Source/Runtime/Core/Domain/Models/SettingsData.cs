using System;
using Core.Domain.Enums;

namespace Core.Domain.Models
{
    [Serializable]
    public class VideoSettingsData
    {
        public int ResolutionIndex;
        public QualityPreset Quality = QualityPreset.High;
        public bool IsFullscreen = true;
        public bool IsVSync = true;
        public bool ShowFps;
        public TargetFpsOption TargetFps = TargetFpsOption.Fps60;
    }

    [Serializable]
    public class DisplaySettingsData
    {
        public float Brightness = 0.5f;
        public float Gamma = 1f;
        public float UiScale = 1f;
        public bool IsColorblindMode;
        public ColorblindType ColorblindType;
    }

    [Serializable]
    public class ControlsSettingsData
    {
        public float MouseSensitivity = 5f;
        public bool InvertYAxis;
    }

    [Serializable]
    public class GameSettingsData
    {
        public GameLanguage Language = GameLanguage.English;
        public bool IsAutoSave = true;
        public bool ShowTutorials = true;
        public bool ScreenShake = true;
        public bool MatureContent;
    }
}
