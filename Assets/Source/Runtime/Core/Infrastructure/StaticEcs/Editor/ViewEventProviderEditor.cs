using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

namespace Core.Infrastructure.StaticEcs.Editor
{
    [CustomEditor(typeof(ViewEventProvider)), CanEditMultipleObjects]
    public class ViewEventProviderEditor : StaticEcsEvenTEntityProviderEditor<ViewWorld, ViewEventProvider> { }
}
