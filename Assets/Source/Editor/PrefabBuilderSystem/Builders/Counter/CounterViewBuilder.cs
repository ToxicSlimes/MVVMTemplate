#if UNITY_EDITOR
using Core.Abstractions.Popup;
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.MVVM.Views.Counter;
using Core.UI.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Builders.Counter
{
    public class CounterViewBuilder : PanelViewBuilder<CounterView>
    {
        private const string TooltipPrefabPath = "Assets/Prefabs/Popups/TooltipPopup.prefab";
        private const string ConfirmPrefabPath = "Assets/Prefabs/Popups/ConfirmPopup.prefab";

        public CounterViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/CounterView.prefab";

        protected override bool HasBackground => false;

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
        private BaseButton _closeButton;

        // Gallery panel + containers
        private GameObject _galleryPanel;
        private Transform _textCardsContainer;
        private Transform _statBarsContainer;
        private Transform _actionItemsContainer;

        protected override void BuildPanelStructure()
        {
            var s = Settings;

            // Modal
            var modal = new ModalBuilder("CounterModal")
                .WithTitle("Counter (CQRS Demo)")
                .WithSize(s.ModalWidth, s.ModalHeight)
                .WithDimColor(s.BgDimColor)
                .WithBackgroundColor(s.ModalBgColor)
                .WithHeaderColor(s.HeaderColor)
                .Build(Root);

            var closeTransform = modal.transform.Find("Window/Header/CloseButton");
            if (closeTransform != null)
            {
                _closeButton = closeTransform.GetComponent<BaseButton>();
            }

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
            BuildGalleryPanel(content);

            AddPanelAnimator(modal.gameObject, 0, "Preset_PopupModal");
        }

        private void BuildGalleryPanel(Transform content)
        {
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
                .WithText("Elements created via ReconcileElementsCommand at runtime.\nHover for tooltips. Click action buttons for callbacks.")
                .WithFontSize(12f)
                .Centered()
                .WithColor(new Color(0.7f, 0.7f, 0.7f))
                .WithPreferredSize(null, 30f)
                .Build(_galleryPanel.transform);

            // --- TextCards section ---
            new TextBuilder("TextCardsLabel")
                .WithText("TextCardElement")
                .WithFontSize(14f)
                .Bold()
                .WithColor(new Color(0.5f, 0.8f, 1f))
                .WithPreferredSize(null, 22f)
                .Build(_galleryPanel.transform);

            var textCardsVlg = new VerticalLayoutBuilder("TextCardsContainer")
                .WithSpacing(4f)
                .Build(_galleryPanel.transform);
            _textCardsContainer = textCardsVlg.transform;

            // --- StatBars section ---
            new TextBuilder("StatBarsLabel")
                .WithText("StatBarElement")
                .WithFontSize(14f)
                .Bold()
                .WithColor(new Color(0.5f, 1f, 0.6f))
                .WithPreferredSize(null, 22f)
                .Build(_galleryPanel.transform);

            var statBarsVlg = new VerticalLayoutBuilder("StatBarsContainer")
                .WithSpacing(4f)
                .Build(_galleryPanel.transform);
            _statBarsContainer = statBarsVlg.transform;

            // --- ActionListItems section ---
            new TextBuilder("ActionItemsLabel")
                .WithText("ActionListItemElement")
                .WithFontSize(14f)
                .Bold()
                .WithColor(new Color(1f, 0.7f, 0.4f))
                .WithPreferredSize(null, 22f)
                .Build(_galleryPanel.transform);

            var actionItemsVlg = new VerticalLayoutBuilder("ActionItemsContainer")
                .WithSpacing(4f)
                .Build(_galleryPanel.transform);
            _actionItemsContainer = actionItemsVlg.transform;
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
            SerializationHelper.Wire(so, "_closeButton", _closeButton);
            SerializationHelper.Wire(so, "_counterText", _counterText);
            SerializationHelper.Wire(so, "_incrementsText", _incrementsText);
            SerializationHelper.Wire(so, "_incrementButton", _incrementButton);
            SerializationHelper.Wire(so, "_decrementButton", _decrementButton);
            SerializationHelper.Wire(so, "_resetButton", _resetButton);

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

            // Gallery containers (elements spawned at runtime via ReconcileElementsCommand)
            SerializationHelper.Wire(so, "_textCardsContainer", _textCardsContainer);
            SerializationHelper.Wire(so, "_statBarsContainer", _statBarsContainer);
            SerializationHelper.Wire(so, "_actionItemsContainer", _actionItemsContainer);

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
