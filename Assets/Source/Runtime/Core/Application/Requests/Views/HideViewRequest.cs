using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Requests.Views
{
    public readonly struct HideViewRequest<TView, TViewModel> : IRequest<bool>
        where TView : BindableView<TViewModel>
        where TViewModel : DisposableObject
    {
    }
}
