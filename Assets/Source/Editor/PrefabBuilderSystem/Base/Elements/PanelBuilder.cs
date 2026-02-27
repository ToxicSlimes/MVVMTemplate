#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class PanelBuilder : BaseElementBuilder<PanelBuilder, Image>
    {
        protected override string NamingPrefix => "Panel";

        private string _title;
        private float _titleFontSize = 18f;
        private float _headerHeight = 40f;
        private Color _backgroundColor = new(0.12f, 0.14f, 0.18f, 1f);
        private Color _headerColor = new(0.15f, 0.17f, 0.22f, 1f);
        private Color _titleColor = Color.white;
        private Color _borderColor = new(0.58f, 0.64f, 0.72f, 0.1f);
        private RectOffset _contentPadding = new(16, 16, 16, 16);
        private float _contentSpacing = 8f;
        private bool _collapsible;

        public PanelBuilder(string name) : base(name) { }

        public PanelBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public PanelBuilder WithTitleFontSize(float size)
        {
            _titleFontSize = size;
            return this;
        }

        public PanelBuilder WithHeaderHeight(float height)
        {
            _headerHeight = height;
            return this;
        }

        public PanelBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public PanelBuilder WithHeaderColor(Color color)
        {
            _headerColor = color;
            return this;
        }

        public PanelBuilder WithContentPadding(int all)
        {
            _contentPadding = new RectOffset(all, all, all, all);
            return this;
        }

        public PanelBuilder WithContentSpacing(float spacing)
        {
            _contentSpacing = spacing;
            return this;
        }

        public PanelBuilder Collapsible()
        {
            _collapsible = true;
            return this;
        }

        public PanelBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.cardBackground ?? new Color(0.12f, 0.14f, 0.18f, 1f);
            _headerColor = colors?.elevatedSurface ?? new Color(0.15f, 0.17f, 0.22f, 1f);
            _titleColor = colors?.textPrimary ?? Color.white;
            _borderColor = colors?.cardBorder ?? new Color(0.58f, 0.64f, 0.72f, 0.1f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.Panel;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = _backgroundColor;

            var outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = _borderColor;
            outline.effectDistance = new Vector2(1, -1);

            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0;

            if (!string.IsNullOrEmpty(_title))
            {
                BuildHeader(rt.transform);
            }

            BuildContent(rt.transform);

            ApplyLayoutElement(rt.gameObject);

            return bg;
        }

        private void BuildHeader(Transform parent)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            var headerRt = headerGo.AddComponent<RectTransform>();

            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = _headerHeight;

            var headerBg = headerGo.AddComponent<Image>();
            headerBg.color = _headerColor;

            var hlg = headerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            titleGo.AddComponent<RectTransform>();

            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1;

            var tmp = titleGo.AddComponent<TextMeshProUGUI>();
            tmp.text = _title;
            tmp.fontSize = _titleFontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = _titleColor;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            if (_collapsible)
            {
                var arrowGo = new GameObject("Arrow");
                arrowGo.transform.SetParent(headerGo.transform, false);
                arrowGo.AddComponent<RectTransform>();

                var arrowLe = arrowGo.AddComponent<LayoutElement>();
                arrowLe.preferredWidth = 24;

                var arrowTmp = arrowGo.AddComponent<TextMeshProUGUI>();
                arrowTmp.text = "▼";
                arrowTmp.fontSize = 14f;
                arrowTmp.color = _titleColor;
                arrowTmp.alignment = TextAlignmentOptions.Center;

                headerGo.AddComponent<Button>().targetGraphic = headerBg;
            }
        }

        private void BuildContent(Transform parent)
        {
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(parent, false);
            contentGo.AddComponent<RectTransform>();

            var contentLe = contentGo.AddComponent<LayoutElement>();
            contentLe.flexibleHeight = 1;

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = _contentPadding;
            vlg.spacing = _contentSpacing;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            component.color = _backgroundColor;

            var title = instance.transform.Find("Header/Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null && !string.IsNullOrEmpty(_title))
            {
                title.text = _title;
            }
        }
    }
}
#endif
