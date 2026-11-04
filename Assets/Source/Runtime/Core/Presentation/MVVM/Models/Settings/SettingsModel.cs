using System;
using Core.Abstractions.Services;
using Core.Abstractions.MVVM.Models.Settings;
using Core.Domain.Enums;
using Core.Domain.Models;
using UnityEngine;

namespace Core.Presentation.MVVM.Models.Settings
{
    public class SettingsModel : ISettingsModel
    {
        private const string VideoSettingsKey = "Settings.Video";
        private const string DisplaySettingsKey = "Settings.Display";
        private const string ControlsSettingsKey = "Settings.Controls";
        private const string GameSettingsKey = "Settings.Game";

        private readonly IPrefsService _prefsService;

        private bool _isVisible;
        private SettingsTab _currentTab = SettingsTab.Audio;

        public event Action<bool> IsVisibleChanged;
        public event Action<SettingsTab> CurrentTabChanged;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                IsVisibleChanged?.Invoke(_isVisible);
            }
        }

        public SettingsTab CurrentTab
        {
            get => _currentTab;
            set
            {
                if (_currentTab == value) return;
                _currentTab = value;
                CurrentTabChanged?.Invoke(_currentTab);
            }
        }

        public IAudioSettings Audio { get; }
        public IVideoSettings Video { get; }
        public IDisplaySettings Display { get; }
        public IControlsSettings Controls { get; }
        public IGameSettings Game { get; }

        public bool IsValid => true;

        public SettingsModel(IPrefsService prefsService)
        {
            _prefsService = prefsService;

            Audio = new AudioSettings();
            Video = new VideoSettings();
            Display = new DisplaySettings();
            Controls = new ControlsSettings();
            Game = new GameSettings();
        }

        public void Apply()
        {
            ApplyVideoSettings();
            ApplyDisplaySettings();
            SaveToPlayerPrefs();
        }

        public void ResetToDefaults()
        {
            Video.Reset();
            Display.Reset();
            Controls.Reset();
            Game.Reset();
        }

        public void LoadFromPlayerPrefs()
        {
            LoadVideoFromPrefs();
            LoadDisplayFromPrefs();
            LoadControlsFromPrefs();
            LoadGameFromPrefs();
        }

        public void SaveToPlayerPrefs()
        {
            SaveVideoToPrefs();
            SaveDisplayToPrefs();
            SaveControlsToPrefs();
            SaveGameToPrefs();
        }

        public void Reset()
        {
            IsVisible = false;
            CurrentTab = SettingsTab.Audio;
            ResetToDefaults();
        }

        public void Dispose()
        {
            IsVisibleChanged = null;
            CurrentTabChanged = null;
        }

        private void ApplyVideoSettings()
        {
            QualitySettings.SetQualityLevel((int)Video.Quality);
            QualitySettings.vSyncCount = Video.IsVSync ? 1 : 0;
#if !UNITY_WEBGL || UNITY_EDITOR
            UnityEngine.Application.targetFrameRate = (int)Video.TargetFps;
#endif
            if (UnityEngine.Application.isMobilePlatform)
            {
                return;
            }

            Screen.fullScreen = Video.IsFullscreen;

            if (Video.ResolutionIndex >= 0 && Video.ResolutionIndex < Screen.resolutions.Length)
            {
                var res = Screen.resolutions[Video.ResolutionIndex];
                Screen.SetResolution(res.width, res.height, Video.IsFullscreen);
            }
        }

        private void ApplyDisplaySettings()
        {
        }

        private void LoadVideoFromPrefs()
        {
            var data = _prefsService.Load<VideoSettingsData>(VideoSettingsKey);
            if (data == null) return;

            Video.ResolutionIndex = data.ResolutionIndex;
            Video.Quality = data.Quality;
            Video.IsFullscreen = data.IsFullscreen;
            Video.IsVSync = data.IsVSync;
            Video.ShowFps = data.ShowFps;
            Video.TargetFps = data.TargetFps;
        }

        private void LoadDisplayFromPrefs()
        {
            var data = _prefsService.Load<DisplaySettingsData>(DisplaySettingsKey);
            if (data == null) return;

            Display.Brightness = data.Brightness;
            Display.Gamma = data.Gamma;
            Display.UiScale = data.UiScale;
            Display.IsColorblindMode = data.IsColorblindMode;
            Display.ColorblindType = data.ColorblindType;
        }

        private void LoadControlsFromPrefs()
        {
            var data = _prefsService.Load<ControlsSettingsData>(ControlsSettingsKey);
            if (data == null) return;

            Controls.MouseSensitivity = data.MouseSensitivity;
            Controls.InvertYAxis = data.InvertYAxis;
        }

        private void LoadGameFromPrefs()
        {
            var data = _prefsService.Load<GameSettingsData>(GameSettingsKey);
            if (data == null) return;

            Game.Language = data.Language;
            Game.IsAutoSave = data.IsAutoSave;
            Game.ShowTutorials = data.ShowTutorials;
            Game.ScreenShake = data.ScreenShake;
            Game.MatureContent = data.MatureContent;
        }

        private void SaveVideoToPrefs()
        {
            var data = new VideoSettingsData
            {
                ResolutionIndex = Video.ResolutionIndex,
                Quality = Video.Quality,
                IsFullscreen = Video.IsFullscreen,
                IsVSync = Video.IsVSync,
                ShowFps = Video.ShowFps,
                TargetFps = Video.TargetFps
            };
            _prefsService.Save(VideoSettingsKey, data);
        }

        private void SaveDisplayToPrefs()
        {
            var data = new DisplaySettingsData
            {
                Brightness = Display.Brightness,
                Gamma = Display.Gamma,
                UiScale = Display.UiScale,
                IsColorblindMode = Display.IsColorblindMode,
                ColorblindType = Display.ColorblindType
            };
            _prefsService.Save(DisplaySettingsKey, data);
        }

        private void SaveControlsToPrefs()
        {
            var data = new ControlsSettingsData
            {
                MouseSensitivity = Controls.MouseSensitivity,
                InvertYAxis = Controls.InvertYAxis
            };
            _prefsService.Save(ControlsSettingsKey, data);
        }

        private void SaveGameToPrefs()
        {
            var data = new GameSettingsData
            {
                Language = Game.Language,
                IsAutoSave = Game.IsAutoSave,
                ShowTutorials = Game.ShowTutorials,
                ScreenShake = Game.ScreenShake,
                MatureContent = Game.MatureContent
            };
            _prefsService.Save(GameSettingsKey, data);
        }
    }

    public class AudioSettings : IAudioSettings
    {
        public event Action AudioSettingsChanged;

        private float _masterVolume = 1f;
        private float _musicVolume = 0.7f;
        private float _sfxVolume = 0.8f;
        private float _voiceVolume = 1f;
        private bool _isMuteAll;
        private bool _isMuteMusic;
        private bool _isMuteSfx;

        public float MasterVolume
        {
            get => _masterVolume;
            set { _masterVolume = value; AudioSettingsChanged?.Invoke(); }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set { _musicVolume = value; AudioSettingsChanged?.Invoke(); }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set { _sfxVolume = value; AudioSettingsChanged?.Invoke(); }
        }

        public float VoiceVolume
        {
            get => _voiceVolume;
            set { _voiceVolume = value; AudioSettingsChanged?.Invoke(); }
        }

        public bool IsMuteAll
        {
            get => _isMuteAll;
            set { _isMuteAll = value; AudioSettingsChanged?.Invoke(); }
        }

        public bool IsMuteMusic
        {
            get => _isMuteMusic;
            set { _isMuteMusic = value; AudioSettingsChanged?.Invoke(); }
        }

        public bool IsMuteSfx
        {
            get => _isMuteSfx;
            set { _isMuteSfx = value; AudioSettingsChanged?.Invoke(); }
        }

        public void Reset()
        {
            _masterVolume = 1f;
            _musicVolume = 0.7f;
            _sfxVolume = 0.8f;
            _voiceVolume = 1f;
            _isMuteAll = false;
            _isMuteMusic = false;
            _isMuteSfx = false;
            AudioSettingsChanged?.Invoke();
        }
    }

    public class VideoSettings : IVideoSettings
    {
        public event Action VideoSettingsChanged;

        private int _resolutionIndex;
        private QualityPreset _quality = QualityPreset.High;
        private bool _isFullscreen = true;
        private bool _isVSync = true;
        private bool _showFps;
        private TargetFpsOption _targetFps = TargetFpsOption.Fps60;

        public int ResolutionIndex
        {
            get => _resolutionIndex;
            set { _resolutionIndex = value; VideoSettingsChanged?.Invoke(); }
        }

        public QualityPreset Quality
        {
            get => _quality;
            set { _quality = value; VideoSettingsChanged?.Invoke(); }
        }

        public bool IsFullscreen
        {
            get => _isFullscreen;
            set { _isFullscreen = value; VideoSettingsChanged?.Invoke(); }
        }

        public bool IsVSync
        {
            get => _isVSync;
            set { _isVSync = value; VideoSettingsChanged?.Invoke(); }
        }

        public bool ShowFps
        {
            get => _showFps;
            set { _showFps = value; VideoSettingsChanged?.Invoke(); }
        }

        public TargetFpsOption TargetFps
        {
            get => _targetFps;
            set { _targetFps = value; VideoSettingsChanged?.Invoke(); }
        }

        public void Reset()
        {
            _resolutionIndex = Screen.resolutions.Length > 0 ? Screen.resolutions.Length - 1 : 0;
            _quality = QualityPreset.High;
            _isFullscreen = true;
            _isVSync = true;
            _showFps = false;
            _targetFps = TargetFpsOption.Fps60;
            VideoSettingsChanged?.Invoke();
        }
    }

    public class DisplaySettings : IDisplaySettings
    {
        public event Action DisplaySettingsChanged;

        private float _brightness = 0.5f;
        private float _gamma = 1f;
        private float _uiScale = 1f;
        private bool _isColorblindMode;
        private ColorblindType _colorblindType;

        public float Brightness
        {
            get => _brightness;
            set { _brightness = Mathf.Clamp01(value); DisplaySettingsChanged?.Invoke(); }
        }

        public float Gamma
        {
            get => _gamma;
            set { _gamma = Mathf.Clamp(value, 0.5f, 2f); DisplaySettingsChanged?.Invoke(); }
        }

        public float UiScale
        {
            get => _uiScale;
            set { _uiScale = Mathf.Clamp(value, 0.75f, 1.5f); DisplaySettingsChanged?.Invoke(); }
        }

        public bool IsColorblindMode
        {
            get => _isColorblindMode;
            set { _isColorblindMode = value; DisplaySettingsChanged?.Invoke(); }
        }

        public ColorblindType ColorblindType
        {
            get => _colorblindType;
            set { _colorblindType = value; DisplaySettingsChanged?.Invoke(); }
        }

        public void Reset()
        {
            _brightness = 0.5f;
            _gamma = 1f;
            _uiScale = 1f;
            _isColorblindMode = false;
            _colorblindType = ColorblindType.None;
            DisplaySettingsChanged?.Invoke();
        }
    }

    public class ControlsSettings : IControlsSettings
    {
        public event Action ControlsSettingsChanged;

        private float _mouseSensitivity = 5f;
        private bool _invertYAxis;

        public float MouseSensitivity
        {
            get => _mouseSensitivity;
            set { _mouseSensitivity = Mathf.Clamp(value, 0.1f, 10f); ControlsSettingsChanged?.Invoke(); }
        }

        public bool InvertYAxis
        {
            get => _invertYAxis;
            set { _invertYAxis = value; ControlsSettingsChanged?.Invoke(); }
        }

        public void Reset()
        {
            _mouseSensitivity = 5f;
            _invertYAxis = false;
            ControlsSettingsChanged?.Invoke();
        }
    }

    public class GameSettings : IGameSettings
    {
        public event Action GameSettingsChanged;

        private GameLanguage _language = GameLanguage.English;
        private bool _isAutoSave = true;
        private bool _showTutorials = true;
        private bool _screenShake = true;
        private bool _matureContent;

        public GameLanguage Language
        {
            get => _language;
            set { _language = value; GameSettingsChanged?.Invoke(); }
        }

        public bool IsAutoSave
        {
            get => _isAutoSave;
            set { _isAutoSave = value; GameSettingsChanged?.Invoke(); }
        }

        public bool ShowTutorials
        {
            get => _showTutorials;
            set { _showTutorials = value; GameSettingsChanged?.Invoke(); }
        }

        public bool ScreenShake
        {
            get => _screenShake;
            set { _screenShake = value; GameSettingsChanged?.Invoke(); }
        }

        public bool MatureContent
        {
            get => _matureContent;
            set { _matureContent = value; GameSettingsChanged?.Invoke(); }
        }

        public void Reset()
        {
            _language = GameLanguage.English;
            _isAutoSave = true;
            _showTutorials = true;
            _screenShake = true;
            _matureContent = false;
            GameSettingsChanged?.Invoke();
        }
    }
}
