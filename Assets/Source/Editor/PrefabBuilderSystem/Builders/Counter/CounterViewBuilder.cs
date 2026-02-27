#if UNITY_EDITOR
using Core.Abstractions.Popup;
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.MVVM.Views.Counter;
using Core.Presentation.Elements.Samples;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Builders.Counter
{
    public class CounterViewBuilder : ViewBuilder<CounterView>
    {
        private const string TooltipPrefabPath = "Assets/Prefabs/Popups/TooltipPopup.prefab";
        private const string ConfirmPrefabPath = "Assets/Prefabs/Popups/ConfirmPopup.prefab";

        public CounterViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/CounterView.prefab";

        // Counter panel refs
        private GameObject _counterPanel;
        private TextMeshProUGUI _counterText;
        private TextMeshProUGUI _incrementsText;
        private Button _incrementButton;
        private Button _decrementButton;
        private Button _resetButton;
        private PopupTrigger _incrementTooltip;
        private PopupTrigger _decrementTooltip;
        private PopupTrigger _resetConfirm;

        // Toggle button
        private Button _toggleButton;
        private TextMeshProUGUI _toggleLabel;

        // Gallery panel refs
        private GameObject _galleryPanel;
        private TextCardElement _sampleCard;
        private StatBarElement _sampleStatBar;
        private ActionListItemElement _sampleActionItem;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Modal
            var modal = new ModalBuilder("CounterModal")
                .WithTitle("Demo Showcase")
                .WithSize(s.ModalWidth, s.ModalHeight)
                .WithDimColor(s.BgDimColor)
                .WithBackgroundColor(s.ModalBgColor)
                .WithHeaderColor(s.HeaderColor)
                .NoCloseButton()
                .Build(Root);

            var content = modal.transform.Find("Window/Content");
            if (content == null) content = modal.transform;

            // Toggle button at top
            _toggleButton = new ButtonBuilder("ToggleGalleryButton")
                .WithLabel("Show Elements")
                .WithLabelSize(14f)
                .Ghost()
                .WithPreferredSize(null, 35f)
                .Build(content);
            _toggleLabel = _toggleButton.GetComponentInChildren<TextMeshProUGUI>();

            // === COUNTER PANEL ===
            var counterPanelVlg = new VerticalLayoutBuilder("CounterPanel")
                .WithSpacing(5f)
                .WithAlignment(TextAnchor.MiddleCenter)
                .Build(content);
            _counterPanel = counterPanelVlg.gameObject;

            _counterText = new TextBuilder("CounterText")
                .WithText("Count: 0")
                .WithFontSize(s.CounterFontSize)
                .WithColor(s.CounterTextColor)
                .Bold()
                .Centered()
                .WithPreferredSize(null, 80f)
                .Build(_counterPanel.transform);

            _incrementsText = new TextBuilder("IncrementsText")
                .WithText("Total actions: 0")
                .WithFontSize(s.IncrementsFontSize)
                .WithColor(s.IncrementsTextColor)
                .Centered()
                .WithPreferredSize(null, 25f)
                .Build(_counterPanel.transform);

            new SpacerBuilder("Spacer").Height(10f).Build(_counterPanel.transform);

            var tooltipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TooltipPrefabPath);
            var confirmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfirmPrefabPath);

            var buttonRow = new HorizontalLayoutBuilder("ButtonRow")
                .WithSpacing(s.ButtonSpacing)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(_counterPanel.transform);

            _decrementButton = new ButtonBuilder("DecrementButton")
                .WithLabel("\u2212")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.DecrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);
            _decrementTooltip = AddPopupTrigger(_decrementButton.gameObject, tooltipPrefab, PopupTriggerType.Hover);

            _resetButton = new ButtonBuilder("ResetButton")
                .WithLabel("Reset")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.ResetButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);
            _resetConfirm = AddPopupTrigger(_resetButton.gameObject, confirmPrefab, PopupTriggerType.Click);

            _incrementButton = new ButtonBuilder("IncrementButton")
                .WithLabel("+")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.IncrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);
            _incrementTooltip = AddPopupTrigger(_incrementButton.gameObject, tooltipPrefab, PopupTriggerType.Hover);

            // === GALLERY PANEL (hidden by default) ===
            var galleryVlg = new VerticalLayoutBuilder("GalleryPanel")
                .WithSpacing(10f)
                .WithPadding(10)
                .Build(content);
            _galleryPanel = galleryVlg.gameObject;
            _galleryPanel.SetActive(false);

            // Gallery header
            new TextBuilder("GalleryHeader")
                .WithText("BindableElement Gallery")
                .WithFontSize(18f)
                .Bold()
                .Centered()
                .WithPreferredSize(null, 30f)
                .Build(_galleryPanel.transform);

            new TextBuilder("GalleryDescription")
                .WithText("Sample elements bound with test data. Hover for tooltips.")
                .WithFontSize(12f)
                .Centered()
                .WithColor(new Color(0.7f, 0.7f, 0.7f))
                .WithPreferredSize(null, 20f)
                .Build(_galleryPanel.transform);

            new SpacerBuilder("GallerySpacer").Height(5f).Build(_galleryPanel.transform);

            // TextCardElement
            var cardGo = new GameObject("SampleCard");
            cardGo.transform.SetParent(_galleryPanel.transform, false);
            cardGo.AddComponent<RectTransform>();
            var cardLe = cardGo.AddComponent<LayoutElement>();
            cardLe.preferredHeight = 60f;
            cardGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
            _sampleCard = cardGo.AddComponent<TextCardElement>();
            WireTextCardElement(cardGo);

            // StatBarElement
            var statGo = new GameObject("SampleStatBar");
            statGo.transform.SetParent(_galleryPanel.transform, false);
            statGo.AddComponent<RectTransform>();
            var statLe = statGo.AddComponent<LayoutElement>();
            statLe.preferredHeight = 40f;
            statGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
            _sampleStatBar = statGo.AddComponent<StatBarElement>();
            WireStatBarElement(statGo);

            // ActionListItemElement
            var actionGo = new GameObject("SampleActionItem");
            actionGo.transform.SetParent(_galleryPanel.transform, false);
            actionGo.AddComponent<RectTransform>();
            var actionLe = actionGo.AddComponent<LayoutElement>();
            actionLe.preferredHeight = 50f;
            actionGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
            _sampleActionItem = actionGo.AddComponent<ActionListItemElement>();
            WireActionListItemElement(actionGo);
        }

        private void WireTextCardElement(GameObject go)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var textContainer = new GameObject("TextContainer");
            textContainer.transform.SetParent(go.transform, false);
            textContainer.AddComponent<RectTransform>();
            var vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(textContainer.transform, false);
            titleGo.AddComponent<RectTransform>();
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "TextCard";
            titleTmp.fontSize = 16f;
            titleTmp.fontStyle = FontStyles.Bold;

            var descGo = new GameObject("Description");
            descGo.transform.SetParent(textContainer.transform, false);
            descGo.AddComponent<RectTransform>();
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = "Description here";
            descTmp.fontSize = 12f;
            descTmp.color = new Color(0.7f, 0.7f, 0.7f);

            var so = new SerializedObject(_sampleCard);
            SerializationHelper.Wire(so, "_titleText", titleTmp);
            SerializationHelper.Wire(so, "_descriptionText", descTmp);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void WireStatBarElement(GameObject go)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.AddComponent<RectTransform>();
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 60f;
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "Label";
            labelTmp.fontSize = 14f;

            // Fill bar background
            var barBg = new GameObject("BarBg");
            barBg.transform.SetParent(go.transform, false);
            var barBgRt = barBg.AddComponent<RectTransform>();
            barBg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
            var barBgLe = barBg.AddComponent<LayoutElement>();
            barBgLe.flexibleWidth = 1f;
            barBgLe.preferredHeight = 20f;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barBg.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.8f, 0.3f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0.75f;

            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(go.transform, false);
            valueGo.AddComponent<RectTransform>();
            var valueLe = valueGo.AddComponent<LayoutElement>();
            valueLe.preferredWidth = 60f;
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = "75/100";
            valueTmp.fontSize = 14f;
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;

            var so = new SerializedObject(_sampleStatBar);
            SerializationHelper.Wire(so, "_labelText", labelTmp);
            SerializationHelper.Wire(so, "_valueText", valueTmp);
            SerializationHelper.Wire(so, "_fillImage", fillImg);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void WireActionListItemElement(GameObject go)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var textContainer = new GameObject("TextContainer");
            textContainer.transform.SetParent(go.transform, false);
            textContainer.AddComponent<RectTransform>();
            var vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            var tcLe = textContainer.AddComponent<LayoutElement>();
            tcLe.flexibleWidth = 1f;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(textContainer.transform, false);
            titleGo.AddComponent<RectTransform>();
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Action Item";
            titleTmp.fontSize = 14f;
            titleTmp.fontStyle = FontStyles.Bold;

            var subtitleGo = new GameObject("Subtitle");
            subtitleGo.transform.SetParent(textContainer.transform, false);
            subtitleGo.AddComponent<RectTransform>();
            var subtitleTmp = subtitleGo.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Subtitle";
            subtitleTmp.fontSize = 11f;
            subtitleTmp.color = new Color(0.6f, 0.6f, 0.6f);

            // Action button
            var btnGo = new GameObject("ActionButton");
            btnGo.transform.SetParent(go.transform, false);
            btnGo.AddComponent<RectTransform>();
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredWidth = 80f;
            btnGo.AddComponent<Image>().color = new Color(0.3f, 0.5f, 0.9f);
            var btn = btnGo.AddComponent<Button>();

            var btnLabelGo = new GameObject("Label");
            btnLabelGo.transform.SetParent(btnGo.transform, false);
            var btnLabelRt = btnLabelGo.AddComponent<RectTransform>();
            btnLabelRt.anchorMin = Vector2.zero;
            btnLabelRt.anchorMax = Vector2.one;
            btnLabelRt.offsetMin = Vector2.zero;
            btnLabelRt.offsetMax = Vector2.zero;
            var btnLabelTmp = btnLabelGo.AddComponent<TextMeshProUGUI>();
            btnLabelTmp.text = "Execute";
            btnLabelTmp.fontSize = 12f;
            btnLabelTmp.alignment = TextAlignmentOptions.Center;

            var so = new SerializedObject(_sampleActionItem);
            SerializationHelper.Wire(so, "_titleText", titleTmp);
            SerializationHelper.Wire(so, "_subtitleText", subtitleTmp);
            SerializationHelper.Wire(so, "_actionButton", btn);
            SerializationHelper.Wire(so, "_actionLabel", btnLabelTmp);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PopupTrigger AddPopupTrigger(GameObject go, GameObject popupPrefab, PopupTriggerType triggerType)
        {
            var trigger = go.AddComponent<PopupTrigger>();
            var so = new SerializedObject(trigger);

            if (popupPrefab != null)
            {
                var prefabProp = so.FindProperty("_popupPrefab");
                if (prefabProp != null)
                    prefabProp.objectReferenceValue = popupPrefab;
            }

            var triggerProp = so.FindProperty("_triggerType");
            if (triggerProp != null)
                triggerProp.enumValueIndex = (int)triggerType;

            so.ApplyModifiedPropertiesWithoutUndo();
            return trigger;
        }

        protected override void WireFields(CounterView view)
        {
            var so = new SerializedObject(view);

            // Counter
            SerializationHelper.Wire(so, "_counterText", _counterText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_incrementsText", _incrementsText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_incrementButton", _incrementButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_decrementButton", _decrementButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_resetButton", _resetButton.GetComponent<Button>());

            // Toggle
            SerializationHelper.Wire(so, "_toggleGalleryButton", _toggleButton);
            SerializationHelper.Wire(so, "_toggleButtonLabel", _toggleLabel);

            // Panels
            SerializationHelper.Wire(so, "_counterPanel", _counterPanel);
            SerializationHelper.Wire(so, "_galleryPanel", _galleryPanel);

            // Tooltips
            SerializationHelper.Wire(so, "_incrementTooltip", _incrementTooltip);
            SerializationHelper.Wire(so, "_decrementTooltip", _decrementTooltip);
            SerializationHelper.Wire(so, "_resetConfirm", _resetConfirm);

            // Gallery elements
            SerializationHelper.Wire(so, "_sampleCard", _sampleCard);
            SerializationHelper.Wire(so, "_sampleStatBar", _sampleStatBar);
            SerializationHelper.Wire(so, "_sampleActionItem", _sampleActionItem);

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
