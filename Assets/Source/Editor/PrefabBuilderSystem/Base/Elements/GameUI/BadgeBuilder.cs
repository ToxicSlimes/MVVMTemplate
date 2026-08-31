#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Theming;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements.GameUI
{
    public class BadgeBuilder : BaseElementBuilder<BadgeBuilder, Image>
    {
        protected override string NamingPrefix => "Badge";

        private float _size = 36f;
        private Color _backgroundColor = Color.white;
        private Color _textColor = new(0.2f, 0.2f, 0.2f, 1f);
        private Color _outlineColor = new(0.1f, 0.1f, 0.1f, 1f);
        private float _outlineWidth = 1f;
        private float _fontSize = 12f;
        private string _text = "1";
        private bool _showOutline = true;
        private Sprite _backgroundSprite;

        public BadgeBuilder(string name) : base(name) { }

        public BadgeBuilder WithSize(float size)
        {
            _size = size;
            return this;
        }

        public BadgeBuilder WithText(string text)
        {
            _text = text;
            return this;
        }

        public BadgeBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public BadgeBuilder WithTextColor(Color color)
        {
            _textColor = color;
            return this;
        }

        public BadgeBuilder WithOutline(Color color, float width = 1f)
        {
            _outlineColor = color;
            _outlineWidth = width;
            _showOutline = true;
            return this;
        }

        public BadgeBuilder NoOutline()
        {
            _showOutline = false;
            return this;
        }

        public BadgeBuilder WithFontSize(float size)
        {
            _fontSize = size;
            return this;
        }

        public BadgeBuilder WithSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        public BadgeBuilder Level()
        {
            _backgroundColor = Color.white;
            _textColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            _outlineColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            return this;
        }

        public BadgeBuilder Notification()
        {
            _backgroundColor = new Color(0.9f, 0.2f, 0.2f, 1f);
            _textColor = Color.white;
            _showOutline = false;
            return this;
        }

        public BadgeBuilder Rank()
        {
            _backgroundColor = new Color(1f, 0.84f, 0f, 1f);
            _textColor = new Color(0.3f, 0.2f, 0f, 1f);
            _outlineColor = new Color(0.6f, 0.4f, 0f, 1f);
            return this;
        }

        public BadgeBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            if (colors != null)
            {
                _backgroundColor = colors.elevatedSurface;
                _textColor = colors.textPrimary;
                _outlineColor = colors.darkSurface;
            }
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.GameUI.Badge;

        public override Image Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);

            WithPreferredSize(_size, _size);

            var rt = CreateRectTransform(parent);
            rt.sizeDelta = new Vector2(_size, _size);
            rt.gameObject.AddComponent<CanvasRenderer>();
            
            var rootImg = rt.gameObject.AddComponent<Image>();
            rootImg.color = _backgroundColor;
            rootImg.sprite = _backgroundSprite;
            rootImg.raycastTarget = false;

            if (_showOutline)
            {
                var outline = rt.gameObject.AddComponent<Outline>();
                outline.effectColor = _outlineColor;
                outline.effectDistance = new Vector2(_outlineWidth, -_outlineWidth);
            }

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(rt, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            textGo.AddComponent<CanvasRenderer>();

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = _text;
            tmp.fontSize = _fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = _textColor;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = _fontSize * 0.5f;
            tmp.fontSizeMax = _fontSize;

            ApplyLayoutElement(rt.gameObject);

            return rootImg;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            component.color = _backgroundColor;
            component.sprite = _backgroundSprite;

            var outline = instance.GetComponent<Outline>();
            if (outline == null && _showOutline)
            {
                outline = instance.AddComponent<Outline>();
            }

            if (outline != null)
            {
                outline.effectColor = _outlineColor;
                outline.effectDistance = new Vector2(_outlineWidth, -_outlineWidth);
                outline.enabled = _showOutline;
            }

            var text = instance.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = _text;
                text.color = _textColor;
                text.fontSize = _fontSize;
                text.enableAutoSizing = false;
            }

            var layoutElement = instance.GetComponent<LayoutElement>() ?? instance.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = _size;

            if (instance.transform is RectTransform rect)
            {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, _size);
            }
        }
    }
}
#endif
