using System;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit
{
    public sealed class ToolkitConfirmOverlay
    {
        private readonly VisualElement _overlay;
        private readonly Label _titleLabel;
        private readonly Label _messageLabel;
        private readonly Button _confirmButton;
        private readonly Button _cancelButton;
        private Action _onConfirm;

        public ToolkitConfirmOverlay(VisualElement root)
        {
            _overlay = root.Q<VisualElement>("confirm-overlay");
            _titleLabel = root.Q<Label>("confirm-title");
            _messageLabel = root.Q<Label>("confirm-message");
            _confirmButton = root.Q<Button>("confirm-button");
            _cancelButton = root.Q<Button>("cancel-button");

            if (_confirmButton != null)
                _confirmButton.clicked += OnConfirmClicked;

            if (_cancelButton != null)
                _cancelButton.clicked += Hide;

            Hide();
        }

        public void Show(string title, string message, string confirmText, string cancelText, Action onConfirm)
        {
            if (_overlay == null) return;

            _onConfirm = onConfirm;

            if (_titleLabel != null)
                _titleLabel.text = title;

            if (_messageLabel != null)
                _messageLabel.text = message;

            if (_confirmButton != null)
                _confirmButton.text = confirmText;

            if (_cancelButton != null)
                _cancelButton.text = cancelText;

            _overlay.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (_overlay == null) return;

            _onConfirm = null;
            _overlay.style.display = DisplayStyle.None;
        }

        private void OnConfirmClicked()
        {
            _onConfirm?.Invoke();
            Hide();
        }
    }
}
