using Core.Abstractions.MVVM.ViewModels;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Abstractions.MVVM.Views;
using Core.Application.Requests.Context;

namespace Core.Application.Requests.Views
{
    public readonly struct ShowEmbeddedViewRequest<TViewModel> : IRequest<bool>
        where TViewModel : DisposableObject
    {
        public readonly IViewContext Context;

        public ShowEmbeddedViewRequest(IViewContext context = null)
        {
            Context = context ?? EmptyContext.Instance;
        }
    }
}
