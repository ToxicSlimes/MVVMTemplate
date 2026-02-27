using System;

namespace Core.Abstractions.MVVM.Elements
{
    public interface IBindableElement : IDisposable
    {
        string ElementId { get; }
        bool IsBound { get; }
        void Unbind();
    }

    public interface IBindableElement<in TData> : IBindableElement
        where TData : struct
    {
        void Bind(TData data);
        void Refresh(TData data);
    }
}
