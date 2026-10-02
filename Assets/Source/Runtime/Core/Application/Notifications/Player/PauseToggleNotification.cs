using Core.Mediator.Abstractions;

namespace Core.Application.Notifications.Player
{
    /// <summary>
    /// Игрок нажал клавишу выхода и хочет открыть или закрыть меню паузы.
    /// </summary>
    /// <remarks>
    /// Уведомлением, а не прямым вызовом: клавишу ловит компонент на теле игрока, то есть сетевой
    /// слой, а меню живёт в слое представления, который лежит выше по зависимостям и снизу не
    /// виден. Сообщение — единственный законный способ дотянуться.
    /// </remarks>
    public readonly struct PauseToggleNotification : INotification
    {
    }
}
