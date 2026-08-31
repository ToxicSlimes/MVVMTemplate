using System;
using System.Collections.Generic;
using Fusion;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Core.Network.FusionAdapter.Spawning
{
    /// <summary>
    /// Прогоняет Reflex-инъекцию по заспавненному сетевому префабу до вызова Spawned().
    /// Помнит уже обработанные экземпляры: повторный проход выполнил бы [Inject]-методы
    /// второй раз на живом объекте.
    /// </summary>
    public sealed class ReflexNetworkObjectProvider : NetworkObjectProviderDefault
    {
        private readonly HashSet<int> _injected = new();

        private Container _container;

        public void SetContainer(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void InjectSceneObjects()
        {
            var sceneObjects = UnityEngine.Object.FindObjectsByType<NetworkObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (var i = 0; i < sceneObjects.Length; i++)
            {
                Inject(sceneObjects[i]);
            }
        }

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            RequireContainer(prefab);

            var instance = base.InstantiatePrefab(runner, prefab);
            Inject(instance);

            return instance;
        }

        private void Inject(NetworkObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!_injected.Add(instance.gameObject.GetInstanceID()))
            {
                return;
            }

            GameObjectInjector.InjectRecursive(instance.gameObject, _container);
        }

        private void RequireContainer(NetworkObject prefab)
        {
            if (_container == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReflexNetworkObjectProvider)}: контейнер не задан, инъекция в '{prefab.name}' невозможна");
            }
        }
    }
}
