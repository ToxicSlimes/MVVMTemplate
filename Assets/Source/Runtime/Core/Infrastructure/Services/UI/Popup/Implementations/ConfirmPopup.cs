using LitMotion;
using Core.Domain.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Infrastructure.Popup.Implementations
{
    /// <summary>
    /// TEMPLATE EXAMPLE: A confirmation dialog popup.
    /// Demonstrates: custom PopupBase with typed data, button callbacks, animations.
    /// 
    /// Usage:
    ///   await _popupService.ShowAsync&lt;ConfirmPopup, ConfirmPopupData&gt;(
    ///       targetRect,
    ///       new ConfirmPopupData
    ///       {
    ///           Title = "Delete Item?",
    ///           Message = "This action cannot be undone.",
    ///           ConfirmText = "Delete",
    ///           CancelText = "Keep",
    ///           OnConfirm = () => DeleteItem(),
    ///           OnCancel = () => { }
    ///       });
    /// </summary>
    public class ConfirmPopup : PopupBase<ConfirmPopupData>
    {
        [Header("Confirm Popup Elements")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TMP_Text _confirmButtonText;
        [SerializeField] private TMP_Text _cancelButtonText;

        protected override void Awake()
        {
            base.Awake();

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        protected override void OnDataSet(ConfirmPopupData data)
        {
            if (_titleText != null)
                _titleText.text = data.Title ?? "Confirm";

            if (_messageText != null)
                _messageText.text = data.Message ?? "";

            if (_confirmButtonText != null)
                _confirmButtonText.text = data.ConfirmText ?? "Confirm";

            if (_cancelButtonText != null)
                _cancelButtonText.text = data.CancelText ?? "Cancel";
        }

        protected override void OnDataCleared()
        {
            if (_titleText != null) _titleText.text = "";
            if (_messageText != null) _messageText.text = "";
            if (_confirmButtonText != null) _confirmButtonText.text = "Confirm";
            if (_cancelButtonText != null) _cancelButtonText.text = "Cancel";
        }

        private void OnConfirmClicked()
        {
            Data?.OnConfirm?.Invoke();
            HideAsync().Forget();
        }

        private void OnCancelClicked()
        {
            Data?.OnCancel?.Invoke();
            HideAsync().Forget();
        }

        protected override PopupAnimator GetShowAnimation()
        {
            return Animate()
                .FadeIn(0.2f, Ease.OutQuad)
                .Scale(0.9f, 1f, 0.2f, Ease.OutBack);
        }

        protected override PopupAnimator GetHideAnimation()
        {
            return Animate()
                .FadeOut(0.15f, Ease.InQuad)
                .Scale(1f, 0.9f, 0.15f, Ease.InQuad);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            if (_cancelButton != null) _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }
    }
}
