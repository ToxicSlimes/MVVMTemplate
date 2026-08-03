using Core.Infrastructure.MVVM.Animation;
using Core.MVVM.ViewModels.MainMenu;
using Core.UI.Binding;
using Core.UI.Controls;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.MainMenu
{
    public class MainMenuView : AnimatedView<MainMenuViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private BaseButton _playButton;
        [SerializeField] private BaseButton _settingsButton;
        [SerializeField] private BaseButton _inventoryButton;
        [SerializeField] private BaseButton _quitButton;

        protected override void OnBind(ref DisposableBag disposables)
        {
            if (_playButton != null)
                _playButton.BindTo(ViewModel.PlayCommand, ref disposables);

            if (_settingsButton != null)
                _settingsButton.BindTo(ViewModel.SettingsCommand, ref disposables);

            if (_inventoryButton != null)
                _inventoryButton.BindTo(ViewModel.InventoryCommand, ref disposables);

            if (_quitButton != null)
                _quitButton.BindTo(ViewModel.QuitCommand, ref disposables);
        }

        public override async UniTask ShowAsync()
        {
            ViewModel.IsVisible.Value = true;
            await base.ShowAsync();
        }
    }
}
