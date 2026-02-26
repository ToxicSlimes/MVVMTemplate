using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;

namespace Core.Configs.DesignSystem
{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "EndlessAdventure/Design System/Color Palette")]
    public class ColorPalette : ConfigBase
    {
        #region Background Colors

        [Title("🎨 Background Colors")]
        [Tooltip("Deep Space #0A0F1E - главный фон приложения")]
        public Color deepSpace = new Color(0.039f, 0.059f, 0.118f, 1f);

        [Tooltip("Dark Surface #151B2D - карточки, панели")]
        public Color darkSurface = new Color(0.082f, 0.106f, 0.176f, 1f);

        [Tooltip("Elevated Surface - glassmorphism эффект")]
        public Color elevatedSurface = new Color(0.118f, 0.161f, 0.365f, 0.8f);

        #endregion

        #region Accent Colors

        [Title("✨ Accent Colors")]
        [Tooltip("Indigo #6366F1 - Chat, primary actions")]
        public Color indigo = new Color(0.388f, 0.4f, 0.945f, 1f);

        [Tooltip("Pink #EC4899 - Image Gen, creative actions")]
        public Color pink = new Color(0.925f, 0.282f, 0.6f, 1f);

        [Tooltip("Purple #8B5CF6 - secondary actions")]
        public Color purple = new Color(0.545f, 0.361f, 0.965f, 1f);

        [Tooltip("Cyan #06B6D4 - info, links")]
        public Color cyan = new Color(0.024f, 0.714f, 0.831f, 1f);

        #endregion

        #region Text Colors

        [Title("📝 Text Colors")]
        [Tooltip("Primary #F8FAFC - заголовки, основной текст")]
        public Color textPrimary = new Color(0.973f, 0.980f, 0.988f, 1f);

        [Tooltip("Secondary #94A3B8 - описания")]
        public Color textSecondary = new Color(0.580f, 0.639f, 0.722f, 1f);

        [Tooltip("Tertiary #64748B - метаданные, timestamps")]
        public Color textTertiary = new Color(0.392f, 0.455f, 0.545f, 1f);

        [Tooltip("Disabled #475569 - неактивные элементы")]
        public Color textDisabled = new Color(0.278f, 0.333f, 0.412f, 1f);

        #endregion

        #region Message Bubble Colors

        [Title("💬 Message Bubble Colors")]
        [Tooltip("User Message Background - gradient start #667EEA")]
        public Color userBubbleStart = new Color(0.4f, 0.494f, 0.918f, 0.95f);

        [Tooltip("User Message Background - gradient end #764BA2")]
        public Color userBubbleEnd = new Color(0.463f, 0.294f, 0.635f, 0.95f);

        [Tooltip("AI Message Background rgba(30, 41, 59, 0.95)")]
        public Color aiBubble = new Color(0.118f, 0.161f, 0.365f, 0.95f);

        [Tooltip("AI Message Border rgba(148, 163, 184, 0.1)")]
        public Color aiBubbleBorder = new Color(0.580f, 0.639f, 0.722f, 0.1f);

        [Tooltip("Error Message Background rgba(239, 68, 68, 0.15)")]
        public Color errorBubble = new Color(0.937f, 0.267f, 0.267f, 0.15f);

        [Tooltip("Error Message Text #FCA5A5")]
        public Color errorText = new Color(0.988f, 0.647f, 0.647f, 1f);

        [Tooltip("Error Message Border rgba(239, 68, 68, 0.3)")]
        public Color errorBorder = new Color(0.937f, 0.267f, 0.267f, 0.3f);

        #endregion

        #region Glassmorphism Card Colors

        [Title("🔮 Glassmorphism Card Colors")]
        [Tooltip("Card Background rgba(30, 41, 59, 0.4)")]
        public Color cardBackground = new Color(0.118f, 0.161f, 0.365f, 0.4f);

        [Tooltip("Card Border rgba(148, 163, 184, 0.1)")]
        public Color cardBorder = new Color(0.580f, 0.639f, 0.722f, 0.1f);

        [Tooltip("Card Hover Background rgba(30, 41, 59, 0.6)")]
        public Color cardHoverBackground = new Color(0.118f, 0.161f, 0.365f, 0.6f);

        [Tooltip("Card Hover Border rgba(148, 163, 184, 0.2)")]
        public Color cardHoverBorder = new Color(0.580f, 0.639f, 0.722f, 0.2f);

        #endregion

        #region Button Colors

        [Title("🔘 Button Colors")]
        [Tooltip("Primary Button Background - Indigo Gradient")]
        public Color primaryButtonGradientStart = new Color(0.388f, 0.4f, 0.945f, 1f);

        [Tooltip("Primary Button Background - Purple Gradient")]
        public Color primaryButtonGradientEnd = new Color(0.545f, 0.361f, 0.965f, 1f);

        [Tooltip("Secondary Button Background rgba(30, 41, 59, 0.4)")]
        public Color secondaryButtonBackground = new Color(0.118f, 0.161f, 0.365f, 0.4f);

        [Tooltip("Icon Button Background rgba(30, 41, 59, 0.6)")]
        public Color iconButtonBackground = new Color(0.118f, 0.161f, 0.365f, 0.6f);

        #endregion

        #region Input Field Colors

        [Title("📥 Input Field Colors")]
        [Tooltip("Input Background rgba(30, 41, 59, 0.6)")]
        public Color inputBackground = new Color(0.118f, 0.161f, 0.365f, 0.6f);

        [Tooltip("Input Border rgba(148, 163, 184, 0.1)")]
        public Color inputBorder = new Color(0.580f, 0.639f, 0.722f, 0.1f);

        [Tooltip("Input Focus Border #6366F1")]
        public Color inputFocusBorder = new Color(0.388f, 0.4f, 0.945f, 1f);

        [Tooltip("Input Placeholder #64748B")]
        public Color inputPlaceholder = new Color(0.392f, 0.455f, 0.545f, 1f);

        #endregion
    }
}
