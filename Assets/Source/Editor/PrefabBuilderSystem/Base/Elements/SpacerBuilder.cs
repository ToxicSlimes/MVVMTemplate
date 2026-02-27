#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class SpacerBuilder : BaseElementBuilder<SpacerBuilder, LayoutElement>
    {
        protected override string NamingPrefix => "Spacer";

        private float? _width;
        private float? _height;
        private bool _flexible;

        public SpacerBuilder(string name = "Spacer") : base(name) { }

        public SpacerBuilder Width(float width)
        {
            _width = width;
            return this;
        }

        public SpacerBuilder Height(float height)
        {
            _height = height;
            return this;
        }

        public SpacerBuilder Flexible()
        {
            _flexible = true;
            return this;
        }

        public SpacerBuilder Small() => Height(8f);
        public SpacerBuilder Medium() => Height(16f);
        public SpacerBuilder Large() => Height(24f);
        public SpacerBuilder XLarge() => Height(32f);

        public SpacerBuilder SetPreferredHeight(float height) => Height(height);
        public SpacerBuilder SetPreferredWidth(float width) => Width(width);
        public SpacerBuilder SetFlexibleHeight(float flex) { _flexible = true; _height = flex; return this; }
        public SpacerBuilder SetFlexibleWidth(float flex) { _flexible = true; _width = flex; return this; }

        protected override string GetDefaultPrefabPath() => ElementPaths.Atoms.Spacer;

        public override LayoutElement Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);
            var le = rt.gameObject.AddComponent<LayoutElement>();

            if (_flexible)
            {
                if (_width.HasValue) le.flexibleWidth = 1;
                if (_height.HasValue) le.flexibleHeight = 1;
                if (!_width.HasValue && !_height.HasValue) le.flexibleHeight = 1;
            }
            else
            {
                if (_width.HasValue) le.preferredWidth = _width.Value;
                if (_height.HasValue) le.preferredHeight = _height.Value;
            }

            return le;
        }
    }
}
#endif
