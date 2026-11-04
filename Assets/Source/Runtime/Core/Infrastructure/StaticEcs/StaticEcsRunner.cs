using UnityEngine;
using UnityEngine.Scripting;

namespace Core.Infrastructure.StaticEcs
{
    [Preserve]
    public sealed class StaticEcsRunner : MonoBehaviour
    {
        private StaticEcsOwner _owner;
        internal void Attach(StaticEcsOwner owner) => _owner = owner;
        internal void Detach() => _owner = null;
        private void Update() => _owner?.Update();
        private void OnDestroy() { var owner = _owner; _owner = null; owner?.Dispose(); }
    }
}
