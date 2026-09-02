using System;
using Core.Data.Domain.Models.Elements.Samples;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit.Elements.Samples
{
    public class ActionListItemToolkitElement : ToolkitBindableElement<ActionListItemData>, IToolkitActionElement<ActionListItemData>
    {
        private readonly Label _titleLabel;
        private readonly Label _subtitleLabel;
        private readonly Button _actionButton;
        private Action<ActionListItemData> _onAction;

        public ActionListItemToolkitElement() : this(new VisualElement())
        {
        }

        public ActionListItemToolkitElement(VisualElement root) : base(root)
        {
            _titleLabel = root.Q<Label>("item-title");
            _subtitleLabel = root.Q<Label>("item-subtitle");
            _actionButton = root.Q<Button>("item-action-button");

            if (_actionButton != null)
                _actionButton.clicked += HandleAction;
        }

        public Action<ActionListItemData> OnAction
        {
            set => _onAction = value;
        }

        protected override void OnBind(ActionListItemData data)
        {
            if (_titleLabel != null)
                _titleLabel.text = data.Title;

            if (_subtitleLabel != null)
                _subtitleLabel.text = data.Subtitle;

            if (_actionButton != null)
            {
                _actionButton.text = !string.IsNullOrEmpty(data.ActionLabel) ? data.ActionLabel : "Action";
                _actionButton.SetEnabled(data.IsActionEnabled);
            }
        }

        protected override void OnUnbind()
        {
            if (_titleLabel != null)
                _titleLabel.text = string.Empty;

            if (_subtitleLabel != null)
                _subtitleLabel.text = string.Empty;

            if (_actionButton != null)
                _actionButton.SetEnabled(false);
        }

        protected override void OnDispose()
        {
            if (_actionButton != null)
                _actionButton.clicked -= HandleAction;
        }

        private void HandleAction()
        {
            if (!IsBound) return;
            _onAction?.Invoke(CurrentData);
        }
    }
}
