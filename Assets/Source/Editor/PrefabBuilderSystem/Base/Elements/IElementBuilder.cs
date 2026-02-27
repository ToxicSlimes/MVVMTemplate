#if UNITY_EDITOR
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public interface IElementBuilder<out TComponent> where TComponent : Component
    {
        TComponent Build(Transform parent);
    }

    public interface IGameObjectBuilder
    {
        GameObject Build(Transform parent);
    }
}
#endif
