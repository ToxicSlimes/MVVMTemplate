using System;
using Core.Abstractions.MVVM.Models;

namespace Core.Abstractions.MVVM.Models.MenuSelection
{
    public interface IMenuSelectionModel : IModel
    {
        event Action<string> StatusTextChanged;

        string StatusText { get; set; }
    }
}