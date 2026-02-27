#if UNITY_EDITOR
using Core.Abstractions.Popup;
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.MVVM.Views.Counter;
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

        private TextMeshProUGUI _counterText;
        private TextMeshProUGUI _incrementsText;
        private Button _incrementButton;
        private Button _decrementButton;
        private Button _resetButton;
        private PopupTrigger _incrementTooltip;
        private PopupTrigger _decrementTooltip;
        private PopupTrigger _resetConfirm;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Modal
            var modal = new ModalBuilder("CounterModal")
                .WithTitle("Counter Demo")
                .WithSize(s.ModalWidth, s.ModalHeight)
                .WithDimColor(s.BgDimColor)
                .WithBackgroundColor(s.ModalBgColor)
                .WithHeaderColor(s.HeaderColor)
                .NoCloseButton()
                .Build(Root);

            var content = modal.transform.Find("Window/Content");
            if (content == null) content = modal.transform;

            // Counter display
            _counterText = new TextBuilder("CounterText")
                .WithText("Count: 0")
                .WithFontSize(s.CounterFontSize)
                .WithColor(s.CounterTextColor)
                .Bold()
                .Centered()
                .WithPreferredSize(null, 100f)
                .Build(content);

            // Total increments
            _incrementsText = new TextBuilder("IncrementsText")
                .WithText("Total actions: 0")
                .WithFontSize(s.IncrementsFontSize)
                .WithColor(s.IncrementsTextColor)
                .Centered()
                .WithPreferredSize(null, 30f)
                .Build(content);

            // Spacer
            new SpacerBuilder("Spacer")
                .Height(15f)
                .Build(content);

            // Button row
            var buttonRow = new HorizontalLayoutBuilder("ButtonRow")
                .WithSpacing(s.ButtonSpacing)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(content);

            var tooltipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TooltipPrefabPath);
            var confirmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfirmPrefabPath);

            // Decrement (with hover tooltip)
            _decrementButton = new ButtonBuilder("DecrementButton")
                .WithLabel("\u2212")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.DecrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);
            _decrementTooltip = AddPopupTrigger(_decrementButton.gameObject, tooltipPrefab, PopupTriggerType.Hover);

            // Reset (with click confirm popup)
            _resetButton = new ButtonBuilder("ResetButton")
                .WithLabel("Reset")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.ResetButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);
            _resetConfirm = AddPopupTrigger(_resetButton.gameObject, confirmPrefab, PopupTriggerType.Click);

            // Increment (with hover tooltip)
            _incrementButton = new ButtonBuilder("IncrementButton")
                .WithLabel("+")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.IncrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);
            _incrementTooltip = AddPopupTrigger(_incrementButton.gameObject, tooltipPrefab, PopupTriggerType.Hover);
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
            SerializationHelper.Wire(so, "_counterText", _counterText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_incrementsText", _incrementsText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_incrementButton", _incrementButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_decrementButton", _decrementButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_resetButton", _resetButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_incrementTooltip", _incrementTooltip);
            SerializationHelper.Wire(so, "_decrementTooltip", _decrementTooltip);
            SerializationHelper.Wire(so, "_resetConfirm", _resetConfirm);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
