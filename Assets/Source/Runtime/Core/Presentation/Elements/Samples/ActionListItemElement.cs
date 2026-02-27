using System;
using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements.Samples;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.Elements.Samples
{
    public class ActionListItemElement : BindableElement<ActionListItemData>
    {
        [Header("List Item")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;
        [SerializeField] private Image _iconImage;

        [Header("Action Button")]
        [SerializeField] private Button _actionButton;
        [SerializeField] private TMP_Text _actionLabel;

        private Action<ActionListItemData> _onAction;

        public Action<ActionListItemData> OnAction
        {
            set => _onAction = value;
        }

        protected override void Awake()
        {
            base.Awake();

            if (_actionButton != null)
            {
                _actionButton.OnClickAsObservable()
                    .Subscribe(this, static (_, self) => self.HandleAction())
                    .AddTo(ref Disposables);
            }
        }

        private void HandleAction()
        {
            if (!IsBound) return;
            _onAction?.Invoke(CurrentData);
        }

        protected override void OnBind(ActionListItemData data)
        {
            if (_titleText != null)
                _titleText.text = data.Title;

            if (_subtitleText != null)
                _subtitleText.text = data.Subtitle;

            if (_iconImage != null && data.Icon != null)
                _iconImage.sprite = data.Icon;

            if (_actionLabel != null)
                _actionLabel.text = !string.IsNullOrEmpty(data.ActionLabel) ? data.ActionLabel : "Action";

            if (_actionButton != null)
                _actionButton.interactable = data.IsActionEnabled;
        }

        protected override void OnUnbind()
        {
            if (_titleText != null)
                _titleText.text = string.Empty;

            if (_subtitleText != null)
                _subtitleText.text = string.Empty;

            if (_actionButton != null)
                _actionButton.interactable = false;
        }
    }
}
