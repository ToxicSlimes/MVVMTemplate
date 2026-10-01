using System;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.Services.Input;
using Core.Application.Commands.Common;
using Core.Application.Commands.Matchmaking;
using Core.Application.Requests.Views;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Attributes;
using Core.MVVM.Views.Pause;
using Cysharp.Threading.Tasks;
using R3;

namespace Core.MVVM.ViewModels.Pause
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

        public ReactiveProperty<bool> IsVisible { get; }

        public ReactiveCommand<Unit> ResumeCommand { get; set; }
        public ReactiveCommand<Unit> LeaveCommand { get; set; }

        public PauseViewModel(IMediator mediator, IInputService input)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _input = input ?? throw new ArgumentNullException(nameof(input));

            IsVisible = new ReactiveProperty<bool>(false);

            SetupCommands();
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
            if (IsVisible.Value)
            {
                Resume().Forget();
                return;
            }

            Open().Forget();
        }

        private async UniTaskVoid Open()
        {
            IsVisible.Value = true;

            _input.DisablePlayerActions();
            _input.EnableUIActions();

            await _mediator.Send(new ShowViewRequest<PauseToolkitView, PauseViewModel>());
        }

        private async UniTaskVoid Resume()
        {
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
            IsVisible.Value = false;

            await _mediator.Send(new HideViewRequest<PauseToolkitView, PauseViewModel>());
            await _mediator.Send(new LeaveRoomCommand());

            _input.DisablePlayerActions();
            _input.EnableUIActions();

            await _mediator.Send(Navigate.ToMainMenu());
        }
    }
}
