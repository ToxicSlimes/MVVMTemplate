using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

namespace Core.Infrastructure.StaticEcs.Editor
{
    [CustomEditor(typeof(NetworkEventProvider)), CanEditMultipleObjects]
    public class NetworkEventProviderEditor : StaticEcsEvenTEntityProviderEditor<NetworkWorld, NetworkEventProvider> { }
}
