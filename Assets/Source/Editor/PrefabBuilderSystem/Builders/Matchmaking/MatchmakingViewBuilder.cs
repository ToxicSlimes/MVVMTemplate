#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Presentation.MVVM.Views.Matchmaking;
using Core.UI.Controls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Builders.Matchmaking
{
    public class MatchmakingViewBuilder : PanelViewBuilder<MatchmakingView>
    {
        public MatchmakingViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/MatchmakingView.prefab";

        private GameObject _headerRow;
        private GameObject _cardsRow;
        private GameObject _sessionsPanel;

        private BaseButton _backButton;
        private BaseInputField _nicknameInput;
        private BaseButton _createButton;
        private BaseInputField _codeInput;
        private BaseButton _joinButton;
        private BaseButton _refreshButton;
        private BaseText _statusText;
        private RectTransform _sessionsContent;
        private GameObject _sessionRowTemplate;

        protected override void BuildPanelStructure()
        {
            var s = Settings;

            BuildHeader(s);
            BuildCards(s);
            BuildSessionsPanel(s);
            BuildStatus(s);
        }

        protected override void ApplyPanelAnimators()
        {
            AddAnimator(_headerRow, 0, "Preset_Fade");
            AddAnimator(_cardsRow, 1, "Preset_FadeScaleBounce");
            AddAnimator(_sessionsPanel, 2, "Preset_Fade");
        }

        private void BuildHeader(MatchmakingViewBuilderSettings s)
        {
            var header = new HorizontalLayoutBuilder("Header")
                .WithSpacing(SpacingSize.LG)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithAnchors(new Vector2(0f, 1f), Vector2.one)
                .WithPivot(new Vector2(0.5f, 1f))
                .WithPosition(new Vector2(0f, -s.HeaderTopOffset))
                .WithSize(new Vector2(-s.SideMargin * 2f, s.HeaderHeight))
                .Build(Root);

            _headerRow = header.gameObject;

            _backButton = new ButtonBuilder("BackButton")
                .WithLabel("<")
                .WithLabelSize(s.BackButtonFontSize)
                .IconButton()
                .WithPreferredSize(s.BackButtonSize, s.BackButtonSize)
                .Build(_headerRow.transform);

            new TextBuilder("Title")
                .WithText("MULTIPLAYER")
                .WithFontSize(s.TitleFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .NoAutoSize()
                .WithPreferredSize(s.TitleWidth, s.HeaderHeight)
                .Build(_headerRow.transform);

            new SpacerBuilder("Header")
                .SetFlexibleWidth(1f)
                .Build(_headerRow.transform);

            new TextBuilder("CallsignLabel")
                .WithText("Callsign")
                .Caption()
                .WithAlignment(TextAlignmentOptions.MidlineRight)
                .NoAutoSize()
                .WithPreferredSize(s.CallsignLabelWidth, s.HeaderHeight)
                .Build(_headerRow.transform);

            _nicknameInput = new InputFieldBuilder("NicknameInput")
                .WithPlaceholder("Callsign")
                .WithFontSize(s.NicknameFontSize)
                .WithCharacterLimit(s.NicknameCharacterLimit)
                .Themed()
                .WithPreferredSize(s.NicknameInputWidth, s.NicknameInputHeight)
                .Build(_headerRow.transform);
        }

        private void BuildCards(MatchmakingViewBuilderSettings s)
        {
            var cardsWidth = s.CardWidth * 2f + DesignSystemProvider.GetSpacing(SpacingSize.LG);

            var cards = new HorizontalLayoutBuilder("Cards")
                .WithSpacing(SpacingSize.LG)
                .WithAlignment(TextAnchor.UpperLeft)
                .WithAnchors(new Vector2(0f, 1f), new Vector2(0f, 1f))
                .WithPivot(new Vector2(0f, 1f))
                .WithPosition(new Vector2(s.SideMargin, -s.CardsTopOffset))
                .WithSize(new Vector2(cardsWidth, s.CardHeight))
                .Build(Root);

            _cardsRow = cards.gameObject;

            BuildCreateCard(s);
            BuildJoinCard(s);
        }

        private void BuildCreateCard(MatchmakingViewBuilderSettings s)
        {
            var card = new CardBuilder("Create")
                .Themed()
                .WithBorder(DesignSystemProvider.Colors.indigo, s.CardBorderWidth)
                .Vertical()
                .WithPadding((int)DesignSystemProvider.GetSpacing(SpacingSize.XL))
                .WithSpacing(DesignSystemProvider.GetSpacing(SpacingSize.MD))
                .WithPreferredSize(s.CardWidth, s.CardHeight)
                .Build(_cardsRow.transform);

            var content = card.transform;

            new TextBuilder("CreateTitle")
                .WithText("CREATE")
                .WithFontSize(s.CardTitleFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .NoAutoSize()
                .WithPreferredSize(null, s.CardTitleHeight)
                .Build(content);

            new TextBuilder("CreateHint")
                .WithText("You host the room. The code appears on the next screen.")
                .Body()
                .Secondary()
                .WithAlignment(TextAlignmentOptions.TopLeft)
                .NoAutoSize()
                .WithPreferredSize(null, s.CardHintHeight)
                .Build(content);

            new SpacerBuilder("Create")
                .SetFlexibleHeight(1f)
                .Build(content);

            _createButton = new ButtonBuilder("CreateButton")
                .WithLabel("CREATE ROOM")
                .WithLabelSize(s.CardButtonFontSize)
                .Primary()
                .WithPreferredSize(null, s.CardButtonHeight)
                .Build(content);
        }

        private void BuildJoinCard(MatchmakingViewBuilderSettings s)
        {
            var card = new CardBuilder("Join")
                .Themed()
                .WithBorder(DesignSystemProvider.Colors.cyan, s.CardBorderWidth)
                .Vertical()
                .WithPadding((int)DesignSystemProvider.GetSpacing(SpacingSize.XL))
                .WithSpacing(DesignSystemProvider.GetSpacing(SpacingSize.MD))
                .WithPreferredSize(s.CardWidth, s.CardHeight)
                .Build(_cardsRow.transform);

            var content = card.transform;

            new TextBuilder("JoinTitle")
                .WithText("JOIN BY CODE")
                .WithFontSize(s.CardTitleFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .NoAutoSize()
                .WithPreferredSize(null, s.CardTitleHeight)
                .Build(content);

            _codeInput = new InputFieldBuilder("CodeInput")
                .WithPlaceholder("CODE")
                .WithFontSize(s.CodeInputFontSize)
                .WithCharacterLimit(s.CodeCharacterLimit)
                .Themed()
                .WithPreferredSize(s.CodeInputWidth, s.CodeInputHeight)
                .Build(content);

            ApplyCodeInputStyle(_codeInput);

            new SpacerBuilder("Join")
                .SetFlexibleHeight(1f)
                .Build(content);

            _joinButton = new ButtonBuilder("JoinButton")
                .WithLabel("JOIN")
                .WithLabelSize(s.CardButtonFontSize)
                .Secondary()
                .WithOutline(DesignSystemProvider.Colors.cyan)
                .WithPreferredSize(null, s.CardButtonHeight)
                .Build(content);
        }

        private void BuildSessionsPanel(MatchmakingViewBuilderSettings s)
        {
            var panel = new VerticalLayoutBuilder("Sessions")
                .WithSpacing(0f)
                .WithAlignment(TextAnchor.UpperLeft)
                .WithAnchors(new Vector2(0f, 1f), new Vector2(0f, 1f))
                .WithPivot(new Vector2(0f, 1f))
                .WithPosition(new Vector2(s.SessionsLeft, -s.SessionsTopOffset))
                .WithSize(new Vector2(s.SessionsWidth, s.SessionsPanelHeight))
                .Build(Root);

            _sessionsPanel = panel.gameObject;

            BuildSessionsHeader(s);

            new SpacerBuilder("DividerTop")
                .Height(s.DividerTopGap)
                .Build(_sessionsPanel.transform);

            new DividerBuilder("Sessions")
                .Horizontal()
                .WithThickness(s.DividerThickness)
                .Themed()
                .Build(_sessionsPanel.transform);

            new SpacerBuilder("DividerBottom")
                .Height(s.DividerBottomGap)
                .Build(_sessionsPanel.transform);

            BuildSessionsList(s);
        }

        private void BuildSessionsHeader(MatchmakingViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder("SessionsHeader")
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithPreferredSize(null, s.SessionsHeaderHeight)
                .Build(_sessionsPanel.transform);

            new TextBuilder("SessionsLabel")
                .WithText("OPEN ROOMS")
                .Caption()
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .NoAutoSize()
                .WithPreferredSize(s.SessionsLabelWidth, s.SessionsHeaderHeight)
                .Build(row.transform);

            new SpacerBuilder("Sessions")
                .SetFlexibleWidth(1f)
                .Build(row.transform);

            _refreshButton = new ButtonBuilder("RefreshButton")
                .WithLabel("REFRESH")
                .WithLabelSize(s.RefreshButtonFontSize)
                .Ghost()
                .WithPreferredSize(s.RefreshButtonWidth, s.RefreshButtonHeight)
                .Build(row.transform);
        }

        private void BuildSessionsList(MatchmakingViewBuilderSettings s)
        {
            var scrollView = new ScrollViewBuilder("Scroll_Sessions")
                .Vertical()
                .WithPreferredHeight(s.ScrollHeight)
                .Build(_sessionsPanel.transform);

            _sessionsContent = scrollView.Content;

            var contentLayout = _sessionsContent.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = s.RowSpacing;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = _sessionsContent.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _sessionRowTemplate = BuildSessionRowTemplate(s);
        }

        private GameObject BuildSessionRowTemplate(MatchmakingViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder("SessionRow")
                .DarkSurface()
                .WithSpacing(SpacingSize.MD)
                .WithAlignment(TextAnchor.MiddleLeft)
                .WithPadding((int)s.RowSidePadding, (int)s.RowSidePadding, 0, 0)
                .WithSize(new Vector2(s.SessionsWidth, s.RowHeight))
                .Build(_sessionsContent);

            new TextBuilder("SessionInfo")
                .WithText("K7M2QX   2/4")
                .WithFontSize(s.RowFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .NoAutoSize()
                .WithPreferredSize(s.RowInfoWidth, s.RowHeight)
                .Build(row.transform);

            new SpacerBuilder("Row")
                .SetFlexibleWidth(1f)
                .Build(row.transform);

            new ButtonBuilder("RowJoinButton")
                .WithLabel("JOIN")
                .WithLabelSize(s.RowButtonFontSize)
                .Ghost()
                .WithOutline(DesignSystemProvider.Colors.cyan)
                .WithPreferredSize(s.RowJoinButtonWidth, s.RowJoinButtonHeight)
                .Build(row.transform);

            return row.gameObject;
        }

        private void BuildStatus(MatchmakingViewBuilderSettings s)
        {
            _statusText = new TextBuilder("StatusText")
                .WithText("Ready to connect")
                .Body()
                .Tertiary()
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .NoAutoSize()
                .WithAnchors(Vector2.zero, Vector2.zero)
                .WithPivot(Vector2.zero)
                .WithPosition(new Vector2(s.SideMargin, s.StatusBottomOffset))
                .WithSize(new Vector2(s.StatusWidth, s.StatusHeight))
                .Build(Root);
        }

        private static void ApplyCodeInputStyle(BaseInputField input)
        {
            input.textComponent.alignment = TextAlignmentOptions.Center;
            input.textComponent.fontStyle |= FontStyles.UpperCase;

            if (input.placeholder is TextMeshProUGUI placeholder)
            {
                placeholder.alignment = TextAlignmentOptions.Center;
                placeholder.fontStyle |= FontStyles.UpperCase;
            }
        }

        protected override void WireFields(MatchmakingView view)
        {
            WireAll(view,
                ("_backButton", _backButton),
                ("_nicknameInput", _nicknameInput),
                ("_createButton", _createButton),
                ("_codeInput", _codeInput),
                ("_joinButton", _joinButton),
                ("_refreshButton", _refreshButton),
                ("_statusText", _statusText),
                ("_sessionsContent", _sessionsContent),
                ("_sessionRowTemplate", _sessionRowTemplate));
        }
    }
}
#endif
