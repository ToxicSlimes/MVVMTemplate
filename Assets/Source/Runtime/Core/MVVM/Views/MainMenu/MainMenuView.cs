using Core.Abstractions.MVVM.Views;
using Core.MVVM.ViewModels.MainMenu;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.MainMenu
{
    public class MainMenuView : BindableView<MainMenuViewModel>
    {
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        protected override void OnBind(ref DisposableBag disposables)
        {
            if (_playButton != null)
            {
                _playButton.OnClickAsObservable()
                    .Subscribe(ViewModel.PlayCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            if (_settingsButton != null)
            {
                _settingsButton.OnClickAsObservable()
                    .Subscribe(ViewModel.SettingsCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            if (_quitButton != null)
            {
                _quitButton.OnClickAsObservable()
                    .Subscribe(ViewModel.QuitCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }
        }

        public override async UniTask ShowAsync()
        {
            ViewModel.IsVisible.Value = true;
            await base.ShowAsync();
        }
    }
}
