#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.LobbyRoom
{
    [CreateAssetMenu(fileName = "LobbyRoomViewBuilderSettings", menuName = "PrefabBuilder/Settings/LobbyRoom")]
    public class LobbyRoomViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Room Code")]
        public float RoomCodeTopOffset = 64f;
        public float RoomCodeBlockWidth = 560f;
        public float RoomCodeBlockHeight = 120f;
        public float RoomCodeLabelHeight = 20f;
        public float RoomCodeRowSpacing = 20f;
        public float RoomCodeRowHeight = 96f;
        public float RoomCodeTextWidth = 320f;
        public float CopyButtonSize = 52f;

        [Header("Roster")]
        public float RosterTopOffset = 262f;
        public float RosterRowWidth = 1320f;
        public float SlotWidth = 312f;
        public float SlotHeight = 400f;
        public float SlotBorderWidth = 2f;
        public float AvatarSize = 112f;
        public float SlotNameHeight = 30f;
        public float SlotRoleHeight = 20f;
        public float ReadyBadgeHeight = 38f;
        public float ReadyBadgeBorderWidth = 2f;

        [Header("Footer")]
        public float FooterSideMargin = 80f;
        public float FooterBottomOffset = 56f;
        public float FooterHeight = 60f;
        public float LeaveButtonWidth = 220f;
        public float StartButtonWidth = 300f;
        public float AddBotButtonWidth = 200f;
        public float ReadyToggleWidth = 220f;
        public float ReadyToggleSize = 28f;

        [Header("Typography")]
        public float RoomCodeFontSize = 76f;
        public float CopyButtonFontSize = 14f;
        public float SlotNameFontSize = 22f;
        public float EmptySlotFontSize = 15f;
        public float ReadyBadgeFontSize = 14f;
        public float FooterButtonFontSize = 20f;
        public float ReadyToggleFontSize = 20f;

        public override void ResetToDefaults()
        {
            RoomCodeTopOffset = 64f;
            RoomCodeBlockWidth = 560f;
            RoomCodeBlockHeight = 120f;
            RoomCodeLabelHeight = 20f;
            RoomCodeRowSpacing = 20f;
            RoomCodeRowHeight = 96f;
            RoomCodeTextWidth = 320f;
            CopyButtonSize = 52f;

            RosterTopOffset = 262f;
            RosterRowWidth = 1320f;
            SlotWidth = 312f;
            SlotHeight = 400f;
            SlotBorderWidth = 2f;
            AvatarSize = 112f;
            SlotNameHeight = 30f;
            SlotRoleHeight = 20f;
            ReadyBadgeHeight = 38f;
            ReadyBadgeBorderWidth = 2f;

            FooterSideMargin = 80f;
            FooterBottomOffset = 56f;
            FooterHeight = 60f;
            LeaveButtonWidth = 220f;
            StartButtonWidth = 300f;
            AddBotButtonWidth = 200f;
            ReadyToggleWidth = 220f;
            ReadyToggleSize = 28f;

            RoomCodeFontSize = 76f;
            CopyButtonFontSize = 14f;
            SlotNameFontSize = 22f;
            EmptySlotFontSize = 15f;
            ReadyBadgeFontSize = 14f;
            FooterButtonFontSize = 20f;
            ReadyToggleFontSize = 20f;
        }
    }
}
#endif
