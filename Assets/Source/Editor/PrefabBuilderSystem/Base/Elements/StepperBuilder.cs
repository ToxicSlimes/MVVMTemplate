#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements
{
    public class StepperBuilder : BaseElementBuilder<StepperBuilder, RectTransform>
    {
        protected override string NamingPrefix => "Stepper";

        private int _value;
        private int _minValue;
        private int _maxValue = 100;
        private int _step = 1;
        private float _buttonSize = 32f;
        private float _valueWidth = 60f;
        private float _fontSize = 16f;
        private Color _buttonColor = new(0.388f, 0.4f, 0.945f, 1f);
        private Color _valueColor = new(0.15f, 0.15f, 0.2f, 1f);
        private Color _textColor = Color.white;

        public StepperBuilder(string name) : base(name) { }

        public StepperBuilder WithValue(int value)
        {
            _value = Mathf.Clamp(value, _minValue, _maxValue);
            return this;
        }

        public StepperBuilder WithRange(int min, int max)
        {
            _minValue = min;
            _maxValue = max;
            _value = Mathf.Clamp(_value, min, max);
            return this;
        }

        public StepperBuilder WithStep(int step)
        {
            _step = step;
            return this;
        }

        public StepperBuilder WithButtonSize(float size)
        {
            _buttonSize = size;
            return this;
        }

        public StepperBuilder WithValueWidth(float width)
        {
            _valueWidth = width;
            return this;
        }

        public StepperBuilder WithFontSize(float size)
        {
            _fontSize = size;
            return this;
        }

        public StepperBuilder WithButtonColor(Color color)
        {
            _buttonColor = color;
            return this;
        }

        public StepperBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _buttonColor = colors?.primaryButtonGradientStart ?? new Color(0.388f, 0.4f, 0.945f, 1f);
            _valueColor = colors?.inputBackground ?? new Color(0.15f, 0.15f, 0.2f, 1f);
            _textColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.Stepper;

        public override RectTransform Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);

            var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 2f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            var csf = rt.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = _buttonSize;

            BuildButton(rt.transform, "Btn_Minus", "−");
            BuildValueDisplay(rt.transform);
            BuildButton(rt.transform, "Btn_Plus", "+");

            return rt;
        }

        private void BuildButton(Transform parent, string name, string symbol)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            btnGo.AddComponent<RectTransform>();

            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredWidth = _buttonSize;
            btnLe.preferredHeight = _buttonSize;

            var img = btnGo.AddComponent<Image>();
            img.color = _buttonColor;

            btnGo.AddComponent<Button>().targetGraphic = img;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = symbol;
            tmp.fontSize = _fontSize * 1.2f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = _textColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void BuildValueDisplay(Transform parent)
        {
            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(parent, false);
            valueGo.AddComponent<RectTransform>();

            var valueLe = valueGo.AddComponent<LayoutElement>();
            valueLe.preferredWidth = _valueWidth;
            valueLe.preferredHeight = _buttonSize;

            var bg = valueGo.AddComponent<Image>();
            bg.color = _valueColor;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(valueGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = _value.ToString();
            tmp.fontSize = _fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = _textColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        protected override void ApplyCustomizations(GameObject instance, RectTransform component)
        {
            var valueText = instance.transform.Find("Value/Text")?.GetComponent<TextMeshProUGUI>();
            if (valueText != null)
            {
                valueText.text = _value.ToString();
            }
        }
    }
}
#endif
