using System;
using Core.Abstractions.MVVM.Models;
using Core.Domain.Enums;

namespace Core.Abstractions.MVVM.Models.Settings
{
    public interface ISettingsModel : IModel
    {
        event Action<bool> IsVisibleChanged;
        event Action<SettingsTab> CurrentTabChanged;

        bool IsVisible { get; set; }
        SettingsTab CurrentTab { get; set; }

        IAudioSettings Audio { get; }
        IVideoSettings Video { get; }
        IDisplaySettings Display { get; }
        IControlsSettings Controls { get; }
        IGameSettings Game { get; }

        void Apply();
        void ResetToDefaults();
        void LoadFromPlayerPrefs();
        void SaveToPlayerPrefs();
    }

    public interface IAudioSettings
    {
        event Action AudioSettingsChanged;

        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        float VoiceVolume { get; set; }
        bool IsMuteAll { get; set; }
        bool IsMuteMusic { get; set; }
        bool IsMuteSfx { get; set; }

        void Reset();
    }

    public interface IVideoSettings
    {
        event Action VideoSettingsChanged;

        int ResolutionIndex { get; set; }
        QualityPreset Quality { get; set; }
        bool IsFullscreen { get; set; }
        bool IsVSync { get; set; }
        bool ShowFps { get; set; }
        TargetFpsOption TargetFps { get; set; }

        void Reset();
    }

    public interface IDisplaySettings
    {
        event Action DisplaySettingsChanged;

        float Brightness { get; set; }
        float Gamma { get; set; }
        float UiScale { get; set; }
        bool IsColorblindMode { get; set; }
        ColorblindType ColorblindType { get; set; }

        void Reset();
    }

    public interface IControlsSettings
    {
        event Action ControlsSettingsChanged;

        float MouseSensitivity { get; set; }
        bool InvertYAxis { get; set; }

        void Reset();
    }

    public interface IGameSettings
    {
        event Action GameSettingsChanged;

        GameLanguage Language { get; set; }
        bool IsAutoSave { get; set; }
        bool ShowTutorials { get; set; }
        bool ScreenShake { get; set; }
        bool MatureContent { get; set; }

        void Reset();
    }
}
