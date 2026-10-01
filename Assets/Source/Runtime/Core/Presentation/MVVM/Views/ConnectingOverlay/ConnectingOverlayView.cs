using Core.MVVM.ViewModels.ConnectingOverlay;
using Core.UI.Controls;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.ConnectingOverlay
{
    public class ConnectingOverlayView : PanelView<ConnectingOverlayViewModel>
    {
        private const string TitleLabel = "CONNECTING";

        [Header("Header")]
        [SerializeField] private BaseText _titleText;

        [Header("Status")]
        [SerializeField] private BaseText _statusText;
        [SerializeField] private Image _progressFill;

        [Header("Buttons")]
        [SerializeField] private BaseButton _cancelButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            BindText(_titleText, TitleLabel);

            Bind(_statusText, ViewModel.StatusText, ref disposables);
            BindFill(_progressFill, ViewModel.Progress, ref disposables);
            Bind(_cancelButton, ViewModel.CancelCommand, ref disposables);
        }
    }
}
