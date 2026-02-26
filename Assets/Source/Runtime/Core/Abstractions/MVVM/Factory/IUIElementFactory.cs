using UnityEngine;

namespace Core.Abstractions.MVVM.Factory
{
    public interface IUIElementFactory
    {
        GameObject Instantiate(GameObject prefab, Transform parent);
        T Instantiate<T>(T prefab, Transform parent) where T : Component;
    }
}
