using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.AssetManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Core.Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        private readonly Dictionary<object, AsyncOperationHandle> _handles = new();
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new();

        public async UniTask<T> LoadAsync<T>(string address) where T : class
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            var result = await handle.ToUniTask();
            TrackHandle(result, handle);
            return result;
        }

        public T LoadSync<T>(string address) where T : class
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            var result = handle.WaitForCompletion();
            TrackHandle(result, handle);
            return result;
        }

        public async UniTask<IList<T>> LoadAllAsync<T>(string label) where T : class
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            var result = await handle.ToUniTask();
            foreach (var asset in result)
            {
                TrackHandle(asset, handle);
            }
            return result;
        }

        public IList<T> LoadAllSync<T>(string label) where T : class
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            var result = handle.WaitForCompletion();
            foreach (var asset in result)
            {
                TrackHandle(asset, handle);
            }
            return result;
        }

        public async UniTask<IList<T>> LoadAllAsync<T>(IEnumerable<string> keys) where T : class
        {
            var keyList = keys.Cast<object>().ToList();
            var handle = Addressables.LoadAssetsAsync<T>(keyList, null, Addressables.MergeMode.Union);
            var result = await handle.ToUniTask();
            foreach (var asset in result)
            {
                TrackHandle(asset, handle);
            }
            return result;
        }

        public async UniTask<T> LoadAsync<T>(AssetReference reference) where T : class
        {
            var handle = reference.LoadAssetAsync<T>();
            var result = await handle.ToUniTask();
            TrackHandle(result, handle);
            return result;
        }

        public async UniTask<T> LoadAsync<T>(AssetReferenceT<T> reference) where T : Object
        {
            var handle = reference.LoadAssetAsync();
            var result = await handle.ToUniTask();
            TrackHandle(result, handle);
            return result;
        }

        public async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(address, parent);
            var instance = await handle.ToUniTask();
            _instanceHandles[instance] = handle;
            return instance;
        }

        public async UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null)
        {
            var handle = reference.InstantiateAsync(parent);
            var instance = await handle.ToUniTask();
            _instanceHandles[instance] = handle;
            return instance;
        }

        public async UniTask<TComponent> InstantiateAsync<TComponent>(string address, Transform parent = null) where TComponent : Component
        {
            var instance = await InstantiateAsync(address, parent);
            var component = instance.GetComponent<TComponent>();
            if (component == null)
            {
                ReleaseInstance(instance);
                throw new InvalidOperationException($"Component {typeof(TComponent).Name} not found on instantiated prefab '{address}'");
            }
            return component;
        }

        public async UniTask<TComponent> InstantiateAsync<TComponent>(AssetReferenceT<GameObject> reference, Transform parent = null) where TComponent : Component
        {
            var handle = reference.InstantiateAsync(parent);
            var instance = await handle.ToUniTask();
            _instanceHandles[instance] = handle;
            var component = instance.GetComponent<TComponent>();
            if (component == null)
            {
                ReleaseInstance(instance);
                throw new InvalidOperationException($"Component {typeof(TComponent).Name} not found on instantiated prefab");
            }
            return component;
        }

        public void Release<T>(T asset) where T : class
        {
            if (asset == null)
            {
                return;
            }
            if (_handles.TryGetValue(asset, out var handle))
            {
                Addressables.Release(handle);
                _handles.Remove(asset);
            }
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }
            if (_instanceHandles.TryGetValue(instance, out var handle))
            {
                Addressables.ReleaseInstance(handle);
                _instanceHandles.Remove(instance);
            }
            else
            {
                if (UnityEngine.Application.isPlaying)
                {
                    Object.Destroy(instance);
                }
                else
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(instance);
#else
                    Object.Destroy(instance);
#endif
                }
            }
        }

        public void ReleaseAllByLabel(string label)
        {
            var toRemove = _handles.Where(kvp => kvp.Value.IsValid() && kvp.Value.DebugName != null && kvp.Value.DebugName.Contains(label)).ToList();
            foreach (var kvp in toRemove)
            {
                Addressables.Release(kvp.Value);
                _handles.Remove(kvp.Key);
            }
        }

        public async UniTask PreloadAsync<T>(string address) where T : class
        {
            await LoadAsync<T>(address);
        }

        public async UniTask PreloadAsync<T>(IEnumerable<string> addresses) where T : class
        {
            var tasks = addresses.Select(addr => LoadAsync<T>(addr));
            await UniTask.WhenAll(tasks);
        }

        private void TrackHandle<T>(T asset, AsyncOperationHandle handle)
        {
            if (asset != null && !_handles.ContainsKey(asset))
            {
                _handles[asset] = handle;
            }
        }
    }
}