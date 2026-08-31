using UnityEngine;

namespace Core.Network.Abstractions.Services
{
    /// <summary>
    /// Точки появления игроков в загруженной сцене. Пока сцена не загружена, <see cref="Count"/>
    /// равен нулю — это нормальное состояние, а не ошибка.
    /// </summary>
    public interface INetworkSpawnPoints
    {
        int Count { get; }

        bool TryGet(int index, out Vector3 position, out Quaternion rotation);
    }
}
