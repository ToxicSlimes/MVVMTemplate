using System;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Abstractions.MVVM.Services;
using Core.Domain.Enums;
using Core.Abstractions.Services.Input;
using Core.Application.Commands.Common;
using Core.Application.Commands.Matchmaking;
using Core.Application.Requests.Views;
using Core.Mediator.Abstractions;
using Core.Mediator.Attributes;
using Core.Presentation.MVVM.Views.Pause;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.Presentation.MVVM.ViewModels.Pause
{
    /// <summary>
    /// Меню паузы: выход из матча и возврат в игру.
    /// </summary>
    /// <remarks>
    /// Открытие и закрытие меняют не только видимость, но и режим ввода: в бою курсор заперт в
    /// центре экрана, потому что мышью поворачивают обзор, а по кнопкам мышью надо попадать.
    /// Переключение делает сама вьюмодель — она единственная знает, открыт экран или закрыт.
    /// </remarks>
    [GenerateViewCommands]
    public class PauseViewModel : DisposableObject
    {
        private readonly IMediator _mediator;
        private readonly IInputService _input;
        private readonly ILobbyModel _lobby;
        private readonly IViewService _views;
        private bool _opening;
        private int _revision;

        public ReactiveProperty<bool> IsVisible { get; }

        public ReactiveCommand<Unit> ResumeCommand { get; set; }
        public ReactiveCommand<Unit> LeaveCommand { get; set; }

        public PauseViewModel(IMediator mediator, IInputService input, ILobbyModel lobby, IViewService views)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
            _views = views ?? throw new ArgumentNullException(nameof(views));

            IsVisible = new ReactiveProperty<bool>(false);

            SetupCommands();
            ObserveEvent(h => _lobby.PhaseChanged += h, h => _lobby.PhaseChanged -= h)
                .Subscribe(_ => OnPhaseChanged()).AddTo(ref Disposables);
        }

        private void SetupCommands()
        {
            ResumeCommand = new ReactiveCommand<Unit>(_ => Resume().Forget());
            ResumeCommand.AddTo(ref Disposables);

            LeaveCommand = new ReactiveCommand<Unit>(_ => Leave().Forget());
            LeaveCommand.AddTo(ref Disposables);
        }

        /// <summary>
        /// Переключает паузу по клавише выхода.
        /// </summary>
        public void Toggle()
        {
            if (IsDisposed || _lobby.Phase == SessionPhase.Ended) return;
            if (IsVisible.Value)
            {
                Resume().Forget();
                return;
            }

            Open().Forget();
        }

        private async UniTaskVoid Open()
        {
            if (_opening) return;
            _opening = true;
            var revision = ++_revision;
            IsVisible.Value = true;

            _input.DisablePlayerActions();
            _input.EnableUIActions();

            try
            {
                var view = await _views.ShowAsync<PauseViewModel>();
                if (IsDisposed || revision != _revision || _lobby.Phase == SessionPhase.Ended || !IsVisible.Value)
                    await _views.HideAsync(view);
            }
            finally
            {
                _opening = false;
            }
        }

        private void OnPhaseChanged()
        {
            if (_lobby.Phase != SessionPhase.Ended) return;
            ++_revision;
            IsVisible.Value = false;
            _input.DisablePlayerActions();
            _input.EnableUIActions();
            _views.HideViewAsync<PauseViewModel>().Forget();
        }

        private async UniTaskVoid Resume()
        {
            if (IsDisposed || _lobby.Phase == SessionPhase.Ended) return;
            ++_revision;
            IsVisible.Value = false;

            _input.DisableUIActions();
            _input.EnablePlayerActions();

            await _mediator.Send(new HideViewRequest<PauseToolkitView, PauseViewModel>());
        }

        /// <summary>
        /// Уводит из матча: гасит меню, закрывает сессию и возвращает в главное меню.
        /// </summary>
        /// <remarks>
        /// Переход обязателен и делается здесь. Закрытие сессии само никуда не уводит — на
        /// выключение сети подписан лишь тот, кто чистит модель лобби, — и без этого шага
        /// приложение осталось бы на игровом экране без матча под ним. Кнопка выхода в лобби
        /// устроена так же: сначала закрыть комнату, потом уйти с экрана.
        ///
        /// Ввод отдаётся интерфейсу: в меню курсор нужен свободным, а тело игрока к этому моменту
        /// уже деспавнено и вернуть его само некому.
        /// </remarks>
        private async UniTaskVoid Leave()
        {
            if (IsDisposed || _lobby.Phase == SessionPhase.Ended) return;
            var revision = ++_revision;
            IsVisible.Value = false;

            await _mediator.Send(new HideViewRequest<PauseToolkitView, PauseViewModel>());
            if (IsDisposed || revision != _revision || _lobby.Phase == SessionPhase.Ended) return;
            await _mediator.Send(new LeaveRoomCommand());
            if (IsDisposed || revision != _revision || _lobby.Phase == SessionPhase.Ended) return;

            _input.DisablePlayerActions();
            _input.EnableUIActions();

            await _mediator.Send(Navigate.ToMainMenu());
        }
    }
}
