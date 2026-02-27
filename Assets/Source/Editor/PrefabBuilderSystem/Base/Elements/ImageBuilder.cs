#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class ImageBuilder : BaseElementBuilder<ImageBuilder, Image>
    {
        protected override string NamingPrefix => NamingConvention.Image;
        
        private Color _color = Color.white;
        private Sprite _sprite;
        private Image.Type _imageType = Image.Type.Simple;
        private bool _preserveAspect;
        private bool _raycastTarget = true;

        public ImageBuilder(string name) : base(name) { }

        public ImageBuilder WithColor(Color color)
        {
            _color = color;
            return this;
        }

        public ImageBuilder WithSprite(Sprite sprite)
        {
            _sprite = sprite;
            return this;
        }

        public ImageBuilder Sliced()
        {
            _imageType = Image.Type.Sliced;
            return this;
        }

        public ImageBuilder Tiled()
        {
            _imageType = Image.Type.Tiled;
            return this;
        }

        public ImageBuilder Filled()
        {
            _imageType = Image.Type.Filled;
            return this;
        }

        public ImageBuilder PreserveAspect()
        {
            _preserveAspect = true;
            return this;
        }

        public ImageBuilder NoRaycast()
        {
            _raycastTarget = false;
            return this;
        }

        public ImageBuilder DeepSpace()
        {
            _color = DesignSystemProvider.Colors?.deepSpace ?? new Color(0.039f, 0.059f, 0.118f);
            return this;
        }

        public ImageBuilder DarkSurface()
        {
            _color = DesignSystemProvider.Colors?.darkSurface ?? new Color(0.082f, 0.106f, 0.176f);
            return this;
        }

        public ImageBuilder ElevatedSurface()
        {
            _color = DesignSystemProvider.Colors?.elevatedSurface ?? new Color(0.118f, 0.161f, 0.365f, 0.8f);
            return this;
        }

        public ImageBuilder Card()
        {
            _color = DesignSystemProvider.Colors?.cardBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.4f);
            return this;
        }

        public ImageBuilder InputBackground()
        {
            _color = DesignSystemProvider.Colors?.inputBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.6f);
            return this;
        }

        public ImageBuilder UserBubble()
        {
            _color = DesignSystemProvider.Colors?.userBubbleStart ?? new Color(0.4f, 0.494f, 0.918f, 0.95f);
            return this;
        }

        public ImageBuilder AIBubble()
        {
            _color = DesignSystemProvider.Colors?.aiBubble ?? new Color(0.118f, 0.161f, 0.365f, 0.95f);
            return this;
        }

        public ImageBuilder ErrorBubble()
        {
            _color = DesignSystemProvider.Colors?.errorBubble ?? new Color(0.937f, 0.267f, 0.267f, 0.15f);
            return this;
        }

        public ImageBuilder Indigo()
        {
            _color = DesignSystemProvider.Colors?.indigo ?? new Color(0.388f, 0.4f, 0.945f);
            return this;
        }

        public ImageBuilder Pink()
        {
            _color = DesignSystemProvider.Colors?.pink ?? new Color(0.925f, 0.282f, 0.6f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Atoms.Image;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var img = rt.gameObject.AddComponent<Image>();

            img.color = _color;
            img.sprite = _sprite;
            img.type = _imageType;
            img.preserveAspect = _preserveAspect;
            img.raycastTarget = _raycastTarget;

            ApplyLayoutElement(rt.gameObject);

            return img;
        }
    }
}
#endif
