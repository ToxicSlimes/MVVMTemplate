using Core.Abstractions.Localization;
using Core.Abstractions.Services;
using R3;
using UnityEngine.UIElements;

namespace Core.MVVM.Views.MainMenu
{
    public partial class MainMenuToolkitView
    {
        private void ApplyGeneratedBindings(ref DisposableBag disposables)
        {
            if (_localizationService == null)
                return;

            BindLocalizedByClass(_localizationService, "menu-title", typeof(Label), UiLocalizationKeys.Table, "main_menu.title", ref disposables);
            BindLocalizedByClass(_localizationService, "menu-subtitle", typeof(Label), UiLocalizationKeys.Table, "main_menu.subtitle", ref disposables);
            BindLocalizedByClass(_localizationService, "menu-footer-text", typeof(Label), UiLocalizationKeys.Table, "main_menu.footer_hint", ref disposables);
            BindLocalized(Root.Q<Button>("counter-button"), _localizationService, UiLocalizationKeys.Table, "main_menu.counter", ref disposables);
            BindLocalized(Root.Q<Button>("settings-button"), _localizationService, UiLocalizationKeys.Table, "main_menu.settings", ref disposables);
            BindLocalized(Root.Q<Button>("inventory-button"), _localizationService, UiLocalizationKeys.Table, "main_menu.inventory", ref disposables);
            BindLocalized(Root.Q<Button>("multiplayer-button"), _localizationService, UiLocalizationKeys.Table, "main_menu.multiplayer", ref disposables);
            BindLocalized(Root.Q<Button>("quit-button"), _localizationService, UiLocalizationKeys.Table, "main_menu.quit", ref disposables);
        }

        private void BindLocalizedByClass(ILocalizationService localization, string className, System.Type elementType, string table, string key, ref DisposableBag disposables)
        {
            if (Root == null) return;
            if (elementType == typeof(Label))
            {
                var label = Root.Q<Label>(className: className);
                BindLocalized(label, localization, table, key, ref disposables);
            }
        }
    }
}
