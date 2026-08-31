using System;
using Core.Abstractions.MVVM.Models.MainMenu;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Requests.Views;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Attributes;
using Core.MVVM.ViewModels.Inventory;
using Core.MVVM.ViewModels.Matchmaking;
using Core.MVVM.ViewModels.Settings;
using Core.MVVM.Views.Inventory;
using Core.MVVM.Views.Matchmaking;
using Core.MVVM.Views.Settings;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.MVVM.ViewModels.MainMenu
{
    [GenerateViewCommands]
    public class MainMenuViewModel : DisposableObject, IVisibilityAware
    {
        private readonly IMainMenuModel _model;
        private readonly IMediator _mediator;

        public ReactiveProperty<bool> IsVisible { get; }

        public ReactiveCommand<Unit> PlayCommand { get; set; }
        public ReactiveCommand<Unit> SettingsCommand { get; set; }
        public ReactiveCommand<Unit> InventoryCommand { get; set; }
        public ReactiveCommand<Unit> QuitCommand { get; set; }

        public MainMenuViewModel(IMainMenuModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            IsVisible = new ReactiveProperty<bool>(_model.IsVisible);

            SetupModelSubscriptions();
            SetupViewModelSubscriptions();
            SetupCommands();
        }

        private void SetupModelSubscriptions()
        {
            _model.IsVisibleChanged += OnModelIsVisibleChanged;
        }

        private void SetupViewModelSubscriptions()
        {
            IsVisible
                .Subscribe(this, static (v, s) => s._model.IsVisible = v)
                .AddTo(ref Disposables);
        }

        private void SetupCommands()
        {
            PlayCommand = new ReactiveCommand<Unit>(_ => OnPlay().Forget());
            PlayCommand.AddTo(ref Disposables);

            SettingsCommand = new ReactiveCommand<Unit>(_ => OnSettings().Forget());
            SettingsCommand.AddTo(ref Disposables);

            InventoryCommand = new ReactiveCommand<Unit>(_ => OnInventory().Forget());
            InventoryCommand.AddTo(ref Disposables);

            QuitCommand = new ReactiveCommand<Unit>(_ => OnQuit());
            QuitCommand.AddTo(ref Disposables);
        }

        private void OnModelIsVisibleChanged(bool value) => IsVisible.Value = value;

        private async UniTaskVoid OnPlay()
        {
            await _mediator.Send(new ShowViewRequest<MatchmakingView, MatchmakingViewModel>());
        }

        private async UniTaskVoid OnSettings()
        {
            await _mediator.Send(new ShowViewRequest<SettingsView, SettingsViewModel>());
        }

        private async UniTaskVoid OnInventory()
        {
            await _mediator.Send(new ShowViewRequest<InventoryView, InventoryViewModel>());
        }

        private static void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        public override void Dispose()
        {
            _model.IsVisibleChanged -= OnModelIsVisibleChanged;
            _model.Dispose();
            base.Dispose();
        }
    }
}
