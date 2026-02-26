#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements
{
    public class IconBuilder : BaseElementBuilder<IconBuilder, Image>
    {
        protected override string NamingPrefix => NamingConvention.Image;

        private Sprite _sprite;
        private Color _color = Color.white;
        private float _size = 24f;
        private bool _preserveAspect = true;

        public IconBuilder(string name) : base(name) { }

        public IconBuilder WithSprite(Sprite sprite)
        {
            _sprite = sprite;
            return this;
        }

        public IconBuilder WithColor(Color color)
        {
            _color = color;
            return this;
        }

        public IconBuilder WithSize(float size)
        {
            _size = size;
            return this;
        }

        public IconBuilder Small() => WithSize(16f);
        public IconBuilder Medium() => WithSize(24f);
        public IconBuilder Large() => WithSize(32f);
        public IconBuilder ExtraLarge() => WithSize(48f);

        protected override string GetDefaultPrefabPath() => ElementPaths.Atoms.Icon;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            WithPreferredSize(_size, _size);
            var rt = CreateRectTransform(parent);

            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = _sprite;
            image.color = _color;
            image.preserveAspect = _preserveAspect;
            image.raycastTarget = false;

            ApplyLayoutElement(rt.gameObject);

            return image;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            if (_sprite != null) component.sprite = _sprite;
            component.color = _color;

            var le = instance.GetComponent<LayoutElement>();
            if (le != null)
            {
                le.preferredWidth = _size;
                le.preferredHeight = _size;
            }
        }
    }
}
#endif
