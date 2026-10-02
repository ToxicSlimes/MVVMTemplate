using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Abstractions.MVVM.ViewModels;
using Core.Application.Commands.Common;
using Core.Application.Commands.Matchmaking;
using Core.Application.Requests.Views;
using Core.Domain.Models.Lobby;
using Core.Presentation.MVVM.ViewModels.LobbyRoom;
using Core.Presentation.MVVM.Views.LobbyRoom;
using Core.Mediator.Abstractions;
using Core.Mediator.Attributes;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.Presentation.MVVM.ViewModels.Matchmaking
{
    [GenerateViewCommands]
    public class MatchmakingViewModel : DisposableObject, IVisibilityAware
    {
        private const string CreatingStatus = "Creating room...";
        private const string MissingCodeStatus = "Enter a room code";
        private const string RefreshingStatus = "Refreshing sessions...";
        private const string BusyStatus = "Connecting, please wait...";

        private readonly ILobbyModel _model;
        private readonly IMediator _mediator;
        private readonly Subject<Unit> _sessionsUpdated = new();

        private bool _navigatedToRoom;

        public ReactiveProperty<string> Nickname { get; }
        public ReactiveProperty<string> RoomCodeInput { get; }
        public ReactiveProperty<string> StatusText { get; }
        public ReactiveProperty<bool> IsBusy { get; }
        public ReactiveProperty<bool> IsVisible { get; }

        public IReadOnlyList<SessionSummary> Sessions => _model.Sessions;

        public Observable<Unit> SessionsUpdated => _sessionsUpdated;

        public ReactiveCommand<Unit> CreateCommand { get; set; }
        public ReactiveCommand<Unit> JoinCommand { get; set; }
        public ReactiveCommand<Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit> BackCommand { get; set; }

        public MatchmakingViewModel(ILobbyModel model, IMediator mediator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            Nickname = new ReactiveProperty<string>(_model.Nickname);
            RoomCodeInput = new ReactiveProperty<string>(string.Empty);
            StatusText = new ReactiveProperty<string>(_model.StatusText);
            IsBusy = new ReactiveProperty<bool>(_model.IsBusy);
            IsVisible = new ReactiveProperty<bool>(false);

            _sessionsUpdated.AddTo(ref Disposables);

            SetupModelSubscriptions();
            SetupViewModelSubscriptions();
            SetupCommands();
        }

        private void SetupModelSubscriptions()
        {
            _model.RosterChanged += OnModelRosterChanged;
            _model.SessionsChanged += OnModelSessionsChanged;
            _model.StatusChanged += OnModelStatusChanged;
        }

        private void SetupViewModelSubscriptions()
        {
            Nickname
                .Subscribe(this, static (v, s) => s._model.SetNickname(v))
                .AddTo(ref Disposables);
        }

        private void SetupCommands()
        {
            CreateCommand = new ReactiveCommand<Unit>(_ => OnCreate().Forget());
            CreateCommand.AddTo(ref Disposables);

            JoinCommand = new ReactiveCommand<Unit>(_ => OnJoin().Forget());
            JoinCommand.AddTo(ref Disposables);

            RefreshCommand = new ReactiveCommand<Unit>(_ => OnRefresh().Forget());
            RefreshCommand.AddTo(ref Disposables);

            BackCommand = new ReactiveCommand<Unit>(_ => OnBack().Forget());
            BackCommand.AddTo(ref Disposables);
        }

        /// <summary>
        /// Пустой код комнаты означает, что комнаты больше нет — снимаем флаг перехода.
        /// Вьюмодель живёт в контейнере и переживает выход, поэтому без сброса второй заход
        /// в комнату не открыл бы экран.
        /// </summary>
        private void OnModelRosterChanged()
        {
            Nickname.Value = _model.Nickname;

            if (string.IsNullOrEmpty(_model.RoomCode))
            {
                _navigatedToRoom = false;
                return;
            }

            if (_navigatedToRoom)
            {
                return;
            }

            _navigatedToRoom = true;
            OpenRoom().Forget();
        }

        private async UniTaskVoid OpenRoom()
        {
            await _mediator.Send(new ShowViewRequest<LobbyRoomToolkitView, LobbyRoomViewModel>());
        }

        private void OnModelSessionsChanged() => _sessionsUpdated.OnNext(Unit.Default);

        private void OnModelStatusChanged()
        {
            StatusText.Value = _model.StatusText;
            IsBusy.Value = _model.IsBusy;
        }

        /// <summary>
        /// Пока идёт подключение, второй запрос отбрасывается.
        /// </summary>
        /// <remarks>
        /// Старт сессии длится секунды, и всё это время кнопка остаётся нажимаемой. Раньше второй
        /// клик поднимал второй раннер: игрок входил в комнату дважды, занимая два места в ростере
        /// и получая два тела на матч. Нижний слой такой повтор теперь отвергает исключением, но
        /// доводить до него нельзя — обычный двойной клик не должен выглядеть как сбой.
        /// </remarks>
        private bool IsLaunching()
        {
            if (!IsBusy.Value)
            {
                return false;
            }

            StatusText.Value = BusyStatus;
            return true;
        }

        private async UniTaskVoid OnCreate()
        {
            if (IsLaunching())
            {
                return;
            }

            await _mediator.Send(new CreateRoomCommand(Nickname.Value));
        }

        private async UniTaskVoid OnJoin()
        {
            if (IsLaunching())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(RoomCodeInput.Value))
            {
                StatusText.Value = MissingCodeStatus;
                return;
            }

            await _mediator.Send(new JoinRoomCommand(RoomCodeInput.Value, Nickname.Value));
        }

        /// <summary>
        /// Вход по строке из списка. Код заодно подставляется в поле ввода: игрок видит, куда
        /// именно он идёт, и при обрыве может повторить попытку, ничего не набирая заново.
        /// </summary>
        public void JoinSession(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                return;
            }

            RoomCodeInput.Value = roomCode;
            OnJoin().Forget();
        }

        private async UniTaskVoid OnRefresh()
        {
            StatusText.Value = RefreshingStatus;
            await _mediator.Send(new RefreshSessionListCommand());
        }

        private async UniTaskVoid OnBack()
        {
            await _mediator.Send(new NavigateBackCommand());
        }

        public override void Dispose()
        {
            _model.RosterChanged -= OnModelRosterChanged;
            _model.SessionsChanged -= OnModelSessionsChanged;
            _model.StatusChanged -= OnModelStatusChanged;
            base.Dispose();
        }
    }
}
