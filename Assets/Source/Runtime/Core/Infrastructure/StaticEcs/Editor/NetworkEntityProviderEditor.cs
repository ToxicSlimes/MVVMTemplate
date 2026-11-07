using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

namespace Core.Infrastructure.StaticEcs.Editor
{
    [CustomEditor(typeof(NetworkEntityProvider)), CanEditMultipleObjects]
    public class NetworkEntityProviderEditor : StaticEcsEntityProviderEditor<NetworkWorld, NetworkEntityProvider> { }
}
