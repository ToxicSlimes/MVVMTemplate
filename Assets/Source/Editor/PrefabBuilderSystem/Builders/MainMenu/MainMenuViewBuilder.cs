#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.MainView;
using Core.MVVM.Views.MainMenu;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Builders.MainMenu
{
    public class MainMenuViewBuilder : ViewBuilder<MainMenuView>
    {
        public MainViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/MainMenuView.prefab";

        private Button _playButton;
        private Button _settingsButton;
        private Button _quitButton;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Background
            new PanelBuilder("Background")
                .WithBackgroundColor(s.BgColor)
                .WithAnchors(Vector2.zero, Vector2.one)
                .Build(Root);

            // Center container
            var centerPanel = new VerticalLayoutBuilder("CenterPanel")
                .WithSpacing(s.ButtonSpacing)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPadding((int)s.ContentPadding)
                .WithBackground(s.PanelColor)
                .WithAnchoredPosition(Vector2.zero)
                .WithAnchors(new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f))
                .Build(Root);

            // Title
            new TextBuilder("Title")
                .WithText("MVVM Template")
                .WithFontSize(s.TitleFontSize)
                .WithColor(s.TitleColor)
                .Bold()
                .Centered()
                .WithPreferredSize(null, 80f)
                .Build(centerPanel.transform);

            // Subtitle
            new TextBuilder("Subtitle")
                .WithText("Clean Architecture Starter Kit")
                .WithFontSize(s.SubtitleFontSize)
                .WithColor(s.SubtitleColor)
                .Centered()
                .WithPreferredSize(null, 30f)
                .Build(centerPanel.transform);

            // Spacer
            new SpacerBuilder("Spacer")
                .Height(20f)
                .Build(centerPanel.transform);

            // Play Button
            _playButton = new ButtonBuilder("PlayButton")
                .WithLabel("Play")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.ButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(centerPanel.transform);

            // Settings Button
            _settingsButton = new ButtonBuilder("SettingsButton")
                .WithLabel("Settings")
                .WithLabelSize(s.ButtonFontSize)
                .WithVariant(ButtonVariant.Outline)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(centerPanel.transform);

            // Quit Button
            _quitButton = new ButtonBuilder("QuitButton")
                .WithLabel("Quit")
                .WithLabelSize(s.ButtonFontSize)
                .Ghost()
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(centerPanel.transform);
        }

        protected override void WireFields(MainMenuView view)
        {
            var so = new SerializedObject(view);
            SerializationHelper.Wire(so, "_playButton", _playButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_settingsButton", _settingsButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_quitButton", _quitButton.GetComponent<Button>());
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
