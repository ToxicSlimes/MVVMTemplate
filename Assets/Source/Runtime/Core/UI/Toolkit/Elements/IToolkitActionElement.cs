using System;
using Core.Data.Domain.Models.Elements;

namespace Core.UI.Toolkit.Elements
{
    public interface IToolkitActionElement<TData>
        where TData : struct, IElementData
    {
        Action<TData> OnAction { set; }
    }
}
