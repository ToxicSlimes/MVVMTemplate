using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Abstractions.AssetManagement
{
    public interface IAssetProvider
    {
        UniTask<T> LoadAsync<T>(string address) where T : class;
        T LoadSync<T>(string address) where T : class;
        UniTask<IList<T>> LoadAllAsync<T>(string label) where T : class;
        UniTask<IList<T>> LoadAllAsync<T>(IEnumerable<string> keys) where T : class;
        UniTask<T> LoadAsync<T>(AssetReference reference) where T : class;
        UniTask<T> LoadAsync<T>(AssetReferenceT<T> reference) where T : Object;
        UniTask<GameObject> InstantiateAsync(string address, Transform parent = null);
        UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null);
        UniTask<TComponent> InstantiateAsync<TComponent>(string address, Transform parent = null) where TComponent : Component;
        UniTask<TComponent> InstantiateAsync<TComponent>(AssetReferenceT<GameObject> reference, Transform parent = null) where TComponent : Component;
        void Release<T>(T asset) where T : class;
        void ReleaseInstance(GameObject instance);
        void ReleaseAllByLabel(string label);
        UniTask PreloadAsync<T>(string address) where T : class;
        UniTask PreloadAsync<T>(IEnumerable<string> addresses) where T : class;
    }
}