#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.MVVM.Views.MainMenu;
using Core.UI.Controls;
using UnityEngine;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Builders.MainMenu
{
    public class MainMenuViewBuilder : PanelViewBuilder<MainMenuView>
    {
        public MainMenuViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/MainMenuView.prefab";

        private GameObject _titleBlock;
        private GameObject _menuBlock;
        private GameObject _footerBlock;

        private BaseButton _counterButton;
        private BaseButton _settingsButton;
        private BaseButton _inventoryButton;
        private BaseButton _multiplayerButton;
        private BaseButton _quitButton;

        protected override void BuildPanelStructure()
        {
            var s = Settings;

            BuildTitleBlock(s);
            BuildMenuBlock(s);
            BuildFooterBlock(s);
        }

        protected override void ApplyPanelAnimators()
        {
            AddAnimator(_titleBlock, 0, "Preset_Fade");
            AddAnimator(_menuBlock, 1, "Preset_FadeScaleBounce");
            AddAnimator(_footerBlock, 2, "Preset_Fade");
        }

        private void BuildTitleBlock(MainMenuViewBuilderSettings s)
        {
            var titleLayout = new VerticalLayoutBuilder("TitleBlock")
                .WithSpacing(SpacingSize.SM)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithAnchors(s.TitleBlockAnchorMin, s.TitleBlockAnchorMax)
                .Build(Root);

            _titleBlock = titleLayout.gameObject;
            StretchToAnchors(_titleBlock);

            new TextBuilder("Title")
                .WithText("MVVM TEMPLATE")
                .WithFontSize(s.TitleFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .Centered()
                .NoAutoSize()
                .WithPreferredSize(null, s.TitleHeight)
                .Build(_titleBlock.transform);

            new TextBuilder("Subtitle")
                .WithText("Unity 6 · MVVM + CQRS Examples")
                .BodyLarge()
                .Centered()
                .Secondary()
                .NoAutoSize()
                .WithPreferredSize(null, s.SubtitleHeight)
                .Build(_titleBlock.transform);
        }

        private void BuildMenuBlock(MainMenuViewBuilderSettings s)
        {
            var menuLayout = new VerticalLayoutBuilder("MenuBlock")
                .WithSpacing(SpacingSize.SM)
                .WithAlignment(TextAnchor.MiddleCenter)
                .ForceExpand(false, false)
                .WithAnchors(s.MenuBlockAnchorMin, s.MenuBlockAnchorMax)
                .Build(Root);

            _menuBlock = menuLayout.gameObject;
            StretchToAnchors(_menuBlock);

            _counterButton = new ButtonBuilder("CounterButton")
                .WithLabel("COUNTER")
                .WithLabelSize(s.MenuButtonFontSize)
                .Outline()
                .WithPreferredSize(s.ButtonWidth, s.MenuButtonHeight)
                .Build(_menuBlock.transform);

            _settingsButton = new ButtonBuilder("SettingsButton")
                .WithLabel("SETTINGS")
                .WithLabelSize(s.MenuButtonFontSize)
                .Outline()
                .WithPreferredSize(s.ButtonWidth, s.MenuButtonHeight)
                .Build(_menuBlock.transform);

            _inventoryButton = new ButtonBuilder("InventoryButton")
                .WithLabel("INVENTORY")
                .WithLabelSize(s.MenuButtonFontSize)
                .Outline()
                .WithPreferredSize(s.ButtonWidth, s.MenuButtonHeight)
                .Build(_menuBlock.transform);

            _multiplayerButton = new ButtonBuilder("MultiplayerButton")
                .WithLabel("MULTIPLAYER")
                .WithLabelSize(s.PlayButtonFontSize)
                .Primary()
                .WithPreferredSize(s.ButtonWidth, s.PlayButtonHeight)
                .Build(_menuBlock.transform);

            _quitButton = new ButtonBuilder("QuitButton")
                .WithLabel("QUIT")
                .WithLabelSize(s.MenuButtonFontSize)
                .Ghost()
                .WithPreferredSize(s.ButtonWidth, s.MenuButtonHeight)
                .Build(_menuBlock.transform);
        }

        private void BuildFooterBlock(MainMenuViewBuilderSettings s)
        {
            var footerLayout = new HorizontalLayoutBuilder("FooterBlock")
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPadding((int)s.FooterSideMargin, (int)s.FooterSideMargin, 0, 0)
                .WithAnchors(Vector2.zero, new Vector2(1f, 0f))
                .Build(Root);

            _footerBlock = footerLayout.gameObject;

            var footerRect = _footerBlock.GetComponent<RectTransform>();
            footerRect.pivot = new Vector2(0.5f, 0f);
            footerRect.sizeDelta = new Vector2(0f, s.FooterHeight);
            footerRect.anchoredPosition = new Vector2(0f, s.FooterBottomMargin);

            new TextBuilder("BuildLabel")
                .WithText("Open BootstrapScene → Play")
                .Caption()
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .NoAutoSize()
                .WithPreferredSize(s.FooterLabelWidth, s.FooterHeight)
                .Build(_footerBlock.transform);

            new SpacerBuilder("FooterSpacer")
                .SetFlexibleWidth(1f)
                .Build(_footerBlock.transform);

            new TextBuilder("VersionLabel")
                .WithText("v1.0.0")
                .Caption()
                .WithAlignment(TextAlignmentOptions.MidlineRight)
                .NoAutoSize()
                .WithPreferredSize(s.FooterLabelWidth, s.FooterHeight)
                .Build(_footerBlock.transform);
        }

        private static void StretchToAnchors(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        protected override void WireFields(MainMenuView view)
        {
            WireAll(view,
                ("_counterButton", _counterButton),
                ("_settingsButton", _settingsButton),
                ("_inventoryButton", _inventoryButton),
                ("_multiplayerButton", _multiplayerButton),
                ("_quitButton", _quitButton));
        }
    }
}
#endif
