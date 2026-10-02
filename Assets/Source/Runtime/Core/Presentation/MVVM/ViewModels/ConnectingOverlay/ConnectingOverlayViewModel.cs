using System;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Network;
using Core.Application.Requests.Views;
using Core.Mediator.Abstractions;
using Core.Mediator.Attributes;
using Core.Presentation.MVVM.Views.ConnectingOverlay;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.Presentation.MVVM.ViewModels.ConnectingOverlay
{
    [GenerateViewCommands]
    public class ConnectingOverlayViewModel : DisposableObject, IVisibilityAware
    {
        private const float SearchProgress = 0.25f;
        private const float JoinProgress = 0.6f;
        private const float SyncProgress = 1f;
        private const float StageDelaySeconds = 1.2f;
        private const string SearchStatus = "Searching for session...";
        private const string JoinStatus = "Joining room...";
        private const string SyncStatus = "Syncing game state...";

        private readonly ILobbyModel _model;
        private readonly IMediator _mediator;

        public ReactiveProperty<string> StatusText { get; }
        public ReactiveProperty<float> Progress { get; }
        public ReactiveProperty<bool> IsVisible { get; }

        public ReactiveCommand<Unit> CancelCommand { get; set; }

        public ConnectingOverlayViewModel(ILobbyModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            StatusText = new ReactiveProperty<string>(SearchStatus);
            Progress = new ReactiveProperty<float>(SearchProgress);
            IsVisible = new ReactiveProperty<bool>(false);

            SetupViewModelSubscriptions();
            SetupCommands();
        }

        private void SetupViewModelSubscriptions()
        {
            IsVisible
                .Subscribe(this, static (v, s) =>
                {
                    if (v)
                        s.RunStages().Forget();
                    else
                        s.CancelRequestCts();
                })
                .AddTo(ref Disposables);
        }

        private void SetupCommands()
        {
            CancelCommand = new ReactiveCommand<Unit>(_ => OnCancel().Forget());
            CancelCommand.AddTo(ref Disposables);
        }

        private async UniTaskVoid RunStages()
        {
            var token = CreateNewRequestCts().Token;

            SetStage(SearchProgress, SearchStatus);

            if (await UniTask.Delay(TimeSpan.FromSeconds(StageDelaySeconds), cancellationToken: token).SuppressCancellationThrow())
                return;

            SetStage(JoinProgress, BuildJoinStatus());

            if (await UniTask.Delay(TimeSpan.FromSeconds(StageDelaySeconds), cancellationToken: token).SuppressCancellationThrow())
                return;

            SetStage(SyncProgress, SyncStatus);
        }

        private void SetStage(float progress, string status)
        {
            Progress.Value = progress;
            StatusText.Value = status;
        }

        private string BuildJoinStatus()
        {
            return string.IsNullOrEmpty(_model.RoomCode)
                ? JoinStatus
                : $"Joining room {_model.RoomCode}...";
        }

        private async UniTaskVoid OnCancel()
        {
            IsVisible.Value = false;
            await _mediator.Send(new HideViewRequest<ConnectingOverlayToolkitView, ConnectingOverlayViewModel>());
            await _mediator.Send(new ShutdownNetworkSessionCommand());
        }
    }
}
