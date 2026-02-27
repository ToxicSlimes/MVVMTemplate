#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Infrastructure.Popup.Implementations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

namespace Core.Editor.PrefabBuilderSystem.Builders.ConfirmPopup
{
    public class ConfirmPopupBuilder : ViewBuilder<Infrastructure.Popup.Implementations.ConfirmPopup>
    {
        public ConfirmPopupBuilderSettings Settings { get; set; }

        protected override string PrefabPath => "Assets/Prefabs/Popups/ConfirmPopup.prefab";

        protected override void SetupCanvas(GameObject go)
        {
            // Popups are children of PopupService's overlay canvas
            // Still need Canvas for validation, but set to Inherit (not Overlay)
            var canvas = go.AddComponent<Canvas>();
            Context.Canvas = canvas;

            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var canvasGroup = go.AddComponent<CanvasGroup>();
            Context.CanvasGroup = canvasGroup;

            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
                rt.localScale = Vector3.one;
        }

        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _messageText;
        private Button _confirmButton;
        private Button _cancelButton;

        protected override void BuildViewStructure()
        {
            var s = Settings;

            // Modal background
            var modal = new ModalBuilder("ConfirmModal")
                .WithTitle("Confirm")
                .WithSize(s.ModalWidth, s.ModalHeight)
                .WithDimColor(s.DimColor)
                .WithBackgroundColor(s.BackgroundColor)
                .WithHeaderColor(s.HeaderColor)
                .NoCloseButton()
                .Build(Root);

            var content = modal.transform.Find("ModalWindow/Content");
            if (content == null) content = modal.transform;

            // Title (override modal's built-in title for custom styling)
            _titleText = new TextBuilder("Title")
                .WithText("Are you sure?")
                .WithFontSize(s.TitleFontSize)
                .WithColor(s.TitleColor)
                .Bold()
                .Centered()
                .WithPreferredSize(null, 35f)
                .Build(content);

            // Message
            _messageText = new TextBuilder("Message")
                .WithText("This action cannot be undone.")
                .WithFontSize(s.MessageFontSize)
                .WithColor(s.MessageColor)
                .Centered()
                .Body()
                .WithPreferredSize(null, 50f)
                .Build(content);

            // Spacer
            new SpacerBuilder("Spacer")
                .Height(10f)
                .Build(content);

            // Button row
            var buttonRow = new HorizontalLayoutBuilder("ButtonRow")
                .WithSpacing(s.ButtonSpacing)
                .WithAlignment(TextAnchor.MiddleCenter)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(content);

            // Cancel
            _cancelButton = new ButtonBuilder("CancelButton")
                .WithLabel("Cancel")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.CancelButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);

            // Confirm
            _confirmButton = new ButtonBuilder("ConfirmButton")
                .WithLabel("Confirm")
                .WithLabelSize(s.ButtonFontSize)
                .WithBackgroundColor(s.ConfirmButtonColor)
                .WithLabelColor(s.ButtonTextColor)
                .WithPreferredSize(null, s.ButtonHeight)
                .Build(buttonRow.transform);

            // CanvasGroup already added in SetupCanvas
        }

        protected override void WireFields(Infrastructure.Popup.Implementations.ConfirmPopup view)
        {
            var so = new SerializedObject(view);
            SerializationHelper.Wire(so, "_titleText", _titleText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_messageText", _messageText.GetComponentInChildren<TextMeshProUGUI>());
            SerializationHelper.Wire(so, "_confirmButton", _confirmButton.GetComponent<Button>());
            SerializationHelper.Wire(so, "_cancelButton", _cancelButton.GetComponent<Button>());

            // Wire button label texts
            var confirmLabel = _confirmButton.GetComponentInChildren<TextMeshProUGUI>();
            var cancelLabel = _cancelButton.GetComponentInChildren<TextMeshProUGUI>();
            SerializationHelper.Wire(so, "_confirmButtonText", confirmLabel);
            SerializationHelper.Wire(so, "_cancelButtonText", cancelLabel);

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
