#if UNITY_EDITOR
using Core.Configs.DesignSystem;
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class TextBuilder : BaseElementBuilder<TextBuilder, TextMeshProUGUI>
    {
        protected override string NamingPrefix => NamingConvention.Text;
        
        private string _text = "";
        private float _fontSize = 16f;
        private bool _bold;
        private bool _italic;
        private Color _color = new(0.2f, 0.2f, 0.2f, 1f);
        private TextAlignmentOptions _alignment = TextAlignmentOptions.Left;
        private bool _autoSize = true;
        private float _autoSizeMin = 8f;
        private float? _autoSizeMax;
        private bool _richText = true;
        private TextOverflowModes _overflow = TextOverflowModes.Overflow;
        private float _lineHeight = 1.6f;
        private float _letterSpacing;

        public TextBuilder(string name) : base(name) { }

        public TextBuilder WithText(string text)
        {
            _text = text;
            return this;
        }

        public TextBuilder WithFontSize(float size)
        {
            _fontSize = size;
            _autoSizeMax ??= size;
            return this;
        }

        public TextBuilder Bold()
        {
            _bold = true;
            return this;
        }

        public TextBuilder Italic()
        {
            _italic = true;
            return this;
        }

        public TextBuilder WithColor(Color color)
        {
            _color = color;
            return this;
        }

        public TextBuilder WithAlignment(TextAlignmentOptions alignment)
        {
            _alignment = alignment;
            return this;
        }

        public TextBuilder Centered()
        {
            _alignment = TextAlignmentOptions.Center;
            return this;
        }

        public TextBuilder Right()
        {
            _alignment = TextAlignmentOptions.Right;
            return this;
        }

        public TextBuilder WithAutoSize(bool enable = true, float? min = null, float? max = null)
        {
            _autoSize = enable;
            if (min.HasValue) _autoSizeMin = min.Value;
            if (max.HasValue) _autoSizeMax = max.Value;
            return this;
        }

        public TextBuilder NoAutoSize()
        {
            _autoSize = false;
            return this;
        }

        public TextBuilder WithOverflow(TextOverflowModes mode)
        {
            _overflow = mode;
            return this;
        }

        public TextBuilder Ellipsis()
        {
            _overflow = TextOverflowModes.Ellipsis;
            return this;
        }

        public TextBuilder NoRichText()
        {
            _richText = false;
            return this;
        }

        public TextBuilder WithStyle(TypographySettings.TextStyle style)
        {
            if (style == null) return this;
            
            _fontSize = style.fontSize;
            _bold = style.fontWeight >= 600;
            _alignment = style.alignment;
            _lineHeight = style.lineHeight;
            _letterSpacing = style.letterSpacing;
            return this;
        }

        public TextBuilder WithStyle(TypographySettings.TextStyle style, Color color)
        {
            WithStyle(style);
            _color = color;
            return this;
        }

        public TextBuilder Heading1()
        {
            var style = DesignSystemProvider.Typography?.heading1;
            var color = DesignSystemProvider.Colors?.textPrimary ?? Color.white;
            return style != null ? WithStyle(style, color) : WithFontSize(48f).Bold().Centered().WithColor(Color.white);
        }

        public TextBuilder Heading2()
        {
            var style = DesignSystemProvider.Typography?.heading2;
            var color = DesignSystemProvider.Colors?.textPrimary ?? Color.white;
            return style != null ? WithStyle(style, color) : WithFontSize(32f).Bold().WithColor(Color.white);
        }

        public TextBuilder Heading3()
        {
            var style = DesignSystemProvider.Typography?.heading3;
            var color = DesignSystemProvider.Colors?.textPrimary ?? Color.white;
            return style != null ? WithStyle(style, color) : WithFontSize(24f).Bold().WithColor(Color.white);
        }

        public TextBuilder BodyLarge()
        {
            var style = DesignSystemProvider.Typography?.bodyLarge;
            var color = DesignSystemProvider.Colors?.textPrimary ?? Color.white;
            return style != null ? WithStyle(style, color) : WithFontSize(18f).WithColor(Color.white);
        }

        public TextBuilder Body()
        {
            var style = DesignSystemProvider.Typography?.body;
            var color = DesignSystemProvider.Colors?.textPrimary ?? Color.white;
            return style != null ? WithStyle(style, color) : WithFontSize(14f).WithColor(Color.white);
        }

        public TextBuilder BodySmall()
        {
            var style = DesignSystemProvider.Typography?.bodySmall;
            var color = DesignSystemProvider.Colors?.textSecondary ?? new Color(0.6f, 0.6f, 0.6f);
            return style != null ? WithStyle(style, color) : WithFontSize(12f).WithColor(new Color(0.6f, 0.6f, 0.6f));
        }

        public TextBuilder Caption()
        {
            var style = DesignSystemProvider.Typography?.caption;
            var color = DesignSystemProvider.Colors?.textTertiary ?? new Color(0.4f, 0.4f, 0.4f);
            return style != null ? WithStyle(style, color) : WithFontSize(12f).WithColor(new Color(0.4f, 0.4f, 0.4f));
        }

        public TextBuilder ButtonText()
        {
            var style = DesignSystemProvider.Typography?.button;
            var color = DesignSystemProvider.Colors?.textPrimary ?? Color.white;
            return style != null ? WithStyle(style, color) : WithFontSize(14f).Bold().Centered().WithColor(Color.white);
        }

        public TextBuilder Secondary()
        {
            _color = DesignSystemProvider.Colors?.textSecondary ?? new Color(0.6f, 0.6f, 0.7f);
            return this;
        }

        public TextBuilder Tertiary()
        {
            _color = DesignSystemProvider.Colors?.textTertiary ?? new Color(0.4f, 0.5f, 0.55f);
            return this;
        }

        public TextBuilder Error()
        {
            _color = DesignSystemProvider.Colors?.errorText ?? new Color(0.99f, 0.65f, 0.65f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Atoms.Text;

        public override TextMeshProUGUI Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();

            tmp.text = _text;
            tmp.color = _color;
            tmp.alignment = _alignment;
            tmp.richText = _richText;
            tmp.overflowMode = _overflow;
            tmp.lineSpacing = (_lineHeight - 1f) * 100f;
            tmp.characterSpacing = _letterSpacing * 100f;

            var style = FontStyles.Normal;
            if (_bold) style |= FontStyles.Bold;
            if (_italic) style |= FontStyles.Italic;
            tmp.fontStyle = style;

            if (_autoSize)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = _autoSizeMin;
                tmp.fontSizeMax = _autoSizeMax ?? _fontSize;
            }
            else
            {
                tmp.fontSize = _fontSize;
            }

            if (!PreferredHeight.HasValue)
            {
                PreferredHeight = _fontSize + 6;
            }

            ApplyLayoutElement(rt.gameObject);

            return tmp;
        }
    }
}
#endif
