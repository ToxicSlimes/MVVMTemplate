using System;
using System.Collections.Generic;
using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Config;
using Core.Data.Domain.Models.Elements;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.MVVM.Config
{
    [CreateAssetMenu(fileName = "ElementConfig", menuName = "MVVM/Element Config")]
    public class ElementConfig : ConfigBase, IElementConfig
    {
        [SerializeField]
        internal List<ElementMappingBase> _mappings = new();

        internal Dictionary<Type, ElementMappingBase> _elementTypeCache;
        internal Dictionary<Type, ElementMappingBase> _dataTypeCache;

        private void OnEnable()
        {
            BuildCache();
        }

        internal void BuildCache()
        {
            _elementTypeCache = new Dictionary<Type, ElementMappingBase>();
            _dataTypeCache = new Dictionary<Type, ElementMappingBase>();

            foreach (var mapping in _mappings)
            {
                if (mapping == null) continue;

                _elementTypeCache[mapping.ElementType] = mapping;
                _dataTypeCache[mapping.DataType] = mapping;
            }
        }

        public string GetElementAddress<TElement, TData>()
            where TElement : MonoBehaviour
            where TData : struct, IElementData
        {
            return GetElementAddress(typeof(TElement));
        }

        public string GetElementAddress(Type elementType)
        {
            if (_elementTypeCache == null) BuildCache();

            if (_elementTypeCache == null || !_elementTypeCache.TryGetValue(elementType, out var mapping))
                throw new KeyNotFoundException($"Element mapping not found for: {elementType.Name}");

            if (mapping.ElementPrefab != null && mapping.ElementPrefab.RuntimeKeyIsValid())
            {
                return mapping.ElementPrefab.RuntimeKey.ToString();
            }

            throw new KeyNotFoundException($"Element address not found for: {elementType.Name}");
        }

        public AssetReferenceT<GameObject> GetElementReference<TElement, TData>()
            where TElement : MonoBehaviour
            where TData : struct, IElementData
        {
            return GetElementReference(typeof(TElement));
        }

        public AssetReferenceT<GameObject> GetElementReference(Type elementType)
        {
            if (_elementTypeCache == null) BuildCache();

            if (_elementTypeCache != null && _elementTypeCache.TryGetValue(elementType, out var mapping) && mapping.ElementPrefab != null)
            {
                return mapping.ElementPrefab;
            }

            throw new KeyNotFoundException($"Element reference not found for: {elementType.Name}");
        }

        public bool TryGetMapping<TElement, TData>(out ElementMappingBase mapping)
            where TElement : MonoBehaviour
            where TData : struct, IElementData
        {
            if (_elementTypeCache == null) BuildCache();

            return _elementTypeCache.TryGetValue(typeof(TElement), out mapping);
        }

        public bool TryGetMapping(Type elementType, out ElementMappingBase mapping)
        {
            if (_elementTypeCache == null) BuildCache();

            return _elementTypeCache.TryGetValue(elementType, out mapping);
        }

#if UNITY_EDITOR
        public void AddMapping(ElementMappingBase mapping)
        {
            if (_mappings.Contains(mapping)) return;

            _mappings.Add(mapping);
            BuildCache();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void RemoveMapping(ElementMappingBase mapping)
        {
            _mappings.Remove(mapping);
            BuildCache();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public IReadOnlyList<ElementMappingBase> GetAllMappings() => _mappings;
#endif
    }
}
