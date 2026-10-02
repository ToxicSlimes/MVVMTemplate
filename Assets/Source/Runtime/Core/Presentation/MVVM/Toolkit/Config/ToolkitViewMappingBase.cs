using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.UI.Toolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Presentation.MVVM.Config
{
    public abstract class ToolkitViewMappingBase<TView, TViewModel> : ViewMappingBase
        where TView : ToolkitBindableView<TViewModel>, IView
        where TViewModel : DisposableObject
    {
        public override System.Type ViewType => typeof(TView);
        public override System.Type ViewModelType => typeof(TViewModel);
    }
}
