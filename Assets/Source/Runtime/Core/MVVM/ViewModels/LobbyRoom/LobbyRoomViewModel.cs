using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Common;
using Core.Application.Commands.Matchmaking;
using Core.Application.Commands.Platform;
using Core.Data.Domain.Models.Lobby;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Attributes;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.MVVM.ViewModels.LobbyRoom
{
    [GenerateViewCommands]
    public class LobbyRoomViewModel : DisposableObject
    {
        private const string StartLabel = "START";
        private const string WaitingLabel = "WAITING";

        private readonly ILobbyModel _model;
        private readonly IMediator _mediator;
        private readonly Subject<Unit> _rosterChanged = new();

        public ReactiveProperty<string> RoomCode { get; }
        public ReactiveProperty<string> StartButtonLabel { get; }
        public ReactiveProperty<bool> IsLocalReady { get; }
        public ReactiveProperty<bool> CanStart { get; }
        public ReactiveProperty<bool> IsHost { get; }

        public IReadOnlyList<LobbyPlayerInfo> Roster => _model.Roster;

        public Observable<Unit> RosterChanged => _rosterChanged;

        public ReactiveCommand<Unit> CopyCodeCommand { get; set; }
        public ReactiveCommand<Unit> ToggleReadyCommand { get; set; }
        public ReactiveCommand<Unit> StartCommand { get; set; }
        public ReactiveCommand<Unit> AddBotCommand { get; set; }
        public ReactiveCommand<Unit> LeaveCommand { get; set; }

        public LobbyRoomViewModel(ILobbyModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            RoomCode = new ReactiveProperty<string>(_model.RoomCode);
            StartButtonLabel = new ReactiveProperty<string>(FormatStartLabel(_model.CanStart));
            IsLocalReady = new ReactiveProperty<bool>(_model.IsLocalReady);
            CanStart = new ReactiveProperty<bool>(_model.CanStart);
            IsHost = new ReactiveProperty<bool>(_model.IsHost);

            _rosterChanged.AddTo(ref Disposables);

            SetupModelSubscriptions();
            SetupCommands();
        }

        private void SetupModelSubscriptions()
        {
            _model.RosterChanged += OnModelRosterChanged;
            _model.StatusChanged += OnModelStatusChanged;
        }

        private void SetupCommands()
        {
            CopyCodeCommand = new ReactiveCommand<Unit>(_ => OnCopyCode().Forget());
            CopyCodeCommand.AddTo(ref Disposables);

            ToggleReadyCommand = new ReactiveCommand<Unit>(_ => OnToggleReady().Forget());
            ToggleReadyCommand.AddTo(ref Disposables);

            StartCommand = new ReactiveCommand<Unit>(_ => OnStart().Forget());
            StartCommand.AddTo(ref Disposables);

            AddBotCommand = new ReactiveCommand<Unit>(_ => OnAddBot().Forget());
            AddBotCommand.AddTo(ref Disposables);

            LeaveCommand = new ReactiveCommand<Unit>(_ => OnLeave().Forget());
            LeaveCommand.AddTo(ref Disposables);
        }

        private void OnModelRosterChanged()
        {
            RoomCode.Value = _model.RoomCode;
            IsHost.Value = _model.IsHost;
            IsLocalReady.Value = _model.IsLocalReady;
            RefreshStartState();

            _rosterChanged.OnNext(Unit.Default);
        }

        private void OnModelStatusChanged()
        {
            RefreshStartState();
        }

        private void RefreshStartState()
        {
            CanStart.Value = _model.CanStart;
            StartButtonLabel.Value = FormatStartLabel(_model.CanStart);
        }

        private async UniTaskVoid OnToggleReady()
        {
            await _mediator.Send(new SetReadyCommand(!IsLocalReady.Value));
        }

        private async UniTaskVoid OnCopyCode()
        {
            await _mediator.Send(new CopyToClipboardCommand(RoomCode.Value));
        }

        private async UniTaskVoid OnAddBot()
        {
            await _mediator.Send(new SpawnBotCommand());
        }

        private async UniTaskVoid OnStart()
        {
            if (!CanStart.Value)
            {
                return;
            }

            await _mediator.Send(new StartMatchCommand());
        }

        private async UniTaskVoid OnLeave()
        {
            await _mediator.Send(new LeaveRoomCommand());
            await _mediator.Send(new NavigateBackCommand());
        }

        private static string FormatStartLabel(bool canStart) => canStart ? StartLabel : WaitingLabel;

        public override void Dispose()
        {
            _model.RosterChanged -= OnModelRosterChanged;
            _model.StatusChanged -= OnModelStatusChanged;
            base.Dispose();
        }
    }
}
