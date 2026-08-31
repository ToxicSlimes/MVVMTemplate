using R3;

namespace Core.Abstractions.MVVM.ViewModels
{
    public interface IVisibilityAware
    {
        ReactiveProperty<bool> IsVisible { get; }
    }
}
