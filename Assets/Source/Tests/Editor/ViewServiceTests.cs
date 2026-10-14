using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.Factory;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.Presentation.MVVM.Services;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using R3;
using UnityEngine;

namespace Core.Tests.Infrastructure.MVVM
{
    public sealed class ViewServiceTests
    {
        [Test]
        public async Task GoBackAsync_HidesCurrentViewExactlyOnce()
        {
            var first = new FakeView(new FirstViewModel());
            var second = new FakeView(new SecondViewModel());
            var service = new ViewService(new FakeViewFactory(first, second), new FakeViewAnimator());

            await service.ShowAsync<FirstViewModel>();
            await service.ShowAsync<SecondViewModel>();
            await service.GoBackAsync();

            Assert.That(second.HideCount, Is.EqualTo(1));
            Assert.That(service.CurrentView, Is.SameAs(first));
        }

        [Test]
        public async Task ShowAsync_WhileSameViewIsHiding_KeepsNewViewAndDestroysOldOnce()
        {
            var hiding = new FakeView(new FirstViewModel());
            var shown = new FakeView(new FirstViewModel());
            var factory = new FakeViewFactory(hiding, shown);
            var hideAnimation = new UniTaskCompletionSource();
            var service = new ViewService(factory, new FakeViewAnimator(hideAnimation));

            await service.ShowAsync<FirstViewModel>();
            var firstHide = service.HideViewAsync<FirstViewModel>();
            var secondHide = service.HideViewAsync<FirstViewModel>();
            await service.ShowAsync<FirstViewModel>();
            hideAnimation.TrySetResult();
            await UniTask.WhenAll(firstHide, secondHide);

            Assert.That(service.CurrentView, Is.SameAs(shown));
            Assert.That(service.ActiveViews, Is.EquivalentTo(new[] { shown }));
            Assert.That(factory.Destroyed, Is.EqualTo(new[] { hiding }));
        }

        [Test]
        public async Task ShowAsync_WhilePersistentViewIsHiding_CancelsTheHide()
        {
            var view = new FakeView(new FirstViewModel(), isPersistent: true);
            var hideAnimation = new UniTaskCompletionSource();
            var service = new ViewService(new FakeViewFactory(view), new FakeViewAnimator(hideAnimation));

            await service.ShowAsync<FirstViewModel>();
            var hide = service.HideViewAsync<FirstViewModel>();
            await service.ShowAsync<FirstViewModel>();
            hideAnimation.TrySetResult();

            Assert.That(hide.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(view.HideCount, Is.Zero);
            Assert.That(service.CurrentView, Is.SameAs(view));
        }

        [Test]
        public async Task HideViewAsync_WhileViewIsShowing_HidesItAndCancelsTheShow()
        {
            var showAnimation = new UniTaskCompletionSource();
            var view = new FakeView(new FirstViewModel(), showAnimation: showAnimation);
            var factory = new FakeViewFactory(view);
            var service = new ViewService(factory, new FakeViewAnimator());

            var show = service.ShowAsync<FirstViewModel>();
            var hidden = await service.HideViewAsync<FirstViewModel>();
            showAnimation.TrySetResult();

            Assert.That(hidden, Is.True);
            Assert.That(show.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(service.CurrentView, Is.Null);
            Assert.That(factory.Destroyed, Is.EqualTo(new[] { view }));
        }

        [Test]
        public async Task ShowAsync_TypedViewAfterHide_CreatesNewViewTrackedByViewModelType()
        {
            var hidden = CreateBindableView();
            var shown = CreateBindableView();
            var service = new ViewService(new FakeViewFactory(hidden, shown), new FakeViewAnimator());

            await service.ShowAsync<FakeBindableView, FirstViewModel>();
            await service.HideAsync(hidden);
            var view = await service.ShowAsync<FakeBindableView, FirstViewModel>();

            Assert.That(view, Is.SameAs(shown));
            Assert.That(service.GetViewModel<FirstViewModel>(), Is.SameAs(shown.ViewModel));
            Assert.That(await service.HideViewAsync<FirstViewModel>(), Is.True);
        }

        private static FakeBindableView CreateBindableView()
        {
            var view = new GameObject(nameof(FakeBindableView)).AddComponent<FakeBindableView>();
            view.BindTo(new FirstViewModel());
            return view;
        }

        private sealed class FirstViewModel : DisposableObject
        {
        }

        private sealed class SecondViewModel : DisposableObject
        {
        }

        private sealed class FakeView : IView, IShowable, IHideable
        {
            private readonly UniTaskCompletionSource _showAnimation;

            public bool IsPersistent { get; }
            public IViewModel ViewModel { get; }
            public int HideCount { get; private set; }

            public FakeView(IViewModel viewModel, bool isPersistent = false, UniTaskCompletionSource showAnimation = null)
            {
                ViewModel = viewModel;
                IsPersistent = isPersistent;
                _showAnimation = showAnimation;
            }

            public UniTask ShowAsync() => _showAnimation?.Task ?? UniTask.CompletedTask;

            public UniTask HideAsync()
            {
                HideCount++;
                return UniTask.CompletedTask;
            }

            public void Dispose()
            {
            }
        }

        private sealed class FakeBindableView : BindableView<FirstViewModel>
        {
            protected override void OnBind(ref DisposableBag disposables)
            {
            }
        }

        private sealed class FakeViewFactory : IViewFactory
        {
            private readonly Queue<IView> _views;

            public List<IView> Destroyed { get; } = new();

            public FakeViewFactory(params IView[] views)
            {
                _views = new Queue<IView>(views);
            }

            public UniTask<IView> CreateAsync(Type viewModelType, ViewShowParams showParams = null) =>
                UniTask.FromResult(_views.Dequeue());

            public UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => UniTask.FromResult((TView)_views.Dequeue());

            public UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public void Destroy(IView view)
            {
                Destroyed.Add(view);
                if (view is Component component)
                {
                    UnityEngine.Object.DestroyImmediate(component.gameObject);
                }
            }
        }

        private sealed class FakeViewAnimator : IViewAnimator
        {
            private readonly UniTaskCompletionSource _hideAnimation;
            private IView _hidingView;

            public FakeViewAnimator(UniTaskCompletionSource hideAnimation = null)
            {
                _hideAnimation = hideAnimation;
            }

            public UniTask AnimateShowAsync(
                IView view,
                ViewTransitionStyle transitionStyle,
                float? duration = null) => UniTask.CompletedTask;

            public UniTask AnimateHideAsync(
                IView view,
                ViewTransitionStyle transitionStyle,
                float? duration = null)
            {
                _hidingView = view;
                return _hideAnimation?.Task ?? UniTask.CompletedTask;
            }

            public void StopAnimation(IView view)
            {
                if (ReferenceEquals(view, _hidingView))
                {
                    _hideAnimation?.TrySetCanceled();
                }
            }
        }
    }
}
