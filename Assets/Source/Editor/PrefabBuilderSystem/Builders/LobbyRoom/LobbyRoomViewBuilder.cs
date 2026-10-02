#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Elements.GameUI;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Presentation.MVVM.Views.LobbyRoom;
using Core.UI.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Builders.LobbyRoom
{
    public class LobbyRoomViewBuilder : PanelViewBuilder<LobbyRoomView>
    {
        private const int SlotCount = 4;

        public LobbyRoomViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/LobbyRoomView.prefab";

        private GameObject _roomCodeBlock;
        private GameObject _rosterRow;
        private GameObject _footerBar;

        private BaseText _roomCodeText;
        private BaseButton _copyButton;

        private readonly GameObject[] _slotRoots = new GameObject[SlotCount];
        private readonly Image[] _slotAvatars = new Image[SlotCount];
        private readonly BaseText[] _slotNames = new BaseText[SlotCount];
        private readonly BaseText[] _slotRoles = new BaseText[SlotCount];
        private readonly GameObject[] _slotReadyBadges = new GameObject[SlotCount];

        private BaseToggle _readyToggle;
        private BaseButton _startButton;
        private BaseButton _addBotButton;
        private BaseText _startButtonLabel;
        private BaseButton _leaveButton;

        protected override void BuildPanelStructure()
        {
            var s = Settings;

            BuildRoomCodeBlock(s);
            BuildRosterRow(s);
            BuildFooterBar(s);
        }

        protected override void ApplyPanelAnimators()
        {
            AddAnimator(_roomCodeBlock, 0, "Preset_Fade");
            AddAnimator(_rosterRow, 1, "Preset_FadeScaleBounce");
            AddAnimator(_footerBar, 2, "Preset_Fade");
        }

        private void BuildRoomCodeBlock(LobbyRoomViewBuilderSettings s)
        {
            var block = new VerticalLayoutBuilder("RoomCodeBlock")
                .WithSpacing(SpacingSize.XS)
                .WithAlignment(TextAnchor.UpperCenter)
                .ForceExpand(false, false)
                .WithAnchors(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f))
                .WithPivot(new Vector2(0.5f, 1f))
                .WithPosition(new Vector2(0f, -s.RoomCodeTopOffset))
                .WithSize(new Vector2(s.RoomCodeBlockWidth, s.RoomCodeBlockHeight))
                .Build(Root);

            _roomCodeBlock = block.gameObject;

            new TextBuilder("RoomCodeLabel")
                .WithText("ROOM CODE")
                .Caption()
                .Centered()
                .NoAutoSize()
                .WithPreferredSize(null, s.RoomCodeLabelHeight)
                .Build(_roomCodeBlock.transform);

            var row = new HorizontalLayoutBuilder("RoomCodeRow")
                .WithSpacing(s.RoomCodeRowSpacing)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPreferredSize(null, s.RoomCodeRowHeight)
                .Build(_roomCodeBlock.transform);

            _roomCodeText = new TextBuilder("RoomCode")
                .WithText("K7M2QX")
                .WithFontSize(s.RoomCodeFontSize)
                .WithColor(DesignSystemProvider.Colors.cyan)
                .Bold()
                .Centered()
                .NoAutoSize()
                .WithPreferredSize(s.RoomCodeTextWidth, s.RoomCodeRowHeight)
                .Build(row.transform);

            _copyButton = new ButtonBuilder("CopyButton")
                .WithLabel("COPY")
                .WithLabelSize(s.CopyButtonFontSize)
                .IconButton()
                .WithPreferredSize(s.CopyButtonSize, s.CopyButtonSize)
                .Build(row.transform);
        }

        private void BuildRosterRow(LobbyRoomViewBuilderSettings s)
        {
            var row = new HorizontalLayoutBuilder("RosterRow")
                .WithSpacing(SpacingSize.LG)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithAnchors(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f))
                .WithPivot(new Vector2(0.5f, 1f))
                .WithPosition(new Vector2(0f, -s.RosterTopOffset))
                .WithSize(new Vector2(s.RosterRowWidth, s.SlotHeight))
                .Build(Root);

            _rosterRow = row.gameObject;

            for (var i = 0; i < SlotCount; i++)
                BuildSlot(s, i);
        }

        private void BuildSlot(LobbyRoomViewBuilderSettings s, int index)
        {
            var slot = new ImageBuilder($"Slot{index}")
                .WithColor(Color.clear)
                .NoRaycast()
                .WithPreferredSize(s.SlotWidth, s.SlotHeight)
                .Build(_rosterRow.transform);

            BuildEmptySlot(s, index, slot.transform);
            BuildOccupiedSlot(s, index, slot.transform);
        }

        private void BuildEmptySlot(LobbyRoomViewBuilderSettings s, int index, Transform parent)
        {
            var card = new CardBuilder($"Card_Empty{index}")
                .WithBackgroundColor(DesignSystemProvider.Colors.darkSurface)
                .WithBorder(DesignSystemProvider.Colors.cardBorder)
                .Vertical()
                .WithPadding((int)DesignSystemProvider.GetSpacing(SpacingSize.LG))
                .WithStretch()
                .Build(parent);

            new TextBuilder("EmptyLabel")
                .WithText("EMPTY")
                .WithFontSize(s.EmptySlotFontSize)
                .WithColor(DesignSystemProvider.Colors.textDisabled)
                .Centered()
                .NoAutoSize()
                .WithFlexible(null, 1f)
                .Build(card.transform);
        }

        private void BuildOccupiedSlot(LobbyRoomViewBuilderSettings s, int index, Transform parent)
        {
            var card = new CardBuilder($"Card_Slot{index}")
                .WithBackgroundColor(DesignSystemProvider.Colors.darkSurface)
                .WithBorder(DesignSystemProvider.Colors.purple, s.SlotBorderWidth)
                .Vertical()
                .WithPadding((int)DesignSystemProvider.GetSpacing(SpacingSize.LG))
                .WithSpacing(DesignSystemProvider.GetSpacing(SpacingSize.MD))
                .WithStretch()
                .Build(parent);

            _slotRoots[index] = card.gameObject;

            var avatarRow = new HorizontalLayoutBuilder("AvatarRow")
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPreferredSize(null, s.AvatarSize)
                .Build(card.transform);

            _slotAvatars[index] = new ImageBuilder("Avatar")
                .WithColor(GetSlotColor(index))
                .NoRaycast()
                .WithPreferredSize(s.AvatarSize, s.AvatarSize)
                .Build(avatarRow.transform);

            _slotNames[index] = new TextBuilder("PlayerName")
                .WithText($"PLAYER {index + 1}")
                .WithFontSize(s.SlotNameFontSize)
                .WithColor(DesignSystemProvider.Colors.textPrimary)
                .Bold()
                .Centered()
                .NoAutoSize()
                .WithPreferredSize(null, s.SlotNameHeight)
                .Build(card.transform);

            _slotRoles[index] = new TextBuilder("PlayerRole")
                .WithText(index == 0 ? "HOST" : "PLAYER")
                .Caption()
                .Centered()
                .NoAutoSize()
                .WithPreferredSize(null, s.SlotRoleHeight)
                .Build(card.transform);

            new SpacerBuilder("Slot")
                .SetFlexibleHeight(1f)
                .Build(card.transform);

            _slotReadyBadges[index] = new BadgeBuilder("Badge_Ready")
                .WithText("READY")
                .WithSize(s.ReadyBadgeHeight)
                .WithFontSize(s.ReadyBadgeFontSize)
                .WithBackgroundColor(DesignSystemProvider.Colors.elevatedSurface)
                .WithTextColor(DesignSystemProvider.Colors.cyan)
                .WithOutline(DesignSystemProvider.Colors.cyan, s.ReadyBadgeBorderWidth)
                .Build(card.transform)
                .gameObject;
        }

        private void BuildFooterBar(LobbyRoomViewBuilderSettings s)
        {
            var footer = new HorizontalLayoutBuilder("FooterBar")
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithAnchors(Vector2.zero, new Vector2(1f, 0f))
                .WithPivot(new Vector2(0.5f, 0f))
                .WithPosition(new Vector2(0f, s.FooterBottomOffset))
                .WithSize(new Vector2(-s.FooterSideMargin * 2f, s.FooterHeight))
                .Build(Root);

            _footerBar = footer.gameObject;

            _leaveButton = new ButtonBuilder("LeaveButton")
                .WithLabel("LEAVE")
                .WithLabelSize(s.FooterButtonFontSize)
                .Danger()
                .WithPreferredSize(s.LeaveButtonWidth, s.FooterHeight)
                .Build(_footerBar.transform);

            new SpacerBuilder("FooterLeft")
                .SetFlexibleWidth(1f)
                .Build(_footerBar.transform);

            _readyToggle = new ToggleBuilder("ReadyToggle")
                .WithLabel("I'm ready")
                .WithLabelSize(s.ReadyToggleFontSize)
                .WithToggleSize(s.ReadyToggleSize)
                .Themed()
                .WithPreferredSize(s.ReadyToggleWidth, s.FooterHeight)
                .Build(_footerBar.transform);

            new SpacerBuilder("FooterRight")
                .SetFlexibleWidth(1f)
                .Build(_footerBar.transform);

            _addBotButton = new ButtonBuilder("AddBotButton")
                .WithLabel("ADD BOT")
                .WithLabelSize(s.FooterButtonFontSize)
                .Secondary()
                .WithPreferredSize(s.AddBotButtonWidth, s.FooterHeight)
                .Build(_footerBar.transform);

            _startButton = new ButtonBuilder("StartButton")
                .WithLabel("START")
                .WithLabelSize(s.FooterButtonFontSize)
                .Primary()
                .WithPreferredSize(s.StartButtonWidth, s.FooterHeight)
                .Build(_footerBar.transform);

            _startButtonLabel = _startButton.GetComponentInChildren<BaseText>();
        }

        private static Color GetSlotColor(int index)
        {
            var colors = DesignSystemProvider.Colors;

            return index switch
            {
                0 => colors.indigo,
                1 => colors.cyan,
                2 => colors.purple,
                _ => colors.pink
            };
        }

        protected override void WireFields(LobbyRoomView view)
        {
            WireAll(view, so =>
            {
                SerializationHelper.Wire(so, "_roomCodeText", _roomCodeText);
                SerializationHelper.Wire(so, "_copyButton", _copyButton);

                WireArray(so, "_slotRoots", _slotRoots);
                WireArray(so, "_slotAvatars", _slotAvatars);
                WireArray(so, "_slotNames", _slotNames);
                WireArray(so, "_slotRoles", _slotRoles);
                WireArray(so, "_slotReadyBadges", _slotReadyBadges);

                SerializationHelper.Wire(so, "_readyToggle", _readyToggle);
                SerializationHelper.Wire(so, "_addBotButton", _addBotButton);
                SerializationHelper.Wire(so, "_startButton", _startButton);
                SerializationHelper.Wire(so, "_startButtonLabel", _startButtonLabel);
                SerializationHelper.Wire(so, "_leaveButton", _leaveButton);
            });
        }
    }
}
#endif
