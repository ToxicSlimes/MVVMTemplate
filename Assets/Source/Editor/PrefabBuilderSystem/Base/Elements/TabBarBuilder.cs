#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class TabBarBuilder : BaseElementBuilder<TabBarBuilder, ToggleGroup>
    {
        protected override string NamingPrefix => "TabBar";

        private float _height = 44f;
        private float _spacing = 4f;
        private Color _backgroundColor = new(0.1f, 0.12f, 0.15f, 1f);
        private RectOffset _padding = new(8, 8, 4, 4);
        private bool _equalWidth;

        public TabBarBuilder(string name) : base(name) { }

        public TabBarBuilder WithHeight(float height)
        {
            _height = height;
            return this;
        }

        public TabBarBuilder WithSpacing(float spacing)
        {
            _spacing = spacing;
            return this;
        }

        public TabBarBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public TabBarBuilder WithPadding(int all)
        {
            _padding = new RectOffset(all, all, all, all);
            return this;
        }

        public TabBarBuilder EqualWidth()
        {
            _equalWidth = true;
            return this;
        }

        public TabBarBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.darkSurface ?? new Color(0.1f, 0.12f, 0.15f, 1f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.TabBar;

        public override ToggleGroup Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = _height;

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = _backgroundColor;

            var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = _padding;
            hlg.spacing = _spacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = _equalWidth;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = _equalWidth;
            hlg.childForceExpandHeight = false;

            var toggleGroup = rt.gameObject.AddComponent<ToggleGroup>();
            toggleGroup.allowSwitchOff = false;

            return toggleGroup;
        }

        protected override void ApplyCustomizations(GameObject instance, ToggleGroup component)
        {
            var bg = instance.GetComponent<Image>();
            if (bg != null) bg.color = _backgroundColor;
        }
    }
}
#endif
