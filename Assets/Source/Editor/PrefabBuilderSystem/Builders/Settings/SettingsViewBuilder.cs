#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.SettingsView;
using Core.MVVM.Views.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

namespace Core.Editor.PrefabBuilderSystem.Builders.Settings
{
    public class SettingsViewBuilder : ViewBuilder<MVVM.Views.Settings.SettingsView>
    {
        public SettingsViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/SettingsView.prefab";

        // Wiring refs
        private Image _dimBackground;
        private Button _closeButton;
        private Image _modalWindow;
        private GameObject _tabAudio, _tabVideo, _tabDisplay, _tabControls, _tabGame;
        private VerticalLayoutGroup _audioContent;
        private VerticalLayoutGroup _videoContent;
        private VerticalLayoutGroup _displayContent;
        private VerticalLayoutGroup _controlsContent;
        private VerticalLayoutGroup _gameContent;
        private Slider _masterVolumeSlider;
        private TextMeshProUGUI _masterVolumeValue;
        private Slider _musicVolumeSlider;
        private TextMeshProUGUI _musicVolumeValue;
        private Slider _sfxVolumeSlider;
        private TextMeshProUGUI _sfxVolumeValue;
        private Slider _voiceVolumeSlider;
        private TextMeshProUGUI _voiceVolumeValue;
        private GameObject _muteAllToggle, _muteMusicToggle, _muteSfxToggle;
        private GameObject _resolutionDropdown, _qualityDropdown, _fullscreenToggle, _vsyncToggle, _showFpsToggle, _targetFpsDropdown;
        private Slider _brightnessSlider;
        private TextMeshProUGUI _brightnessValue;
        private Slider _gammaSlider;
        private TextMeshProUGUI _gammaValue;
        private Slider _uiScaleSlider;
        private TextMeshProUGUI _uiScaleValue;
        private GameObject _colorblindModeToggle, _colorblindTypeDropdown;
        private Slider _mouseSensitivitySlider;
        private TextMeshProUGUI _mouseSensitivityValue;
        private GameObject _invertYAxisToggle;
        private GameObject _languageDropdown, _autoSaveToggle, _showTutorialsToggle, _screenShakeToggle, _matureContentToggle;
        private Button _applyButton;
        private Button _resetButton;
        private Button _cancelButton;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Dim background
            _dimBackground = new PanelBuilder("DimBackground")
                .WithBackgroundColor(s.DimBackgroundColor)
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(Root);
            _dimBackground.AddComponent<Button>();

            // Modal
            _modalWindow = new PanelBuilder("ModalWindow")
                .WithBackgroundColor(s.ModalBackgroundColor)
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
                .WithBackground(s.HeaderColor)
                .WithAnchors(new Vector2(0, 1), new Vector2(1, 1))
                .WithPreferredSize(null, s.HeaderHeight)
                .Build(_modalWindow.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, s.HeaderHeight);
            headerRect.pivot = new Vector2(0.5f, 1f);

            new TextBuilder("Title")
                .WithText("Settings")
                .WithFontSize(s.TitleFontSize)
                .WithColor(Color.white)
                .Bold()
                .Build(header.transform);

            _closeButton = new ButtonBuilder("CloseButton")
                .WithLabel("\u2715")
                .WithLabelSize(18f)
                .Ghost()
                .WithSize(s.CloseButtonSize, s.CloseButtonSize)
                .Build(header.transform);

            // Tab bar
            var tabBar = new HorizontalLayoutBuilder("TabBar")
                .WithSpacing(s.TabSpacing)
                .WithPadding(10, 10, 0, 0)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithBackground(s.TabBarColor)
                .WithAnchors(new Vector2(0, 1), new Vector2(1, 1))
                .WithPreferredSize(null, s.TabBarHeight)
                .Build(_modalWindow.transform);
            var tabBarRect = tabBar.GetComponent<RectTransform>();
            tabBarRect.sizeDelta = new Vector2(0, s.TabBarHeight);
            tabBarRect.anchoredPosition = new Vector2(0, -s.HeaderHeight);
            tabBarRect.pivot = new Vector2(0.5f, 1f);

            _tabAudio = BuildTab(tabBar.transform, "Audio");
            _tabVideo = BuildTab(tabBar.transform, "Video");
            _tabDisplay = BuildTab(tabBar.transform, "Display");
            _tabControls = BuildTab(tabBar.transform, "Controls");
            _tabGame = BuildTab(tabBar.transform, "Game");

            // Content area
            var contentOffset = s.HeaderHeight + s.TabBarHeight;
            var contentArea = new PanelBuilder("ContentArea")
                .WithBackgroundColor(new Color(0, 0, 0, 0))
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(_modalWindow.transform);
            var contentRect = contentArea.GetComponent<RectTransform>();
            contentRect.offsetMin = new Vector2(0, s.FooterHeight);
            contentRect.offsetMax = new Vector2(0, -contentOffset);

            _audioContent = BuildTabContent(contentArea.transform, "AudioContent", s);
            _videoContent = BuildTabContent(contentArea.transform, "VideoContent", s);
            _displayContent = BuildTabContent(contentArea.transform, "DisplayContent", s);
            _controlsContent = BuildTabContent(contentArea.transform, "ControlsContent", s);
            _gameContent = BuildTabContent(contentArea.transform, "GameContent", s);

            // Audio
            (_masterVolumeSlider, _masterVolumeValue) = BuildSliderRow(_audioContent.transform, "MasterVolume", "Master Volume", s);
            (_musicVolumeSlider, _musicVolumeValue) = BuildSliderRow(_audioContent.transform, "MusicVolume", "Music Volume", s);
            (_sfxVolumeSlider, _sfxVolumeValue) = BuildSliderRow(_audioContent.transform, "SfxVolume", "SFX Volume", s);
            (_voiceVolumeSlider, _voiceVolumeValue) = BuildSliderRow(_audioContent.transform, "VoiceVolume", "Voice Volume", s);
            _muteAllToggle = BuildToggleRow(_audioContent.transform, "MuteAll", "Mute All");
            _muteMusicToggle = BuildToggleRow(_audioContent.transform, "MuteMusic", "Mute Music");
            _muteSfxToggle = BuildToggleRow(_audioContent.transform, "MuteSfx", "Mute SFX");

            // Video
            _resolutionDropdown = BuildDropdownRow(_videoContent.transform, "Resolution", "Resolution", s);
            _qualityDropdown = BuildDropdownRow(_videoContent.transform, "Quality", "Quality", s);
            _fullscreenToggle = BuildToggleRow(_videoContent.transform, "Fullscreen", "Fullscreen");
            _vsyncToggle = BuildToggleRow(_videoContent.transform, "Vsync", "V-Sync");
            _showFpsToggle = BuildToggleRow(_videoContent.transform, "ShowFps", "Show FPS");
            _targetFpsDropdown = BuildDropdownRow(_videoContent.transform, "TargetFps", "Target FPS", s);

            // Display
            (_brightnessSlider, _brightnessValue) = BuildSliderRow(_displayContent.transform, "Brightness", "Brightness", s);
            (_gammaSlider, _gammaValue) = BuildSliderRow(_displayContent.transform, "Gamma", "Gamma", s);
            (_uiScaleSlider, _uiScaleValue) = BuildSliderRow(_displayContent.transform, "UiScale", "UI Scale", s);
            _colorblindModeToggle = BuildToggleRow(_displayContent.transform, "ColorblindMode", "Colorblind Mode");
            _colorblindTypeDropdown = BuildDropdownRow(_displayContent.transform, "ColorblindType", "Colorblind Type", s);

            // Controls
            (_mouseSensitivitySlider, _mouseSensitivityValue) = BuildSliderRow(_controlsContent.transform, "MouseSensitivity", "Mouse Sensitivity", s);
            _invertYAxisToggle = BuildToggleRow(_controlsContent.transform, "InvertYAxis", "Invert Y-Axis");

            // Game
            _languageDropdown = BuildDropdownRow(_gameContent.transform, "Language", "Language", s);
            _autoSaveToggle = BuildToggleRow(_gameContent.transform, "AutoSave", "Auto-Save");
            _showTutorialsToggle = BuildToggleRow(_gameContent.transform, "ShowTutorials", "Show Tutorials");
            _screenShakeToggle = BuildToggleRow(_gameContent.transform, "ScreenShake", "Screen Shake");
            _matureContentToggle = BuildToggleRow(_gameContent.transform, "MatureContent", "Mature Content");

            _videoContent.gameObject.SetActive(false);
            _displayContent.gameObject.SetActive(false);
            _controlsContent.gameObject.SetActive(false);
            _gameContent.gameObject.SetActive(false);

            // Footer
            var footer = new HorizontalLayoutBuilder("Footer")
                .WithSpacing(10f)
                .WithPadding(15, 15, 0, 0)
                .WithAlignment(TextAnchor.MiddleRight)
                .WithBackground(s.FooterColor)
                .WithAnchors(Vector2.zero, new Vector2(1, 0))
                .WithPreferredSize(null, s.FooterHeight)
                .Build(_modalWindow.transform);
            var footerRect = footer.GetComponent<RectTransform>();
            footerRect.sizeDelta = new Vector2(0, s.FooterHeight);
            footerRect.pivot = new Vector2(0.5f, 0f);

            _cancelButton = new ButtonBuilder("CancelButton")
                .WithLabel("Cancel")
                .Ghost()
                .WithPreferredSize(null, 36f)
                .Build(footer.transform);

            _resetButton = new ButtonBuilder("ResetButton")
                .WithLabel("Reset")
                .WithVariant(ButtonVariant.Outline)
                .WithPreferredSize(null, 36f)
                .Build(footer.transform);

            _applyButton = new ButtonBuilder("ApplyButton")
                .WithLabel("Apply")
                .Primary()
                .WithPreferredSize(null, 36f)
                .Build(footer.transform);
        }

        private static GameObject BuildTab(Transform parent, string label)
        {
            var toggle = new ToggleBuilder(label + "Tab")
                .WithLabel(label)
                .Themed()
                .Build(parent);
            return toggle.gameObject;
        }

        private static VerticalLayoutGroup BuildTabContent(Transform parent, string name, SettingsViewBuilderSettings s)
        {
            var content = new VerticalLayoutBuilder(name)
                .WithSpacing(s.RowSpacing)
                .WithPadding((int)s.ContentPadding)
                .WithAlignment(TextAnchor.UpperLeft)
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(parent);
            var rect = content.GetComponent<RectTransform>();
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return content;
        }

        private static (Slider slider, TextMeshProUGUI valueText) BuildSliderRow(Transform parent, string id, string label, SettingsViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder(id + "Row")
                .WithSpacing(10f)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithPreferredSize(null, s.RowHeight)
                .Build(parent);

            new TextBuilder(id + "Label")
                .WithText(label)
                .WithFontSize(s.LabelFontSize)
                .WithColor(s.LabelColor)
                .WithPreferredSize(s.LabelWidth, null)
                .Build(row.transform);

            var slider = new SliderBuilder(id + "Slider")
                .WithRange(0f, 1f)
                .Themed()
                .Build(row.transform);

            var valueText = new TextBuilder(id + "Value")
                .WithText("100%")
                .WithFontSize(s.ValueFontSize)
                .WithColor(s.ValueColor)
                .Right()
                .WithPreferredSize(60f, null)
                .Build(row.transform);

            return (slider, valueText);
        }

        private static GameObject BuildToggleRow(Transform parent, string id, string label)
        {
            var row = new HorizontalLayoutBuilder(id + "Row")
                .WithSpacing(10f)
                .WithAlignment(TextAnchor.MiddleLeft)
                .Build(parent);

            new TextBuilder(id + "Label")
                .WithText(label)
                .Build(row.transform);

            var toggle = new ToggleBuilder(id + "Toggle")
                .Themed()
                .Build(row.transform);

            return toggle.gameObject;
        }

        private static GameObject BuildDropdownRow(Transform parent, string id, string label, SettingsViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder(id + "Row")
                .WithSpacing(10f)
                .WithAlignment(TextAnchor.MiddleLeft)
                .Build(parent);

            new TextBuilder(id + "Label")
                .WithText(label)
                .Build(row.transform);

            var dropdown = new DropdownBuilder(id + "Dropdown")
                .Themed()
                .Build(row.transform);

            return dropdown.gameObject;
        }

        protected override void WireFields(MVVM.Views.Settings.SettingsView view)
        {
            var so = new SerializedObject(view);

            SerializationHelper.Wire(so, "_dimBackground", _dimBackground.GetComponent<Button>());
            SerializationHelper.Wire(so, "_closeButton", _closeButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_modalWindow", _modalWindow);

            SerializationHelper.Wire(so, "_tabAudio", _tabAudio.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabVideo", _tabVideo.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabDisplay", _tabDisplay.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabControls", _tabControls.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_tabGame", _tabGame.GetComponent<Toggle>());

            SerializationHelper.Wire(so, "_audioContent", _audioContent);
            SerializationHelper.Wire(so, "_videoContent", _videoContent);
            SerializationHelper.Wire(so, "_displayContent", _displayContent);
            SerializationHelper.Wire(so, "_controlsContent", _controlsContent);
            SerializationHelper.Wire(so, "_gameContent", _gameContent);

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

            SerializationHelper.Wire(so, "_resolutionDropdown", _resolutionDropdown.GetComponent<TMP_Dropdown>());
            SerializationHelper.Wire(so, "_qualityDropdown", _qualityDropdown.GetComponent<TMP_Dropdown>());
            SerializationHelper.Wire(so, "_fullscreenToggle", _fullscreenToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_vsyncToggle", _vsyncToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_showFpsToggle", _showFpsToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_targetFpsDropdown", _targetFpsDropdown.GetComponent<TMP_Dropdown>());

            SerializationHelper.Wire(so, "_brightnessSlider", _brightnessSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_brightnessValue", _brightnessValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_gammaSlider", _gammaSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_gammaValue", _gammaValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_uiScaleSlider", _uiScaleSlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_uiScaleValue", _uiScaleValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_colorblindModeToggle", _colorblindModeToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_colorblindTypeDropdown", _colorblindTypeDropdown.GetComponent<TMP_Dropdown>());

            SerializationHelper.Wire(so, "_mouseSensitivitySlider", _mouseSensitivitySlider.GetComponent<Slider>());
            SerializationHelper.Wire(so, "_mouseSensitivityValue", _mouseSensitivityValue.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_invertYAxisToggle", _invertYAxisToggle.GetComponent<Toggle>());

            SerializationHelper.Wire(so, "_languageDropdown", _languageDropdown.GetComponent<TMP_Dropdown>());
            SerializationHelper.Wire(so, "_autoSaveToggle", _autoSaveToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_showTutorialsToggle", _showTutorialsToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_screenShakeToggle", _screenShakeToggle.GetComponent<Toggle>());
            SerializationHelper.Wire(so, "_matureContentToggle", _matureContentToggle.GetComponent<Toggle>());

            SerializationHelper.Wire(so, "_applyButton", _applyButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_resetButton", _resetButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_cancelButton", _cancelButton.GetComponent<Button>());

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
