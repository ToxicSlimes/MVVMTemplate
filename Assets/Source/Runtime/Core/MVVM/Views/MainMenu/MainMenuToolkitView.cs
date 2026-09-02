using Core.Abstractions.Localization;
using Core.Abstractions.Services;
using Core.MVVM.ViewModels.MainMenu;
using Core.UI.Toolkit;
using Reflex.Attributes;
using R3;
using UnityEngine.UIElements;

namespace Core.MVVM.Views.MainMenu
{
    public class MainMenuToolkitView : ToolkitPanelView<MainMenuViewModel>
    {
        [Inject] private ILocalizationService _localizationService;

        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _footerHintLabel;
        private Button _counterButton;
        private Button _settingsButton;
        private Button _inventoryButton;
        private Button _multiplayerButton;
        private Button _quitButton;

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();

            if (_localizationService != null)
            {
                BindLocalized(_titleLabel, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.Title, ref disposables);
                BindLocalized(_subtitleLabel, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.Subtitle, ref disposables);
                BindLocalized(_footerHintLabel, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.FooterHint, ref disposables);
                BindLocalized(_counterButton, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.Counter, ref disposables);
                BindLocalized(_settingsButton, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.Settings, ref disposables);
                BindLocalized(_inventoryButton, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.Inventory, ref disposables);
                BindLocalized(_multiplayerButton, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.Multiplayer, ref disposables);
                BindLocalized(_quitButton, _localizationService, UiLocalizationKeys.Table, UiLocalizationKeys.MainMenu.Quit, ref disposables);
            }

            Bind(_counterButton, ViewModel.CounterCommand, ref disposables);
            Bind(_settingsButton, ViewModel.SettingsCommand, ref disposables);
            Bind(_inventoryButton, ViewModel.InventoryCommand, ref disposables);
            Bind(_multiplayerButton, ViewModel.MultiplayerCommand, ref disposables);
            Bind(_quitButton, ViewModel.QuitCommand, ref disposables);
        }

        private void QueryElements()
        {
            if (Root == null)
                return;

            _titleLabel = Root.Q<Label>(className: "menu-title");
            _subtitleLabel = Root.Q<Label>(className: "menu-subtitle");
            _footerHintLabel = Root.Q<Label>(className: "menu-footer-text");
            _counterButton = Root.Q<Button>("counter-button");
            _settingsButton = Root.Q<Button>("settings-button");
            _inventoryButton = Root.Q<Button>("inventory-button");
            _multiplayerButton = Root.Q<Button>("multiplayer-button");
            _quitButton = Root.Q<Button>("quit-button");
        }
    }
}
