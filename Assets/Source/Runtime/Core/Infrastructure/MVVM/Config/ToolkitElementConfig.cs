using System;
using System.Collections.Generic;
using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Core.Infrastructure.MVVM.Config
{
    [CreateAssetMenu(fileName = "ToolkitElementConfig", menuName = "MVVM/Toolkit Element Config")]
    public class ToolkitElementConfig : ConfigBase, IToolkitElementConfig
    {
        [SerializeField]
        internal List<ToolkitElementMappingBase> _mappings = new();

        internal Dictionary<Type, ToolkitElementMappingBase> _elementTypeCache;

        private void OnEnable()
        {
            BuildCache();
        }

        internal void BuildCache()
        {
            _elementTypeCache = new Dictionary<Type, ToolkitElementMappingBase>();

            foreach (var mapping in _mappings)
            {
                if (mapping == null) continue;
                _elementTypeCache[mapping.ElementType] = mapping;
            }
        }

        public string GetElementAddress(Type elementType)
        {
            if (_elementTypeCache == null) BuildCache();

            if (_elementTypeCache == null || !_elementTypeCache.TryGetValue(elementType, out var mapping))
                throw new KeyNotFoundException($"Toolkit element mapping not found for: {elementType.Name}");

            if (mapping.ElementTemplate != null && mapping.ElementTemplate.RuntimeKeyIsValid())
                return mapping.ElementTemplate.RuntimeKey.ToString();

            throw new KeyNotFoundException($"Toolkit element address not found for: {elementType.Name}");
        }

        public AssetReferenceT<VisualTreeAsset> GetElementReference(Type elementType)
        {
            if (_elementTypeCache == null) BuildCache();

            if (_elementTypeCache != null && _elementTypeCache.TryGetValue(elementType, out var mapping) && mapping.ElementTemplate != null)
                return mapping.ElementTemplate;

            throw new KeyNotFoundException($"Toolkit element reference not found for: {elementType.Name}");
        }
    }
}
