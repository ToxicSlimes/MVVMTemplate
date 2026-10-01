using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;

namespace Core.Configs.DesignSystem
{
    [CreateAssetMenu(fileName = "SpacingSettings", menuName = "MVVM/Design System/Spacing Settings")]
    public class SpacingSettings : ConfigBase
    {
        #region Spacing Scale

        [Title("📏 Spacing Scale (8px base)")]
        [Tooltip("XS: 4px - минимальный отступ")]
        [Range(2, 8)]
        public float xs = 4f;

        [Tooltip("SM: 8px - маленький отступ")]
        [Range(4, 16)]
        public float sm = 8f;

        [Tooltip("MD: 16px - средний отступ")]
        [Range(8, 32)]
        public float md = 16f;

        [Tooltip("LG: 24px - большой отступ")]
        [Range(16, 48)]
        public float lg = 24f;

        [Tooltip("XL: 32px - очень большой отступ")]
        [Range(24, 64)]
        public float xl = 32f;

        [Tooltip("2XL: 48px - огромный отступ")]
        [Range(32, 96)]
        public float xxl = 48f;

        [Tooltip("3XL: 64px - максимальный отступ")]
        [Range(48, 128)]
        public float xxxl = 64f;

        #endregion

        #region Component Spacing

        [Title("📦 Component Specific Spacing")]
        [Tooltip("Card Padding (Desktop): 32px")]
        [Range(16, 64)]
        public float cardPaddingDesktop = 32f;

        [Tooltip("Card Padding (Mobile): 20px")]
        [Range(8, 48)]
        public float cardPaddingMobile = 20f;

        [Tooltip("Button Padding Horizontal: 24px")]
        [Range(8, 48)]
        public float buttonPaddingHorizontal = 24f;

        [Tooltip("Button Padding Vertical: 12px")]
        [Range(4, 24)]
        public float buttonPaddingVertical = 12f;

        [Tooltip("Input Padding Horizontal: 16px")]
        [Range(8, 32)]
        public float inputPaddingHorizontal = 16f;

        [Tooltip("Input Padding Vertical: 12px")]
        [Range(4, 24)]
        public float inputPaddingVertical = 12f;

        [Tooltip("Message Bubble Padding Horizontal: 16px")]
        [Range(8, 32)]
        public float messagePaddingHorizontal = 16f;

        [Tooltip("Message Bubble Padding Vertical: 12px")]
        [Range(4, 24)]
        public float messagePaddingVertical = 12f;

        [Tooltip("Message Spacing (Between Messages): 12px")]
        [Range(4, 32)]
        public float messageSpacing = 12f;

        #endregion

        #region Border Radius

        [Title("🔲 Border Radius")]
        [Tooltip("Small Border Radius: 8px")]
        [Range(4, 16)]
        public float borderRadiusSmall = 8f;

        [Tooltip("Medium Border Radius: 12px")]
        [Range(8, 24)]
        public float borderRadiusMedium = 12f;

        [Tooltip("Large Border Radius: 16px")]
        [Range(12, 32)]
        public float borderRadiusLarge = 16f;

        [Tooltip("Extra Large Border Radius: 24px")]
        [Range(16, 48)]
        public float borderRadiusXLarge = 24f;

        #endregion

        #region Grid System

        [Title("📐 Grid System")]
        [Tooltip("Container Max Width: 1920px")]
        [Range(1024, 3840)]
        public float containerMaxWidth = 1920f;

        [Tooltip("Container Padding Desktop: 32px")]
        [Range(16, 64)]
        public float containerPaddingDesktop = 32f;

        [Tooltip("Container Padding Mobile: 16px")]
        [Range(8, 32)]
        public float containerPaddingMobile = 16f;

        [Tooltip("Grid Gutter: 24px")]
        [Range(8, 48)]
        public float gridGutter = 24f;

        #endregion
    }
}
