using System;
using System.Linq;
using Core.Abstractions.MVVM.Views;
using Core.Abstractions.Services.Input;
using Core.Data.Domain.Enums;
using Cysharp.Threading.Tasks;
using R3;
using Reflex.Attributes;
using Core.MVVM.ViewModels.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.Settings
{
    public class SettingsView : BindableView<SettingsViewModel>
    {
        public override bool IsPersistent => true;

        [Header("Modal")]
        [SerializeField] private Button _dimBackground;
        [SerializeField] private Button _closeButton;
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
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private TMP_Text _masterVolumeValue;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private TMP_Text _musicVolumeValue;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private TMP_Text _sfxVolumeValue;
        [SerializeField] private Slider _voiceVolumeSlider;
        [SerializeField] private TMP_Text _voiceVolumeValue;
        [SerializeField] private Toggle _muteAllToggle;
        [SerializeField] private Toggle _muteMusicToggle;
        [SerializeField] private Toggle _muteSfxToggle;

        [Header("Video Settings")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Dropdown _qualityDropdown;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Toggle _vsyncToggle;
        [SerializeField] private Toggle _showFpsToggle;
        [SerializeField] private TMP_Dropdown _targetFpsDropdown;

        [Header("Display Settings")]
        [SerializeField] private Slider _brightnessSlider;
        [SerializeField] private TMP_Text _brightnessValue;
        [SerializeField] private Slider _gammaSlider;
        [SerializeField] private TMP_Text _gammaValue;
        [SerializeField] private Slider _uiScaleSlider;
        [SerializeField] private TMP_Text _uiScaleValue;
        [SerializeField] private Toggle _colorblindModeToggle;
        [SerializeField] private TMP_Dropdown _colorblindTypeDropdown;

        [Header("Controls Settings")]
        [SerializeField] private Slider _mouseSensitivitySlider;
        [SerializeField] private TMP_Text _mouseSensitivityValue;
        [SerializeField] private Toggle _invertYAxisToggle;

        [Header("Game Settings")]
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private Toggle _autoSaveToggle;
        [SerializeField] private Toggle _showTutorialsToggle;
        [SerializeField] private Toggle _screenShakeToggle;
        [SerializeField] private Toggle _matureContentToggle;

        [Header("Footer")]
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _cancelButton;

        [Header("Tab Visuals")]
        [SerializeField] private Color _tabNormalColor = new(0.25f, 0.25f, 0.3f, 1f);
        [SerializeField] private Color _tabSelectedColor = new(0.4f, 0.6f, 1f, 1f);

        private Toggle[] _tabToggles;
        private GameObject[] _tabContents;
        private Image[] _tabImages;

        [Inject] private IInputService _inputService;

        protected override void OnBind(ref DisposableBag disposables)
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
            if (_dimBackground != null)
            {
                _dimBackground.OnClickAsObservable()
                    .Subscribe(ViewModel.CloseCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            if (_closeButton != null)
            {
                _closeButton.OnClickAsObservable()
                    .Subscribe(ViewModel.CloseCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }
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
            BindSlider(_masterVolumeSlider, _masterVolumeValue, ViewModel.MasterVolume, ref disposables);
            BindSlider(_musicVolumeSlider, _musicVolumeValue, ViewModel.MusicVolume, ref disposables);
            BindSlider(_sfxVolumeSlider, _sfxVolumeValue, ViewModel.SfxVolume, ref disposables);
            BindSlider(_voiceVolumeSlider, _voiceVolumeValue, ViewModel.VoiceVolume, ref disposables);

            BindToggle(_muteAllToggle, ViewModel.IsMuteAll, ref disposables);
            BindToggle(_muteMusicToggle, ViewModel.IsMuteMusic, ref disposables);
            BindToggle(_muteSfxToggle, ViewModel.IsMuteSfx, ref disposables);
        }

        private void BindVideoSettings(ref DisposableBag disposables)
        {
            BindDropdown(_resolutionDropdown, ViewModel.ResolutionIndex, ref disposables);

            if (_qualityDropdown != null)
            {
                ViewModel.Quality
                    .Subscribe(_qualityDropdown, static (q, dd) => dd.value = (int)q)
                    .AddTo(ref disposables);

                _qualityDropdown.onValueChanged.AsObservable()
                    .Subscribe(ViewModel.Quality, static (v, prop) => prop.Value = (QualityPreset)v)
                    .AddTo(ref disposables);
            }

            BindToggle(_fullscreenToggle, ViewModel.IsFullscreen, ref disposables);
            BindToggle(_vsyncToggle, ViewModel.IsVSync, ref disposables);
            BindToggle(_showFpsToggle, ViewModel.ShowFps, ref disposables);

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
            BindSlider(_brightnessSlider, _brightnessValue, ViewModel.Brightness, ref disposables);

            if (_gammaSlider != null)
            {
                _gammaSlider.minValue = 0.5f;
                _gammaSlider.maxValue = 2f;

                ViewModel.Gamma
                    .Subscribe((_gammaSlider, _gammaValue), static (v, state) =>
                    {
                        state._gammaSlider.value = v;
                        if (state._gammaValue != null)
                            state._gammaValue.text = v.ToString("F1");
                    })
                    .AddTo(ref disposables);

                _gammaSlider.onValueChanged.AsObservable()
                    .Subscribe(ViewModel.Gamma, static (v, prop) => prop.Value = v)
                    .AddTo(ref disposables);
            }

            if (_uiScaleSlider != null)
            {
                _uiScaleSlider.minValue = 0.75f;
                _uiScaleSlider.maxValue = 1.5f;

                ViewModel.UiScale
                    .Subscribe((_uiScaleSlider, _uiScaleValue), static (v, state) =>
                    {
                        state._uiScaleSlider.value = v;
                        if (state._uiScaleValue != null)
                            state._uiScaleValue.text = $"{(int)(v * 100)}%";
                    })
                    .AddTo(ref disposables);

                _uiScaleSlider.onValueChanged.AsObservable()
                    .Subscribe(ViewModel.UiScale, static (v, prop) => prop.Value = v)
                    .AddTo(ref disposables);
            }

            BindToggle(_colorblindModeToggle, ViewModel.IsColorblindMode, ref disposables);

            if (_colorblindTypeDropdown == null)
                return;
            {
                ViewModel.ColorblindType
                    .Subscribe(_colorblindTypeDropdown, static (t, dd) => dd.value = (int)t)
                    .AddTo(ref disposables);

                _colorblindTypeDropdown.onValueChanged.AsObservable()
                    .Subscribe(ViewModel.ColorblindType, static (v, prop) => prop.Value = (ColorblindType)v)
                    .AddTo(ref disposables);
            }
        }

        private void BindControlsSettings(ref DisposableBag disposables)
        {
            if (_mouseSensitivitySlider != null)
            {
                _mouseSensitivitySlider.minValue = 0.1f;
                _mouseSensitivitySlider.maxValue = 10f;

                ViewModel.MouseSensitivity
                    .Subscribe((_mouseSensitivitySlider, _mouseSensitivityValue), static (v, state) =>
                    {
                        state._mouseSensitivitySlider.value = v;
                        if (state._mouseSensitivityValue != null)
                            state._mouseSensitivityValue.text = v.ToString("F1");
                    })
                    .AddTo(ref disposables);

                _mouseSensitivitySlider.onValueChanged.AsObservable()
                    .Subscribe(ViewModel.MouseSensitivity, static (v, prop) => prop.Value = v)
                    .AddTo(ref disposables);
            }

            BindToggle(_invertYAxisToggle, ViewModel.InvertYAxis, ref disposables);
        }

        private void BindGameSettings(ref DisposableBag disposables)
        {
            if (_languageDropdown != null)
            {
                ViewModel.Language
                    .Subscribe(_languageDropdown, static (lang, dd) => dd.value = (int)lang)
                    .AddTo(ref disposables);

                _languageDropdown.onValueChanged.AsObservable()
                    .Subscribe(ViewModel.Language, static (v, prop) => prop.Value = (GameLanguage)v)
                    .AddTo(ref disposables);
            }

            BindToggle(_autoSaveToggle, ViewModel.IsAutoSave, ref disposables);
            BindToggle(_showTutorialsToggle, ViewModel.ShowTutorials, ref disposables);
            BindToggle(_screenShakeToggle, ViewModel.ScreenShake, ref disposables);
            BindToggle(_matureContentToggle, ViewModel.MatureContent, ref disposables);
        }

        private void BindFooterButtons(ref DisposableBag disposables)
        {
            if (_applyButton != null)
            {
                _applyButton.OnClickAsObservable()
                    .Subscribe(ViewModel.ApplyCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            if (_resetButton != null)
            {
                _resetButton.OnClickAsObservable()
                    .Subscribe(ViewModel.ResetCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            if (_cancelButton != null)
            {
                _cancelButton.OnClickAsObservable()
                    .Subscribe(ViewModel.CancelCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }
        }

        private static void BindSlider(Slider slider, TMP_Text valueText, ReactiveProperty<float> property, ref DisposableBag disposables)
        {
            if (slider == null) return;

            property
                .Subscribe((slider, valueText), static (v, state) =>
                {
                    state.slider.value = v;
                    if (state.valueText != null)
                        state.valueText.text = $"{(int)(v * 100)}%";
                })
                .AddTo(ref disposables);

            slider.onValueChanged.AsObservable()
                .Subscribe(property, static (v, prop) => prop.Value = v)
                .AddTo(ref disposables);
        }

        private static void BindToggle(Toggle toggle, ReactiveProperty<bool> property, ref DisposableBag disposables)
        {
            if (toggle == null) return;

            property
                .Subscribe(toggle, static (v, t) => t.isOn = v)
                .AddTo(ref disposables);

            toggle.onValueChanged.AsObservable()
                .Subscribe(property, static (v, prop) => prop.Value = v)
                .AddTo(ref disposables);
        }

        private static void BindDropdown(TMP_Dropdown dropdown, ReactiveProperty<int> property, ref DisposableBag disposables)
        {
            if (dropdown == null) return;

            property
                .Subscribe(dropdown, static (v, dd) => dd.value = v)
                .AddTo(ref disposables);

            dropdown.onValueChanged.AsObservable()
                .Subscribe(property, static (v, prop) => prop.Value = v)
                .AddTo(ref disposables);
        }

        public override async UniTask ShowAsync()
        {
            ViewModel.IsVisible.Value = true;
            await base.ShowAsync();
        }
    }
}
