using UnityEngine;

namespace Core.Network.FusionAdapter.Player
{
    /// <summary>
    /// Группа точек спавна, живущая в игровой сцене. Сцена грузится сетью и в контейнер не
    /// попадает, поэтому связь со слоем приложения идёт через статическую ссылку на активную
    /// группу, а потребители ходят в неё через <see cref="SceneSpawnPoints"/>.
    /// </summary>
    public sealed class PlayerSpawnPoints : MonoBehaviour
    {
        public static PlayerSpawnPoints Active { get; private set; }

        [SerializeField] private Transform[] _points;

        public int Count => _points?.Length ?? 0;

        public bool TryGet(int index, out Vector3 position, out Quaternion rotation)
        {
            if (_points == null || index < 0 || index >= _points.Length || _points[index] == null)
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }

            position = _points[index].position;
            rotation = _points[index].rotation;
            return true;
        }

        private void OnEnable() => Active = this;

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
        }
    }
}
