using Core.MVVM.ViewModels.Pause;
using Core.UI.Controls;
using R3;
using UnityEngine;

namespace Core.MVVM.Views.Pause
{
    public class PauseView : PanelView<PauseViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private BaseButton _resumeButton;
        [SerializeField] private BaseButton _leaveButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            Bind(_resumeButton, ViewModel.ResumeCommand, ref disposables);
            Bind(_leaveButton, ViewModel.LeaveCommand, ref disposables);
        }
    }
}
