using System;
using UnityEngine.UIElements;

namespace Core.Abstractions.MVVM.Elements
{
    public interface IToolkitBindableElement : IDisposable
    {
        VisualElement Root { get; }
        string ElementId { get; }
        bool IsBound { get; }
        void Unbind();
    }

    public interface IToolkitBindableElement<in TData> : IToolkitBindableElement
        where TData : struct
    {
        void Bind(TData data);
        void Refresh(TData data);
    }
}
