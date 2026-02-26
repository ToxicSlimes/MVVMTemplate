#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements.GameUI
{
    public class HealthBarBuilder : BaseElementBuilder<HealthBarBuilder, Image>
    {
        protected override string NamingPrefix => "HealthBar";

        private float _value = 1f;
        private int _currentValue = 100;
        private int _maxValue = 100;
        private Color _backgroundColor = new(0.3f, 0.3f, 0.3f, 1f);
        private Color _fillColor = new(0.9f, 0.2f, 0.2f, 1f);
        private Color _textColor = Color.white;
        private float _height = 24f;
        private float _fontSize = 14f;
        private bool _showText = true;
        private string _textFormat = "{0}/{1}";

        public HealthBarBuilder(string name) : base(name) { }

        public HealthBarBuilder WithValue(float normalized)
        {
            _value = Mathf.Clamp01(normalized);
            _currentValue = Mathf.RoundToInt(_value * _maxValue);
            return this;
        }

        public HealthBarBuilder WithValues(int current, int max)
        {
            _currentValue = current;
            _maxValue = Mathf.Max(1, max);
            _value = (float)current / _maxValue;
            return this;
        }

        public HealthBarBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public HealthBarBuilder WithFillColor(Color color)
        {
            _fillColor = color;
            return this;
        }

        public HealthBarBuilder WithTextColor(Color color)
        {
            _textColor = color;
            return this;
        }

        public HealthBarBuilder WithHeight(float height)
        {
            _height = height;
            return this;
        }

        public HealthBarBuilder WithFontSize(float size)
        {
            _fontSize = size;
            return this;
        }

        public HealthBarBuilder ShowText(bool show = true)
        {
            _showText = show;
            return this;
        }

        public HealthBarBuilder HideText()
        {
            _showText = false;
            return this;
        }

        public HealthBarBuilder WithTextFormat(string format)
        {
            _textFormat = format;
            return this;
        }

        public HealthBarBuilder Health()
        {
            _fillColor = new Color(0.9f, 0.22f, 0.21f, 1f);
            _backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            return this;
        }

        public HealthBarBuilder Mana()
        {
            _fillColor = new Color(0.2f, 0.4f, 0.9f, 1f);
            _backgroundColor = new Color(0.15f, 0.15f, 0.3f, 1f);
            return this;
        }

        public HealthBarBuilder Experience()
        {
            _fillColor = new Color(0.9f, 0.75f, 0.1f, 1f);
            _backgroundColor = new Color(0.3f, 0.25f, 0.1f, 1f);
            return this;
        }

        public HealthBarBuilder Energy()
        {
            _fillColor = new Color(0.2f, 0.85f, 0.4f, 1f);
            _backgroundColor = new Color(0.1f, 0.25f, 0.15f, 1f);
            return this;
        }

        public HealthBarBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            if (colors != null)
            {
                _backgroundColor = colors.darkSurface;
            }
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.GameUI.HealthBar;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            WithPreferredSize(null, _height);
            WithFlexible(1f, null);

            var rt = CreateRectTransform(parent);
            rt.gameObject.AddComponent<CanvasRenderer>();

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = _backgroundColor;
            bg.raycastTarget = false;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(rt, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fillGo.AddComponent<CanvasRenderer>();

            var fill = fillGo.AddComponent<Image>();
            fill.color = _fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = _value;
            fill.raycastTarget = false;

            if (_showText)
            {
                var textGo = new GameObject("Text");
                textGo.transform.SetParent(rt, false);
                var textRt = textGo.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
                textGo.AddComponent<CanvasRenderer>();

                var tmp = textGo.AddComponent<TextMeshProUGUI>();
                tmp.text = string.Format(_textFormat, _currentValue, _maxValue);
                tmp.fontSize = _fontSize;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = _textColor;
                tmp.raycastTarget = false;
            }

            ApplyLayoutElement(rt.gameObject);

            return bg;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            component.color = _backgroundColor;

            var fill = instance.transform.Find("Fill")?.GetComponent<Image>();
            if (fill != null)
            {
                fill.color = _fillColor;
                fill.fillAmount = _value;
            }

            var text = instance.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = string.Format(_textFormat, _currentValue, _maxValue);
                text.color = _textColor;
                text.fontSize = _fontSize;
            }
        }
    }
}
#endif
