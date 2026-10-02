using Core.Abstractions.Localization;
using Core.Abstractions.Services;
using R3;
using UnityEngine.UIElements;

namespace Core.Presentation.MVVM.Views.Settings
{
    public partial class SettingsToolkitView
    {
        private void ApplyGeneratedBindings(ref DisposableBag disposables)
        {
            if (_localizationService == null)
                return;

            BindLocalizedByClass(_localizationService, "screen-title", typeof(Label), UiLocalizationKeys.Table, "settings.title", ref disposables);
            BindLocalized(Root.Q<Button>("tab-audio"), _localizationService, UiLocalizationKeys.Table, "settings.tab_audio", ref disposables);
            BindLocalized(Root.Q<Button>("tab-video"), _localizationService, UiLocalizationKeys.Table, "settings.tab_video", ref disposables);
            BindLocalized(Root.Q<Button>("tab-display"), _localizationService, UiLocalizationKeys.Table, "settings.tab_display", ref disposables);
            BindLocalized(Root.Q<Button>("tab-controls"), _localizationService, UiLocalizationKeys.Table, "settings.tab_controls", ref disposables);
            BindLocalized(Root.Q<Button>("tab-game"), _localizationService, UiLocalizationKeys.Table, "settings.tab_game", ref disposables);
            BindLocalizedLabel(Root.Q<DropdownField>("language-dropdown"), _localizationService, UiLocalizationKeys.Table, "settings.language", ref disposables);
            BindLocalized(Root.Q<Button>("apply-button"), _localizationService, UiLocalizationKeys.Table, "settings.apply", ref disposables);
            BindLocalized(Root.Q<Button>("reset-button"), _localizationService, UiLocalizationKeys.Table, "settings.reset", ref disposables);
            BindLocalized(Root.Q<Button>("cancel-button"), _localizationService, UiLocalizationKeys.Table, "settings.cancel", ref disposables);
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
