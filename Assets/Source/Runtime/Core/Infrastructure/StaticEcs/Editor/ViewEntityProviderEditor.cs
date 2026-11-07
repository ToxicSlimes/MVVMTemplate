using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

namespace Core.Infrastructure.StaticEcs.Editor
{
    [CustomEditor(typeof(ViewEntityProvider)), CanEditMultipleObjects]
    public class ViewEntityProviderEditor : StaticEcsEntityProviderEditor<ViewWorld, ViewEntityProvider> { }
}
