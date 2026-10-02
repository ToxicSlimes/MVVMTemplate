using System.Threading;
using Core.Abstractions.MVVM.Services;
using Core.Abstractions.MVVM.ViewModels;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Commands.Common;

namespace Core.Application.Handlers.Common
{
    public sealed class HideViewHandler<TViewModel> : IRequestHandler<HideViewCommand<TViewModel>, bool>
        where TViewModel : DisposableObject
    {
        private readonly IViewService _viewService;

        public HideViewHandler(IViewService viewService)
        {
            _viewService = viewService;
        }

        public async UniTask<bool> Handle(HideViewCommand<TViewModel> command, CancellationToken ct)
        {
            return await _viewService.HideViewAsync<TViewModel>();
        }
    }
}
