using System.Threading;
using Core.Abstractions.MVVM.ViewModels;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Presentation.MVVM.Hosts;
using Core.Application.Requests.Views;

namespace Core.Application.Handlers.Views
{
    public sealed class EmbeddedViewRequestHandler<TViewModel>
        : IRequestHandler<ShowEmbeddedViewRequest<TViewModel>, bool>
        where TViewModel : DisposableObject
    {
        private readonly IEmbeddedPanelHost _host;

        public EmbeddedViewRequestHandler(IEmbeddedPanelHost host)
        {
            _host = host;
        }

        public UniTask<bool> Handle(
            ShowEmbeddedViewRequest<TViewModel> request,
            CancellationToken ct)
        {
            return _host.ShowEmbeddedAsync<TViewModel>(request.Context, ct);
        }
    }
}
