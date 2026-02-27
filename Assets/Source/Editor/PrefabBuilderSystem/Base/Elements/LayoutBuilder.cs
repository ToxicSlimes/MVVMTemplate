#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class HorizontalLayoutBuilder : BaseElementBuilder<HorizontalLayoutBuilder, HorizontalLayoutGroup>
    {
        protected override string NamingPrefix => NamingConvention.Layout;
        
        private float _spacing;
        private TextAnchor _alignment = TextAnchor.MiddleCenter;
        private RectOffset _padding = new(0, 0, 0, 0);
        private bool _controlWidth = true;
        private bool _controlHeight = true;
        private bool _forceExpandWidth;
        private bool _forceExpandHeight;
        private bool _addContentSizeFitter;
        private ContentSizeFitter.FitMode _horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        private ContentSizeFitter.FitMode _verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        private Color? _backgroundColor;

        public HorizontalLayoutBuilder(string name) : base(name) { }

        public HorizontalLayoutBuilder WithSpacing(float spacing)
        {
            _spacing = spacing;
            return this;
        }

        public HorizontalLayoutBuilder WithAlignment(TextAnchor alignment)
        {
            _alignment = alignment;
            return this;
        }

        public HorizontalLayoutBuilder WithPadding(int left, int right, int top, int bottom)
        {
            _padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public HorizontalLayoutBuilder WithPadding(int all)
        {
            _padding = new RectOffset(all, all, all, all);
            return this;
        }

        public HorizontalLayoutBuilder ControlChildren(bool width = true, bool height = true)
        {
            _controlWidth = width;
            _controlHeight = height;
            return this;
        }

        public HorizontalLayoutBuilder ForceExpand(bool width = false, bool height = false)
        {
            _forceExpandWidth = width;
            _forceExpandHeight = height;
            return this;
        }

        public HorizontalLayoutBuilder WithContentSizeFitter(
            ContentSizeFitter.FitMode horizontal = ContentSizeFitter.FitMode.PreferredSize,
            ContentSizeFitter.FitMode vertical = ContentSizeFitter.FitMode.Unconstrained)
        {
            _addContentSizeFitter = true;
            _horizontalFit = horizontal;
            _verticalFit = vertical;
            return this;
        }

        public HorizontalLayoutBuilder FitContent()
        {
            return WithContentSizeFitter(ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.PreferredSize);
        }

        public HorizontalLayoutBuilder WithBackground(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public HorizontalLayoutBuilder WithSpacing(SpacingSize size)
        {
            _spacing = DesignSystemProvider.GetSpacing(size);
            return this;
        }

        public HorizontalLayoutBuilder WithPadding(SpacingSize size)
        {
            var p = (int)DesignSystemProvider.GetSpacing(size);
            _padding = new RectOffset(p, p, p, p);
            return this;
        }

        public HorizontalLayoutBuilder WithCardPadding(bool mobile = false)
        {
            var spacing = DesignSystemProvider.Spacing;
            var p = (int)(mobile 
                ? (spacing?.cardPaddingMobile ?? 20f) 
                : (spacing?.cardPaddingDesktop ?? 32f));
            _padding = new RectOffset(p, p, p, p);
            return this;
        }

        public HorizontalLayoutBuilder WithMessageSpacing()
        {
            _spacing = DesignSystemProvider.Spacing?.messageSpacing ?? 12f;
            return this;
        }

        public HorizontalLayoutBuilder DeepSpace()
        {
            _backgroundColor = DesignSystemProvider.Colors?.deepSpace ?? new Color(0.039f, 0.059f, 0.118f);
            return this;
        }

        public HorizontalLayoutBuilder DarkSurface()
        {
            _backgroundColor = DesignSystemProvider.Colors?.darkSurface ?? new Color(0.082f, 0.106f, 0.176f);
            return this;
        }

        public HorizontalLayoutBuilder CardBackground()
        {
            _backgroundColor = DesignSystemProvider.Colors?.cardBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.4f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.HorizontalLayout;

        protected override void ApplyCustomizations(GameObject instance, HorizontalLayoutGroup hlg)
        {
            if (hlg == null) return;

            if (_backgroundColor.HasValue)
            {
                var img = instance.GetComponent<Image>() ?? instance.AddComponent<Image>();
                img.color = _backgroundColor.Value;
            }

            hlg.spacing = _spacing;
            hlg.childAlignment = _alignment;
            hlg.padding = _padding;
            hlg.childControlWidth = _controlWidth;
            hlg.childControlHeight = _controlHeight;
            hlg.childForceExpandWidth = _forceExpandWidth;
            hlg.childForceExpandHeight = _forceExpandHeight;

            if (_addContentSizeFitter)
            {
                var csf = instance.GetComponent<ContentSizeFitter>() ?? instance.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = _horizontalFit;
                csf.verticalFit = _verticalFit;
            }

            ApplyLayoutElement(instance);
        }

        public override HorizontalLayoutGroup Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            if (_backgroundColor.HasValue)
            {
                var img = go.AddComponent<Image>();
                img.color = _backgroundColor.Value;
            }

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = _spacing;
            hlg.childAlignment = _alignment;
            hlg.padding = _padding;
            hlg.childControlWidth = _controlWidth;
            hlg.childControlHeight = _controlHeight;
            hlg.childForceExpandWidth = _forceExpandWidth;
            hlg.childForceExpandHeight = _forceExpandHeight;

            if (_addContentSizeFitter)
            {
                var csf = go.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = _horizontalFit;
                csf.verticalFit = _verticalFit;
            }

            ApplyLayoutElement(go);

            return hlg;
        }
    }

    public class VerticalLayoutBuilder : BaseElementBuilder<VerticalLayoutBuilder, VerticalLayoutGroup>
    {
        protected override string NamingPrefix => NamingConvention.Layout;
        
        private float _spacing;
        private TextAnchor _alignment = TextAnchor.UpperCenter;
        private RectOffset _padding = new(0, 0, 0, 0);
        private bool _controlWidth = true;
        private bool _controlHeight = true;
        private bool _forceExpandWidth = true;
        private bool _forceExpandHeight;
        private bool _addContentSizeFitter;
        private ContentSizeFitter.FitMode _horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        private ContentSizeFitter.FitMode _verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        private Color? _backgroundColor;

        public VerticalLayoutBuilder(string name) : base(name) { }

        public VerticalLayoutBuilder WithSpacing(float spacing)
        {
            _spacing = spacing;
            return this;
        }

        public VerticalLayoutBuilder WithAlignment(TextAnchor alignment)
        {
            _alignment = alignment;
            return this;
        }

        public VerticalLayoutBuilder WithPadding(int left, int right, int top, int bottom)
        {
            _padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public VerticalLayoutBuilder WithPadding(int all)
        {
            _padding = new RectOffset(all, all, all, all);
            return this;
        }

        public VerticalLayoutBuilder ControlChildren(bool width = true, bool height = true)
        {
            _controlWidth = width;
            _controlHeight = height;
            return this;
        }

        public VerticalLayoutBuilder ForceExpand(bool width = true, bool height = false)
        {
            _forceExpandWidth = width;
            _forceExpandHeight = height;
            return this;
        }

        public VerticalLayoutBuilder WithContentSizeFitter(
            ContentSizeFitter.FitMode horizontal = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode vertical = ContentSizeFitter.FitMode.PreferredSize)
        {
            _addContentSizeFitter = true;
            _horizontalFit = horizontal;
            _verticalFit = vertical;
            return this;
        }

        public VerticalLayoutBuilder FitContent()
        {
            return WithContentSizeFitter(ContentSizeFitter.FitMode.PreferredSize, ContentSizeFitter.FitMode.PreferredSize);
        }

        public VerticalLayoutBuilder WithBackground(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public VerticalLayoutBuilder WithSpacing(SpacingSize size)
        {
            _spacing = DesignSystemProvider.GetSpacing(size);
            return this;
        }

        public VerticalLayoutBuilder WithPadding(SpacingSize size)
        {
            var p = (int)DesignSystemProvider.GetSpacing(size);
            _padding = new RectOffset(p, p, p, p);
            return this;
        }

        public VerticalLayoutBuilder WithCardPadding(bool mobile = false)
        {
            var spacing = DesignSystemProvider.Spacing;
            var p = (int)(mobile 
                ? (spacing?.cardPaddingMobile ?? 20f) 
                : (spacing?.cardPaddingDesktop ?? 32f));
            _padding = new RectOffset(p, p, p, p);
            return this;
        }

        public VerticalLayoutBuilder WithMessageSpacing()
        {
            _spacing = DesignSystemProvider.Spacing?.messageSpacing ?? 12f;
            return this;
        }

        public VerticalLayoutBuilder DeepSpace()
        {
            _backgroundColor = DesignSystemProvider.Colors?.deepSpace ?? new Color(0.039f, 0.059f, 0.118f);
            return this;
        }

        public VerticalLayoutBuilder DarkSurface()
        {
            _backgroundColor = DesignSystemProvider.Colors?.darkSurface ?? new Color(0.082f, 0.106f, 0.176f);
            return this;
        }

        public VerticalLayoutBuilder CardBackground()
        {
            _backgroundColor = DesignSystemProvider.Colors?.cardBackground ?? new Color(0.118f, 0.161f, 0.365f, 0.4f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.VerticalLayout;

        protected override void ApplyCustomizations(GameObject instance, VerticalLayoutGroup vlg)
        {
            if (vlg == null) return;

            if (_backgroundColor.HasValue)
            {
                var img = instance.GetComponent<Image>() ?? instance.AddComponent<Image>();
                img.color = _backgroundColor.Value;
            }

            vlg.spacing = _spacing;
            vlg.childAlignment = _alignment;
            vlg.padding = _padding;
            vlg.childControlWidth = _controlWidth;
            vlg.childControlHeight = _controlHeight;
            vlg.childForceExpandWidth = _forceExpandWidth;
            vlg.childForceExpandHeight = _forceExpandHeight;

            if (_addContentSizeFitter)
            {
                var csf = instance.GetComponent<ContentSizeFitter>() ?? instance.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = _horizontalFit;
                csf.verticalFit = _verticalFit;
            }

            ApplyLayoutElement(instance);
        }

        public override VerticalLayoutGroup Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            if (_backgroundColor.HasValue)
            {
                var img = go.AddComponent<Image>();
                img.color = _backgroundColor.Value;
            }

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = _spacing;
            vlg.childAlignment = _alignment;
            vlg.padding = _padding;
            vlg.childControlWidth = _controlWidth;
            vlg.childControlHeight = _controlHeight;
            vlg.childForceExpandWidth = _forceExpandWidth;
            vlg.childForceExpandHeight = _forceExpandHeight;

            if (_addContentSizeFitter)
            {
                var csf = go.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = _horizontalFit;
                csf.verticalFit = _verticalFit;
            }

            ApplyLayoutElement(go);

            return vlg;
        }
    }
}
#endif
