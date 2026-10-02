using Core.Mediator.Abstractions;

namespace Core.Application.Commands.Matchmaking
{
    /// <summary>
    /// Исход не возвращается: он ложится в модель лобби статусом и кодом комнаты. Иначе
    /// вызывающему из UI пришлось бы знать сетевые типы.
    /// </summary>
    public readonly struct CreateRoomCommand : IRequest<Nothing>
    {
        public readonly string Nickname;

        public CreateRoomCommand(string nickname)
        {
            Nickname = nickname;
        }
    }
}
