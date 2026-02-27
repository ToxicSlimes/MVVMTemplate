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

        private TextMeshProUGUI _counterText;
        private TextMeshProUGUI _incrementsText;
        private Button _incrementButton;
        private Button _decrementButton;
        private Button _resetButton;

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

            var content = modal.transform.Find("ModalWindow/Content");
            if (content == null) content = modal.transform;

            // Counter display
            _counterText = new TextBuilder("CounterText")
                .FromPrefab()
                .WithText("Count: 0")
                .WithFontSize(s.CounterFontSize)
                .WithColor(s.CounterTextColor)
                .Bold()
                .Centered()
                .WithPreferredSize(null, 100f)
                .Build(content);

            // Total increments
            _incrementsText = new TextBuilder("IncrementsText")
                .FromPrefab()
                .WithText("Total actions: 0")
                .WithFontSize(s.IncrementsFontSize)
                .WithColor(s.IncrementsTextColor)
                .Centered()
                .WithPreferredSize(null, 30f)
                .Build(content);

            // Spacer
            new SpacerBuilder("Spacer")
                .FromPrefab()
                .Height(15f)
                .Build(content);

            // Button row
            var buttonRow = new HorizontalLayoutBuilder("ButtonRow")
                .FromPrefab()
                .WithSpacing(s.ButtonSpacing)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(content);

            // Decrement
            _decrementButton = new ButtonBuilder("DecrementButton")
                .FromPrefab()
                .WithLabel("\u2212")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.DecrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);

            // Reset
            _resetButton = new ButtonBuilder("ResetButton")
                .FromPrefab()
                .WithLabel("Reset")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.ResetButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);

            // Increment
            _incrementButton = new ButtonBuilder("IncrementButton")
                .FromPrefab()
                .WithLabel("+")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.IncrementButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
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
