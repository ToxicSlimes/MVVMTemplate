using System;
using Core.Data.Domain.Models.Elements;
using UnityEngine.AddressableAssets;

namespace Core.Abstractions.MVVM.Config
{
    public interface IToolkitElementConfig
    {
        string GetElementAddress(Type elementType);

        AssetReferenceT<UnityEngine.UIElements.VisualTreeAsset> GetElementReference(Type elementType);
    }
}
