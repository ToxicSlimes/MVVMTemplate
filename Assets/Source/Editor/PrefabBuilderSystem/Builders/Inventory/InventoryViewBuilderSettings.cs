#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Settings;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Inventory
{
    [CreateAssetMenu(fileName = "InventoryViewBuilderSettings", menuName = "PrefabBuilder/Settings/Inventory")]
    public class InventoryViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Layout")]
        public float ModalWidth = 500f;
        public float ModalHeight = 450f;
        public float GridCellSize = 80f;
        public float GridSpacing = 6f;
        public int GridConstraintCount = 5;
        public float DetailPanelHeight = 100f;

        [Header("Filter Bar")]
        public float FilterButtonWidth = 60f;
        public float FilterButtonHeight = 28f;
        public float FilterBarSpacing = 4f;
        public float FilterBarHeight = 36f;
        public Color FilterActiveColor = new(0.3f, 0.6f, 1f, 1f);
        public Color FilterInactiveColor = new(0.25f, 0.25f, 0.3f, 1f);
        public float FilterFontSize = 11f;

        [Header("Typography")]
        public float HeaderFontSize = 20f;
        public float ItemCountFontSize = 14f;
        public float DetailNameFontSize = 18f;
        public float DetailDescFontSize = 13f;

        [Header("Colors")]
        public Color BgDimColor = new(0f, 0f, 0f, 0.7f);
        public Color ModalBgColor = new(0.14f, 0.14f, 0.16f, 1f);
        public Color HeaderColor = new(0.18f, 0.18f, 0.20f, 1f);
        public Color DetailPanelColor = new(0.16f, 0.16f, 0.19f, 1f);
        public Color RemoveButtonColor = new(0.85f, 0.25f, 0.25f, 1f);
        public Color BackButtonColor = new(0.3f, 0.3f, 0.35f, 1f);
        public Color ItemCountColor = new(0.6f, 0.6f, 0.65f, 1f);

        public override void ResetToDefaults()
        {
            ModalWidth = 500f;
            ModalHeight = 450f;
            GridCellSize = 80f;
            GridSpacing = 6f;
            GridConstraintCount = 5;
            DetailPanelHeight = 100f;
            FilterButtonWidth = 60f;
            FilterButtonHeight = 28f;
            FilterBarSpacing = 4f;
            FilterBarHeight = 36f;
            FilterActiveColor = new Color(0.3f, 0.6f, 1f, 1f);
            FilterInactiveColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            FilterFontSize = 11f;
            HeaderFontSize = 20f;
            ItemCountFontSize = 14f;
            DetailNameFontSize = 18f;
            DetailDescFontSize = 13f;
            BgDimColor = new Color(0f, 0f, 0f, 0.7f);
            ModalBgColor = new Color(0.14f, 0.14f, 0.16f, 1f);
            HeaderColor = new Color(0.18f, 0.18f, 0.20f, 1f);
            DetailPanelColor = new Color(0.16f, 0.16f, 0.19f, 1f);
            RemoveButtonColor = new Color(0.85f, 0.25f, 0.25f, 1f);
            BackButtonColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            ItemCountColor = new Color(0.6f, 0.6f, 0.65f, 1f);
        }
    }
}
#endif
