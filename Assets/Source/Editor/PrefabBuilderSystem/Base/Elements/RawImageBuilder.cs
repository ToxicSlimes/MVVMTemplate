#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class RawImageBuilder : BaseElementBuilder<RawImageBuilder, RawImage>
    {
        protected override string NamingPrefix => "RawImg";

        private Texture _texture;
        private Color _color = Color.white;
        private Rect _uvRect = new(0, 0, 1, 1);
        private bool _raycastTarget = true;

        public RawImageBuilder(string name) : base(name) { }

        public RawImageBuilder WithTexture(Texture texture)
        {
            _texture = texture;
            return this;
        }

        public RawImageBuilder WithColor(Color color)
        {
            _color = color;
            return this;
        }

        public RawImageBuilder WithUVRect(Rect rect)
        {
            _uvRect = rect;
            return this;
        }

        public RawImageBuilder NoRaycast()
        {
            _raycastTarget = false;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Atoms.RawImage;

        public override RawImage Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);
            var rawImage = rt.gameObject.AddComponent<RawImage>();

            rawImage.texture = _texture;
            rawImage.color = _color;
            rawImage.uvRect = _uvRect;
            rawImage.raycastTarget = _raycastTarget;

            ApplyLayoutElement(rt.gameObject);

            return rawImage;
        }

        protected override void ApplyCustomizations(GameObject instance, RawImage component)
        {
            if (_texture != null) component.texture = _texture;
            component.color = _color;
        }
    }
}
#endif
