using Core.Abstractions.MVVM.ViewModels;
using Core.Mediator.Abstractions;
using Core.Abstractions.MVVM.Views;
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
