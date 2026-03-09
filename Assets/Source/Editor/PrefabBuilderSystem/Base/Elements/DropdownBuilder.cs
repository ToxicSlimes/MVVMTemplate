#if UNITY_EDITOR
using System.Collections.Generic;
using Core.Editor.PrefabBuilderSystem.Conventions;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Infrastructure.UI;
using Core.UI.Controls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public class DropdownBuilder : BaseElementBuilder<DropdownBuilder, BaseDropdown>
    {
        protected override string NamingPrefix => NamingConvention.Dropdown;
        
        private readonly List<string> _options = new();
        private int _value;
        private float _fontSize = 14f;
        private Color _textColor = Color.white;
        private Color _backgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
        private Color _dropdownColor = new(0.15f, 0.15f, 0.15f, 1f);
        private Color _highlightColor = new(0.4f, 0.6f, 1f, 0.5f);
        private float _itemHeight = 30f;
        private Sprite _backgroundSprite;
        private Sprite _arrowSprite;
        private Sprite _checkmarkSprite;

        public DropdownBuilder(string name) : base(name) { }

        public DropdownBuilder WithOptions(params string[] options)
        {
            _options.AddRange(options);
            return this;
        }

        public DropdownBuilder WithOptions(IEnumerable<string> options)
        {
            _options.AddRange(options);
            return this;
        }

        public DropdownBuilder WithValue(int value)
        {
            _value = value;
            return this;
        }

        public DropdownBuilder WithFontSize(float size)
        {
            _fontSize = size;
            return this;
        }

        public DropdownBuilder WithTextColor(Color color)
        {
            _textColor = color;
            return this;
        }

        public DropdownBuilder WithBackgroundColor(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public DropdownBuilder WithDropdownColor(Color color)
        {
            _dropdownColor = color;
            return this;
        }

        public DropdownBuilder WithHighlightColor(Color color)
        {
            _highlightColor = color;
            return this;
        }

        public DropdownBuilder WithItemHeight(float height)
        {
            _itemHeight = height;
            return this;
        }

        public DropdownBuilder WithBackgroundSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        public DropdownBuilder WithArrowSprite(Sprite sprite)
        {
            _arrowSprite = sprite;
            return this;
        }

        public DropdownBuilder WithCheckmarkSprite(Sprite sprite)
        {
            _checkmarkSprite = sprite;
            return this;
        }

        public DropdownBuilder Themed()
        {
            var colors = DesignSystemProvider.Colors;
            _backgroundColor = colors?.inputBackground ?? new Color(0.2f, 0.2f, 0.2f, 1f);
            _dropdownColor = colors?.darkSurface ?? new Color(0.15f, 0.15f, 0.15f, 1f);
            _highlightColor = colors?.cardHoverBackground ?? new Color(0.4f, 0.6f, 1f, 0.5f);
            _textColor = colors?.textPrimary ?? Color.white;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Interactive.Dropdown;

        protected override void ApplyCustomizations(GameObject instance, BaseDropdown dropdown)
        {
            if (dropdown == null)
            {
                var native = instance.GetComponent<TMP_Dropdown>();
                if (native != null)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(native);
                    Object.DestroyImmediate(native);
                    dropdown = instance.AddComponent<BaseDropdown>();
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(dropdown);
                }
            }
            if (dropdown == null) return;

            if (dropdown.template == null)
            {
                var tmpl = instance.transform.Find("Template");
                if (tmpl != null) dropdown.template = tmpl as RectTransform;
            }
            if (dropdown.captionText == null)
            {
                var label = instance.transform.Find("Label");
                if (label != null) dropdown.captionText = label.GetComponent<TMPro.TMP_Text>();
            }
            if (dropdown.captionImage == null)
            {
                var arrow = instance.transform.Find("Arrow");
                if (arrow != null) dropdown.captionImage = arrow.GetComponent<Image>();
            }
            if (dropdown.itemText == null && dropdown.template != null)
            {
                var itemLabel = dropdown.template.Find("Viewport/Content/Item/Item Label");
                if (itemLabel != null) dropdown.itemText = itemLabel.GetComponent<TMPro.TMP_Text>();
            }
            if (dropdown.itemImage == null && dropdown.template != null)
            {
                var itemBg = dropdown.template.Find("Viewport/Content/Item/Item Background");
                if (itemBg != null) dropdown.itemImage = itemBg.GetComponent<Image>();
            }
            if (dropdown.targetGraphic == null)
            {
                dropdown.targetGraphic = instance.GetComponent<Graphic>();
            }

            dropdown.ClearOptions();
            if (_options.Count > 0) dropdown.AddOptions(_options);
            dropdown.value = _value;

            var bg = instance.GetComponent<Image>();
            if (bg != null) bg.color = _backgroundColor;

            if (dropdown.captionText != null)
            {
                dropdown.captionText.fontSize = _fontSize;
                dropdown.captionText.color = _textColor;
            }

            if (instance.GetComponent<CanvasGroup>() == null)
                instance.AddComponent<CanvasGroup>();
            var bcb = instance.GetComponent<BaseControlBehaviour>() ?? instance.AddComponent<BaseControlBehaviour>();
            ControlDefaults.ApplyTo(bcb);

            ApplyLayoutElement(instance);
        }

        public override BaseDropdown Build(Transform parent)
        {
            if (ShouldUsePrefab())
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            if (Size == Vector2.zero)
            {
                rt.sizeDelta = new Vector2(160f, 30f);
            }

            var bgImg = go.AddComponent<Image>();
            bgImg.color = _backgroundColor;
            bgImg.sprite = _backgroundSprite;
            if (_backgroundSprite != null)
            {
                bgImg.type = Image.Type.Sliced;
            }

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(10, 0);
            labelRt.offsetMax = new Vector2(-30, 0);

            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.fontSize = _fontSize;
            labelTmp.color = _textColor;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var arrowGo = new GameObject("Arrow");
            arrowGo.transform.SetParent(go.transform, false);
            var arrowRt = arrowGo.AddComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(1, 0.5f);
            arrowRt.anchorMax = new Vector2(1, 0.5f);
            arrowRt.pivot = new Vector2(1, 0.5f);
            arrowRt.anchoredPosition = new Vector2(-10, 0);
            arrowRt.sizeDelta = new Vector2(20, 20);

            var arrowImg = arrowGo.AddComponent<Image>();
            arrowImg.color = _textColor;
            arrowImg.sprite = _arrowSprite;

            var templateGo = new GameObject("Template");
            templateGo.transform.SetParent(go.transform, false);
            var templateRt = templateGo.AddComponent<RectTransform>();
            templateRt.anchorMin = new Vector2(0, 0);
            templateRt.anchorMax = new Vector2(1, 0);
            templateRt.pivot = new Vector2(0.5f, 1);
            templateRt.anchoredPosition = Vector2.zero;
            templateRt.sizeDelta = new Vector2(0, 150);

            var templateImg = templateGo.AddComponent<Image>();
            templateImg.color = _dropdownColor;
            templateImg.sprite = _backgroundSprite;
            if (_backgroundSprite != null)
            {
                templateImg.type = Image.Type.Sliced;
            }

            var templateCanvas = templateGo.AddComponent<Canvas>();
            templateCanvas.overrideSorting = true;
            templateCanvas.sortingOrder = 30001;
            templateGo.AddComponent<GraphicRaycaster>();

            var templateSr = templateGo.AddComponent<ScrollRect>();

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(templateGo.transform, false);
            var viewportRt = viewportGo.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewportRt.pivot = new Vector2(0, 1);

            viewportGo.AddComponent<Mask>().showMaskGraphic = false;
            viewportGo.AddComponent<Image>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, _itemHeight);

            templateSr.viewport = viewportRt;
            templateSr.content = contentRt;
            templateSr.horizontal = false;
            templateSr.vertical = true;
            templateSr.movementType = ScrollRect.MovementType.Clamped;

            var itemGo = new GameObject("Item");
            itemGo.transform.SetParent(contentGo.transform, false);
            var itemRt = itemGo.AddComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0, 0.5f);
            itemRt.anchorMax = new Vector2(1, 0.5f);
            itemRt.sizeDelta = new Vector2(0, _itemHeight);

            var itemToggle = itemGo.AddComponent<BaseToggle>();

            var itemBgGo = new GameObject("Item Background");
            itemBgGo.transform.SetParent(itemGo.transform, false);
            var itemBgRt = itemBgGo.AddComponent<RectTransform>();
            itemBgRt.anchorMin = Vector2.zero;
            itemBgRt.anchorMax = Vector2.one;
            itemBgRt.offsetMin = Vector2.zero;
            itemBgRt.offsetMax = Vector2.zero;

            var itemBgImg = itemBgGo.AddComponent<Image>();
            itemBgImg.color = _highlightColor;

            var itemCheckmarkGo = new GameObject("Item Checkmark");
            itemCheckmarkGo.transform.SetParent(itemGo.transform, false);
            var itemCheckmarkRt = itemCheckmarkGo.AddComponent<RectTransform>();
            itemCheckmarkRt.anchorMin = new Vector2(0, 0.5f);
            itemCheckmarkRt.anchorMax = new Vector2(0, 0.5f);
            itemCheckmarkRt.anchoredPosition = new Vector2(15, 0);
            itemCheckmarkRt.sizeDelta = new Vector2(20, 20);

            var itemCheckmarkImg = itemCheckmarkGo.AddComponent<Image>();
            itemCheckmarkImg.color = _textColor;
            itemCheckmarkImg.sprite = _checkmarkSprite;

            var itemLabelGo = new GameObject("Item Label");
            itemLabelGo.transform.SetParent(itemGo.transform, false);
            var itemLabelRt = itemLabelGo.AddComponent<RectTransform>();
            itemLabelRt.anchorMin = Vector2.zero;
            itemLabelRt.anchorMax = Vector2.one;
            itemLabelRt.offsetMin = new Vector2(30, 0);
            itemLabelRt.offsetMax = new Vector2(-10, 0);

            var itemLabelTmp = itemLabelGo.AddComponent<TextMeshProUGUI>();
            itemLabelTmp.fontSize = _fontSize;
            itemLabelTmp.color = _textColor;
            itemLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            itemToggle.targetGraphic = itemBgImg;
            itemToggle.graphic = itemCheckmarkImg;
            itemToggle.isOn = true;

            templateGo.SetActive(false);

            var dropdown = go.AddComponent<BaseDropdown>();
            dropdown.template = templateRt;
            dropdown.captionText = labelTmp;
            dropdown.itemText = itemLabelTmp;
            dropdown.targetGraphic = bgImg;

            dropdown.ClearOptions();
            if (_options.Count > 0)
            {
                dropdown.AddOptions(_options);
            }
            dropdown.value = _value;

            go.AddComponent<CanvasGroup>();
            ControlDefaults.ApplyTo(go.AddComponent<BaseControlBehaviour>());

            ApplyLayoutElement(go);

            return dropdown;
        }
    }
}
#endif
