#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Matchmaking
{
    [CreateAssetMenu(fileName = "MatchmakingViewBuilderSettings", menuName = "PrefabBuilder/Settings/Matchmaking")]
    public class MatchmakingViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Header")]
        public float SideMargin = 80f;
        public float HeaderTopOffset = 56f;
        public float HeaderHeight = 56f;
        public float BackButtonSize = 56f;
        public float TitleWidth = 300f;
        public float CallsignLabelWidth = 120f;
        public float NicknameInputWidth = 380f;
        public float NicknameInputHeight = 52f;

        [Header("Cards")]
        public float CardsTopOffset = 160f;
        public float CardWidth = 408f;
        public float CardHeight = 300f;
        public float CardBorderWidth = 1f;
        public float CardTitleHeight = 32f;
        public float CardHintHeight = 72f;
        public float CardButtonHeight = 60f;
        public float CodeInputWidth = 344f;
        public float CodeInputHeight = 72f;

        [Header("Sessions")]
        public float SessionsLeft = 944f;
        public float SessionsWidth = 896f;
        public float SessionsTopOffset = 160f;
        public float SessionsPanelHeight = 720f;
        public float SessionsHeaderHeight = 34f;
        public float SessionsLabelWidth = 200f;
        public float RefreshButtonWidth = 120f;
        public float RefreshButtonHeight = 34f;
        public float DividerTopGap = 6f;
        public float DividerThickness = 1f;
        public float DividerBottomGap = 15f;
        public float ScrollHeight = 664f;

        [Header("Session Row")]
        public float RowHeight = 64f;
        public float RowSpacing = 8f;
        public float RowSidePadding = 20f;
        public float RowInfoWidth = 320f;
        public float RowJoinButtonWidth = 92f;
        public float RowJoinButtonHeight = 36f;

        [Header("Status")]
        public float StatusBottomOffset = 56f;
        public float StatusWidth = 600f;
        public float StatusHeight = 32f;

        [Header("Typography")]
        public float BackButtonFontSize = 28f;
        public float TitleFontSize = 32f;
        public float NicknameFontSize = 18f;
        public float CardTitleFontSize = 24f;
        public float CardButtonFontSize = 20f;
        public float CodeInputFontSize = 34f;
        public float RefreshButtonFontSize = 14f;
        public float RowFontSize = 22f;
        public float RowButtonFontSize = 14f;

        [Header("Input Limits")]
        public int NicknameCharacterLimit = 16;
        public int CodeCharacterLimit = 6;

        public override void ResetToDefaults()
        {
            SideMargin = 80f;
            HeaderTopOffset = 56f;
            HeaderHeight = 56f;
            BackButtonSize = 56f;
            TitleWidth = 300f;
            CallsignLabelWidth = 120f;
            NicknameInputWidth = 380f;
            NicknameInputHeight = 52f;

            CardsTopOffset = 160f;
            CardWidth = 408f;
            CardHeight = 300f;
            CardBorderWidth = 1f;
            CardTitleHeight = 32f;
            CardHintHeight = 72f;
            CardButtonHeight = 60f;
            CodeInputWidth = 344f;
            CodeInputHeight = 72f;

            SessionsLeft = 944f;
            SessionsWidth = 896f;
            SessionsTopOffset = 160f;
            SessionsPanelHeight = 720f;
            SessionsHeaderHeight = 34f;
            SessionsLabelWidth = 200f;
            RefreshButtonWidth = 120f;
            RefreshButtonHeight = 34f;
            DividerTopGap = 6f;
            DividerThickness = 1f;
            DividerBottomGap = 15f;
            ScrollHeight = 664f;

            RowHeight = 64f;
            RowSpacing = 8f;
            RowSidePadding = 20f;
            RowInfoWidth = 320f;
            RowJoinButtonWidth = 92f;
            RowJoinButtonHeight = 36f;

            StatusBottomOffset = 56f;
            StatusWidth = 600f;
            StatusHeight = 32f;

            BackButtonFontSize = 28f;
            TitleFontSize = 32f;
            NicknameFontSize = 18f;
            CardTitleFontSize = 24f;
            CardButtonFontSize = 20f;
            CodeInputFontSize = 34f;
            RefreshButtonFontSize = 14f;
            RowFontSize = 22f;
            RowButtonFontSize = 14f;

            NicknameCharacterLimit = 16;
            CodeCharacterLimit = 6;
        }
    }
}
#endif
