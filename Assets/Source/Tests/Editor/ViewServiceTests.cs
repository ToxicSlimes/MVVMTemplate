using System;
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
            private readonly IView _first;
            private readonly IView _second;

            public FakeViewFactory(IView first, IView second)
            {
                _first = first;
                _second = second;
            }

            public UniTask<IView> CreateAsync(Type viewModelType, ViewShowParams showParams = null)
            {
                var view = viewModelType == typeof(FirstViewModel) ? _first : _second;
                return UniTask.FromResult(view);
            }

            public UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams)
                where TView : BindableView<TViewModel>
                where TViewModel : DisposableObject => throw new NotSupportedException();

            public void Destroy(IView view)
            {
            }
        }

        private sealed class FakeViewAnimator : IViewAnimator
        {
            public UniTask AnimateShowAsync(
                IView view,
                ViewTransitionStyle transitionStyle,
                float? duration = null) => UniTask.CompletedTask;

            public UniTask AnimateHideAsync(
                IView view,
                ViewTransitionStyle transitionStyle,
                float? duration = null) => UniTask.CompletedTask;

            public void StopAnimation(IView view)
            {
            }
        }
    }
}
