using System;
using System.Linq;
using Core.Abstractions.Localization;
using Core.Abstractions.Services.Input;
using Core.Abstractions.Services;
using Core.Data.Domain.Enums;
using Core.MVVM.ViewModels.Settings;
using Core.UI.Toolkit;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.MVVM.Views.Settings
{
    public partial class SettingsToolkitView : ToolkitPanelView<SettingsViewModel>
    {
        [Inject] private IInputService _inputService;
        [Inject] private ILocalizationService _localizationService;

        private Button _dimButton;
        private Button _closeButton;
        private Button _applyButton;
        private Button _resetButton;
        private Button _cancelButton;
        private readonly Button[] _tabButtons = new Button[5];
        private readonly VisualElement[] _tabContents = new VisualElement[5];

        private Slider _masterVolume;
        private Label _masterVolumeValue;
        private Slider _musicVolume;
        private Label _musicVolumeValue;
        private Slider _sfxVolume;
        private Label _sfxVolumeValue;
        private Slider _voiceVolume;
        private Label _voiceVolumeValue;
        private Toggle _muteAllToggle;
        private Toggle _muteMusicToggle;
        private Toggle _muteSfxToggle;

        private DropdownField _resolutionDropdown;
        private DropdownField _qualityDropdown;
        private Toggle _fullscreenToggle;
        private Toggle _vsyncToggle;
        private Toggle _showFpsToggle;
        private DropdownField _targetFpsDropdown;

        private Slider _brightnessSlider;
        private Label _brightnessValue;
        private Slider _gammaSlider;
        private Label _gammaValue;
        private Slider _uiScaleSlider;
        private Label _uiScaleValue;
        private Toggle _colorblindToggle;
        private DropdownField _colorblindTypeDropdown;

        private Slider _mouseSensitivitySlider;
        private Label _mouseSensitivityValue;
        private Toggle _invertYToggle;

        private DropdownField _languageDropdown;
        private Toggle _autoSaveToggle;
        private Toggle _tutorialsToggle;
        private Toggle _screenShakeToggle;
        private Toggle _matureContentToggle;

        public override bool IsPersistent => true;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();
            SetupDropdowns();
            BindCloseButtons(ref disposables);
            BindTabs(ref disposables);
            BindKeyboardNavigation(ref disposables);
            BindAudio(ref disposables);
            BindVideo(ref disposables);
            BindDisplay(ref disposables);
            BindControls(ref disposables);
            BindGame(ref disposables);
            BindFooter(ref disposables);
            UpdateTabVisuals(ViewModel.CurrentTab.Value);
            ApplyGeneratedBindings(ref disposables);
        }

        private void QueryElements()
        {
            if (Root == null) return;

            _dimButton = Root.Q<Button>("dim-button");
            _closeButton = Root.Q<Button>("close-button");
            _applyButton = Root.Q<Button>("apply-button");
            _resetButton = Root.Q<Button>("reset-button");
            _cancelButton = Root.Q<Button>("cancel-button");

            _tabButtons[0] = Root.Q<Button>("tab-audio");
            _tabButtons[1] = Root.Q<Button>("tab-video");
            _tabButtons[2] = Root.Q<Button>("tab-display");
            _tabButtons[3] = Root.Q<Button>("tab-controls");
            _tabButtons[4] = Root.Q<Button>("tab-game");

            _tabContents[0] = Root.Q<VisualElement>("content-audio");
            _tabContents[1] = Root.Q<VisualElement>("content-video");
            _tabContents[2] = Root.Q<VisualElement>("content-display");
            _tabContents[3] = Root.Q<VisualElement>("content-controls");
            _tabContents[4] = Root.Q<VisualElement>("content-game");

            _masterVolume = Root.Q<Slider>("master-volume");
            _masterVolumeValue = Root.Q<Label>("master-volume-value");
            _musicVolume = Root.Q<Slider>("music-volume");
            _musicVolumeValue = Root.Q<Label>("music-volume-value");
            _sfxVolume = Root.Q<Slider>("sfx-volume");
            _sfxVolumeValue = Root.Q<Label>("sfx-volume-value");
            _voiceVolume = Root.Q<Slider>("voice-volume");
            _voiceVolumeValue = Root.Q<Label>("voice-volume-value");
            _muteAllToggle = Root.Q<Toggle>("mute-all-toggle");
            _muteMusicToggle = Root.Q<Toggle>("mute-music-toggle");
            _muteSfxToggle = Root.Q<Toggle>("mute-sfx-toggle");

            _resolutionDropdown = Root.Q<DropdownField>("resolution-dropdown");
            _qualityDropdown = Root.Q<DropdownField>("quality-dropdown");
            _fullscreenToggle = Root.Q<Toggle>("fullscreen-toggle");
            _vsyncToggle = Root.Q<Toggle>("vsync-toggle");
            _showFpsToggle = Root.Q<Toggle>("show-fps-toggle");
            _targetFpsDropdown = Root.Q<DropdownField>("target-fps-dropdown");

            _brightnessSlider = Root.Q<Slider>("brightness-slider");
            _brightnessValue = Root.Q<Label>("brightness-value");
            _gammaSlider = Root.Q<Slider>("gamma-slider");
            _gammaValue = Root.Q<Label>("gamma-value");
            _uiScaleSlider = Root.Q<Slider>("ui-scale-slider");
            _uiScaleValue = Root.Q<Label>("ui-scale-value");
            _colorblindToggle = Root.Q<Toggle>("colorblind-toggle");
            _colorblindTypeDropdown = Root.Q<DropdownField>("colorblind-type-dropdown");

            _mouseSensitivitySlider = Root.Q<Slider>("mouse-sensitivity-slider");
            _mouseSensitivityValue = Root.Q<Label>("mouse-sensitivity-value");
            _invertYToggle = Root.Q<Toggle>("invert-y-toggle");

            _languageDropdown = Root.Q<DropdownField>("language-dropdown");
            _autoSaveToggle = Root.Q<Toggle>("autosave-toggle");
            _tutorialsToggle = Root.Q<Toggle>("tutorials-toggle");
            _screenShakeToggle = Root.Q<Toggle>("screen-shake-toggle");
            _matureContentToggle = Root.Q<Toggle>("mature-content-toggle");
        }

        private void SetupDropdowns()
        {
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.choices = Screen.resolutions
                    .Select(r => $"{r.width}x{r.height}")
                    .Distinct()
                    .ToList();
            }

            if (_qualityDropdown != null)
                _qualityDropdown.choices = Enum.GetNames(typeof(QualityPreset)).ToList();

            if (_targetFpsDropdown != null)
                _targetFpsDropdown.choices = new System.Collections.Generic.List<string> { "30 FPS", "60 FPS", "120 FPS", "Unlimited" };

            if (_colorblindTypeDropdown != null)
                _colorblindTypeDropdown.choices = Enum.GetNames(typeof(ColorblindType)).ToList();

            if (_languageDropdown != null)
                _languageDropdown.choices = Enum.GetNames(typeof(GameLanguage)).ToList();
        }

        private void BindCloseButtons(ref DisposableBag disposables)
        {
            Bind(_dimButton, ViewModel.CloseCommand, ref disposables);
            Bind(_closeButton, ViewModel.CloseCommand, ref disposables);
        }

        private void BindTabs(ref DisposableBag disposables)
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                if (_tabButtons[i] == null) continue;
                var tab = (SettingsTab)i;
                var button = _tabButtons[i];
                void OnTabClicked() => ViewModel.SelectTabCommand.Execute(tab);
                button.clicked += OnTabClicked;
                disposables.Add(Disposable.Create(() => button.clicked -= OnTabClicked));
            }

            ViewModel.CurrentTab
                .Subscribe(this, static (tab, self) => self.UpdateTabVisuals(tab))
                .AddTo(ref disposables);
        }

        private void UpdateTabVisuals(SettingsTab selectedTab)
        {
            var selectedIndex = (int)selectedTab;

            for (int i = 0; i < _tabContents.Length; i++)
            {
                var isSelected = i == selectedIndex;
                if (_tabContents[i] != null)
                {
                    _tabContents[i].EnableInClassList("tab-content-active", isSelected);
                    _tabContents[i].style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (_tabButtons[i] != null)
                {
                    _tabButtons[i].EnableInClassList("tab-button-active", isSelected);
                    _tabButtons[i].EnableInClassList("btn-primary", isSelected);
                    _tabButtons[i].EnableInClassList("btn-outline", !isSelected);
                }
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

        private void BindAudio(ref DisposableBag disposables)
        {
            Bind(_masterVolume, ViewModel.MasterVolume, ref disposables);
            Bind(_musicVolume, ViewModel.MusicVolume, _musicVolumeValue, "{0:P0}", ref disposables);
            Bind(_sfxVolume, ViewModel.SfxVolume, _sfxVolumeValue, "{0:P0}", ref disposables);
            Bind(_voiceVolume, ViewModel.VoiceVolume, _voiceVolumeValue, "{0:P0}", ref disposables);

            if (_localizationService != null && _masterVolumeValue != null)
            {
                RefreshMasterLabelFromVolume();
                ViewModel.MasterVolume.Subscribe(this, static (_, self) => self.RefreshMasterLabelFromVolume())
                    .AddTo(ref disposables);
                Action<GameLanguage> onLanguageChanged = _ => RefreshMasterLabelFromVolume();
                _localizationService.LanguageChanged += onLanguageChanged;
                disposables.Add(Disposable.Create(() => _localizationService.LanguageChanged -= onLanguageChanged));
            }
            else
            {
                Bind(_masterVolume, ViewModel.MasterVolume, _masterVolumeValue, "{0:P0}", ref disposables);
            }

            Bind(_muteAllToggle, ViewModel.IsMuteAll, ref disposables);
            Bind(_muteMusicToggle, ViewModel.IsMuteMusic, ref disposables);
            Bind(_muteSfxToggle, ViewModel.IsMuteSfx, ref disposables);
        }

        private void BindVideo(ref DisposableBag disposables)
        {
            Bind(_resolutionDropdown, ViewModel.ResolutionIndex, ref disposables);
            BindEnum(_qualityDropdown, ViewModel.Quality, ref disposables);
            Bind(_fullscreenToggle, ViewModel.IsFullscreen, ref disposables);
            Bind(_vsyncToggle, ViewModel.IsVSync, ref disposables);
            Bind(_showFpsToggle, ViewModel.ShowFps, ref disposables);
            BindTargetFps(ref disposables);
        }

        private void BindTargetFps(ref DisposableBag disposables)
        {
            if (_targetFpsDropdown == null) return;

            ViewModel.TargetFps
                .Subscribe(_targetFpsDropdown, static (fps, dropdown) =>
                {
                    dropdown.index = fps switch
                    {
                        TargetFpsOption.Fps30 => 0,
                        TargetFpsOption.Fps60 => 1,
                        TargetFpsOption.Fps120 => 2,
                        TargetFpsOption.Unlimited => 3,
                        _ => 1
                    };
                })
                .AddTo(ref disposables);

            void OnChanged(ChangeEvent<string> evt)
            {
                var index = _targetFpsDropdown.choices.IndexOf(evt.newValue);
                ViewModel.TargetFps.Value = index switch
                {
                    0 => TargetFpsOption.Fps30,
                    1 => TargetFpsOption.Fps60,
                    2 => TargetFpsOption.Fps120,
                    3 => TargetFpsOption.Unlimited,
                    _ => TargetFpsOption.Fps60
                };
            }

            _targetFpsDropdown.RegisterValueChangedCallback(OnChanged);
            disposables.Add(Disposable.Create(() => _targetFpsDropdown.UnregisterValueChangedCallback(OnChanged)));
        }

        private void BindDisplay(ref DisposableBag disposables)
        {
            if (_gammaSlider != null)
            {
                _gammaSlider.lowValue = 0.5f;
                _gammaSlider.highValue = 2f;
            }

            if (_uiScaleSlider != null)
            {
                _uiScaleSlider.lowValue = 0.75f;
                _uiScaleSlider.highValue = 1.5f;
            }

            Bind(_brightnessSlider, ViewModel.Brightness, _brightnessValue, "{0:P0}", ref disposables);
            Bind(_gammaSlider, ViewModel.Gamma, _gammaValue, "{0:F1}", ref disposables);
            Bind(_uiScaleSlider, ViewModel.UiScale, _uiScaleValue, "{0:P0}", ref disposables);
            Bind(_colorblindToggle, ViewModel.IsColorblindMode, ref disposables);
            BindEnum(_colorblindTypeDropdown, ViewModel.ColorblindType, ref disposables);
        }

        private void BindControls(ref DisposableBag disposables)
        {
            if (_mouseSensitivitySlider != null)
            {
                _mouseSensitivitySlider.lowValue = 0.1f;
                _mouseSensitivitySlider.highValue = 10f;
            }

            Bind(_mouseSensitivitySlider, ViewModel.MouseSensitivity, _mouseSensitivityValue, "{0:F1}", ref disposables);
            Bind(_invertYToggle, ViewModel.InvertYAxis, ref disposables);
        }

        private void BindGame(ref DisposableBag disposables)
        {
            BindEnum(_languageDropdown, ViewModel.Language, ref disposables);
            Bind(_autoSaveToggle, ViewModel.IsAutoSave, ref disposables);
            Bind(_tutorialsToggle, ViewModel.ShowTutorials, ref disposables);
            Bind(_screenShakeToggle, ViewModel.ScreenShake, ref disposables);
            Bind(_matureContentToggle, ViewModel.MatureContent, ref disposables);
        }

        private void BindFooter(ref DisposableBag disposables)
        {
            Bind(_applyButton, ViewModel.ApplyCommand, ref disposables);
            Bind(_resetButton, ViewModel.ResetCommand, ref disposables);
            Bind(_cancelButton, ViewModel.CancelCommand, ref disposables);
        }

        private void RefreshMasterLabelFromVolume()
        {
            if (_localizationService == null || _masterVolumeValue == null)
                return;

            _masterVolumeValue.text = _localizationService.Localize(
                UiLocalizationKeys.Table,
                UiLocalizationKeys.Settings.VolumeLabel,
                Mathf.RoundToInt(ViewModel.MasterVolume.Value * 100f));
        }
    }
}
