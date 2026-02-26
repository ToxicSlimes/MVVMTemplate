#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements.GameUI
{
    public class AvatarBuilder : BaseElementBuilder<AvatarBuilder, Image>
    {
        protected override string NamingPrefix => "Avatar";

        private float _size = 64f;
        private Color _backgroundColor = new(0.3f, 0.3f, 0.3f, 1f);
        private Color _borderColor = new(0.1f, 0.1f, 0.1f, 1f);
        private float _borderWidth = 2f;
        private bool _showBorder = true;
        private Sprite _sprite;
        private Sprite _maskSprite;
        private bool _preserveAspect = true;

        public AvatarBuilder(string name) : base(name) { }

        public AvatarBuilder WithSize(float size)
        {
            _size = size;
            return this;
        }

        public AvatarBuilder WithSprite(Sprite sprite)
        {
            _sprite = sprite;
            return this;
        }

        public AvatarBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public AvatarBuilder WithBorder(Color color, float width = 2f)
        {
            _borderColor = color;
            _borderWidth = width;
            _showBorder = true;
            return this;
        }

        public AvatarBuilder NoBorder()
        {
            _showBorder = false;
            return this;
        }

        public AvatarBuilder WithMask(Sprite maskSprite)
        {
            _maskSprite = maskSprite;
            return this;
        }

        public AvatarBuilder PreserveAspect(bool preserve = true)
        {
            _preserveAspect = preserve;
            return this;
        }

        public AvatarBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            if (colors != null)
            {
                _backgroundColor = colors.darkSurface;
                _borderColor = colors.elevatedSurface;
            }
            return this;
        }

        public AvatarBuilder Hero()
        {
            _borderColor = new Color(0.8f, 0.7f, 0.2f, 1f);
            _borderWidth = 3f;
            return this;
        }

        public AvatarBuilder Enemy()
        {
            _borderColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            _borderWidth = 2f;
            return this;
        }

        public AvatarBuilder NPC()
        {
            _borderColor = new Color(0.2f, 0.6f, 0.8f, 1f);
            _borderWidth = 2f;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.GameUI.Avatar;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            WithPreferredSize(_size, _size);

            var rt = CreateRectTransform(parent);
            rt.sizeDelta = new Vector2(_size, _size);
            rt.gameObject.AddComponent<CanvasRenderer>();

            var rootImg = rt.gameObject.AddComponent<Image>();
            rootImg.color = _showBorder ? _borderColor : Color.clear;
            rootImg.raycastTarget = false;

            var padding = _showBorder ? _borderWidth : 0f;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(rt, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(padding, padding);
            iconRt.offsetMax = new Vector2(-padding, -padding);
            iconGo.AddComponent<CanvasRenderer>();

            var icon = iconGo.AddComponent<Image>();
            icon.color = _sprite != null ? Color.white : _backgroundColor;
            icon.sprite = _sprite;
            icon.preserveAspect = _preserveAspect;
            icon.raycastTarget = false;

            if (_maskSprite != null)
            {
                var maskGo = new GameObject("Mask");
                maskGo.transform.SetParent(rt, false);
                maskGo.transform.SetAsFirstSibling();
                var maskRt = maskGo.AddComponent<RectTransform>();
                maskRt.anchorMin = Vector2.zero;
                maskRt.anchorMax = Vector2.one;
                maskRt.offsetMin = new Vector2(padding, padding);
                maskRt.offsetMax = new Vector2(-padding, -padding);
                maskGo.AddComponent<CanvasRenderer>();

                var maskImg = maskGo.AddComponent<Image>();
                maskImg.sprite = _maskSprite;
                maskGo.AddComponent<Mask>().showMaskGraphic = false;

                iconGo.transform.SetParent(maskGo.transform, false);
                iconRt.anchorMin = Vector2.zero;
                iconRt.anchorMax = Vector2.one;
                iconRt.offsetMin = Vector2.zero;
                iconRt.offsetMax = Vector2.zero;
            }

            ApplyLayoutElement(rt.gameObject);

            return rootImg;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            component.color = _showBorder ? _borderColor : Color.clear;

            var icon = instance.transform.Find("Icon")?.GetComponent<Image>()
                       ?? instance.transform.Find("Mask/Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = _sprite;
                icon.color = _sprite != null ? Color.white : _backgroundColor;
                icon.preserveAspect = _preserveAspect;
            }
        }
    }
}
#endif
