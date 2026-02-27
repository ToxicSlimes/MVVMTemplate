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

            // Spawn sample elements from prefabs
            const string elementsDir = "Assets/Prefabs/Elements/Samples";

            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{elementsDir}/TextCard.prefab");
            var statPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{elementsDir}/StatBar.prefab");
            var actionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{elementsDir}/ActionListItem.prefab");

            if (cardPrefab != null)
            {
                var cardGo = SpawnPrefab(cardPrefab, _galleryPanel.transform, "SampleCard");
                _sampleCard = cardGo.GetComponent<TextCardElement>();
            }
            else Debug.LogWarning("[CounterViewBuilder] TextCard.prefab not found! Run Tools → UI Prefab Builder → Build Sample Elements first.");

            if (statPrefab != null)
            {
                var statGo = SpawnPrefab(statPrefab, _galleryPanel.transform, "SampleStatBar");
                _sampleStatBar = statGo.GetComponent<StatBarElement>();
            }
            else Debug.LogWarning("[CounterViewBuilder] StatBar.prefab not found!");

            if (actionPrefab != null)
            {
                var actionGo = SpawnPrefab(actionPrefab, _galleryPanel.transform, "SampleActionItem");
                _sampleActionItem = actionGo.GetComponent<ActionListItemElement>();
            }
            else Debug.LogWarning("[CounterViewBuilder] ActionListItem.prefab not found!");
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
