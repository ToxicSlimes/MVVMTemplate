using System;
using Core.Abstractions.Attributes;
using Core.Abstractions.Config;
using TMPro;
using TriInspector;
using UnityEngine;

namespace Core.Presentation.UI.DesignSystem
{
    [CreateAssetMenu(fileName = "TypographySettings", menuName = "MVVM/Design System/Typography Settings")]
    public class TypographySettings : ConfigBase
    {
        [Serializable]
        [ConfigEntryStyle(ColorPreset.Purple)]
        public class TextStyle
        {
            [Tooltip("Размер шрифта в пикселях")]
            [Range(8, 72)]
            public float fontSize = 14f;

            [Tooltip("Вес шрифта (400=Regular, 600=SemiBold, 700=Bold)")]
            [Range(100, 900)]
            public int fontWeight = 400;

            [Tooltip("Межстрочный интервал (line height multiplier)")]
            [Range(1.0f, 2.0f)]
            public float lineHeight = 1.6f;

            [Tooltip("Межбуквенное расстояние")]
            [Range(-0.1f, 0.1f)]
            public float letterSpacing = 0f;

            [Tooltip("Выравнивание текста")]
            public TextAlignmentOptions alignment = TextAlignmentOptions.Left;
        }

        #region Headings

        [Title("📰 Headings")]
        [Tooltip("H1: 48px Bold - Главные заголовки страниц")]
        public TextStyle heading1 = new TextStyle
        {
            fontSize = 48f,
            fontWeight = 700,
            lineHeight = 1.2f,
            letterSpacing = -0.02f,
            alignment = TextAlignmentOptions.Center
        };

        [Tooltip("H2: 32px SemiBold - Секционные заголовки")]
        public TextStyle heading2 = new TextStyle
        {
            fontSize = 32f,
            fontWeight = 600,
            lineHeight = 1.3f,
            letterSpacing = -0.01f,
            alignment = TextAlignmentOptions.Left
        };

        [Tooltip("H3: 24px SemiBold - Заголовки карточек")]
        public TextStyle heading3 = new TextStyle
        {
            fontSize = 24f,
            fontWeight = 600,
            lineHeight = 1.4f,
            letterSpacing = 0f,
            alignment = TextAlignmentOptions.Left
        };

        #endregion

        #region Body Text

        [Title("📝 Body Text")]
        [Tooltip("Body Large: 18px Regular - Описания карточек")]
        public TextStyle bodyLarge = new TextStyle
        {
            fontSize = 18f,
            fontWeight = 400,
            lineHeight = 1.6f,
            letterSpacing = 0f,
            alignment = TextAlignmentOptions.Left
        };

        [Tooltip("Body: 14px Regular - Основной текст, сообщения")]
        public TextStyle body = new TextStyle
        {
            fontSize = 14f,
            fontWeight = 400,
            lineHeight = 1.6f,
            letterSpacing = 0f,
            alignment = TextAlignmentOptions.Left
        };

        [Tooltip("Body Small: 12px Regular - Мелкий текст")]
        public TextStyle bodySmall = new TextStyle
        {
            fontSize = 12f,
            fontWeight = 400,
            lineHeight = 1.5f,
            letterSpacing = 0f,
            alignment = TextAlignmentOptions.Left
        };

        #endregion

        #region Special

        [Title("✨ Special")]
        [Tooltip("Caption: 12px Medium - Метаданные, timestamps")]
        public TextStyle caption = new TextStyle
        {
            fontSize = 12f,
            fontWeight = 500,
            lineHeight = 1.5f,
            letterSpacing = 0f,
            alignment = TextAlignmentOptions.Left
        };

        [Tooltip("Button: 14px SemiBold - Текст кнопок")]
        public TextStyle button = new TextStyle
        {
            fontSize = 14f,
            fontWeight = 600,
            lineHeight = 1.4f,
            letterSpacing = 0.01f,
            alignment = TextAlignmentOptions.Center
        };

        #endregion

        #region Methods

        public void ApplyTextStyle(TMP_Text tmpText, TextStyle style, Color color)
        {
            if (tmpText == null || style == null) return;

            tmpText.fontSize = style.fontSize;
            tmpText.fontWeight = (FontWeight)style.fontWeight;
            tmpText.lineSpacing = (style.lineHeight - 1f) * 100f;
            tmpText.characterSpacing = style.letterSpacing * 100f;
            tmpText.alignment = style.alignment;
            tmpText.color = color;
        }

        #endregion
    }
}
