using System;
using Core.Domain.Models.Elements;
using UnityEngine.AddressableAssets;

namespace Core.Abstractions.MVVM.Config
{
    public interface IToolkitElementConfig
    {
        string GetElementAddress(Type elementType);

        AssetReferenceT<UnityEngine.UIElements.VisualTreeAsset> GetElementReference(Type elementType);
    }
}
