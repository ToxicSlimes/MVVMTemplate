using System.Threading;
using Core.Abstractions.MVVM.Services;
using Core.Abstractions.MVVM.ViewModels;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Commands.Common;

namespace Core.Application.Handlers.Common
{
    public sealed class ShowViewHandler<TViewModel> : IRequestHandler<ShowViewCommand<TViewModel>, bool>
        where TViewModel : DisposableObject
    {
        private readonly IViewService _viewService;

        public ShowViewHandler(IViewService viewService)
        {
            _viewService = viewService;
        }

        public async UniTask<bool> Handle(ShowViewCommand<TViewModel> command, CancellationToken ct)
        {
            var view = await _viewService.ShowAsync<TViewModel>(command.ShowParams);
            return view != null;
        }
    }
}
