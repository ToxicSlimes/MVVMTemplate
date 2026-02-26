using System.Threading;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using Core.Abstractions.MVVM.Views;

namespace Core.MVVM.Hosts
{
    public interface IEmbeddedPanelHost
    {
        void RegisterPanel<TViewModel>(BindableView<TViewModel> view)
            where TViewModel : DisposableObject;

        UniTask<bool> ShowEmbeddedAsync<TViewModel>(
            IViewContext context,
            CancellationToken ct = default)
            where TViewModel : DisposableObject;

        UniTask HideEmbeddedAsync<TViewModel>(
            CancellationToken ct = default)
            where TViewModel : DisposableObject;

        bool IsEmbeddedVisible<TViewModel>()
            where TViewModel : DisposableObject;

        TViewModel GetEmbeddedViewModel<TViewModel>()
            where TViewModel : DisposableObject;
    }
}
