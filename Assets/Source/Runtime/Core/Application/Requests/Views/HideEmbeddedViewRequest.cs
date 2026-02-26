using Core.Abstractions.MVVM.ViewModels;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Requests.Views
{
    public readonly struct HideEmbeddedViewRequest<TViewModel> : IRequest
        where TViewModel : DisposableObject
    {
    }
}
