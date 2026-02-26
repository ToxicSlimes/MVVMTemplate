#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements
{
    public class CardBuilder : BaseElementBuilder<CardBuilder, Image>
    {
        protected override string NamingPrefix => "Card";

        private Color _backgroundColor = new(0.12f, 0.14f, 0.18f, 1f);
        private Color _borderColor = new(0.58f, 0.64f, 0.72f, 0.1f);
        private float _borderWidth = 1f;
        private RectOffset _padding = new(16, 16, 16, 16);
        private float _spacing = 8f;
        private bool _vertical = true;
        private Sprite _backgroundSprite;

        public CardBuilder(string name) : base(name) { }

        public CardBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public CardBuilder WithBorder(Color color, float width = 1f)
        {
            _borderColor = color;
            _borderWidth = width;
            return this;
        }

        public CardBuilder WithPadding(int all)
        {
            _padding = new RectOffset(all, all, all, all);
            return this;
        }

        public CardBuilder WithPadding(int horizontal, int vertical)
        {
            _padding = new RectOffset(horizontal, horizontal, vertical, vertical);
            return this;
        }

        public CardBuilder WithPadding(int left, int right, int top, int bottom)
        {
            _padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public CardBuilder WithSpacing(float spacing)
        {
            _spacing = spacing;
            return this;
        }

        public CardBuilder Horizontal()
        {
            _vertical = false;
            return this;
        }

        public CardBuilder Vertical()
        {
            _vertical = true;
            return this;
        }

        public CardBuilder WithBackground(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        public CardBuilder NoBorder()
        {
            _borderWidth = 0;
            return this;
        }

        public CardBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.cardBackground ?? new Color(0.12f, 0.14f, 0.18f, 1f);
            _borderColor = colors?.cardBorder ?? new Color(0.58f, 0.64f, 0.72f, 0.1f);
            return this;
        }

        public CardBuilder Dark()
        {
            _backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
            return this;
        }

        public CardBuilder Light()
        {
            _backgroundColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            _borderColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.Card;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = _backgroundColor;
            bg.sprite = _backgroundSprite;
            if (_backgroundSprite != null)
            {
                bg.type = Image.Type.Sliced;
            }

            if (_borderWidth > 0)
            {
                var outline = rt.gameObject.AddComponent<Outline>();
                outline.effectColor = _borderColor;
                outline.effectDistance = new Vector2(_borderWidth, -_borderWidth);
            }

            if (_vertical)
            {
                var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.padding = _padding;
                vlg.spacing = _spacing;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
            }
            else
            {
                var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = _padding;
                hlg.spacing = _spacing;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
            }

            ApplyLayoutElement(rt.gameObject);

            return bg;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            component.color = _backgroundColor;

            var outline = instance.GetComponent<Outline>();
            if (outline != null && _borderWidth > 0)
            {
                outline.effectColor = _borderColor;
            }
        }
    }
}
#endif
