using Core.MVVM.ViewModels.MainMenu;
using Core.UI.Controls;
using R3;
using UnityEngine;

namespace Core.MVVM.Views.MainMenu
{
    public class MainMenuView : PanelView<MainMenuViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private BaseButton _playButton;
        [SerializeField] private BaseButton _quitButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            Bind(_playButton, ViewModel.PlayCommand, ref disposables);
            Bind(_quitButton, ViewModel.QuitCommand, ref disposables);
        }
    }
}
