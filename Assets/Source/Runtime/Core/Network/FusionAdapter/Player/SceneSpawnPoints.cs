using Core.Network.Abstractions.Services;
using UnityEngine;

namespace Core.Network.FusionAdapter.Player
{
    /// <summary>
    /// Адаптер над группой точек из текущей сцены. Держит не саму группу, а обращение к
    /// активной: сцена переживает не весь сеанс, а сервис в контейнере — весь.
    /// </summary>
    public sealed class SceneSpawnPoints : INetworkSpawnPoints
    {
        public int Count => PlayerSpawnPoints.Active != null ? PlayerSpawnPoints.Active.Count : 0;

        public bool TryGet(int index, out Vector3 position, out Quaternion rotation)
        {
            var active = PlayerSpawnPoints.Active;

            if (active == null)
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }

            return active.TryGet(index, out position, out rotation);
        }
    }
}
