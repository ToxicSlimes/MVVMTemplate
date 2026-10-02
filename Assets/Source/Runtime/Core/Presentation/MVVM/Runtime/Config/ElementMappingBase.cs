using Core.Abstractions.MVVM.Elements;
using Core.Domain.Models.Elements;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Presentation.MVVM.Config
{
    public abstract class ElementMappingBase : ScriptableObject
    {
        public AssetReferenceT<GameObject> ElementPrefab;
        public abstract System.Type ElementType { get; }
        public abstract System.Type DataType { get; }
    }

    public abstract class ElementMappingBase<TElement, TData> : ElementMappingBase
        where TElement : BindableElement<TData>
        where TData : struct, IElementData
    {
        public override System.Type ElementType => typeof(TElement);
        public override System.Type DataType => typeof(TData);
    }
}
