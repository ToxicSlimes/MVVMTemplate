namespace Core.Network.Abstractions.Services
{
    /// <summary>
    /// Намерения локального игрока. Запись состояния остаётся за хостом — здесь только отправка,
    /// поэтому методы ничего не возвращают: результат придёт репликацией в ростер.
    /// </summary>
    public interface INetworkLocalPlayer
    {
        bool Exists { get; }

        void SetReady(bool isReady);

        void SetNickname(string nickname);
    }
}
