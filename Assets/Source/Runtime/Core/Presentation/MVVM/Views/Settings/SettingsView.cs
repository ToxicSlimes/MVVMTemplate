using System;
using System.Linq;
using Core.Abstractions.Services.Input;
using Core.Domain.Enums;
using Core.Presentation.MVVM.Views;
using R3;
using Reflex.Attributes;
using Core.Presentation.MVVM.ViewModels.Settings;
using Core.UI.Binding;
using Core.UI.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.MVVM.Views.Settings
{
    public class SettingsView : PanelView<SettingsViewModel>
    {
        public override bool IsPersistent => true;

        [Header("Modal")]
        [SerializeField] private BaseButton _dimBackground;
        [SerializeField] private BaseButton _closeButton;
        [SerializeField] private GameObject _modalWindow;

        [Header("Tabs")]
        [SerializeField] private Toggle _tabAudio;
        [SerializeField] private Toggle _tabVideo;
        [SerializeField] private Toggle _tabDisplay;
        [SerializeField] private Toggle _tabControls;
        [SerializeField] private Toggle _tabGame;

        [Header("Tab Contents")]
        [SerializeField] private GameObject _audioContent;
        [SerializeField] private GameObject _videoContent;
        [SerializeField] private GameObject _displayContent;
        [SerializeField] private GameObject _controlsContent;
        [SerializeField] private GameObject _gameContent;

        [Header("Audio Settings")]
        [SerializeField] private BaseSlider _masterVolumeSlider;
        [SerializeField] private TMP_Text _masterVolumeValue;
        [SerializeField] private BaseSlider _musicVolumeSlider;
        [SerializeField] private TMP_Text _musicVolumeValue;
        [SerializeField] private BaseSlider _sfxVolumeSlider;
        [SerializeField] private TMP_Text _sfxVolumeValue;
        [SerializeField] private BaseSlider _voiceVolumeSlider;
        [SerializeField] private TMP_Text _voiceVolumeValue;
        [SerializeField] private BaseToggle _muteAllToggle;
        [SerializeField] private BaseToggle _muteMusicToggle;
        [SerializeField] private BaseToggle _muteSfxToggle;

        [Header("Video Settings")]
        [SerializeField] private BaseDropdown _resolutionDropdown;
        [SerializeField] private BaseDropdown _qualityDropdown;
        [SerializeField] private BaseToggle _fullscreenToggle;
        [SerializeField] private BaseToggle _vsyncToggle;
        [SerializeField] private BaseToggle _showFpsToggle;
        [SerializeField] private BaseDropdown _targetFpsDropdown;

        [Header("Display Settings")]
        [SerializeField] private BaseSlider _brightnessSlider;
        [SerializeField] private TMP_Text _brightnessValue;
        [SerializeField] private BaseSlider _gammaSlider;
        [SerializeField] private TMP_Text _gammaValue;
        [SerializeField] private BaseSlider _uiScaleSlider;
        [SerializeField] private TMP_Text _uiScaleValue;
        [SerializeField] private BaseToggle _colorblindModeToggle;
        [SerializeField] private BaseDropdown _colorblindTypeDropdown;

        [Header("Controls Settings")]
        [SerializeField] private BaseSlider _mouseSensitivitySlider;
        [SerializeField] private TMP_Text _mouseSensitivityValue;
        [SerializeField] private BaseToggle _invertYAxisToggle;

        [Header("Game Settings")]
        [SerializeField] private BaseDropdown _languageDropdown;
        [SerializeField] private BaseToggle _autoSaveToggle;
        [SerializeField] private BaseToggle _showTutorialsToggle;
        [SerializeField] private BaseToggle _screenShakeToggle;
        [SerializeField] private BaseToggle _matureContentToggle;

        [Header("Footer")]
        [SerializeField] private BaseButton _applyButton;
        [SerializeField] private BaseButton _resetButton;
        [SerializeField] private BaseButton _cancelButton;

        [Header("Tab Visuals")]
        [SerializeField] private Color _tabNormalColor = new(0.25f, 0.25f, 0.3f, 1f);
        [SerializeField] private Color _tabSelectedColor = new(0.4f, 0.6f, 1f, 1f);

        private Toggle[] _tabToggles;
        private GameObject[] _tabContents;
        private Image[] _tabImages;

        [Inject] private IInputService _inputService;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            InitializeTabArrays();
            SetupDropdowns();

            BindCloseButtons(ref disposables);
            BindTabs(ref disposables);
            BindKeyboardNavigation(ref disposables);
            BindAudioSettings(ref disposables);
            BindVideoSettings(ref disposables);
            BindDisplaySettings(ref disposables);
            BindControlsSettings(ref disposables);
            BindGameSettings(ref disposables);
            BindFooterButtons(ref disposables);
        }

        private void InitializeTabArrays()
        {
            _tabToggles = new[] { _tabAudio, _tabVideo, _tabDisplay, _tabControls, _tabGame };
            _tabContents = new[] { _audioContent, _videoContent, _displayContent, _controlsContent, _gameContent };
            _tabImages = _tabToggles
                .Where(t => t != null)
                .Select(t => t.GetComponent<Image>())
                .ToArray();
        }

        private void SetupDropdowns()
        {
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.ClearOptions();
                var resolutions = Screen.resolutions
                    .Select(r => $"{r.width}x{r.height}")
                    .Distinct()
                    .ToList();
                _resolutionDropdown.AddOptions(resolutions);
            }

            if (_qualityDropdown != null)
            {
                _qualityDropdown.ClearOptions();
                _qualityDropdown.AddOptions(Enum.GetNames(typeof(QualityPreset)).ToList());
            }

            if (_targetFpsDropdown != null)
            {
                _targetFpsDropdown.ClearOptions();
                _targetFpsDropdown.AddOptions(new[] { "30 FPS", "60 FPS", "120 FPS", "Unlimited" }.ToList());
            }

            if (_colorblindTypeDropdown != null)
            {
                _colorblindTypeDropdown.ClearOptions();
                _colorblindTypeDropdown.AddOptions(Enum.GetNames(typeof(ColorblindType)).ToList());
            }

            if (_languageDropdown != null)
            {
                _languageDropdown.ClearOptions();
                _languageDropdown.AddOptions(Enum.GetNames(typeof(GameLanguage)).ToList());
            }
        }

        private void BindCloseButtons(ref DisposableBag disposables)
        {
            Bind(_dimBackground, ViewModel.CloseCommand, ref disposables);
            Bind(_closeButton, ViewModel.CloseCommand, ref disposables);
        }

        private void BindTabs(ref DisposableBag disposables)
        {
            for (var i = 0; i < _tabToggles.Length; i++)
            {
                if (_tabToggles[i] == null) continue;

                var tab = (SettingsTab)i;

                _tabToggles[i].onValueChanged.AsObservable()
                    .Where(isOn => isOn)
                    .Subscribe((tab, ViewModel.SelectTabCommand), static (_, state) =>
                        state.Item2.Execute(state.tab))
                    .AddTo(ref disposables);
            }

            ViewModel.CurrentTab
                .Subscribe(this, static (tab, self) => self.UpdateTabVisuals(tab))
                .AddTo(ref disposables);
        }

        private void UpdateTabVisuals(SettingsTab selectedTab)
        {
            var selectedIndex = (int)selectedTab;

            for (var i = 0; i < _tabContents.Length; i++)
            {
                var isSelected = i == selectedIndex;

                if (_tabContents[i] != null)
                    _tabContents[i].SetActive(isSelected);

                if (i < _tabToggles.Length && _tabToggles[i] != null)
                    _tabToggles[i].SetIsOnWithoutNotify(isSelected);

                if (i < _tabImages.Length && _tabImages[i] != null)
                    _tabImages[i].color = isSelected ? _tabSelectedColor : _tabNormalColor;
            }
        }

        private void BindKeyboardNavigation(ref DisposableBag disposables)
        {
            if (_inputService == null) return;

            _inputService.OnNavigate
                .Where(this, static (_, self) => self.ViewModel.IsVisible.CurrentValue)
                .Where(static dir => Mathf.Abs(dir.x) > 0.5f)
                .ThrottleFirst(TimeSpan.FromMilliseconds(200))
                .Subscribe(this, static (dir, self) =>
                {
                    if (dir.x > 0)
                        self.ViewModel.NextTabCommand.Execute(Unit.Default);
                    else
                        self.ViewModel.PreviousTabCommand.Execute(Unit.Default);
                })
                .AddTo(ref disposables);

            _inputService.OnCancel
                .Where(this, static (_, self) => self.ViewModel.IsVisible.CurrentValue)
                .Subscribe(ViewModel.CancelCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                .AddTo(ref disposables);
        }

        private void BindAudioSettings(ref DisposableBag disposables)
        {
            _masterVolumeSlider.BindTo(ViewModel.MasterVolume, _masterVolumeValue, "{0:P0}", ref disposables);
            _musicVolumeSlider.BindTo(ViewModel.MusicVolume, _musicVolumeValue, "{0:P0}", ref disposables);
            _sfxVolumeSlider.BindTo(ViewModel.SfxVolume, _sfxVolumeValue, "{0:P0}", ref disposables);
            _voiceVolumeSlider.BindTo(ViewModel.VoiceVolume, _voiceVolumeValue, "{0:P0}", ref disposables);

            _muteAllToggle.BindTo(ViewModel.IsMuteAll, ref disposables);
            _muteMusicToggle.BindTo(ViewModel.IsMuteMusic, ref disposables);
            _muteSfxToggle.BindTo(ViewModel.IsMuteSfx, ref disposables);
        }

        private void BindVideoSettings(ref DisposableBag disposables)
        {
            _resolutionDropdown.BindTo(ViewModel.ResolutionIndex, ref disposables);

            if (_qualityDropdown != null)
                _qualityDropdown.BindToEnum(ViewModel.Quality, ref disposables);

            _fullscreenToggle.BindTo(ViewModel.IsFullscreen, ref disposables);
            _vsyncToggle.BindTo(ViewModel.IsVSync, ref disposables);
            _showFpsToggle.BindTo(ViewModel.ShowFps, ref disposables);

            if (_targetFpsDropdown == null)
                return;
            {
                ViewModel.TargetFps
                    .Subscribe(_targetFpsDropdown, static (fps, dd) =>
                    {
                        dd.value = fps switch
                        {
                            TargetFpsOption.Fps30 => 0,
                            TargetFpsOption.Fps60 => 1,
                            TargetFpsOption.Fps120 => 2,
                            TargetFpsOption.Unlimited => 3,
                            _ => 1
                        };
                    })
                    .AddTo(ref disposables);

                _targetFpsDropdown.onValueChanged.AsObservable()
                    .Subscribe(ViewModel.TargetFps, static (v, prop) =>
                    {
                        prop.Value = v switch
                        {
                            0 => TargetFpsOption.Fps30,
                            1 => TargetFpsOption.Fps60,
                            2 => TargetFpsOption.Fps120,
                            3 => TargetFpsOption.Unlimited,
                            _ => TargetFpsOption.Fps60
                        };
                    })
                    .AddTo(ref disposables);
            }
        }

        private void BindDisplaySettings(ref DisposableBag disposables)
        {
            _brightnessSlider.BindTo(ViewModel.Brightness, _brightnessValue, "{0:P0}", ref disposables);

            if (_gammaSlider != null)
            {
                _gammaSlider.minValue = 0.5f;
                _gammaSlider.maxValue = 2f;
                _gammaSlider.BindTo(ViewModel.Gamma, _gammaValue, "{0:F1}", ref disposables);
            }

            if (_uiScaleSlider != null)
            {
                _uiScaleSlider.minValue = 0.75f;
                _uiScaleSlider.maxValue = 1.5f;
                _uiScaleSlider.BindTo(ViewModel.UiScale, _uiScaleValue, "{0:P0}", ref disposables);
            }

            _colorblindModeToggle.BindTo(ViewModel.IsColorblindMode, ref disposables);

            if (_colorblindTypeDropdown != null)
                _colorblindTypeDropdown.BindToEnum(ViewModel.ColorblindType, ref disposables);
        }

        private void BindControlsSettings(ref DisposableBag disposables)
        {
            if (_mouseSensitivitySlider != null)
            {
                _mouseSensitivitySlider.minValue = 0.1f;
                _mouseSensitivitySlider.maxValue = 10f;
                _mouseSensitivitySlider.BindTo(ViewModel.MouseSensitivity, _mouseSensitivityValue, "{0:F1}", ref disposables);
            }

            _invertYAxisToggle.BindTo(ViewModel.InvertYAxis, ref disposables);
        }

        private void BindGameSettings(ref DisposableBag disposables)
        {
            if (_languageDropdown != null)
                _languageDropdown.BindToEnum(ViewModel.Language, ref disposables);

            _autoSaveToggle.BindTo(ViewModel.IsAutoSave, ref disposables);
            _showTutorialsToggle.BindTo(ViewModel.ShowTutorials, ref disposables);
            _screenShakeToggle.BindTo(ViewModel.ScreenShake, ref disposables);
            _matureContentToggle.BindTo(ViewModel.MatureContent, ref disposables);
        }

        private void BindFooterButtons(ref DisposableBag disposables)
        {
            Bind(_applyButton, ViewModel.ApplyCommand, ref disposables);
            Bind(_resetButton, ViewModel.ResetCommand, ref disposables);
            Bind(_cancelButton, ViewModel.CancelCommand, ref disposables);
        }
    }
}
