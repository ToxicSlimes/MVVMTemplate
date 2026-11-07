using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

namespace Core.Infrastructure.StaticEcs.Editor
{
    public class ViewEcsView : StaticEcsView<ViewWorld, ViewEntityProvider, ViewEventProvider>
    {
        [MenuItem("Window/View ECS")]
        public static void OpenWindow()
        {
            var window = GetWindow<ViewEcsView>();
            window.Show();
            window.Focus();
        }
    }
}
