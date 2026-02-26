using Core.Abstractions.MVVM.Views;

namespace Core.Abstractions.MVVM.ViewModels
{
    public interface IPanelTransitionProvider
    {
        ViewTransitionStyle TransitionStyle { get; }
    }
}
