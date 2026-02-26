using System.Threading;
using Core.Abstractions.MVVM.Services;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Requests.Views;

namespace Core.Application.Handlers.Views
{
    public sealed class HideViewRequestHandler<TView, TViewModel>
        : IRequestHandler<HideViewRequest<TView, TViewModel>, bool>
        where TView : BindableView<TViewModel>
        where TViewModel : DisposableObject
    {
        private readonly IViewService _viewService;

        public HideViewRequestHandler(IViewService viewService)
        {
            _viewService = viewService;
        }

        public async UniTask<bool> Handle(
            HideViewRequest<TView, TViewModel> request,
            CancellationToken ct)
        {
            var view = _viewService.GetActiveView<TView>();
            if (view != null)
            {
                await _viewService.HideAsync(view);
                return true;
            }
            return false;
        }
    }
}
