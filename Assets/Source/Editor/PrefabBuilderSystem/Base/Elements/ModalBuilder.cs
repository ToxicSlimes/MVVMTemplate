#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class ModalBuilder : BaseElementBuilder<ModalBuilder, Image>
    {
        protected override string NamingPrefix => "Modal";

        private string _title;
        private float _width = 500f;
        private float _height = 400f;
        private float _headerHeight = 50f;
        private float _footerHeight = 60f;
        private Color _dimColor = new(0f, 0f, 0f, 0.7f);
        private Color _backgroundColor = new(0.12f, 0.14f, 0.18f, 1f);
        private Color _headerColor = new(0.15f, 0.17f, 0.22f, 1f);
        private Color _footerColor = new(0.1f, 0.12f, 0.15f, 1f);
        private Color _titleColor = Color.white;
        private bool _showCloseButton = true;
        private bool _closeOnDimClick = true;
        private RectOffset _contentPadding = new(20, 20, 20, 20);

        public ModalBuilder(string name) : base(name) { }

        public ModalBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public new ModalBuilder WithSize(float width, float height)
        {
            _width = width;
            _height = height;
            return this;
        }

        public ModalBuilder WithDimColor(Color color)
        {
            _dimColor = color;
            return this;
        }

        public ModalBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public ModalBuilder WithHeaderColor(Color color)
        {
            _headerColor = color;
            return this;
        }

        public ModalBuilder WithFooterColor(Color color)
        {
            _footerColor = color;
            return this;
        }

        public ModalBuilder WithContentPadding(int all)
        {
            _contentPadding = new RectOffset(all, all, all, all);
            return this;
        }

        public ModalBuilder NoCloseButton()
        {
            _showCloseButton = false;
            return this;
        }

        public ModalBuilder NoDimClick()
        {
            _closeOnDimClick = false;
            return this;
        }

        public ModalBuilder NoFooter()
        {
            _footerHeight = 0;
            return this;
        }

        public ModalBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.cardBackground ?? new Color(0.12f, 0.14f, 0.18f, 1f);
            _headerColor = colors?.elevatedSurface ?? new Color(0.15f, 0.17f, 0.22f, 1f);
            _footerColor = colors?.darkSurface ?? new Color(0.1f, 0.12f, 0.15f, 1f);
            _dimColor = new Color(0f, 0f, 0f, 0.7f);
            _titleColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.Modal;

        public override Image Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);

            var rt = CreateRectTransform(parent);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var dimBg = rt.gameObject.AddComponent<Image>();
            dimBg.color = _dimColor;

            if (_closeOnDimClick)
            {
                var dimBtn = rt.gameObject.AddComponent<Button>();
                dimBtn.transition = Selectable.Transition.None;
            }

            BuildWindow(rt.transform);

            return dimBg;
        }

        private void BuildWindow(Transform parent)
        {
            var windowGo = new GameObject("Window");
            windowGo.transform.SetParent(parent, false);
            var windowRt = windowGo.AddComponent<RectTransform>();
            windowRt.anchorMin = new Vector2(0.5f, 0.5f);
            windowRt.anchorMax = new Vector2(0.5f, 0.5f);
            windowRt.pivot = new Vector2(0.5f, 0.5f);
            windowRt.sizeDelta = new Vector2(_width, _height);

            var windowBg = windowGo.AddComponent<Image>();
            windowBg.color = _backgroundColor;

            var outline = windowGo.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.1f);
            outline.effectDistance = new Vector2(1, -1);

            var vlg = windowGo.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0;

            BuildHeader(windowGo.transform);
            BuildContent(windowGo.transform);

            if (_footerHeight > 0)
            {
                BuildFooter(windowGo.transform);
            }
        }

        private void BuildHeader(Transform parent)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            headerGo.AddComponent<RectTransform>();

            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = _headerHeight;

            var headerBg = headerGo.AddComponent<Image>();
            headerBg.color = _headerColor;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = new Vector2(_showCloseButton ? -50 : -20, 0);

            var tmp = titleGo.AddComponent<TextMeshProUGUI>();
            tmp.text = _title ?? "Modal";
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = _titleColor;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            if (_showCloseButton)
            {
                var closeGo = new GameObject("CloseButton");
                closeGo.transform.SetParent(headerGo.transform, false);
                var closeRt = closeGo.AddComponent<RectTransform>();
                closeRt.anchorMin = new Vector2(1, 0.5f);
                closeRt.anchorMax = new Vector2(1, 0.5f);
                closeRt.pivot = new Vector2(1, 0.5f);
                closeRt.sizeDelta = new Vector2(36, 36);
                closeRt.anchoredPosition = new Vector2(-10, 0);

                var closeBg = closeGo.AddComponent<Image>();
                closeBg.color = new Color(1, 1, 1, 0.1f);

                closeGo.AddComponent<Button>().targetGraphic = closeBg;

                var closeText = new GameObject("X");
                closeText.transform.SetParent(closeGo.transform, false);
                var closeTextRt = closeText.AddComponent<RectTransform>();
                closeTextRt.anchorMin = Vector2.zero;
                closeTextRt.anchorMax = Vector2.one;
                closeTextRt.offsetMin = Vector2.zero;
                closeTextRt.offsetMax = Vector2.zero;

                var closeTmp = closeText.AddComponent<TextMeshProUGUI>();
                closeTmp.text = "×";
                closeTmp.fontSize = 24f;
                closeTmp.fontStyle = FontStyles.Bold;
                closeTmp.color = _titleColor;
                closeTmp.alignment = TextAlignmentOptions.Center;
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
            vlg.spacing = 10f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        private void BuildFooter(Transform parent)
        {
            var footerGo = new GameObject("Footer");
            footerGo.transform.SetParent(parent, false);
            footerGo.AddComponent<RectTransform>();

            var footerLe = footerGo.AddComponent<LayoutElement>();
            footerLe.preferredHeight = _footerHeight;

            var footerBg = footerGo.AddComponent<Image>();
            footerBg.color = _footerColor;

            var hlg = footerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
        }

        protected override void ApplyCustomizations(GameObject instance, Image component)
        {
            component.color = _dimColor;

            var title = instance.transform.Find("Window/Header/Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null && !string.IsNullOrEmpty(_title))
            {
                title.text = _title;
            }
        }
    }
}
#endif
