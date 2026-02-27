#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class ToggleBuilder : BaseElementBuilder<ToggleBuilder, Toggle>
    {
        protected override string NamingPrefix => NamingConvention.Toggle;
        
        private string _label;
        private bool _isOn;
        private float _labelFontSize = 14f;
        private Color _labelColor = Color.white;
        private float _spacing = 8f;
        private Color _checkmarkColor = Color.white;
        private Color _backgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
        private float _toggleSize = 20f;
        private Sprite _backgroundSprite;
        private Sprite _checkmarkSprite;

        public ToggleBuilder(string name) : base(name) { }

        public ToggleBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }

        public ToggleBuilder IsOn(bool value)
        {
            _isOn = value;
            return this;
        }

        public ToggleBuilder WithLabelSize(float size)
        {
            _labelFontSize = size;
            return this;
        }

        public ToggleBuilder WithLabelColor(Color color)
        {
            _labelColor = color;
            return this;
        }

        public ToggleBuilder WithSpacing(float spacing)
        {
            _spacing = spacing;
            return this;
        }

        public ToggleBuilder WithCheckmarkColor(Color color)
        {
            _checkmarkColor = color;
            return this;
        }

        public ToggleBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public ToggleBuilder WithToggleSize(float size)
        {
            _toggleSize = size;
            return this;
        }

        public ToggleBuilder WithBackgroundSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        public ToggleBuilder WithCheckmarkSprite(Sprite sprite)
        {
            _checkmarkSprite = sprite;
            return this;
        }

        public ToggleBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.inputBackground ?? new Color(0.2f, 0.2f, 0.2f, 1f);
            _checkmarkColor = colors?.indigo ?? new Color(0.388f, 0.4f, 0.945f, 1f);
            _labelColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.Toggle;

        protected override void ApplyCustomizations(GameObject instance, Toggle toggle)
        {
            if (toggle == null) return;

            toggle.isOn = _isOn;

            var bg = instance.transform.Find("Background");
            if (bg != null)
            {
                var bgImg = bg.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.color = _backgroundColor;
                    if (_backgroundSprite != null) { bgImg.sprite = _backgroundSprite; bgImg.type = Image.Type.Sliced; }
                }

                var checkmark = bg.Find("Checkmark");
                if (checkmark != null)
                {
                    var cmImg = checkmark.GetComponent<Image>();
                    if (cmImg != null)
                    {
                        cmImg.color = _checkmarkColor;
                        if (_checkmarkSprite != null) cmImg.sprite = _checkmarkSprite;
                    }
                }
            }

            var label = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null && !string.IsNullOrEmpty(_label))
            {
                label.text = _label;
                label.fontSize = _labelFontSize;
                label.color = _labelColor;
            }

            ApplyLayoutElement(instance);
        }

        public override Toggle Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = _spacing;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var checkboxGo = new GameObject("Background");
            checkboxGo.transform.SetParent(go.transform, false);
            var checkboxRt = checkboxGo.AddComponent<RectTransform>();
            checkboxRt.sizeDelta = new Vector2(_toggleSize, _toggleSize);

            var checkboxImg = checkboxGo.AddComponent<Image>();
            checkboxImg.color = _backgroundColor;
            checkboxImg.sprite = _backgroundSprite;
            if (_backgroundSprite != null)
            {
                checkboxImg.type = Image.Type.Sliced;
            }

            var checkboxLe = checkboxGo.AddComponent<LayoutElement>();
            checkboxLe.preferredWidth = _toggleSize;
            checkboxLe.preferredHeight = _toggleSize;

            var checkmarkGo = new GameObject("Checkmark");
            checkmarkGo.transform.SetParent(checkboxGo.transform, false);
            var checkmarkRt = checkmarkGo.AddComponent<RectTransform>();
            checkmarkRt.anchorMin = Vector2.zero;
            checkmarkRt.anchorMax = Vector2.one;
            checkmarkRt.offsetMin = new Vector2(4, 4);
            checkmarkRt.offsetMax = new Vector2(-4, -4);

            var checkmarkImg = checkmarkGo.AddComponent<Image>();
            checkmarkImg.color = _checkmarkColor;
            checkmarkImg.sprite = _checkmarkSprite;

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = checkboxImg;
            toggle.graphic = checkmarkImg;
            toggle.isOn = _isOn;

            if (!string.IsNullOrEmpty(_label))
            {
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(go.transform, false);
                var labelRt = labelGo.AddComponent<RectTransform>();

                var tmp = labelGo.AddComponent<TextMeshProUGUI>();
                tmp.text = _label;
                tmp.fontSize = _labelFontSize;
                tmp.color = _labelColor;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;

                var labelLe = labelGo.AddComponent<LayoutElement>();
                labelLe.flexibleWidth = 1;
            }

            ApplyLayoutElement(go);

            return toggle;
        }
    }
}
#endif
