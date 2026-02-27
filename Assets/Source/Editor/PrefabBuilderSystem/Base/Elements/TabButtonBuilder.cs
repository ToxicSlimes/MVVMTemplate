#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class TabButtonBuilder : BaseElementBuilder<TabButtonBuilder, Toggle>
    {
        protected override string NamingPrefix => NamingConvention.Button;

        private string _label = "Tab";
        private float _fontSize = 14f;
        private Color _normalColor = new(0.15f, 0.15f, 0.2f, 1f);
        private Color _selectedColor = new(0.388f, 0.4f, 0.945f, 1f);
        private Color _hoverColor = new(0.25f, 0.25f, 0.3f, 1f);
        private Color _textColor = Color.white;
        private float _minWidth = 80f;
        private float _height = 40f;
        private bool _showIndicator = true;
        private ToggleGroup _group;

        public TabButtonBuilder(string name) : base(name) { }

        public TabButtonBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }

        public TabButtonBuilder WithFontSize(float size)
        {
            _fontSize = size;
            return this;
        }

        public TabButtonBuilder WithNormalColor(Color color)
        {
            _normalColor = color;
            return this;
        }

        public TabButtonBuilder WithSelectedColor(Color color)
        {
            _selectedColor = color;
            return this;
        }

        public TabButtonBuilder WithTextColor(Color color)
        {
            _textColor = color;
            return this;
        }

        public TabButtonBuilder WithMinWidth(float width)
        {
            _minWidth = width;
            return this;
        }

        public TabButtonBuilder WithHeight(float height)
        {
            _height = height;
            return this;
        }

        public TabButtonBuilder WithGroup(ToggleGroup group)
        {
            _group = group;
            return this;
        }

        public TabButtonBuilder NoIndicator()
        {
            _showIndicator = false;
            return this;
        }

        public TabButtonBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _normalColor = colors?.darkSurface ?? new Color(0.15f, 0.15f, 0.2f, 1f);
            _selectedColor = colors?.indigo ?? new Color(0.388f, 0.4f, 0.945f, 1f);
            _hoverColor = colors?.elevatedSurface ?? new Color(0.25f, 0.25f, 0.3f, 1f);
            _textColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.TabButton;

        public override Toggle Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);
            
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minWidth = _minWidth;
            le.preferredHeight = _height;

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = _normalColor;

            var toggle = rt.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.group = _group;

            var colors = toggle.colors;
            colors.normalColor = _normalColor;
            colors.highlightedColor = _hoverColor;
            colors.selectedColor = _selectedColor;
            colors.pressedColor = _selectedColor;
            toggle.colors = colors;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rt, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(10, _showIndicator ? 4 : 0);
            labelRt.offsetMax = new Vector2(-10, 0);

            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = _label;
            tmp.fontSize = _fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = _textColor;
            tmp.alignment = TextAlignmentOptions.Center;

            if (_showIndicator)
            {
                var indicatorGo = new GameObject("Indicator");
                indicatorGo.transform.SetParent(rt, false);
                var indicatorRt = indicatorGo.AddComponent<RectTransform>();
                indicatorRt.anchorMin = new Vector2(0, 0);
                indicatorRt.anchorMax = new Vector2(1, 0);
                indicatorRt.pivot = new Vector2(0.5f, 0);
                indicatorRt.sizeDelta = new Vector2(0, 3);
                indicatorRt.anchoredPosition = Vector2.zero;

                var indicator = indicatorGo.AddComponent<Image>();
                indicator.color = _selectedColor;
                
                toggle.graphic = indicator;
            }

            return toggle;
        }

        protected override void ApplyCustomizations(GameObject instance, Toggle component)
        {
            var label = instance.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = _label;
                label.color = _textColor;
            }

            if (_group != null)
            {
                component.group = _group;
            }
        }
    }
}
#endif
