#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.SettingsView;
using Core.MVVM.Views.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Builders.Settings
{
    public class SettingsViewBuilder : ViewBuilder<Core.MVVM.Views.Settings.SettingsView>
    {
        public SettingsViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/SettingsView.prefab";

        // Wiring refs
        private GameObject _dimBackground;
        private GameObject _closeButton;
        private GameObject _modalWindow;

        // Tabs
        private GameObject _tabAudio, _tabVideo, _tabDisplay, _tabControls, _tabGame;
        private GameObject _audioContent, _videoContent, _displayContent, _controlsContent, _gameContent;

        // Audio
        private GameObject _masterVolumeSlider, _masterVolumeValue;
        private GameObject _musicVolumeSlider, _musicVolumeValue;
        private GameObject _sfxVolumeSlider, _sfxVolumeValue;
        private GameObject _voiceVolumeSlider, _voiceVolumeValue;
        private GameObject _muteAllToggle, _muteMusicToggle, _muteSfxToggle;

        // Video
        private GameObject _resolutionDropdown, _qualityDropdown;
        private GameObject _fullscreenToggle, _vsyncToggle, _showFpsToggle;
        private GameObject _targetFpsDropdown;

        // Display
        private GameObject _brightnessSlider, _brightnessValue;
        private GameObject _gammaSlider, _gammaValue;
        private GameObject _uiScaleSlider, _uiScaleValue;
        private GameObject _colorblindModeToggle, _colorblindTypeDropdown;

        // Controls
        private GameObject _mouseSensitivitySlider, _mouseSensitivityValue;
        private GameObject _invertYAxisToggle;

        // Game
        private GameObject _languageDropdown;
        private GameObject _autoSaveToggle, _showTutorialsToggle, _screenShakeToggle, _matureContentToggle;

        // Footer
        private GameObject _applyButton, _resetButton, _cancelButton;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Dim background (clickable to close)
            _dimBackground = new PanelBuilder("DimBackground")
                .WithColor(s.DimBackgroundColor)
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(Root);
            _dimBackground.AddComponent<Button>();

            // Modal window
            _modalWindow = new PanelBuilder("ModalWindow")
                .WithColor(s.ModalBackgroundColor)
                .WithSize(s.ModalWidth, s.ModalHeight)
                .WithAnchoredPosition(Vector2.zero)
                .Build(Root);

            var modalRect = _modalWindow.GetComponent<RectTransform>();
            modalRect.anchorMin = new Vector2(0.5f, 0.5f);
            modalRect.anchorMax = new Vector2(0.5f, 0.5f);

            // Header
            var header = new HorizontalLayoutBuilder("Header")
                .WithPadding(15, 15, 0, 0)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPreferredHeight(s.HeaderHeight)
                .WithBackgroundColor(s.HeaderColor)
                .WithControlChildSize(true, true)
                .WithForceExpand(true, false)
                .WithAnchors(new Vector2(0, 1), new Vector2(1, 1))
                .Build(_modalWindow.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, s.HeaderHeight);
            headerRect.pivot = new Vector2(0.5f, 1f);

            new TextBuilder("Title")
                .WithText("Settings")
                .WithFontSize(s.TitleFontSize)
                .WithColor(Color.white)
                .WithBold()
                .Build(header.transform);

            _closeButton = new ButtonBuilder("CloseButton")
                .WithLabel("✕")
                .WithLabelFontSize(18f)
                .WithVariant(ButtonVariant.Ghost)
                .WithSize(s.CloseButtonSize, s.CloseButtonSize)
                .Build(header.transform);

            // Tab bar
            var tabBar = new HorizontalLayoutBuilder("TabBar")
                .WithSpacing(s.TabSpacing)
                .WithPadding(10, 10, 0, 0)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithControlChildSize(true, true)
                .WithPreferredHeight(s.TabBarHeight)
                .WithBackgroundColor(s.TabBarColor)
                .WithAnchors(new Vector2(0, 1), new Vector2(1, 1))
                .Build(_modalWindow.transform);
            var tabBarRect = tabBar.GetComponent<RectTransform>();
            tabBarRect.sizeDelta = new Vector2(0, s.TabBarHeight);
            tabBarRect.anchoredPosition = new Vector2(0, -s.HeaderHeight);
            tabBarRect.pivot = new Vector2(0.5f, 1f);

            _tabAudio = BuildTab(tabBar.transform, "Audio", s);
            _tabVideo = BuildTab(tabBar.transform, "Video", s);
            _tabDisplay = BuildTab(tabBar.transform, "Display", s);
            _tabControls = BuildTab(tabBar.transform, "Controls", s);
            _tabGame = BuildTab(tabBar.transform, "Game", s);

            // Content area
            var contentOffset = s.HeaderHeight + s.TabBarHeight;
            var contentArea = new PanelBuilder("ContentArea")
                .WithColor(new Color(0, 0, 0, 0))
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(_modalWindow.transform);
            var contentRect = contentArea.GetComponent<RectTransform>();
            contentRect.offsetMin = new Vector2(0, s.FooterHeight);
            contentRect.offsetMax = new Vector2(0, -contentOffset);

            // Tab contents
            _audioContent = BuildTabContent(contentArea.transform, "AudioContent", s);
            _videoContent = BuildTabContent(contentArea.transform, "VideoContent", s);
            _displayContent = BuildTabContent(contentArea.transform, "DisplayContent", s);
            _controlsContent = BuildTabContent(contentArea.transform, "ControlsContent", s);
            _gameContent = BuildTabContent(contentArea.transform, "GameContent", s);

            // Audio tab content
            (_masterVolumeSlider, _masterVolumeValue) = BuildSliderRow(_audioContent.transform, "Master Volume", s);
            (_musicVolumeSlider, _musicVolumeValue) = BuildSliderRow(_audioContent.transform, "Music Volume", s);
            (_sfxVolumeSlider, _sfxVolumeValue) = BuildSliderRow(_audioContent.transform, "SFX Volume", s);
            (_voiceVolumeSlider, _voiceVolumeValue) = BuildSliderRow(_audioContent.transform, "Voice Volume", s);
            _muteAllToggle = BuildToggleRow(_audioContent.transform, "Mute All", s);
            _muteMusicToggle = BuildToggleRow(_audioContent.transform, "Mute Music", s);
            _muteSfxToggle = BuildToggleRow(_audioContent.transform, "Mute SFX", s);

            // Video tab content
            _resolutionDropdown = BuildDropdownRow(_videoContent.transform, "Resolution", s);
            _qualityDropdown = BuildDropdownRow(_videoContent.transform, "Quality", s);
            _fullscreenToggle = BuildToggleRow(_videoContent.transform, "Fullscreen", s);
            _vsyncToggle = BuildToggleRow(_videoContent.transform, "V-Sync", s);
            _showFpsToggle = BuildToggleRow(_videoContent.transform, "Show FPS", s);
            _targetFpsDropdown = BuildDropdownRow(_videoContent.transform, "Target FPS", s);

            // Display tab content
            (_brightnessSlider, _brightnessValue) = BuildSliderRow(_displayContent.transform, "Brightness", s);
            (_gammaSlider, _gammaValue) = BuildSliderRow(_displayContent.transform, "Gamma", s);
            (_uiScaleSlider, _uiScaleValue) = BuildSliderRow(_displayContent.transform, "UI Scale", s);
            _colorblindModeToggle = BuildToggleRow(_displayContent.transform, "Colorblind Mode", s);
            _colorblindTypeDropdown = BuildDropdownRow(_displayContent.transform, "Colorblind Type", s);

            // Controls tab content
            (_mouseSensitivitySlider, _mouseSensitivityValue) = BuildSliderRow(_controlsContent.transform, "Mouse Sensitivity", s);
            _invertYAxisToggle = BuildToggleRow(_controlsContent.transform, "Invert Y-Axis", s);

            // Game tab content
            _languageDropdown = BuildDropdownRow(_gameContent.transform, "Language", s);
            _autoSaveToggle = BuildToggleRow(_gameContent.transform, "Auto-Save", s);
            _showTutorialsToggle = BuildToggleRow(_gameContent.transform, "Show Tutorials", s);
            _screenShakeToggle = BuildToggleRow(_gameContent.transform, "Screen Shake", s);
            _matureContentToggle = BuildToggleRow(_gameContent.transform, "Mature Content", s);

            // Only show first tab
            _videoContent.SetActive(false);
            _displayContent.SetActive(false);
            _controlsContent.SetActive(false);
            _gameContent.SetActive(false);

            // Footer
            var footer = new HorizontalLayoutBuilder("Footer")
                .WithSpacing(10f)
                .WithPadding(15, 15, 0, 0)
                .WithAlignment(TextAnchor.MiddleRight)
                .WithControlChildSize(true, true)
                .WithForceExpand(false, false)
                .WithBackgroundColor(s.FooterColor)
                .WithPreferredHeight(s.FooterHeight)
                .WithAnchors(Vector2.zero, new Vector2(1, 0))
                .Build(_modalWindow.transform);
            var footerRect = footer.GetComponent<RectTransform>();
            footerRect.sizeDelta = new Vector2(0, s.FooterHeight);
            footerRect.pivot = new Vector2(0.5f, 0f);

            _cancelButton = new ButtonBuilder("CancelButton")
                .WithLabel("Cancel")
                .WithVariant(ButtonVariant.Ghost)
                .WithPreferredHeight(36f)
                .Build(footer.transform);

            _resetButton = new ButtonBuilder("ResetButton")
                .WithLabel("Reset")
                .WithVariant(ButtonVariant.Outline)
                .WithPreferredHeight(36f)
                .Build(footer.transform);

            _applyButton = new ButtonBuilder("ApplyButton")
                .WithLabel("Apply")
                .WithVariant(ButtonVariant.Primary)
                .WithPreferredHeight(36f)
                .Build(footer.transform);
        }

        private static GameObject BuildTab(Transform parent, string label, SettingsViewBuilderSettings s)
        {
            var tab = new ToggleBuilder(label + "Tab")
                .WithLabel(label)
                .WithSize(s.TabButtonMinWidth, s.TabButtonHeight)
                .Build(parent);
            return tab;
        }

        private static GameObject BuildTabContent(Transform parent, string name, SettingsViewBuilderSettings s)
        {
            var content = new VerticalLayoutBuilder(name)
                .WithSpacing(s.RowSpacing)
                .WithPadding((int)s.ContentPadding)
                .WithAlignment(TextAnchor.UpperLeft)
                .WithControlChildSize(true, true)
                .WithForceExpand(true, false)
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(parent);
            var rect = content.GetComponent<RectTransform>();
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return content;
        }

        private static (GameObject slider, GameObject valueText) BuildSliderRow(Transform parent, string label, SettingsViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder(label.Replace(" ", "") + "Row")
                .WithSpacing(10f)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithControlChildSize(true, true)
                .WithForceExpand(false, false)
                .WithPreferredHeight(s.RowHeight)
                .Build(parent);

            new TextBuilder(label.Replace(" ", "") + "Label")
                .WithText(label)
                .WithFontSize(s.LabelFontSize)
                .WithColor(s.LabelColor)
                .WithPreferredWidth(s.LabelWidth)
                .WithPreferredHeight(s.RowHeight)
                .Build(row.transform);

            var slider = new SliderBuilder(label.Replace(" ", "") + "Slider")
                .WithRange(0f, 1f)
                .WithPreferredHeight(s.RowHeight)
                .Build(row.transform);

            var valueText = new TextBuilder(label.Replace(" ", "") + "Value")
                .WithText("100%")
                .WithFontSize(s.ValueFontSize)
                .WithColor(s.ValueColor)
                .WithAlignment(TextAlignmentOptions.Right)
                .WithPreferredWidth(60f)
                .WithPreferredHeight(s.RowHeight)
                .Build(row.transform);

            return (slider, valueText);
        }

        private static GameObject BuildToggleRow(Transform parent, string label, SettingsViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder(label.Replace(" ", "") + "Row")
                .WithSpacing(10f)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithControlChildSize(true, true)
                .WithForceExpand(false, false)
                .WithPreferredHeight(s.RowHeight)
                .Build(parent);

            new TextBuilder(label.Replace(" ", "") + "Label")
                .WithText(label)
                .WithFontSize(s.LabelFontSize)
                .WithColor(s.LabelColor)
                .WithPreferredWidth(s.LabelWidth)
                .WithPreferredHeight(s.RowHeight)
                .Build(row.transform);

            var toggle = new ToggleBuilder(label.Replace(" ", "") + "Toggle")
                .Build(row.transform);

            return toggle;
        }

        private static GameObject BuildDropdownRow(Transform parent, string label, SettingsViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder(label.Replace(" ", "") + "Row")
                .WithSpacing(10f)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithControlChildSize(true, true)
                .WithForceExpand(false, false)
                .WithPreferredHeight(s.RowHeight)
                .Build(parent);

            new TextBuilder(label.Replace(" ", "") + "Label")
                .WithText(label)
                .WithFontSize(s.LabelFontSize)
                .WithColor(s.LabelColor)
                .WithPreferredWidth(s.LabelWidth)
                .WithPreferredHeight(s.RowHeight)
                .Build(row.transform);

            var dropdown = new DropdownBuilder(label.Replace(" ", "") + "Dropdown")
                .WithPreferredHeight(s.RowHeight)
                .Build(row.transform);

            return dropdown;
        }

        protected override void WireFields(Core.MVVM.Views.Settings.SettingsView view)
        {
            var so = new SerializedObject(view);

            // Modal
            SerializationHelper.Wire(so, "_dimBackground", _dimBackground.GetComponent<Button>());
            SerializationHelper.Wire(so, "_closeButton", _closeButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_modalWindow", _modalWindow);

            // Tabs
            SerializationHelper.Wire(so, "_tabAudio", _tabAudio.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabVideo", _tabVideo.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabDisplay", _tabDisplay.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabControls", _tabControls.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabGame", _tabGame.GetComponent<Toggle>());

            // Tab contents
            SerializationHelper.Wire(so, "_audioContent", _audioContent);
            SerializationHelper.Wire(so, "_videoContent", _videoContent);
            SerializationHelper.Wire(so, "_displayContent", _displayContent);
            SerializationHelper.Wire(so, "_controlsContent", _controlsContent);
            SerializationHelper.Wire(so, "_gameContent", _gameContent);

            // Audio
            SerializationHelper.Wire(so, "_masterVolumeSlider", _masterVolumeSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_masterVolumeValue", _masterVolumeValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_musicVolumeSlider", _musicVolumeSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_musicVolumeValue", _musicVolumeValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_sfxVolumeSlider", _sfxVolumeSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_sfxVolumeValue", _sfxVolumeValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_voiceVolumeSlider", _voiceVolumeSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_voiceVolumeValue", _voiceVolumeValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_muteAllToggle", _muteAllToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_muteMusicToggle", _muteMusicToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_muteSfxToggle", _muteSfxToggle.GetComponent<Toggle>());

            // Video
            SerializationHelper.Wire(so, "_resolutionDropdown", _resolutionDropdown.GetComponent<TMP_Dropdown>());
            SerializationHelper.Wire(so, "_qualityDropdown", _qualityDropdown.GetComponent<TMP_Dropdown>());
            SerializationHelper.Wire(so, "_fullscreenToggle", _fullscreenToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_vsyncToggle", _vsyncToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_showFpsToggle", _showFpsToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_targetFpsDropdown", _targetFpsDropdown.GetComponent<TMP_Dropdown>());

            // Display
            SerializationHelper.Wire(so, "_brightnessSlider", _brightnessSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_brightnessValue", _brightnessValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_gammaSlider", _gammaSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_gammaValue", _gammaValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_uiScaleSlider", _uiScaleSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_uiScaleValue", _uiScaleValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_colorblindModeToggle", _colorblindModeToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_colorblindTypeDropdown", _colorblindTypeDropdown.GetComponent<TMP_Dropdown>());

            // Controls
            SerializationHelper.Wire(so, "_mouseSensitivitySlider", _mouseSensitivitySlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_mouseSensitivityValue", _mouseSensitivityValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_invertYAxisToggle", _invertYAxisToggle.GetComponent<Toggle>());

            // Game
            SerializationHelper.Wire(so, "_languageDropdown", _languageDropdown.GetComponent<TMP_Dropdown>());
            SerializationHelper.Wire(so, "_autoSaveToggle", _autoSaveToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_showTutorialsToggle", _showTutorialsToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_screenShakeToggle", _screenShakeToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_matureContentToggle", _matureContentToggle.GetComponent<Toggle>());

            // Footer
            SerializationHelper.Wire(so, "_applyButton", _applyButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_resetButton", _resetButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_cancelButton", _cancelButton.GetComponent<Button>());

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
