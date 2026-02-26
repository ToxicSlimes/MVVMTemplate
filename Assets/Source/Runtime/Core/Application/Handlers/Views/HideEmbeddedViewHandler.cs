using System.Threading;
using Core.Abstractions.MVVM.ViewModels;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.MVVM.Hosts;
using Core.Application.Requests.Views;

namespace Core.Application.Handlers.Views
{
    public sealed class HideEmbeddedViewHandler<TViewModel>
        : IRequestHandler<HideEmbeddedViewRequest<TViewModel>>
        where TViewModel : DisposableObject
    {
        private readonly IEmbeddedPanelHost _host;

        public HideEmbeddedViewHandler(IEmbeddedPanelHost host)
        {
            _host = host;
        }

        public async UniTask<Nothing> Handle(
            HideEmbeddedViewRequest<TViewModel> request,
            CancellationToken ct)
        {
            await _host.HideEmbeddedAsync<TViewModel>(ct);
            return Nothing.Value;
        }
    }
}
