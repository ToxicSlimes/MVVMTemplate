using Core.Presentation.MVVM.ViewModels.Pause;
using Core.UI.Toolkit;
using R3;
using UnityEngine.UIElements;

namespace Core.Presentation.MVVM.Views.Pause
{
    public class PauseToolkitView : ToolkitPanelView<PauseViewModel>
    {
        private Button _resumeButton;
        private Button _leaveButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();
            Bind(_resumeButton, ViewModel.ResumeCommand, ref disposables);
            Bind(_leaveButton, ViewModel.LeaveCommand, ref disposables);
        }

        private void QueryElements()
        {
            if (Root == null) return;
            _resumeButton = Root.Q<Button>("resume-button");
            _leaveButton = Root.Q<Button>("leave-button");
        }
    }
}
