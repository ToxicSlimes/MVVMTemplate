#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EndlessAdventure.Editor.PrefabBuilderSystem.Elements;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements.Composites
{
    public class SliderRowBuilder
    {
        private readonly string _name;
        private string _label = "Label";
        private float _labelWidth = 200f;
        private float _valueWidth = 70f;
        private float _height = 40f;
        private float _minValue;
        private float _maxValue = 1f;
        private float _value = 0.5f;
        private bool _wholeNumbers;
        private string _valueFormat = "{0:P0}";
        private float _fontSize = 14f;
        private Color _labelColor = Color.white;
        private Color _valueColor = Color.white;

        public SliderRowBuilder(string name) => _name = name;

        public SliderRowBuilder WithLabel(string label) { _label = label; return this; }
        public SliderRowBuilder WithLabelWidth(float width) { _labelWidth = width; return this; }
        public SliderRowBuilder WithValueWidth(float width) { _valueWidth = width; return this; }
        public SliderRowBuilder WithHeight(float height) { _height = height; return this; }
        public SliderRowBuilder WithRange(float min, float max) { _minValue = min; _maxValue = max; return this; }
        public SliderRowBuilder WithValue(float value) { _value = value; return this; }
        public SliderRowBuilder WholeNumbers() { _wholeNumbers = true; _valueFormat = "{0:0}"; return this; }
        public SliderRowBuilder WithValueFormat(string format) { _valueFormat = format; return this; }
        public SliderRowBuilder WithFontSize(float size) { _fontSize = size; return this; }
        public SliderRowBuilder WithColors(Color label, Color value) { _labelColor = label; _valueColor = value; return this; }

        public SliderRowBuilder Percentage() { _valueFormat = "{0:P0}"; return this; }
        public SliderRowBuilder Decimal(int digits = 1) { _valueFormat = "{0:F" + digits + "}"; return this; }

        public (RectTransform root, Slider slider, TMP_Text valueText) Build(Transform parent)
        {
            var root = new GameObject(_name);
            root.transform.SetParent(parent, false);
            var rootRt = root.AddComponent<RectTransform>();
            
            // Stretch horizontal, auto height via LayoutElement
            rootRt.anchorMin = new Vector2(0, 0.5f);
            rootRt.anchorMax = new Vector2(1, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.anchoredPosition = Vector2.zero;
            rootRt.sizeDelta = new Vector2(0, _height);

            var le = root.AddComponent<LayoutElement>();
            le.preferredHeight = _height;
            le.flexibleWidth = 1;

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Label — use Text prefab
            var labelTmp = UI.Text("Label")
                .FromPrefab(ElementPaths.Atoms.Text)
                .WithText(_label)
                .WithFontSize(_fontSize)
                .WithColor(_labelColor)
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .WithPreferredSize(_labelWidth, null)
                .WithMinSize(_labelWidth, null)
                .Build(root.transform);

            // Slider — use Slider prefab
            var slider = UI.Slider("Slider")
                .FromPrefab(ElementPaths.Interactive.Slider)
                .WithRange(_minValue, _maxValue)
                .WithValue(_value)
                .WithFlexible(1f, null)
                .WithMinSize(100f, null)
                .Build(root.transform);
            
            if (_wholeNumbers) slider.wholeNumbers = true;

            // Value Text — use Text prefab
            var valueTmp = UI.Text("Value")
                .FromPrefab(ElementPaths.Atoms.Text)
                .WithText(string.Format(_valueFormat, _value))
                .WithFontSize(_fontSize)
                .WithColor(_valueColor)
                .WithAlignment(TextAlignmentOptions.MidlineRight)
                .WithPreferredSize(_valueWidth, null)
                .Build(root.transform);

            return (rootRt, slider, valueTmp);
        }

        public GameObject SaveAsPrefab(string path)
        {
            var tempParent = new GameObject("_TempParent");
            try
            {
                var (root, _, _) = Build(tempParent.transform);
                root.transform.SetParent(null);
                var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
                Object.DestroyImmediate(root.gameObject);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(tempParent);
            }
        }
    }
}
#endif
