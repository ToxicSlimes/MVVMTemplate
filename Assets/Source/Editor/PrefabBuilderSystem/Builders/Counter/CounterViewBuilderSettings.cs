#if UNITY_EDITOR
using Core.Configs.DesignSystem;
using Core.Editor.PrefabBuilderSystem.Settings;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Builders.Counter
{
    [CreateAssetMenu(fileName = "CounterViewBuilderSettings", menuName = "PrefabBuilder/Settings/Counter")]
    public class CounterViewBuilderSettings : BuilderSettingsBase
    {
        [Header("Layout")]
        public float ModalWidth = 400f;
        public float ModalHeight = 350f;
        public float ButtonSpacing = 15f;
        public float ButtonHeight = 50f;

        [Header("Typography")]
        public float CounterFontSize = 72f;
        public float IncrementsFontSize = 16f;
        public float ButtonFontSize = 20f;
        public float TitleFontSize = 24f;

        [Header("Colors")]
        public Color BgDimColor = new(0.024f, 0.043f, 0.102f, 0.7f);
        public Color ModalBgColor = new(0.051f, 0.090f, 0.188f, 1f);
        public Color HeaderColor = new(0.086f, 0.149f, 0.290f, 0.8f);
        public Color CounterTextColor = new(0.973f, 0.980f, 0.988f, 1f);
        public Color IncrementsTextColor = new(0.482f, 0.553f, 0.659f, 1f);
        public Color IncrementButtonColor = new(0.184f, 0.816f, 0.451f, 1f);
        public Color DecrementButtonColor = new(1f, 0.541f, 0.239f, 1f);
        public Color ResetButtonColor = new(0.086f, 0.149f, 0.290f, 0.4f);
        public Color ButtonTextColor = new(0.973f, 0.980f, 0.988f, 1f);

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
            ModalWidth = 400f;
            ModalHeight = 350f;
            ButtonSpacing = 15f;
            ButtonHeight = 50f;
            CounterFontSize = 72f;
            IncrementsFontSize = 16f;
            ButtonFontSize = 20f;
            TitleFontSize = 24f;
            BgDimColor = Dim(0.7f);
            ModalBgColor = Palette.darkSurface;
            HeaderColor = Palette.elevatedSurface;
            CounterTextColor = Palette.textPrimary;
            IncrementsTextColor = Palette.textTertiary;
            IncrementButtonColor = Palette.indigo;
            DecrementButtonColor = Palette.pink;
            ResetButtonColor = Palette.cardBackground;
            ButtonTextColor = Palette.textPrimary;
        }
    }
}
#endif
