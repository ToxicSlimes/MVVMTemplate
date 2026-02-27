#if UNITY_EDITOR
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
        public CounterViewBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Views/CounterView.prefab";

        private GameObject _counterText;
        private GameObject _incrementsText;
        private GameObject _incrementButton;
        private GameObject _decrementButton;
        private GameObject _resetButton;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Modal with dim background, header, content, footer
            var modal = new ModalBuilder("CounterModal")
                .WithTitle("Counter Demo")
                .WithSize(s.ModalWidth, s.ModalHeight)
                .WithDimColor(s.BgDimColor)
                .WithBackgroundColor(s.ModalBgColor)
                .WithHeaderColor(s.HeaderColor)
                .WithShowCloseButton(false)
                .Build(Root);

            // Find content area inside modal
            var content = modal.transform.Find("ModalWindow/Content");
            if (content == null) content = modal.transform;

            // Counter display
            _counterText = new TextBuilder("CounterText")
                .WithText("Count: 0")
                .WithFontSize(s.CounterFontSize)
                .WithColor(s.CounterTextColor)
                .WithBold()
                .WithAlignment(TextAlignmentOptions.Center)
                .WithAutoSize(false)
                .WithPreferredHeight(100f)
                .Build(content);

            // Total increments
            _incrementsText = new TextBuilder("IncrementsText")
                .WithText("Total actions: 0")
                .WithFontSize(s.IncrementsFontSize)
                .WithColor(s.IncrementsTextColor)
                .WithAlignment(TextAlignmentOptions.Center)
                .WithAutoSize(false)
                .WithPreferredHeight(30f)
                .Build(content);

            // Spacer
            new SpacerBuilder("Spacer")
                .WithHeight(15f)
                .Build(content);

            // Button row
            var buttonRow = new HorizontalLayoutBuilder("ButtonRow")
                .WithSpacing(s.ButtonSpacing)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithControlChildSize(true, true)
                .WithForceExpand(true, false)
                .WithPreferredHeight(s.ButtonHeight)
                .Build(content);

            // Decrement
            _decrementButton = new ButtonBuilder("DecrementButton")
                .WithLabel("−")
                .WithLabelFontSize(s.ButtonFontSize)
                .WithBackgroundColor(s.DecrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredHeight(s.ButtonHeight)
                .Build(buttonRow.transform);

            // Reset
            _resetButton = new ButtonBuilder("ResetButton")
                .WithLabel("Reset")
                .WithLabelFontSize(s.ButtonFontSize)
                .WithBackgroundColor(s.ResetButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredHeight(s.ButtonHeight)
                .Build(buttonRow.transform);

            // Increment
            _incrementButton = new ButtonBuilder("IncrementButton")
                .WithLabel("+")
                .WithLabelFontSize(s.ButtonFontSize)
                .WithBackgroundColor(s.IncrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredHeight(s.ButtonHeight)
                .Build(buttonRow.transform);
        }

        protected override void WireFields(CounterView view)
        {
            var so = new SerializedObject(view);
            SerializationHelper.Wire(so, "_counterText", _counterText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_incrementsText", _incrementsText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_incrementButton", _incrementButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_decrementButton", _decrementButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_resetButton", _resetButton.GetComponent<Button>());
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
