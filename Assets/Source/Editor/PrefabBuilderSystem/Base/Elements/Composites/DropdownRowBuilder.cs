#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Core.Editor.PrefabBuilderSystem.Elements;

namespace Core.Editor.PrefabBuilderSystem.Elements.Composites
{
    public class DropdownRowBuilder
    {
        private readonly string _name;
        private string _label = "Label";
        private float _labelWidth = 200f;
        private float _height = 40f;
        private float _fontSize = 14f;
        private Color _labelColor = Color.white;
        private readonly List<string> _options = new();
        private int _selectedIndex;

        public DropdownRowBuilder(string name) => _name = name;

        public DropdownRowBuilder WithLabel(string label) { _label = label; return this; }
        public DropdownRowBuilder WithLabelWidth(float width) { _labelWidth = width; return this; }
        public DropdownRowBuilder WithHeight(float height) { _height = height; return this; }
        public DropdownRowBuilder WithFontSize(float size) { _fontSize = size; return this; }
        public DropdownRowBuilder WithLabelColor(Color color) { _labelColor = color; return this; }
        public DropdownRowBuilder WithOptions(params string[] options) { _options.AddRange(options); return this; }
        public DropdownRowBuilder WithSelectedIndex(int index) { _selectedIndex = index; return this; }

        public (RectTransform root, TMP_Dropdown dropdown) Build(Transform parent)
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
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Label — use Text prefab
            UI.Text("Label")
                .FromPrefab(ElementPaths.Atoms.Text)
                .WithText(_label)
                .WithFontSize(_fontSize)
                .WithColor(_labelColor)
                .WithAlignment(TextAlignmentOptions.MidlineLeft)
                .WithPreferredSize(_labelWidth, null)
                .WithMinSize(_labelWidth, null)
                .Build(root.transform);

            // Dropdown — use Dropdown prefab
            var dropdown = UI.Dropdown("Dropdown")
                .FromPrefab(ElementPaths.Interactive.Dropdown)
                .WithFontSize(_fontSize)
                .WithFlexible(1f, null)
                .Build(root.transform);

            if (_options.Count > 0)
            {
                dropdown.ClearOptions();
                dropdown.AddOptions(_options);
                dropdown.value = _selectedIndex;
            }

            return (rootRt, dropdown);
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
