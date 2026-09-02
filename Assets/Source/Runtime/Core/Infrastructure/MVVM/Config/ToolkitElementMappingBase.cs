using Core.Abstractions.MVVM.Elements;
using Core.Data.Domain.Models.Elements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Core.Infrastructure.MVVM.Config
{
    public abstract class ToolkitElementMappingBase : ScriptableObject
    {
        public AssetReferenceT<VisualTreeAsset> ElementTemplate;
        public abstract System.Type ElementType { get; }
        public abstract System.Type DataType { get; }
    }

    public abstract class ToolkitElementMappingBase<TElement, TData> : ToolkitElementMappingBase
        where TElement : class, IToolkitBindableElement<TData>
        where TData : struct, IElementData
    {
        public override System.Type ElementType => typeof(TElement);
        public override System.Type DataType => typeof(TData);
    }
}
