using System;
using System.Collections.Generic;
using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.ViewModels;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.MVVM.Config
{
    [CreateAssetMenu(fileName = "ViewConfig", menuName = "MVVM/View Config")]
    public class ViewConfig : ConfigBase, IViewConfig
    {
        [SerializeField] internal List<ViewMappingBase> _mappings = new();
        internal Dictionary<Type, ViewMappingBase> _typeCache;

        private void OnEnable()
        {
            BuildCache();
        }

        internal void BuildCache()
        {
            _typeCache = new Dictionary<Type, ViewMappingBase>();
            foreach (var mapping in _mappings)
            {
                if (mapping == null)
                {
                    continue;
                }
                var viewModelType = mapping.ViewModelType;
                if (viewModelType == null)
                {
                    continue;
                }
                _typeCache[viewModelType] = mapping;
            }
        }

        public string GetViewAddress<TViewModel>() where TViewModel : DisposableObject
        {
            return GetViewAddress(typeof(TViewModel));
        }

        public string GetViewAddress(Type viewModelType)
        {
            if (_typeCache == null)
            {
                BuildCache();
            }
            
            if (_typeCache == null || !_typeCache.TryGetValue(viewModelType, out var mapping))
                throw new KeyNotFoundException($"View mapping not found for ViewModel: {viewModelType.Name}");
            
            if (mapping.ViewPrefab != null)
            {
                var key = mapping.ViewPrefab.RuntimeKey.ToString();
                if (!string.IsNullOrEmpty(key))
                {
                    return key;
                }
            }
            
            throw new KeyNotFoundException($"View mapping not found for ViewModel: {viewModelType.Name}");
        }

        public AssetReferenceT<GameObject> GetViewReference<TViewModel>() where TViewModel : class
        {
            return GetViewReference(typeof(TViewModel));
        }

        public AssetReferenceT<GameObject> GetViewReference(Type viewModelType)
        {
            if (_typeCache == null)
            {
                BuildCache();
            }
            
            if (_typeCache != null && _typeCache.TryGetValue(viewModelType, out var mapping) && mapping.ViewPrefab != null)
            {
                return mapping.ViewPrefab;
            }
            
            throw new KeyNotFoundException($"View reference not found for ViewModel: {viewModelType.Name}");
        }
    }
}