using Core.Abstractions.MVVM.ViewModels;
using Core.Mediator.Abstractions;

namespace Core.Application.Requests.Views
{
    public readonly struct HideEmbeddedViewRequest<TViewModel> : IRequest
        where TViewModel : DisposableObject
    {
    }
}
