using System;

namespace Core.Abstractions.MVVM.Elements
{
    public interface IClickableElement<TData> : IBindableElement<TData>
        where TData : struct
    {
        Action<TData> OnClicked { set; }
    }
}
