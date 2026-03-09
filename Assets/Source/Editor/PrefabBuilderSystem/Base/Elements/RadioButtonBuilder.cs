#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.UI.Controls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class RadioButtonBuilder : BaseElementBuilder<RadioButtonBuilder, Toggle>
    {
        protected override string NamingPrefix => "Radio";

        private string _label;
        private float _fontSize = 14f;
        private float _radioSize = 20f;
        private Color _uncheckedColor = new(0.3f, 0.3f, 0.3f, 1f);
        private Color _checkedColor = new(0.388f, 0.4f, 0.945f, 1f);
        private Color _textColor = Color.white;
        private ToggleGroup _group;

        public RadioButtonBuilder(string name) : base(name) { }

        public RadioButtonBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }

        public RadioButtonBuilder WithFontSize(float size)
        {
            _fontSize = size;
            return this;
        }

        public RadioButtonBuilder WithRadioSize(float size)
        {
            _radioSize = size;
            return this;
        }

        public RadioButtonBuilder WithColors(Color unchecked_, Color checked_)
        {
            _uncheckedColor = unchecked_;
            _checkedColor = checked_;
            return this;
        }

        public RadioButtonBuilder WithTextColor(Color color)
        {
            _textColor = color;
            return this;
        }

        public RadioButtonBuilder WithGroup(ToggleGroup group)
        {
            _group = group;
            return this;
        }

        public RadioButtonBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _uncheckedColor = colors?.inputBackground ?? new Color(0.3f, 0.3f, 0.3f, 1f);
            _checkedColor = colors?.indigo ?? new Color(0.388f, 0.4f, 0.945f, 1f);
            _textColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.RadioButton;

        public override Toggle Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);
            
            var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;

            var csf = rt.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var radioGo = new GameObject("Radio");
            radioGo.transform.SetParent(rt, false);
            var radioRt = radioGo.AddComponent<RectTransform>();

            var radioLe = radioGo.AddComponent<LayoutElement>();
            radioLe.preferredWidth = _radioSize;
            radioLe.preferredHeight = _radioSize;

            var outerCircle = radioGo.AddComponent<Image>();
            outerCircle.color = _uncheckedColor;

            var innerGo = new GameObject("Inner");
            innerGo.transform.SetParent(radioGo.transform, false);
            var innerRt = innerGo.AddComponent<RectTransform>();
            innerRt.anchorMin = new Vector2(0.2f, 0.2f);
            innerRt.anchorMax = new Vector2(0.8f, 0.8f);
            innerRt.offsetMin = Vector2.zero;
            innerRt.offsetMax = Vector2.zero;

            var innerCircle = innerGo.AddComponent<Image>();
            innerCircle.color = _checkedColor;

            var toggle = rt.gameObject.AddComponent<BaseToggle>();
            toggle.targetGraphic = outerCircle;
            toggle.graphic = innerCircle;
            toggle.group = _group;

            if (!string.IsNullOrEmpty(_label))
            {
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(rt, false);
                labelGo.AddComponent<RectTransform>();

                var tmp = labelGo.AddComponent<TextMeshProUGUI>();
                tmp.text = _label;
                tmp.fontSize = _fontSize;
                tmp.color = _textColor;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;

                var labelLe = labelGo.AddComponent<LayoutElement>();
                labelLe.preferredHeight = _fontSize + 6;
            }

            return toggle;
        }

        protected override void ApplyCustomizations(GameObject instance, Toggle component)
        {
            var label = instance.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null && !string.IsNullOrEmpty(_label))
            {
                label.text = _label;
            }

            if (_group != null)
            {
                component.group = _group;
            }
        }
    }
}
#endif
