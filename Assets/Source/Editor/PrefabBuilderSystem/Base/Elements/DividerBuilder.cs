#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements
{
    public class DividerBuilder : BaseElementBuilder<DividerBuilder, Image>
    {
        protected override string NamingPrefix => "Divider";

        private Color _color = new(0.3f, 0.3f, 0.3f, 0.5f);
        private float _thickness = 1f;
        private bool _horizontal = true;
        private float _margin;

        public DividerBuilder(string name = "Divider") : base(name) { }

        public DividerBuilder WithColor(Color color)
        {
            _color = color;
            return this;
        }

        public DividerBuilder WithThickness(float thickness)
        {
            _thickness = thickness;
            return this;
        }

        public DividerBuilder Horizontal()
        {
            _horizontal = true;
            return this;
        }

        public DividerBuilder Vertical()
        {
            _horizontal = false;
            return this;
        }

        public DividerBuilder WithMargin(float margin)
        {
            _margin = margin;
            return this;
        }

        public DividerBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _color = colors?.cardBorder ?? new Color(0.58f, 0.64f, 0.72f, 0.1f);
            return this;
        }

        public DividerBuilder Light() => WithColor(new Color(1f, 1f, 1f, 0.1f));
        public DividerBuilder Dark() => WithColor(new Color(0f, 0f, 0f, 0.2f));

        protected override string GetDefaultPrefabPath() => ElementPaths.Atoms.Divider;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);
            
            var le = rt.gameObject.AddComponent<LayoutElement>();
            if (_horizontal)
            {
                le.preferredHeight = _thickness;
                le.flexibleWidth = 1;
            }
            else
            {
                le.preferredWidth = _thickness;
                le.flexibleHeight = 1;
            }

            var image = rt.gameObject.AddComponent<Image>();
            image.color = _color;
            image.raycastTarget = false;

            return image;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            component.color = _color;
            
            var le = instance.GetComponent<LayoutElement>();
            if (le != null)
            {
                if (_horizontal)
                {
                    le.preferredHeight = _thickness;
                }
                else
                {
                    le.preferredWidth = _thickness;
                }
            }
        }
    }
}
#endif
