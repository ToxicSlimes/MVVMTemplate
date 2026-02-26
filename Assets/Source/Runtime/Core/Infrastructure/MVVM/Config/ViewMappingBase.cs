using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.MVVM.Config
{
  public abstract class ViewMappingBase : ScriptableObject
  {
    public AssetReferenceT<GameObject> ViewPrefab;
    public abstract System.Type ViewType { get; }
    public abstract System.Type ViewModelType { get; }
  }

  public abstract class ViewMappingBase<TView, TViewModel> : ViewMappingBase
    where TView : BindableView<TViewModel>, IView
    where TViewModel : DisposableObject
  {
    public override System.Type ViewType => typeof(TView);
    public override System.Type ViewModelType => typeof(TViewModel);
  }
}