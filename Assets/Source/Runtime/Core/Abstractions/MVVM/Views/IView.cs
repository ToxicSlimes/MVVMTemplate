using System;
using Core.Abstractions.MVVM.ViewModels;

namespace Core.Abstractions.MVVM.Views
{
    public interface IView : IDisposable
    {
        bool IsPersistent { get; }
        IViewModel ViewModel { get; }
    }
}
