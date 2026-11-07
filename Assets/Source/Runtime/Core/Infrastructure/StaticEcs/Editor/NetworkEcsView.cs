using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

namespace Core.Infrastructure.StaticEcs.Editor
{
    public class NetworkEcsView : StaticEcsView<NetworkWorld, NetworkEntityProvider, NetworkEventProvider>
    {
        [MenuItem("Window/Network ECS")]
        public static void OpenWindow()
        {
            var window = GetWindow<NetworkEcsView>();
            window.Show();
            window.Focus();
        }
    }
}
