using Core.Presentation.MVVM.ViewModels.ConnectingOverlay;
using Core.UI.Toolkit;
using R3;
using UnityEngine.UIElements;

namespace Core.Presentation.MVVM.Views.ConnectingOverlay
{
    public class ConnectingOverlayToolkitView : ToolkitPanelView<ConnectingOverlayViewModel>
    {
        private Label _statusLabel;
        private VisualElement _progressFill;
        private Button _cancelButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();
            Bind(_statusLabel, ViewModel.StatusText, ref disposables);
            BindFill(_progressFill, ViewModel.Progress, ref disposables);
            Bind(_cancelButton, ViewModel.CancelCommand, ref disposables);
        }

        private void QueryElements()
        {
            if (Root == null) return;
            _statusLabel = Root.Q<Label>("status-label");
            _progressFill = Root.Q<VisualElement>("progress-fill");
            _cancelButton = Root.Q<Button>("cancel-button");
        }
    }
}
