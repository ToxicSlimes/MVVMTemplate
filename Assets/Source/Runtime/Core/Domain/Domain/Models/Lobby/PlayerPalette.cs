using UnityEngine;

namespace Core.Data.Domain.Models.Lobby
{
    /// <summary>
    /// Цвета игроков по номеру места в комнате.
    /// </summary>
    /// <remarks>
    /// Лежит в самом нижнем слое, потому что нужен с двух сторон: слот в лобби рисует его вью, а
    /// тело в доме красит сетевой слой. Пока палитра была полем вью, второй стороне оставалось
    /// только завести свою копию — и цвет игрока в лобби разошёлся бы с цветом того же игрока
    /// в игре, то есть ровно там, где по цвету и различают, кто есть кто.
    /// </remarks>
    public static class PlayerPalette
    {
        private static readonly Color[] Colors =
        {
            new(0.388f, 0.4f, 0.945f, 1f),
            new(0.024f, 0.714f, 0.831f, 1f),
            new(0.545f, 0.361f, 0.965f, 1f),
            new(0.925f, 0.282f, 0.6f, 1f)
        };

        public static int Count => Colors.Length;

        /// <summary>
        /// Цвет места. Номер зажимается, а не отбрасывается: мест в комнате больше, чем цветов,
        /// быть не должно, но бесцветное тело выглядело бы поломкой, а повтор цвета — нет.
        /// </summary>
        public static Color Resolve(int colorIndex)
        {
            return Colors[Mathf.Clamp(colorIndex, 0, Colors.Length - 1)];
        }
    }
}
