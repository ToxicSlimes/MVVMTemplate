using System;
using System.Threading;
using Core.Network.Abstractions.Entities;
using Core.Network.Abstractions.Types;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Network.Abstractions.Services
{
    public interface INetworkSpawner
    {
        /// <summary>
        /// Немедленный спавн. Префаб обязан быть загружен синхронно, а симуляция — запущена.
        /// </summary>
        INetworkEntity Spawn(
            NetworkPrefabKey prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            NetworkPlayerId? inputAuthority = null,
            Action<INetworkEntity> beforeSpawned = null);

        /// <summary>
        /// Спавн с ожиданием. Выдерживает асинхронную загрузку префаба и постановку в очередь,
        /// пока симуляция не готова. Отмена проверяется до начала операции: после передачи
        /// запроса реализации успешный результат возвращается даже при поздней отмене токена.
        /// При смене сессии реализация очищает объект в исходной сессии и отменяет операцию;
        /// если очистка не удалась, она сообщает об ошибке и сохраняет сведения для восстановления.
        /// Callback вызывается только для ещё действующей исходной сессии и может изменить её.
        /// </summary>
        UniTask<INetworkEntity> SpawnAsync(
            NetworkPrefabKey prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            NetworkPlayerId? inputAuthority = null,
            Action<INetworkEntity> beforeSpawned = null,
            CancellationToken cancellationToken = default);

        void Despawn(INetworkEntity entity);

        bool TryGetEntity(NetworkEntityId id, out INetworkEntity entity);
    }
}
