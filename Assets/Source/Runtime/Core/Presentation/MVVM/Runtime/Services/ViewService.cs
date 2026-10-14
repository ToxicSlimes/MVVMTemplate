using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.Factory;
using Core.Abstractions.MVVM.Services;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.MVVM.Services
{
    public class ViewService : IViewService
    {
        private readonly IViewFactory _viewFactory;
        private readonly IViewAnimator _viewAnimator;
        private readonly Stack<IView> _activeViews = new();
        private readonly Stack<IView> _viewHistory = new();
        private readonly ConcurrentDictionary<Type, IView> _viewCache = new();
        private readonly Dictionary<IView, ViewShowParams> _viewParams = new();
        private readonly Dictionary<IView, int> _viewSortingOrders = new();
        private readonly object _sync = new();

        private IView _currentView;
        private int _currentMaxSortingOrder;

        public event Action<IView> ViewShown;
        public event Action<IView> ViewHidden;

        public ViewService(IViewFactory viewFactory, IViewAnimator viewAnimator)
        {
            _viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            _viewAnimator = viewAnimator ?? throw new ArgumentNullException(nameof(viewAnimator));
        }

        public IView CurrentView => _currentView;
        public IReadOnlyCollection<IView> ViewHistory => _viewHistory.ToArray();
        public IReadOnlyCollection<IView> ActiveViews
        {
            get
            {
                lock (_sync)
                {
                    return _activeViews.ToArray();
                }
            }
        }

        public async UniTask<TView> ShowAsync<TView, TViewModel>(bool hideCurrentView = true) where TView : BindableView<TViewModel> where TViewModel : DisposableObject
        {
            var viewModelType = typeof(TViewModel);

            if (hideCurrentView && _currentView != null && !_currentView.IsPersistent)
            {
                await HideViewInternal(_currentView, true);
            }

            TView view;
            if (_viewCache.TryGetValue(viewModelType, out var cachedView))
            {
                view = (TView)cachedView;
            }
            else
            {
                view = await _viewFactory.CreateAsync<TView, TViewModel>();
                _viewCache[viewModelType] = view;
            }
            AssignSortingOrder(view);

            lock (_sync)
            {
                if (!_activeViews.Contains(view))
                {
                    _activeViews.Push(view);
                }
                Interlocked.Exchange(ref _currentView, view);
            }

            await ShowViewInternal(view);
            ViewShown?.Invoke(view);
            return view;
        }

        public async UniTask<IView> ShowAsync<TViewModel>(ViewShowParams showParams = null) where TViewModel : DisposableObject
        {
            var viewModelType = typeof(TViewModel);
            var effectiveParams = showParams ?? new ViewShowParams();
            
            if (effectiveParams.DisplayMode == ViewDisplayMode.Single && _currentView != null && !_currentView.IsPersistent)
            {
                await HideViewInternal(_currentView, true);
            }

            IView view;
            if (_viewCache.TryGetValue(viewModelType, out var cachedView))
            {
                view = cachedView;
            }
            else
            {
                view = await _viewFactory.CreateAsync(viewModelType, effectiveParams);
                _viewCache[viewModelType] = view;
            }
            
            AssignSortingOrder(view);

            lock (_sync)
            {
                if (!_activeViews.Contains(view))
                {
                    _activeViews.Push(view);
                }
                Interlocked.Exchange(ref _currentView, view);
                _viewParams[view] = effectiveParams;
            }

            await ShowViewInternal(view);
            ViewShown?.Invoke(view);
            return view;
        }

        public async UniTask<bool> HideViewAsync<TViewModel>() where TViewModel : DisposableObject
        {
            var viewModelType = typeof(TViewModel);
            IView viewToHide = null;
            
            lock (_sync)
            {
                if (_viewCache.TryGetValue(viewModelType, out var cachedView) && _activeViews.Contains(cachedView))
                {
                    viewToHide = cachedView;
                }
            }
            
            if (viewToHide == null)
                return false;
            
            await HideAsync(viewToHide);
            return true;
        }

        public TViewModel GetViewModel<TViewModel>() where TViewModel : DisposableObject
        {
            var viewModelType = typeof(TViewModel);
            
            lock (_sync)
            {
                if (_viewCache.TryGetValue(viewModelType, out var view) && _activeViews.Contains(view))
                {
                    return view.ViewModel as TViewModel;
                }
            }
            
            return null;
        }

        public async UniTask<TView> ShowAsync<TView, TViewModel>(ViewShowParams showParams) where TView : BindableView<TViewModel> where TViewModel : DisposableObject
        {
            if (showParams == null)
            {
                throw new ArgumentNullException(nameof(showParams));
            }

            var viewModelType = typeof(TViewModel);

            switch (showParams.DisplayMode)
            {
                case ViewDisplayMode.Single:
                    IView[] viewsToClose;
                    lock (_sync)
                    {
                        viewsToClose = _activeViews
                            .Where(v => !v.IsPersistent)
                            .ToArray();
                    }
                    foreach (var viewToClose in viewsToClose)
                    {
                        if (_viewParams.TryGetValue(viewToClose, out var viewParams))
                        {
                            await HideViewInternalWithParams(viewToClose, true, viewParams);
                        }
                        else
                        {
                            await HideViewInternal(viewToClose, true);
                        }
                        lock (_sync)
                        {
                            var buffer = new Stack<IView>();
                            while (_activeViews.Count > 0)
                            {
                                var top = _activeViews.Pop();
                                if (!ReferenceEquals(top, viewToClose))
                                {
                                    buffer.Push(top);
                                }
                            }
                            while (buffer.Count > 0)
                            {
                                _activeViews.Push(buffer.Pop());
                            }
                            _viewParams.Remove(viewToClose);
                            if (ReferenceEquals(_currentView, viewToClose))
                            {
                                var newCurrent = _activeViews.Count > 0 ? _activeViews.Peek() : null;
                                Interlocked.Exchange(ref _currentView, newCurrent);
                            }
                        }
                    }
                    break;

                case ViewDisplayMode.SingleClearAll:
                    await CloseAllNonPersistentAsync();
                    break;

                case ViewDisplayMode.Replace:
                    if (_currentView != null)
                    {
                        var viewToReplace = _currentView;
                        await HideViewInternalWithParams(viewToReplace, false, showParams);
                        lock (_sync)
                        {
                            var buffer = new Stack<IView>();
                            while (_activeViews.Count > 0)
                            {
                                var top = _activeViews.Pop();
                                if (!ReferenceEquals(top, viewToReplace))
                                {
                                    buffer.Push(top);
                                }
                            }
                            while (buffer.Count > 0)
                            {
                                _activeViews.Push(buffer.Pop());
                            }
                            _viewParams.Remove(viewToReplace);
                        }
                    }
                    break;

                case ViewDisplayMode.Additive:
                    break;
            }

            TView view;
            if (_viewCache.TryGetValue(viewModelType, out var cachedView))
            {
                view = (TView)cachedView;
            }
            else
            {
                view = await _viewFactory.CreateAsync<TView, TViewModel>(showParams);
                _viewCache[viewModelType] = view;
            }

            AssignSortingOrder(view);

            lock (_sync)
            {
                if (!_activeViews.Contains(view))
                {
                    _activeViews.Push(view);
                }
                Interlocked.Exchange(ref _currentView, view);
                _viewParams[view] = showParams;
            }

            await ShowViewInternalWithParams(view, showParams);
            ViewShown?.Invoke(view);

            if (showParams.OnShown != null)
            {
                await showParams.OnShown.Invoke();
            }

            return view;
        }

        public async UniTask HideAsync<TView>() where TView : IView
        {
            var view = _activeViews.OfType<TView>().FirstOrDefault();
            if (view != null)
            {
                await HideAsync(view);
            }
        }

        public async UniTask HideAsync(IView view)
        {
            if (view == null)
            {
                return;
            }
            ViewShowParams viewParams;

            lock (_sync)
            {
                if (!_activeViews.Contains(view))
                {
                    return;
                }

                _viewParams.TryGetValue(view, out viewParams);

                var buffer = new Stack<IView>();
                while (_activeViews.Count > 0)
                {
                    var top = _activeViews.Pop();

                    if (!ReferenceEquals(top, view))
                    {
                        buffer.Push(top);
                    }
                }
                while (buffer.Count > 0)
                {
                    _activeViews.Push(buffer.Pop());
                }

                _viewParams.Remove(view);

                var newCurrent = _activeViews.Count > 0 ? _activeViews.Peek() : null;
                Interlocked.Exchange(ref _currentView, newCurrent);

                if (!view.IsPersistent && view.ViewModel != null)
                {
                    _viewCache.TryRemove(view.ViewModel.GetType(), out _);
                }
            }

            if (viewParams != null)
            {
                await HideViewInternalWithParams(view, false, viewParams);
            }
            else
            {
                await HideViewInternal(view, false);
            }

            ReleaseSortingOrder(view);
            ViewHidden?.Invoke(view);
            if (!view.IsPersistent)
            {
                _viewFactory.Destroy(view);
            }
        }

        public async UniTask HideCurrentAsync()
        {
            if (CurrentView != null)
            {
                await HideAsync(CurrentView);
            }
        }

        public async UniTask GoBackAsync()
        {
            IView previousView;
            
            lock (_sync)
            {
                if (_viewHistory.Count == 0)
                {
                    return;
                }
                previousView = _viewHistory.Pop();
            }

            var current = _currentView;
            
            if (current != null)
            {
                await HideAsync(current);
            }

            AssignSortingOrder(previousView);

            lock (_sync)
            {
                if (!_activeViews.Contains(previousView))
                {
                    _activeViews.Push(previousView);
                }
                Interlocked.Exchange(ref _currentView, previousView);
            }

            await ShowViewInternal(previousView);
            ViewShown?.Invoke(previousView);
        }

        public async UniTask GoToRootAsync()
        {
            while (_viewHistory.Count > 0)
            {
                await GoBackAsync();
            }
        }

        public async UniTask CloseAllNonPersistentAsync()
        {
            IView[] viewsToClose;

            lock (_sync)
            {
                viewsToClose = _activeViews
                    .Where(v => !v.IsPersistent)
                    .ToArray();
            }

            foreach (var view in viewsToClose)
            {
                await HideAsync(view);
            }
        }

        public void ClearHistory()
        {
            lock (_sync)
            {
                _viewHistory.Clear();
            }
        }

        public bool IsViewActive<TView>() where TView : IView
        {
            lock (_sync)
            {
                return _activeViews.OfType<TView>().Any();
            }
        }

        public TView GetActiveView<TView>() where TView : IView
        {
            lock (_sync)
            {
                return _activeViews.OfType<TView>().FirstOrDefault();
            }
        }

        public void ClearCache()
        {
            foreach (var kvp in _viewCache)
            {
                var view = kvp.Value;
                switch (view)
                {
                    case IUnbindable unbindable:
                        unbindable.Unbind();
                        break;
                }
                
                _viewFactory.Destroy(view);
            }
            _viewCache.Clear();
        }

        private void AssignSortingOrder(IView view)
        {
            if (view is not Component component) return;

            var uiDocument = component.GetComponent<UnityEngine.UIElements.UIDocument>();
            if (uiDocument != null)
            {
                _currentMaxSortingOrder++;
                uiDocument.sortingOrder = _currentMaxSortingOrder;

                lock (_sync)
                {
                    _viewSortingOrders[view] = _currentMaxSortingOrder;
                }

                return;
            }

            var canvas = component.GetComponent<UnityEngine.Canvas>();
            if (canvas == null)
            {
                canvas = component.gameObject.AddComponent<UnityEngine.Canvas>();
                component.gameObject.AddComponent<GraphicRaycaster>();
            }

            _currentMaxSortingOrder++;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _currentMaxSortingOrder;

            lock (_sync)
            {
                _viewSortingOrders[view] = _currentMaxSortingOrder;
            }
        }

        private void ReleaseSortingOrder(IView view)
        {
            lock (_sync)
            {
                _viewSortingOrders.Remove(view);
                
                if (_viewSortingOrders.Count == 0)
                {
                    _currentMaxSortingOrder = 0;
                }
                else
                {
                    _currentMaxSortingOrder = _viewSortingOrders.Values.Max();
                }
            }
        }

        private async UniTask HideViewInternal(IView view, bool addToHistory)
        {
            _viewAnimator.StopAnimation(view);

            if (addToHistory)
            {
                lock (_sync)
                {
                    if (!_viewHistory.Contains(view))
                    {
                        _viewHistory.Push(view);
                    }
                }
            }
            
            switch (view)
            {
                case IHideable hideable:
                    await hideable.HideAsync();
                    break;
                
                case UnityEngine.Component component:
                    component.gameObject.SetActive(false);
                    break;
            }
        }

        private async UniTask ShowViewInternal(IView view)
        {
            _viewAnimator.StopAnimation(view);

            switch (view)
            {
                case IShowable showable:
                    await showable.ShowAsync();
                    break;

                case UnityEngine.Component component:
                    component.gameObject.SetActive(true);
                    break;
            }

            ThrowIfHiddenWhileShowing(view);
        }

        private async UniTask HideViewInternalWithParams(IView view, bool addToHistory, ViewShowParams showParams)
        {
            _viewAnimator.StopAnimation(view);

            if (showParams.OnHidden != null)
            {
                await showParams.OnHidden.Invoke();
            }

            if (addToHistory)
            {
                lock (_sync)
                {
                    if (!_viewHistory.Contains(view))
                    {
                        _viewHistory.Push(view);
                    }
                }
            }

            if (showParams.TransitionStyle != ViewTransitionStyle.None)
            {
                await _viewAnimator.AnimateHideAsync(view, showParams.TransitionStyle, showParams.AnimationDuration);
            }

            switch (view)
            {
                case IHideable hideable:
                    await hideable.HideAsync();
                    break;

                case UnityEngine.Component component:
                    component.gameObject.SetActive(false);
                    break;
            }
        }

        private async UniTask ShowViewInternalWithParams(IView view, ViewShowParams showParams)
        {
            _viewAnimator.StopAnimation(view);
            await _viewAnimator.AnimateShowAsync(view, showParams.TransitionStyle, showParams.AnimationDuration);

            switch (view)
            {
                case IShowable showable:
                    await showable.ShowAsync();
                    break;

                case UnityEngine.Component component:
                    component.gameObject.SetActive(true);
                    break;
            }

            ThrowIfHiddenWhileShowing(view);
        }

        private void ThrowIfHiddenWhileShowing(IView view)
        {
            lock (_sync)
            {
                if (!_activeViews.Contains(view))
                {
                    throw new OperationCanceledException();
                }
            }
        }
    }
}