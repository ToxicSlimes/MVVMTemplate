using System;
using Core.Abstractions.MVVM.Models.MainMenu;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Common;
using Core.Application.Requests.Views;
using Core.Mediator.Abstractions;
using Core.Mediator.Attributes;
using Core.Presentation.MVVM.Views.Matchmaking;
using Core.Presentation.MVVM.ViewModels.Counter;
using Core.Presentation.MVVM.ViewModels.Inventory;
using Core.Presentation.MVVM.ViewModels.Matchmaking;
using Core.Presentation.MVVM.ViewModels.Settings;
using Core.Presentation.MVVM.Views.Inventory;
using Core.Presentation.MVVM.Views.Settings;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.Presentation.MVVM.ViewModels.MainMenu
{
    [GenerateViewCommands]
    public class MainMenuViewModel : DisposableObject, IVisibilityAware
    {
        private readonly IMainMenuModel _model;
        private readonly IMediator _mediator;

        public ReactiveProperty<bool> IsVisible { get; }

        public ReactiveCommand<Unit> CounterCommand { get; set; }
        public ReactiveCommand<Unit> SettingsCommand { get; set; }
        public ReactiveCommand<Unit> InventoryCommand { get; set; }
        public ReactiveCommand<Unit> MultiplayerCommand { get; set; }
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
            CounterCommand = new ReactiveCommand<Unit>(_ => OnCounter().Forget());
            CounterCommand.AddTo(ref Disposables);

            SettingsCommand = new ReactiveCommand<Unit>(_ => OnSettings().Forget());
            SettingsCommand.AddTo(ref Disposables);

            InventoryCommand = new ReactiveCommand<Unit>(_ => OnInventory().Forget());
            InventoryCommand.AddTo(ref Disposables);

            MultiplayerCommand = new ReactiveCommand<Unit>(_ => OnMultiplayer().Forget());
            MultiplayerCommand.AddTo(ref Disposables);

            QuitCommand = new ReactiveCommand<Unit>(_ => OnQuit());
            QuitCommand.AddTo(ref Disposables);
        }

        private void OnModelIsVisibleChanged(bool value) => IsVisible.Value = value;

        private async UniTaskVoid OnCounter()
        {
            await _mediator.Send(new ShowViewCommand<CounterViewModel>());
        }

        private async UniTaskVoid OnSettings()
        {
            await _mediator.Send(new ShowViewRequest<SettingsToolkitView, SettingsViewModel>());
        }

        private async UniTaskVoid OnInventory()
        {
            await _mediator.Send(new ShowViewRequest<InventoryToolkitView, InventoryViewModel>());
        }

        private async UniTaskVoid OnMultiplayer()
        {
            await _mediator.Send(new ShowViewRequest<MatchmakingToolkitView, MatchmakingViewModel>());
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
