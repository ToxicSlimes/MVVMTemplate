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
        // IDs are reserved before injection to prevent reentrant duplicate Inject calls.
        private readonly HashSet<int> _injected = new();
        private readonly Dictionary<int, Exception> _injectionFailures = new();

        private Container _container;

        public void SetContainer(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void InjectSceneObjects()
        {
            var sceneObjects = UnityEngine.Object.FindObjectsByType<NetworkObject>(
                FindObjectsInactive.Include);

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

            var instanceId = instance.gameObject.GetEntityId().GetHashCode();
            if (_injectionFailures.TryGetValue(instanceId, out var failure))
            {
                throw new InvalidOperationException(
                    $"{nameof(ReflexNetworkObjectProvider)}: предыдущая инъекция объекта с ID {instanceId} завершилась ошибкой", failure);
            }

            if (!_injected.Add(instanceId))
            {
                return;
            }

            try
            {
                GameObjectInjector.InjectRecursive(instance.gameObject, _container);
            }
            catch (Exception exception)
            {
                // A partial injection cannot safely be retried on a live object.
                _injectionFailures.Add(instanceId, exception);
                throw;
            }
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
