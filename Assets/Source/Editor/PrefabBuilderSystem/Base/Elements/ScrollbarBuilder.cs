#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using EndlessAdventure.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements
{
    public class ScrollbarBuilder : BaseElementBuilder<ScrollbarBuilder, Scrollbar>
    {
        protected override string NamingPrefix => "Scrollbar";

        private bool _horizontal;
        private float _size = 12f;
        private Color _backgroundColor = new(0.1f, 0.1f, 0.1f, 0.5f);
        private Color _handleColor = new(0.5f, 0.5f, 0.5f, 1f);
        private float _handleMinSize = 0.1f;

        public ScrollbarBuilder(string name) : base(name) { }

        public ScrollbarBuilder Horizontal()
        {
            _horizontal = true;
            return this;
        }

        public ScrollbarBuilder Vertical()
        {
            _horizontal = false;
            return this;
        }

        public ScrollbarBuilder WithSize(float size)
        {
            _size = size;
            return this;
        }

        public ScrollbarBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public ScrollbarBuilder WithHandleColor(Color color)
        {
            _handleColor = color;
            return this;
        }

        public ScrollbarBuilder Thin() => WithSize(8f);
        public ScrollbarBuilder Normal() => WithSize(12f);
        public ScrollbarBuilder Thick() => WithSize(16f);

        public ScrollbarBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.darkSurface ?? new Color(0.1f, 0.1f, 0.1f, 0.5f);
            _handleColor = colors?.textSecondary ?? new Color(0.5f, 0.5f, 0.5f, 1f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.Scrollbar;

        public override Scrollbar Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);

            if (_horizontal)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.sizeDelta = new Vector2(0, _size);
            }
            else
            {
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 0.5f);
                rt.sizeDelta = new Vector2(_size, 0);
            }

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = _backgroundColor;

            var slidingArea = new GameObject("Sliding Area");
            slidingArea.transform.SetParent(rt, false);
            var slidingRt = slidingArea.AddComponent<RectTransform>();
            slidingRt.anchorMin = Vector2.zero;
            slidingRt.anchorMax = Vector2.one;
            slidingRt.offsetMin = new Vector2(2, 2);
            slidingRt.offsetMax = new Vector2(-2, -2);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform, false);
            var handleRt = handle.AddComponent<RectTransform>();
            handleRt.anchorMin = Vector2.zero;
            handleRt.anchorMax = Vector2.one;
            handleRt.offsetMin = Vector2.zero;
            handleRt.offsetMax = Vector2.zero;

            var handleImg = handle.AddComponent<Image>();
            handleImg.color = _handleColor;

            var scrollbar = rt.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRt;
            scrollbar.targetGraphic = handleImg;
            scrollbar.direction = _horizontal ? Scrollbar.Direction.LeftToRight : Scrollbar.Direction.BottomToTop;
            scrollbar.size = _handleMinSize;

            return scrollbar;
        }

        protected override void ApplyCustomizations(GameObject instance, Scrollbar component)
        {
            var bg = instance.GetComponent<Image>();
            if (bg != null) bg.color = _backgroundColor;

            var handle = instance.transform.Find("Sliding Area/Handle")?.GetComponent<Image>();
            if (handle != null) handle.color = _handleColor;
        }
    }
}
#endif
