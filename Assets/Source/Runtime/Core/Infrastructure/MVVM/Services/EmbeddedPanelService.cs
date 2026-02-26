using System;
using System.Collections.Generic;
using System.Threading;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.Infrastructure.Mediator.Abstractions;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Core.MVVM.Hosts;
using Core.Application.Notifications;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Infrastructure.MVVM.Services
{
    public class EmbeddedPanelService : IEmbeddedPanelHost
    {
        private readonly Container _container;
        private readonly IViewAnimator _viewAnimator;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        private readonly Dictionary<Type, EmbeddedPanelInfo> _panels = new();

        private const float AnimationDuration = 0.3f;

        public EmbeddedPanelService(
            Container container,
            IViewAnimator viewAnimator,
            IMediator mediator,
            ILogger logger)
        {
            _container = container;
            _viewAnimator = viewAnimator;
            _mediator = mediator;
            _logger = logger;
        }

        public void RegisterPanel<TViewModel>(BindableView<TViewModel> view)
            where TViewModel : DisposableObject
        {
            var vmType = typeof(TViewModel);

            if (_panels.ContainsKey(vmType))
            {
                _logger.Warn("[EmbeddedPanelService] Panel already registered: {0}", vmType.Name);
                return;
            }

            view.gameObject.SetActive(false);

            var viewModel = _container.Resolve<TViewModel>();

            _panels[vmType] = new EmbeddedPanelInfo
            {
                View = view,
                GameObject = view.gameObject,
                ViewModel = viewModel,
                IsVisible = false,
                IsBound = false
            };

            _logger.Info("[EmbeddedPanelService] Registered panel: {0}", vmType.Name);
        }

        public async UniTask<bool> ShowEmbeddedAsync<TViewModel>(
            IViewContext context,
            CancellationToken ct = default)
            where TViewModel : DisposableObject
        {
            var vmType = typeof(TViewModel);

            if (!_panels.TryGetValue(vmType, out var panel))
            {
                _logger.Error("[EmbeddedPanelService] Panel not registered: {0}", vmType.Name);
                return false;
            }

            if (panel.IsVisible)
            {
                ApplyContext(panel.ViewModel, context);
                return true;
            }

            BindPanelIfNeeded<TViewModel>(panel);
            ApplyContext(panel.ViewModel, context);

            panel.GameObject.SetActive(true);
            panel.IsVisible = true;

            var transition = GetTransitionStyle(panel.ViewModel);
            await _viewAnimator.AnimateShowAsync(panel.View, transition, AnimationDuration);

            await _mediator.Publish(new EmbeddedPanelStateChangedNotification(vmType, true), ct);

            return true;
        }

        public async UniTask HideEmbeddedAsync<TViewModel>(CancellationToken ct = default)
            where TViewModel : DisposableObject
        {
            var vmType = typeof(TViewModel);

            if (!_panels.TryGetValue(vmType, out var panel) || !panel.IsVisible)
                return;

            panel.IsVisible = false;

            var transition = GetTransitionStyle(panel.ViewModel);
            await _viewAnimator.AnimateHideAsync(panel.View, transition, AnimationDuration);

            panel.GameObject.SetActive(false);

            await _mediator.Publish(new EmbeddedPanelStateChangedNotification(vmType, false), ct);
        }

        public bool IsEmbeddedVisible<TViewModel>()
            where TViewModel : DisposableObject
        {
            return _panels.TryGetValue(typeof(TViewModel), out var panel) && panel.IsVisible;
        }

        public TViewModel GetEmbeddedViewModel<TViewModel>()
            where TViewModel : DisposableObject
        {
            return _panels.TryGetValue(typeof(TViewModel), out var panel)
                ? (TViewModel)panel.ViewModel
                : null;
        }

        private void BindPanelIfNeeded<TViewModel>(EmbeddedPanelInfo panel)
            where TViewModel : DisposableObject
        {
            if (panel.IsBound || panel.View == null || panel.ViewModel == null)
                return;

            if (panel.View is BindableView<TViewModel> bindableView)
            {
                bindableView.BindTo((TViewModel)panel.ViewModel);
                panel.IsBound = true;
            }
        }

        private void ApplyContext(DisposableObject viewModel, IViewContext context)
        {
            if (context is EmptyContext || viewModel == null)
                return;

            if (viewModel is IContextApplicable applicable)
                applicable.ApplyContext(context);
        }

        private static ViewTransitionStyle GetTransitionStyle(DisposableObject viewModel)
        {
            return viewModel is IPanelTransitionProvider provider
                ? provider.TransitionStyle
                : ViewTransitionStyle.SlideLeft | ViewTransitionStyle.Fade;
        }

        private class EmbeddedPanelInfo
        {
            public IView View;
            public GameObject GameObject;
            public DisposableObject ViewModel;
            public bool IsVisible;
            public bool IsBound;
        }
    }
}
