using System.Threading;
using Core.Application.Notifications.Player;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.MVVM.ViewModels.Pause
{
    /// <summary>
    /// Открывает и закрывает меню паузы по клавише выхода.
    /// </summary>
    /// <remarks>
    /// Обработчиком, а не подпиской в конструкторе вьюмодели: вьюмодель — ленивый синглтон, и до
    /// первого показа экрана её никто не создаёт. Подписка внутри неё не существовала бы ровно до
    /// того момента, когда впервые понадобится, — то есть никогда. Обработчик же контейнер
    /// собирает в момент доставки сообщения и заодно поднимает вьюмодель.
    /// </remarks>
    public sealed class PauseToggleHandler : INotificationHandler<PauseToggleNotification>
    {
        private readonly PauseViewModel _pause;

        public PauseToggleHandler(PauseViewModel pause)
        {
            _pause = pause;
        }

        public UniTask Handle(PauseToggleNotification notification, CancellationToken ct)
        {
            _pause.Toggle();

            return UniTask.CompletedTask;
        }
    }
}
