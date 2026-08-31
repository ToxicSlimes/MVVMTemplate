#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements
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
            if (ShouldUsePrefab())
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
            component.sprite = _backgroundSprite;

            if (_backgroundSprite != null)
            {
                component.type = Image.Type.Sliced;
            }

            ApplyOutline(instance);
            ApplyLayoutGroup(instance);
            ApplyLayoutElement(instance);
        }

        private void ApplyOutline(GameObject instance)
        {
            var outline = instance.GetComponent<Outline>();

            if (_borderWidth <= 0)
            {
                if (outline != null)
                {
                    outline.enabled = false;
                }

                return;
            }

            outline ??= instance.AddComponent<Outline>();
            outline.enabled = true;
            outline.effectColor = _borderColor;
            outline.effectDistance = new Vector2(_borderWidth, -_borderWidth);
        }

        private void ApplyLayoutGroup(GameObject instance)
        {
            var existing = instance.GetComponent<HorizontalOrVerticalLayoutGroup>();
            var matches = _vertical ? existing is VerticalLayoutGroup : existing is HorizontalLayoutGroup;

            if (existing != null && !matches)
            {
                Object.DestroyImmediate(existing);
                existing = null;
            }

            existing ??= _vertical
                ? instance.AddComponent<VerticalLayoutGroup>()
                : (HorizontalOrVerticalLayoutGroup)instance.AddComponent<HorizontalLayoutGroup>();

            existing.padding = new RectOffset(_padding.left, _padding.right, _padding.top, _padding.bottom);
            existing.spacing = _spacing;
            existing.childControlWidth = true;
            existing.childControlHeight = true;
            existing.childForceExpandWidth = _vertical;
            existing.childForceExpandHeight = !_vertical;
        }
    }
}
#endif
