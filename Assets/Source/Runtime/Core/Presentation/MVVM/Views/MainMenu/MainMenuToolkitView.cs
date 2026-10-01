using Core.Abstractions.Localization;
using Core.Abstractions.Services;
using Core.MVVM.ViewModels.MainMenu;
using Core.UI.Toolkit;
using Reflex.Attributes;
using R3;
using UnityEngine.UIElements;

namespace Core.MVVM.Views.MainMenu
{
    public partial class MainMenuToolkitView : ToolkitPanelView<MainMenuViewModel>
    {
        [Inject] private ILocalizationService _localizationService;

        private Button _counterButton;
        private Button _settingsButton;
        private Button _inventoryButton;
        private Button _multiplayerButton;
        private Button _quitButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();

            Bind(_counterButton, ViewModel.CounterCommand, ref disposables);
            Bind(_settingsButton, ViewModel.SettingsCommand, ref disposables);
            Bind(_inventoryButton, ViewModel.InventoryCommand, ref disposables);
            Bind(_multiplayerButton, ViewModel.MultiplayerCommand, ref disposables);
            Bind(_quitButton, ViewModel.QuitCommand, ref disposables);
            ApplyGeneratedBindings(ref disposables);
        }

        private void QueryElements()
        {
            if (Root == null)
                return;

            _counterButton = Root.Q<Button>("counter-button");
            _settingsButton = Root.Q<Button>("settings-button");
            _inventoryButton = Root.Q<Button>("inventory-button");
            _multiplayerButton = Root.Q<Button>("multiplayer-button");
            _quitButton = Root.Q<Button>("quit-button");
        }
    }
}
