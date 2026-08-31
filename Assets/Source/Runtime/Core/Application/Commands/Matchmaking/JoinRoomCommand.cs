using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Matchmaking
{
    /// <summary>
    /// Исход не возвращается: он ложится в модель лобби статусом и кодом комнаты. Иначе
    /// вызывающему из UI пришлось бы знать сетевые типы.
    /// </summary>
    public readonly struct JoinRoomCommand : IRequest<Nothing>
    {
        public readonly string RoomCode;
        public readonly string Nickname;

        public JoinRoomCommand(string roomCode, string nickname)
        {
            RoomCode = roomCode;
            Nickname = nickname;
        }
    }
}
