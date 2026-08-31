#if UNITY_EDITOR
using Core.Configs.DesignSystem;
using Core.Editor.PrefabBuilderSystem.Settings;
using Core.Editor.PrefabBuilderSystem.Theming;
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
        public Color FilterActiveColor = new(0.184f, 0.816f, 0.451f, 1f);
        public Color FilterInactiveColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public float FilterFontSize = 11f;

        [Header("Typography")]
        public float HeaderFontSize = 20f;
        public float ItemCountFontSize = 14f;
        public float DetailNameFontSize = 18f;
        public float DetailDescFontSize = 13f;

        [Header("Colors")]
        public Color BgDimColor = new(0.024f, 0.043f, 0.102f, 0.7f);
        public Color ModalBgColor = new(0.051f, 0.090f, 0.188f, 1f);
        public Color HeaderColor = new(0.086f, 0.149f, 0.290f, 0.8f);
        public Color DetailPanelColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color RemoveButtonColor = new(0.902f, 0.224f, 0.275f, 0.45f);
        public Color BackButtonColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color ItemCountColor = new(0.482f, 0.553f, 0.659f, 1f);

        /// <summary>
        /// Палитра проекта. Цвета этих экранов раньше стояли числами и темы не знали вовсе —
        /// после перекраски интерфейса они остались бы серыми от прежнего проекта.
        /// </summary>
        private static ColorPalette Palette => DesignSystemProvider.Colors;

        /// <summary>
        /// Затемнение под модальным окном. Берётся от фона ночи, а не чёрным: чистый чёрный на
        /// тёмно-синих панелях читается дырой, а не тенью.
        /// </summary>
        private static Color Dim(float alpha)
        {
            var color = Palette.deepSpace;
            color.a = alpha;

            return color;
        }


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
            FilterActiveColor = Palette.indigo;
            FilterInactiveColor = Palette.cardBackground;
            FilterFontSize = 11f;
            HeaderFontSize = 20f;
            ItemCountFontSize = 14f;
            DetailNameFontSize = 18f;
            DetailDescFontSize = 13f;
            BgDimColor = Dim(0.7f);
            ModalBgColor = Palette.darkSurface;
            HeaderColor = Palette.elevatedSurface;
            DetailPanelColor = Palette.cardBackground;
            RemoveButtonColor = Palette.errorBorder;
            BackButtonColor = Palette.cardBackground;
            ItemCountColor = Palette.textTertiary;
        }
    }
}
#endif
