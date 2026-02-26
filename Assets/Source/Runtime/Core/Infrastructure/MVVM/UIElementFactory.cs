using Core.Abstractions.MVVM.Factory;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Core.Infrastructure.MVVM
{
    public class UIElementFactory : IUIElementFactory
    {
        private readonly Container _container;

        public UIElementFactory(Container container)
        {
            _container = container;
        }

        public GameObject Instantiate(GameObject prefab, Transform parent)
        {
            var instance = Object.Instantiate(prefab, parent);
            GameObjectInjector.InjectRecursive(instance, _container);
            return instance;
        }

        public T Instantiate<T>(T prefab, Transform parent) where T : Component
        {
            var instance = Object.Instantiate(prefab, parent);
            GameObjectInjector.InjectRecursive(instance.gameObject, _container);
            return instance;
        }
    }
}
