using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.AssetManagement;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.MVVM.Services;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Reflex.Injectors;
using Core.Data.Domain.Models.Elements;
using UnityEngine;

namespace Core.Infrastructure.MVVM.Services
{
    public class UIElementService : IUIElementService
    {
        private readonly Container _container;
        private readonly IAssetProvider _assetProvider;
        private readonly IElementConfig _elementConfig;

        private readonly Dictionary<Transform, List<IBindableElement>> _containerElements = new();

        public UIElementService(Container container, IAssetProvider assetProvider, IElementConfig elementConfig)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _assetProvider = assetProvider ?? throw new ArgumentNullException(nameof(assetProvider));
            _elementConfig = elementConfig ?? throw new ArgumentNullException(nameof(elementConfig));
        }

        public async UniTask<TElement> CreateElementAsync<TElement, TData>(
            Transform container,
            TData data,
            Action<TData> onClick = null)
            where TElement : MonoBehaviour, IBindableElement<TData>
            where TData : struct, IElementData
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            var address = _elementConfig.GetElementAddress(typeof(TElement));
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"[UIElementService] Element address not found for {typeof(TElement).Name}");
                return null;
            }

            var instance = await _assetProvider.InstantiateAsync<TElement>(address, container);
            GameObjectInjector.InjectRecursive(instance.gameObject, _container);

            if (onClick != null && instance is IClickableElement<TData> clickable)
            {
                clickable.OnClicked = onClick;
            }

            instance.Bind(data);

            TrackElement(container, instance);

            return instance;
        }

        public void DestroyElement(IBindableElement element)
        {
            if (element == null) return;

            element.Dispose();

            if (element is MonoBehaviour mb && mb != null)
            {
                var container = mb.transform.parent;
                UntrackElement(container, element);
                _assetProvider.ReleaseInstance(mb.gameObject);
            }
        }

        public void ClearContainer(Transform container)
        {
            if (container == null) return;

            if (_containerElements.TryGetValue(container, out var elements))
            {
                foreach (var element in elements.ToList())
                {
                    element?.Dispose();

                    if (element is MonoBehaviour mb && mb != null)
                    {
                        _assetProvider.ReleaseInstance(mb.gameObject);
                    }
                }

                elements.Clear();
            }

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                _assetProvider.ReleaseInstance(child.gameObject);
            }
        }

        public IReadOnlyList<TElement> GetElementsInContainer<TElement, TData>(Transform container)
            where TElement : MonoBehaviour, IBindableElement<TData>
            where TData : struct, IElementData
        {
            if (container == null) return Array.Empty<TElement>();

            if (!_containerElements.TryGetValue(container, out var elements))
            {
                return Array.Empty<TElement>();
            }

            return elements.OfType<TElement>().ToList();
        }

        public async UniTask ReconcileElementsAsync<TElement, TData>(
            Transform container,
            IReadOnlyList<TData> newData,
            Action<TData> onClick = null)
            where TElement : MonoBehaviour, IBindableElement<TData>
            where TData : struct, IElementData, IEquatable<TData>
        {
            if (container == null) return;
            if (newData == null || newData.Count == 0)
            {
                ClearContainer(container);
                return;
            }

            var existingElements = GetElementsInContainer<TElement, TData>(container)
                .Where(e => e.ElementId != null)
                .ToDictionary(e => e.ElementId, e => e);

            var newDataIds = new HashSet<string>(newData.Count);
            foreach (var data in newData)
            {
                newDataIds.Add(data.Id);
            }

            foreach (var kvp in existingElements)
            {
                if (!newDataIds.Contains(kvp.Key))
                {
                    DestroyElement(kvp.Value);
                }
            }

            var orderedElements = new List<TElement>(newData.Count);
            var createTasks = new List<UniTask<TElement>>();
            var createIndices = new List<int>();

            for (int i = 0; i < newData.Count; i++)
            {
                var data = newData[i];
                var id = data.Id;

                if (existingElements.TryGetValue(id, out var existing))
                {
                    existing.Refresh(data);
                    orderedElements.Add(existing);
                }
                else
                {
                    orderedElements.Add(null);
                    createTasks.Add(CreateElementAsync<TElement, TData>(container, data, onClick));
                    createIndices.Add(i);
                }
            }

            if (createTasks.Count > 0)
            {
                var created = await UniTask.WhenAll(createTasks);
                for (int i = 0; i < created.Length; i++)
                {
                    orderedElements[createIndices[i]] = created[i];
                }
            }

            for (int i = 0; i < orderedElements.Count; i++)
            {
                if (orderedElements[i] is MonoBehaviour mb && mb != null)
                {
                    mb.transform.SetSiblingIndex(i);
                }
            }
        }

        private void TrackElement(Transform container, IBindableElement element)
        {
            if (!_containerElements.TryGetValue(container, out var list))
            {
                list = new List<IBindableElement>();
                _containerElements[container] = list;
            }
            list.Add(element);
        }

        private void UntrackElement(Transform container, IBindableElement element)
        {
            if (container == null) return;
            if (_containerElements.TryGetValue(container, out var list))
            {
                list.Remove(element);
            }
        }
    }
}
