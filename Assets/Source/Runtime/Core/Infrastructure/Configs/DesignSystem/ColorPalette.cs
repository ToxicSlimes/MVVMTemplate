using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;

namespace Core.Configs.DesignSystem
{
    /// <summary>
    /// UI color tokens for the MVVM Template design system.
    /// </summary>
    /// <remarks>
    /// Token names (indigo, pink, purple, bubble colors) are stable API for prefab builders and templates.
    /// Customize values in the ColorPalette asset; renaming fields requires rebuilding affected prefabs.
    /// </remarks>
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "MVVM/Design System/Color Palette")]
    public class ColorPalette : ConfigBase
    {
        #region Background Colors

        [Title("🎨 Background Colors")]
        [Tooltip("Night #060B1A - ночь за окном, главный фон приложения")]
        public Color deepSpace = new Color(0.024f, 0.043f, 0.102f, 1f);

        [Tooltip("Night Surface #0D1730 - карточки, панели")]
        public Color darkSurface = new Color(0.051f, 0.090f, 0.188f, 1f);

        [Tooltip("Elevated Surface #16264A - приподнятые панели, glassmorphism")]
        public Color elevatedSurface = new Color(0.086f, 0.149f, 0.290f, 0.8f);

        #endregion

        #region Accent Colors

        [Title("✨ Accent Colors")]
        [Tooltip("Ship Green #2FD073 - корпус тарелки, главное действие и прогресс")]
        public Color indigo = new Color(0.184f, 0.816f, 0.451f, 1f);

        [Tooltip("Ember #FF8A3D - угли попаданий, предупреждение перед тревогой")]
        public Color pink = new Color(1f, 0.541f, 0.239f, 1f);

        [Tooltip("Lantern #FFD34E - фонарь деда, нейтральное состояние и счётчики")]
        public Color purple = new Color(1f, 0.827f, 0.306f, 1f);

        [Tooltip("Beam #7AECDC - луч тарелки, подсказки, ввод, ссылки")]
        public Color cyan = new Color(0.478f, 0.925f, 0.863f, 1f);

        #endregion

        #region Text Colors

        [Title("📝 Text Colors")]
        [Tooltip("Primary #F8FAFC - заголовки, основной текст")]
        public Color textPrimary = new Color(0.973f, 0.980f, 0.988f, 1f);

        [Tooltip("Secondary #A3B4CC - описания")]
        public Color textSecondary = new Color(0.639f, 0.706f, 0.800f, 1f);

        [Tooltip("Tertiary #7B8DA8 - метаданные, timestamps")]
        public Color textTertiary = new Color(0.482f, 0.553f, 0.659f, 1f);

        [Tooltip("Disabled #5E708C - неактивные элементы")]
        public Color textDisabled = new Color(0.369f, 0.439f, 0.549f, 1f);

        #endregion

        #region Message Bubble Colors

        [Title("💬 Message Bubble Colors")]
        [Tooltip("Accent Surface - gradient start #0F5C42, тёмный корпус тарелки под белым текстом")]
        public Color userBubbleStart = new Color(0.059f, 0.361f, 0.259f, 0.95f);

        [Tooltip("Accent Surface - gradient end #0E5A66, тёмный луч")]
        public Color userBubbleEnd = new Color(0.055f, 0.353f, 0.400f, 0.95f);

        [Tooltip("Neutral Surface #14203B - нейтральная подложка сообщения")]
        public Color aiBubble = new Color(0.078f, 0.125f, 0.231f, 0.95f);

        [Tooltip("Neutral Surface Border rgba(163, 180, 204, 0.1)")]
        public Color aiBubbleBorder = new Color(0.639f, 0.706f, 0.800f, 0.1f);

        [Tooltip("Alarm Background rgba(230, 57, 70, 0.18) - опасность и деструктивные действия")]
        public Color errorBubble = new Color(0.902f, 0.224f, 0.275f, 0.18f);

        [Tooltip("Alarm Text #FF5C5C - тревога, дед рядом")]
        public Color errorText = new Color(1f, 0.361f, 0.361f, 1f);

        [Tooltip("Alarm Border rgba(230, 57, 70, 0.45)")]
        public Color errorBorder = new Color(0.902f, 0.224f, 0.275f, 0.45f);

        #endregion

        #region Glassmorphism Card Colors

        [Title("🔮 Glassmorphism Card Colors")]
        [Tooltip("Card Background rgba(22, 38, 74, 0.4)")]
        public Color cardBackground = new Color(0.086f, 0.149f, 0.290f, 0.4f);

        [Tooltip("Card Border rgba(163, 180, 204, 0.12)")]
        public Color cardBorder = new Color(0.639f, 0.706f, 0.800f, 0.12f);

        [Tooltip("Card Hover Background rgba(22, 38, 74, 0.6)")]
        public Color cardHoverBackground = new Color(0.086f, 0.149f, 0.290f, 0.6f);

        [Tooltip("Card Hover Border rgba(47, 208, 115, 0.3) - зелёная подсветка наведения")]
        public Color cardHoverBorder = new Color(0.184f, 0.816f, 0.451f, 0.3f);

        #endregion

        #region Button Colors

        [Title("🔘 Button Colors")]
        [Tooltip("Primary Button Background #0C7A4B - тёмный зелёный, держит белую подпись")]
        public Color primaryButtonGradientStart = new Color(0.047f, 0.478f, 0.294f, 1f);

        [Tooltip("Primary Button Background #0B6E74 - тёмный бирюзовый конец градиента")]
        public Color primaryButtonGradientEnd = new Color(0.043f, 0.431f, 0.455f, 1f);

        [Tooltip("Secondary Button Background rgba(22, 38, 74, 0.5)")]
        public Color secondaryButtonBackground = new Color(0.086f, 0.149f, 0.290f, 0.5f);

        [Tooltip("Icon Button Background rgba(22, 38, 74, 0.65)")]
        public Color iconButtonBackground = new Color(0.086f, 0.149f, 0.290f, 0.65f);

        #endregion

        #region Input Field Colors

        [Title("📥 Input Field Colors")]
        [Tooltip("Input Background rgba(20, 32, 59, 0.65)")]
        public Color inputBackground = new Color(0.078f, 0.125f, 0.231f, 0.65f);

        [Tooltip("Input Border rgba(163, 180, 204, 0.12)")]
        public Color inputBorder = new Color(0.639f, 0.706f, 0.800f, 0.12f);

        [Tooltip("Input Focus Border #2FD073 - зелёный фокус")]
        public Color inputFocusBorder = new Color(0.184f, 0.816f, 0.451f, 1f);

        [Tooltip("Input Placeholder #7B8DA8")]
        public Color inputPlaceholder = new Color(0.482f, 0.553f, 0.659f, 1f);

        #endregion
    }
}
