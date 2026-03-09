#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Infrastructure.UI;
using Core.UI.Controls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public enum ButtonVariant
    {
        Default,
        Primary,
        Secondary,
        Outline,
        Ghost,
        Danger,
        IconButton
    }

    public class ButtonBuilder : BaseElementBuilder<ButtonBuilder, BaseButton>
    {
        protected override string NamingPrefix => NamingConvention.Button;
        
        private string _label;
        private float _labelFontSize = 16f;
        private Color _backgroundColor = Color.white;
        private Color _labelColor = new(0.2f, 0.2f, 0.2f, 1f);
        private Sprite _backgroundSprite;
        private Sprite _icon;
        private float _iconSize = 24f;
        private bool _iconOnly;
        private bool _addOutline;
        private Color _outlineColor = new(0.1f, 0.1f, 0.1f, 1f);
        private Vector2 _outlineDistance = new(1, -1);

        private ColorBlock? _colors;

        public ButtonBuilder(string name) : base(name) { }

        public ButtonBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }

        public ButtonBuilder WithLabelSize(float size)
        {
            _labelFontSize = size;
            return this;
        }

        public ButtonBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public ButtonBuilder WithLabelColor(Color color)
        {
            _labelColor = color;
            return this;
        }

        public ButtonBuilder WithBackground(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        public ButtonBuilder WithIcon(Sprite icon, float size = 24f)
        {
            _icon = icon;
            _iconSize = size;
            return this;
        }

        public ButtonBuilder IconOnly()
        {
            _iconOnly = true;
            return this;
        }

        public ButtonBuilder WithOutline(Color? color = null, Vector2? distance = null)
        {
            _addOutline = true;
            if (color.HasValue) _outlineColor = color.Value;
            if (distance.HasValue) _outlineDistance = distance.Value;
            return this;
        }

        public ButtonBuilder WithColors(ColorBlock colors)
        {
            _colors = colors;
            return this;
        }

        public ButtonBuilder WithVariant(ButtonVariant variant)
        {
            var colors = DesignSystemProvider.Colors;
            
            switch (variant)
            {
                case ButtonVariant.Primary:
                    _backgroundColor = colors?.primaryButtonGradientStart ?? new Color(0.388f, 0.4f, 0.945f, 1f);
                    _labelColor = colors?.textPrimary ?? Color.white;
                    break;
                case ButtonVariant.Secondary:
                    _backgroundColor = colors?.secondaryButtonBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.4f);
                    _labelColor = colors?.textPrimary ?? Color.white;
                    break;
                case ButtonVariant.Outline:
                    _backgroundColor = Color.clear;
                    _addOutline = true;
                    _outlineColor = colors?.cardBorder ?? new Color(0.58f, 0.64f, 0.72f, 0.1f);
                    break;
                case ButtonVariant.Ghost:
                    _backgroundColor = Color.clear;
                    _labelColor = colors?.textSecondary ?? new Color(0.58f, 0.64f, 0.72f);
                    break;
                case ButtonVariant.Danger:
                    _backgroundColor = colors?.errorBubble ?? new Color(0.937f, 0.267f, 0.267f, 0.15f);
                    _labelColor = colors?.errorText ?? new Color(0.988f, 0.647f, 0.647f, 1f);
                    _addOutline = true;
                    _outlineColor = colors?.errorBorder ?? new Color(0.937f, 0.267f, 0.267f, 0.3f);
                    break;
                case ButtonVariant.IconButton:
                    _backgroundColor = colors?.iconButtonBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.6f);
                    _labelColor = colors?.textPrimary ?? Color.white;
                    break;
            }
            return this;
        }

        public ButtonBuilder Primary()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.primaryButtonGradientStart ?? new Color(0.388f, 0.4f, 0.945f, 1f);
            _labelColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        public ButtonBuilder Secondary()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.secondaryButtonBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.4f);
            _labelColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        public ButtonBuilder Ghost()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = Color.clear;
            _labelColor = colors?.textSecondary ?? new Color(0.58f, 0.64f, 0.72f);
            return this;
        }

        public ButtonBuilder Danger()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.errorBubble ?? new Color(0.937f, 0.267f, 0.267f, 0.15f);
            _labelColor = colors?.errorText ?? new Color(0.988f, 0.647f, 0.647f, 1f);
            _addOutline = true;
            _outlineColor = colors?.errorBorder ?? new Color(0.937f, 0.267f, 0.267f, 0.3f);
            return this;
        }

        public ButtonBuilder IconButton()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.iconButtonBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.6f);
            _labelColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        public ButtonBuilder Indigo()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.indigo ?? new Color(0.388f, 0.4f, 0.945f, 1f);
            _labelColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        public ButtonBuilder Pink()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.pink ?? new Color(0.925f, 0.282f, 0.6f, 1f);
            _labelColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.Button;

        protected override void ApplyCustomizations(GameObject instance, BaseButton button)
        {
            if (button == null)
            {
                var native = instance.GetComponent<Button>();
                if (native != null)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(native);
                    Object.DestroyImmediate(native);
                    button = instance.AddComponent<BaseButton>();
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(button);
                }
            }
            if (button == null) return;

            if (button.targetGraphic == null)
                button.targetGraphic = instance.GetComponent<Graphic>();

            var bg = instance.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = _backgroundColor;
                if (_backgroundSprite != null)
                {
                    bg.sprite = _backgroundSprite;
                    bg.type = Image.Type.Sliced;
                }
            }

            if (_colors.HasValue)
                button.colors = _colors.Value;

            if (_addOutline)
            {
                var outline = instance.GetComponent<Outline>() ?? instance.AddComponent<Outline>();
                outline.effectColor = _outlineColor;
                outline.effectDistance = _outlineDistance;
            }

            var label = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                if (!string.IsNullOrEmpty(_label)) label.text = _label;
                label.fontSize = _labelFontSize;
                label.color = _labelColor;
            }

            if (instance.GetComponent<CanvasGroup>() == null)
                instance.AddComponent<CanvasGroup>();
            var bcb = instance.GetComponent<BaseControlBehaviour>() ?? instance.AddComponent<BaseControlBehaviour>();
            ControlDefaults.ApplyTo(bcb);

            ApplyLayoutElement(instance);
        }

        public override BaseButton Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            var bg = go.AddComponent<Image>();
            bg.color = _backgroundColor;
            bg.sprite = _backgroundSprite;
            if (_backgroundSprite != null)
            {
                bg.type = Image.Type.Sliced;
            }

            var button = go.AddComponent<BaseButton>();
            button.targetGraphic = bg;

            if (_colors.HasValue)
            {
                button.colors = _colors.Value;
            }

            if (_addOutline)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = _outlineColor;
                outline.effectDistance = _outlineDistance;
            }

            if (_icon != null || (!_iconOnly && !string.IsNullOrEmpty(_label)))
            {
                BuildContent(go.transform);
            }

            go.AddComponent<CanvasGroup>();
            ControlDefaults.ApplyTo(go.AddComponent<BaseControlBehaviour>());

            ApplyLayoutElement(go);

            return button;
        }

        private void BuildContent(Transform parent)
        {
            if (_icon != null && !string.IsNullOrEmpty(_label) && !_iconOnly)
            {
                var hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.spacing = 8f;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
            }

            if (_icon != null)
            {
                new ImageBuilder("Icon")
                    .WithSprite(_icon)
                    .WithColor(Color.white)
                    .WithPreferredSize(_iconSize, _iconSize)
                    .Build(parent);
            }

            if (!_iconOnly && !string.IsNullOrEmpty(_label))
            {
                new TextBuilder("Label")
                    .WithText(_label)
                    .WithFontSize(_labelFontSize)
                    .WithColor(_labelColor)
                    .Centered()
                    .Bold()
                    .WithStretch()
                    .Build(parent);
            }
        }
    }
}
#endif
