using System;

namespace Core.Abstractions.MVVM.Models
{
    public interface IModel : IDisposable
    {
        bool IsValid { get; }
        void Reset();
    }
}
