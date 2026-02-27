#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class AccordionBuilder : BaseElementBuilder<AccordionBuilder, Toggle>
    {
        protected override string NamingPrefix => "Accordion";

        private string _title = "Section";
        private float _headerHeight = 44f;
        private Color _headerColor = new(0.15f, 0.17f, 0.22f, 1f);
        private Color _headerHoverColor = new(0.2f, 0.22f, 0.28f, 1f);
        private Color _contentColor = new(0.12f, 0.14f, 0.18f, 1f);
        private Color _titleColor = Color.white;
        private Color _borderColor = new(0.58f, 0.64f, 0.72f, 0.1f);
        private RectOffset _contentPadding = new(16, 16, 16, 16);
        private float _contentSpacing = 8f;
        private bool _startExpanded = true;

        public AccordionBuilder(string name) : base(name) { }

        public AccordionBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public AccordionBuilder WithHeaderHeight(float height)
        {
            _headerHeight = height;
            return this;
        }

        public AccordionBuilder WithHeaderColor(Color color)
        {
            _headerColor = color;
            return this;
        }

        public AccordionBuilder WithContentColor(Color color)
        {
            _contentColor = color;
            return this;
        }

        public AccordionBuilder WithContentPadding(int all)
        {
            _contentPadding = new RectOffset(all, all, all, all);
            return this;
        }

        public AccordionBuilder WithContentSpacing(float spacing)
        {
            _contentSpacing = spacing;
            return this;
        }

        public AccordionBuilder Collapsed()
        {
            _startExpanded = false;
            return this;
        }

        public AccordionBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _headerColor = colors?.elevatedSurface ?? new Color(0.15f, 0.17f, 0.22f, 1f);
            _contentColor = colors?.cardBackground ?? new Color(0.12f, 0.14f, 0.18f, 1f);
            _titleColor = colors?.textPrimary ?? Color.white;
            _borderColor = colors?.cardBorder ?? new Color(0.58f, 0.64f, 0.72f, 0.1f);
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.Accordion;

        public override Toggle Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);

            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0;

            var csf = rt.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var headerGo = BuildHeader(rt.transform);
            var contentGo = BuildContent(rt.transform);

            var toggle = rt.gameObject.AddComponent<Toggle>();
            toggle.isOn = _startExpanded;
            toggle.targetGraphic = headerGo.GetComponent<Image>();
            toggle.graphic = contentGo.GetComponent<Image>();

            contentGo.SetActive(_startExpanded);

            return toggle;
        }

        private GameObject BuildHeader(Transform parent)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            headerGo.AddComponent<RectTransform>();

            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = _headerHeight;

            var headerBg = headerGo.AddComponent<Image>();
            headerBg.color = _headerColor;

            var outline = headerGo.AddComponent<Outline>();
            outline.effectColor = _borderColor;
            outline.effectDistance = new Vector2(1, -1);

            var arrowGo = new GameObject("Arrow");
            arrowGo.transform.SetParent(headerGo.transform, false);
            var arrowRt = arrowGo.AddComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(0, 0.5f);
            arrowRt.anchorMax = new Vector2(0, 0.5f);
            arrowRt.pivot = new Vector2(0, 0.5f);
            arrowRt.sizeDelta = new Vector2(24, 24);
            arrowRt.anchoredPosition = new Vector2(12, 0);

            var arrowTmp = arrowGo.AddComponent<TextMeshProUGUI>();
            arrowTmp.text = _startExpanded ? "▼" : "▶";
            arrowTmp.fontSize = 12f;
            arrowTmp.color = _titleColor;
            arrowTmp.alignment = TextAlignmentOptions.Center;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(40, 0);
            titleRt.offsetMax = new Vector2(-16, 0);

            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = _title;
            titleTmp.fontSize = 16f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = _titleColor;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;

            return headerGo;
        }

        private GameObject BuildContent(Transform parent)
        {
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(parent, false);
            contentGo.AddComponent<RectTransform>();

            var contentBg = contentGo.AddComponent<Image>();
            contentBg.color = _contentColor;

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = _contentPadding;
            vlg.spacing = _contentSpacing;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return contentGo;
        }

        protected override void ApplyCustomizations(GameObject instance, Toggle component)
        {
            var title = instance.transform.Find("Header/Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.text = _title;
            }

            component.isOn = _startExpanded;
            var content = instance.transform.Find("Content");
            if (content != null)
            {
                content.gameObject.SetActive(_startExpanded);
            }
        }
    }
}
#endif
