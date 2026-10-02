using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.AssetManagement;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.MVVM.Services;
using Core.Domain.Models.Elements;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Core.UI.Toolkit.Elements;

namespace Core.Presentation.MVVM.Services
{
    public class ToolkitElementService : IToolkitElementService
    {
        private readonly IAssetProvider _assetProvider;
        private readonly IToolkitElementConfig _elementConfig;
        private readonly Dictionary<VisualElement, List<IToolkitBindableElement>> _containerElements = new();

        public ToolkitElementService(IAssetProvider assetProvider, IToolkitElementConfig elementConfig)
        {
            _assetProvider = assetProvider ?? throw new ArgumentNullException(nameof(assetProvider));
            _elementConfig = elementConfig ?? throw new ArgumentNullException(nameof(elementConfig));
        }

        public async UniTask<TElement> CreateElementAsync<TElement, TData>(
            VisualElement container,
            TData data,
            Action<TData> onClick = null)
            where TElement : class, IToolkitBindableElement<TData>
            where TData : struct, IElementData
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            var address = _elementConfig.GetElementAddress(typeof(TElement));
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"[ToolkitElementService] Element address not found for {typeof(TElement).Name}");
                return null;
            }

            var template = await _assetProvider.LoadAsync<VisualTreeAsset>(address);
            if (template == null)
            {
                Debug.LogError($"[ToolkitElementService] Failed to load template for {typeof(TElement).Name}");
                return null;
            }

            var instance = template.Instantiate();
            container.Add(instance);

            var element = (TElement)Activator.CreateInstance(typeof(TElement), instance);
            if (onClick != null && element is IToolkitActionElement<TData> actionElement)
                actionElement.OnAction = onClick;

            element.Bind(data);
            TrackElement(container, element);
            return element;
        }

        public void DestroyElement(IToolkitBindableElement element)
        {
            if (element == null) return;

            var root = element.Root;
            var container = root?.parent;
            element.Dispose();
            root?.RemoveFromHierarchy();

            if (container != null)
                UntrackElement(container, element);
        }

        public void ClearContainer(VisualElement container)
        {
            if (container == null) return;

            if (_containerElements.TryGetValue(container, out var elements))
            {
                foreach (var element in elements.ToList())
                    element?.Dispose();

                elements.Clear();
            }

            container.Clear();
        }

        public IReadOnlyList<TElement> GetElementsInContainer<TElement, TData>(VisualElement container)
            where TElement : class, IToolkitBindableElement<TData>
            where TData : struct, IElementData
        {
            if (container == null) return Array.Empty<TElement>();

            if (!_containerElements.TryGetValue(container, out var elements))
                return Array.Empty<TElement>();

            return elements.OfType<TElement>().ToList();
        }

        public async UniTask ReconcileElementsAsync<TElement, TData>(
            VisualElement container,
            IReadOnlyList<TData> newData,
            Action<TData> onClick = null)
            where TElement : class, IToolkitBindableElement<TData>
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
                newDataIds.Add(data.Id);

            foreach (var kvp in existingElements)
            {
                if (!newDataIds.Contains(kvp.Key))
                    DestroyElement(kvp.Value);
            }

            var orderedElements = new List<TElement>(newData.Count);
            var createTasks = new List<UniTask<TElement>>();
            var createIndices = new List<int>();

            for (int i = 0; i < newData.Count; i++)
            {
                var data = newData[i];
                if (existingElements.TryGetValue(data.Id, out var existing))
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
                    orderedElements[createIndices[i]] = created[i];
            }

            for (int i = 0; i < orderedElements.Count; i++)
            {
                var element = orderedElements[i];
                if (element is ToolkitBindableElement<TData> bindable && bindable.Root != null)
                {
                    var child = bindable.Root;
                    child.RemoveFromHierarchy();
                    container.Insert(i, child);
                }
            }
        }

        private void TrackElement(VisualElement container, IToolkitBindableElement element)
        {
            if (!_containerElements.TryGetValue(container, out var list))
            {
                list = new List<IToolkitBindableElement>();
                _containerElements[container] = list;
            }

            list.Add(element);
        }

        private void UntrackElement(VisualElement container, IToolkitBindableElement element)
        {
            if (container == null) return;

            if (_containerElements.TryGetValue(container, out var list))
                list.Remove(element);
        }
    }
}
