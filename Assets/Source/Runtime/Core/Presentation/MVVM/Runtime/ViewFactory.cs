using System;
using Core.Abstractions.AssetManagement;
using Core.Abstractions.MVVM.Canvas;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.Factory;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Presentation.MVVM
{
    public class ViewFactory : IViewFactory
    {
        private readonly Container _container;
        private readonly IAssetProvider _assetProvider;
        private readonly IViewConfig _viewConfig;
        private readonly ICanvasProvider _canvasProvider;
        private readonly Transform _uiRoot;

        public ViewFactory(Container container, IAssetProvider assetProvider, IViewConfig viewConfig, ICanvasProvider canvasProvider = null)
        {
            _container = container;
            _assetProvider = assetProvider;
            _viewConfig = viewConfig;
            _canvasProvider = canvasProvider;
            _uiRoot = CreateUIRoot();
        }

        public async UniTask<IView> CreateAsync(Type viewModelType, ViewShowParams showParams = null)
        {
            var address = _viewConfig.GetViewAddress(viewModelType);
            var parent = showParams != null ? ResolveParent(showParams) : _uiRoot;
            var prefab = await _assetProvider.LoadAsync<GameObject>(address);
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            GameObjectInjector.InjectRecursive(instance, _container);
            
            var view = instance.GetComponent<IView>();
            if (view == null)
                throw new InvalidOperationException($"Prefab at {address} does not have IView component");
            
            var viewModel = _container.Resolve(viewModelType);
            
            var bindMethod = view.GetType().GetMethod("BindTo");
            bindMethod?.Invoke(view, new[] { viewModel });
            
            return view;
        }

        public async UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null) where TView : BindableView<TViewModel> where TViewModel : DisposableObject
        {
            var address = _viewConfig.GetViewAddress<TViewModel>();
            return await CreateAsync<TView, TViewModel>(address, parent);
        }

        public async UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null) where TView : BindableView<TViewModel> where TViewModel : DisposableObject
        {
            var targetParent = parent != null ? parent : _uiRoot;
            var view = await _assetProvider.InstantiateAsync<TView>(address, targetParent);
            GameObjectInjector.InjectRecursive(view.gameObject, _container);
            var viewModel = _container.Resolve<TViewModel>();
            view.BindTo(viewModel);
            return view;
        }

        public async UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams) where TView : BindableView<TViewModel> where TViewModel : DisposableObject
        {
            var address = _viewConfig.GetViewAddress<TViewModel>();
            var parent = ResolveParent(showParams);
            var view = await _assetProvider.InstantiateAsync<TView>(address, parent);
            GameObjectInjector.InjectRecursive(view.gameObject, _container);
            var viewModel = _container.Resolve<TViewModel>();
            view.BindTo(viewModel);
            return view;
        }

        public void Destroy(IView view)
        {
            if (view is not Component component)
                return;
            
            if (view is IUnbindable unbindable)
                unbindable.Unbind();
            
            _assetProvider.ReleaseInstance(component.gameObject);
        }

        private Transform ResolveParent(ViewShowParams showParams)
        {
            if (showParams.CustomParent != null)
            {
                return showParams.CustomParent;
            }

            if (showParams.TargetCanvas.HasValue && _canvasProvider != null)
            {
                var canvasTransform = _canvasProvider.GetCanvasTransform(showParams.TargetCanvas.Value);
                if (canvasTransform != null)
                {
                    return canvasTransform;
                }
            }

            return _uiRoot;
        }

        private static Transform CreateUIRoot()
        {
            var existing = GameObject.Find("[UIRoot]");
            if (existing != null)
            {
                return existing.transform;
            }
            var root = new GameObject("[UIRoot]");

            if (UnityEngine.Application.isPlaying)
            {
                Object.DontDestroyOnLoad(root);
            }

            return root.transform;
        }
    }
}
