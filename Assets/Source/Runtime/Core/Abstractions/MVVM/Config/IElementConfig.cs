using System;
using Core.Data.Domain.Models.Elements;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Abstractions.MVVM.Config
{
    public interface IElementConfig
    {
        string GetElementAddress<TElement, TData>()
            where TElement : MonoBehaviour
            where TData : struct, IElementData;

        string GetElementAddress(Type elementType);

        AssetReferenceT<GameObject> GetElementReference<TElement, TData>()
            where TElement : MonoBehaviour
            where TData : struct, IElementData;

        AssetReferenceT<GameObject> GetElementReference(Type elementType);
    }
}
