using System.Threading;
using Core.Abstractions.MVVM.Services;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Requests.Views;

namespace Core.Application.Handlers.Views
{
    public sealed class ShowViewRequestHandler<TView, TViewModel>
        : IRequestHandler<ShowViewRequest<TView, TViewModel>, bool>
        where TView : BindableView<TViewModel>
        where TViewModel : DisposableObject
    {
        private readonly IViewService _viewService;

        public ShowViewRequestHandler(IViewService viewService)
        {
            _viewService = viewService;
        }

        public async UniTask<bool> Handle(
            ShowViewRequest<TView, TViewModel> request,
            CancellationToken ct)
        {
            await _viewService.ShowAsync<TView, TViewModel>();
            return true;
        }
    }
}
