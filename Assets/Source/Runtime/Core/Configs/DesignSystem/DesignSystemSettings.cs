using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;

namespace Core.Configs.DesignSystem
{
    [CreateAssetMenu(fileName = "DesignSystemSettings", menuName = "MVVM/Design System/Design System Settings")]
    public class DesignSystemSettings : ServiceConfigBase
    {
        #region Design System Components

        [Title("🎨 Design System Components")]
        [Tooltip("Цветовая палитра приложения")]
        [Required]
        public ColorPalette colors;

        [Tooltip("Настройки типографики")]
        [Required]
        public TypographySettings typography;

        [Tooltip("Настройки отступов и spacing")]
        [Required]
        public SpacingSettings spacing;

        #endregion

        #region Animation Settings

        [Title("⏱️ Animation Settings")]
        [Tooltip("Fast Duration: 150ms - buttons, hovers")]
        [Range(0.05f, 0.5f)]
        public float durationFast = 0.15f;

        [Tooltip("Medium Duration: 300ms - modals, dropdowns")]
        [Range(0.1f, 1f)]
        public float durationMedium = 0.3f;

        [Tooltip("Slow Duration: 500ms - page transitions")]
        [Range(0.2f, 2f)]
        public float durationSlow = 0.5f;

        #endregion

        #region View-Specific Settings

        // TEMPLATE: Add your view-specific design settings here.
        // Example:
        // [Title("🖼️ View-Specific Settings")]
        // [Tooltip("MainMenu: Play button uses accent color")]
        // public bool mainMenuPlayUsesAccent = true;

        #endregion

        #region Validation

        protected override void ValidateConfig()
        {
            base.ValidateConfig();

            if (colors == null)
            {
                Debug.LogWarning($"[{nameof(DesignSystemSettings)}] ColorPalette is not assigned!");
            }

            if (typography == null)
            {
                Debug.LogWarning($"[{nameof(DesignSystemSettings)}] TypographySettings is not assigned!");
            }

            if (spacing == null)
            {
                Debug.LogWarning($"[{nameof(DesignSystemSettings)}] SpacingSettings is not assigned!");
            }
        }

        #endregion
    }
}
