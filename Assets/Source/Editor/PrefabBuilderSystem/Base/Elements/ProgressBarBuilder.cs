#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements
{
    public class ProgressBarBuilder : BaseElementBuilder<ProgressBarBuilder, Image>
    {
        protected override string NamingPrefix => "Progress";

        private float _value = 1f;
        private Color _backgroundColor = new(0.2f, 0.2f, 0.2f, 0.8f);
        private Color _fillColor = new(0.388f, 0.4f, 0.945f, 1f);
        private float _height = 20f;
        private bool _showText;
        private string _textFormat = "{0:P0}";

        public ProgressBarBuilder(string name) : base(name) { }

        public ProgressBarBuilder WithValue(float value)
        {
            _value = Mathf.Clamp01(value);
            return this;
        }

        public ProgressBarBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public ProgressBarBuilder WithFillColor(Color color)
        {
            _fillColor = color;
            return this;
        }

        public ProgressBarBuilder WithHeight(float height)
        {
            _height = height;
            return this;
        }

        public ProgressBarBuilder ShowText(string format = "{0:P0}")
        {
            _showText = true;
            _textFormat = format;
            return this;
        }

        public ProgressBarBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.darkSurface ?? new Color(0.2f, 0.2f, 0.2f, 0.8f);
            _fillColor = colors?.indigo ?? new Color(0.388f, 0.4f, 0.945f, 1f);
            return this;
        }

        public ProgressBarBuilder Health()
        {
            _fillColor = new Color(0.2f, 0.8f, 0.2f, 1f);
            _backgroundColor = new Color(0.5f, 0.1f, 0.1f, 0.8f);
            return this;
        }

        public ProgressBarBuilder Mana()
        {
            _fillColor = new Color(0.2f, 0.4f, 0.9f, 1f);
            _backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.8f);
            return this;
        }

        public ProgressBarBuilder Experience()
        {
            _fillColor = new Color(0.9f, 0.8f, 0.2f, 1f);
            _backgroundColor = new Color(0.3f, 0.25f, 0.1f, 0.8f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.ProgressBar;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            WithPreferredSize(null, _height);
            WithFlexible(1f, null);

            var rt = CreateRectTransform(parent);
            
            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = _backgroundColor;
            bg.raycastTarget = false;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(rt, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(_value, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            var fill = fillGo.AddComponent<Image>();
            fill.color = _fillColor;
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

                var tmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
                tmp.text = string.Format(_textFormat, _value);
                tmp.fontSize = _height * 0.6f;
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                tmp.color = Color.white;
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
                var fillRt = fill.GetComponent<RectTransform>();
                fillRt.anchorMax = new Vector2(_value, 1f);
            }
        }
    }
}
#endif
