#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Editor.PrefabBuilderSystem.Elements;

namespace Core.Editor.PrefabBuilderSystem.Elements.Composites
{
    public class ToggleRowBuilder
    {
        private readonly string _name;
        private string _label = "Label";
        private float _height = 40f;
        private float _toggleSize = 24f;
        private float _spacing = 15f;
        private bool _isOn;
        private float _fontSize = 14f;
        private Color _labelColor = Color.white;

        public ToggleRowBuilder(string name) => _name = name;

        public ToggleRowBuilder WithLabel(string label) { _label = label; return this; }
        public ToggleRowBuilder WithHeight(float height) { _height = height; return this; }
        public ToggleRowBuilder WithToggleSize(float size) { _toggleSize = size; return this; }
        public ToggleRowBuilder WithSpacing(float spacing) { _spacing = spacing; return this; }
        public ToggleRowBuilder IsOn(bool value = true) { _isOn = value; return this; }
        public ToggleRowBuilder WithFontSize(float size) { _fontSize = size; return this; }
        public ToggleRowBuilder WithLabelColor(Color color) { _labelColor = color; return this; }

        public (RectTransform root, Toggle toggle) Build(Transform parent)
        {
            var root = new GameObject(_name);
            root.transform.SetParent(parent, false);
            var rootRt = root.AddComponent<RectTransform>();
            
            // Stretch horizontal
            rootRt.anchorMin = new Vector2(0, 0.5f);
            rootRt.anchorMax = new Vector2(1, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.anchoredPosition = Vector2.zero;
            rootRt.sizeDelta = new Vector2(0, _height);

            var le = root.AddComponent<LayoutElement>();
            le.preferredHeight = _height;
            le.flexibleWidth = 1;

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = _spacing;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;

            // Toggle — use Toggle prefab
            var toggle = UI.Toggle("Toggle")
                .FromPrefab(ElementPaths.Interactive.Toggle)
                .WithToggleSize(_toggleSize)
                .IsOn(_isOn)
                .WithPreferredSize(_toggleSize, _toggleSize)
                .Build(root.transform);

            // Label — use Text prefab
            UI.Text("Label")
                .FromPrefab(ElementPaths.Atoms.Text)
                .WithText(_label)
                .WithFontSize(_fontSize)
                .WithColor(_labelColor)
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .WithFlexible(1f, null)
                .Build(root.transform);

            return (rootRt, toggle);
        }

        public GameObject SaveAsPrefab(string path)
        {
            var tempParent = new GameObject("_TempParent");
            try
            {
                var (root, _) = Build(tempParent.transform);
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
