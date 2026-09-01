using Core.MVVM.ViewModels.MainMenu;
using Core.UI.Controls;
using R3;
using UnityEngine;

namespace Core.MVVM.Views.MainMenu
{
    public class MainMenuView : PanelView<MainMenuViewModel>
    {
        [Header("Demo Examples")]
        [SerializeField] private BaseButton _counterButton;
        [SerializeField] private BaseButton _settingsButton;
        [SerializeField] private BaseButton _inventoryButton;

        [Header("Actions")]
        [SerializeField] private BaseButton _multiplayerButton;
        [SerializeField] private BaseButton _quitButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            Bind(_counterButton, ViewModel.CounterCommand, ref disposables);
            Bind(_settingsButton, ViewModel.SettingsCommand, ref disposables);
            Bind(_inventoryButton, ViewModel.InventoryCommand, ref disposables);
            Bind(_multiplayerButton, ViewModel.MultiplayerCommand, ref disposables);
            Bind(_quitButton, ViewModel.QuitCommand, ref disposables);
        }
    }
}
