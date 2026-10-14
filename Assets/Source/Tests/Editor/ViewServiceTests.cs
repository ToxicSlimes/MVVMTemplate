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

        private sealed class FirstViewModel : DisposableObject
        {
        }

        private sealed class SecondViewModel : DisposableObject
        {
        }

        private sealed class FakeView : IView, IShowable, IHideable
        {
            public bool IsPersistent => false;
            public IViewModel ViewModel { get; }
            public int HideCount { get; private set; }

            public FakeView(IViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public UniTask ShowAsync() => UniTask.CompletedTask;

            public UniTask HideAsync()
            {
                HideCount++;
                return UniTask.CompletedTask;
            }

            public void Dispose()
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
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public void Destroy(IView view) => Destroyed.Add(view);
        }

        private sealed class FakeViewAnimator : IViewAnimator
        {
            private readonly UniTaskCompletionSource _hideAnimation;

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
                float? duration = null) => _hideAnimation?.Task ?? UniTask.CompletedTask;

            public void StopAnimation(IView view)
            {
            }
        }
    }
}
