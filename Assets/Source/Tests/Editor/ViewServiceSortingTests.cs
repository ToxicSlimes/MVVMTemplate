using System;
using System.Reflection;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.Factory;
using Core.Abstractions.MVVM.ViewModels;
using Core.Abstractions.MVVM.Views;
using Core.Presentation.MVVM.Services;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Core.Tests.Infrastructure.MVVM
{
    public sealed class ViewServiceSortingTests
    {
        private GameObject _object;
        private ViewService _service;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("sorting test");
            _service = new ViewService(new Factory(), new Animator());
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_object);

        private void Assign(IView view) => typeof(ViewService)
            .GetMethod("AssignSortingOrder", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(_service, new object[] { view });

        private void Release(IView view) => typeof(ViewService)
            .GetMethod("ReleaseSortingOrder", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(_service, new object[] { view });

#if UNITY_6000_5_OR_NEWER
        [Test]
        public void PanelRenderer_AssignsOrderWithoutAddingCanvas_AndReleaseResetsCounter()
        {
            var view = _object.AddComponent<SortingView>();
            var panel = _object.AddComponent<PanelRenderer>();
            Assign(view);
            Assert.That(panel.sortingOrder, Is.EqualTo(1));
            Assert.That(_object.GetComponent<Canvas>(), Is.Null);
            Assert.That(_object.GetComponent<GraphicRaycaster>(), Is.Null);
            Assign(view);
            Assert.That(panel.sortingOrder, Is.EqualTo(2));
            Release(view);
            Assign(view);
            Assert.That(panel.sortingOrder, Is.EqualTo(1));
        }

        [Test]
        public void MixedViews_ShareCounter_AndReleaseReusesHighestOrder()
        {
            var panelView = _object.AddComponent<SortingView>();
            var panel = _object.AddComponent<PanelRenderer>();
            var other = new GameObject("canvas sorting test");
            try
            {
                var canvasView = other.AddComponent<SortingView>();
                var canvas = other.AddComponent<Canvas>();
                Assign(panelView);
                Assign(canvasView);
                Assert.That(panel.sortingOrder, Is.EqualTo(1));
                Assert.That(canvas.sortingOrder, Is.EqualTo(2));
                Release(canvasView);
                Assign(panelView);
                Assert.That(panel.sortingOrder, Is.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(other);
            }
        }

        [Test]
        public void PanelRenderer_TakesPrecedenceOverExistingCanvas()
        {
            var view = _object.AddComponent<SortingView>();
            var canvas = _object.AddComponent<Canvas>();
            canvas.sortingOrder = 19;
            var panel = _object.AddComponent<PanelRenderer>();
            Assign(view);
            Assert.That(panel.sortingOrder, Is.EqualTo(1));
            Assert.That(canvas.sortingOrder, Is.EqualTo(19));
            Assert.That(canvas.overrideSorting, Is.False);
        }
#endif

        [Test]
        public void UIDocument_TakesPrecedenceOverCanvas()
        {
            var view = _object.AddComponent<SortingView>();
            var canvas = _object.AddComponent<Canvas>();
            canvas.sortingOrder = 19;
            var document = _object.AddComponent<UIDocument>();
            Assign(view);
            Assert.That(document.sortingOrder, Is.EqualTo(1));
            Assert.That(canvas.sortingOrder, Is.EqualTo(19));
        }

        [Test]
        public void ExistingCanvas_AssignsOrderWithoutAddingRaycaster()
        {
            var view = _object.AddComponent<SortingView>();
            var canvas = _object.AddComponent<Canvas>();
            Assign(view);
            Assert.That(canvas.sortingOrder, Is.EqualTo(1));
            Assert.That(canvas.isRootCanvas, Is.True);
            Assert.That(_object.GetComponent<GraphicRaycaster>(), Is.Null);
        }

        [Test]
        public void MissingCanvas_AddsCanvasAndRaycaster()
        {
            var view = _object.AddComponent<SortingView>();
            Assign(view);
            var canvas = _object.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null);
            Assert.That(canvas.isRootCanvas, Is.True);
            Assert.That(canvas.sortingOrder, Is.EqualTo(1));
            Assert.That(_object.GetComponent<GraphicRaycaster>(), Is.Not.Null);
        }

        public sealed class SortingView : MonoBehaviour, IView
        {
            public bool IsPersistent => false;
            public IViewModel ViewModel => null;
            public void Dispose() { }
        }

        private sealed class Factory : IViewFactory
        {
            public UniTask<IView> CreateAsync(Type type, ViewShowParams showParams = null) => throw new NotSupportedException();
            public UniTask<TView> CreateAsync<TView, TViewModel>(Transform parent = null) where TView : BindableView<TViewModel> where TViewModel : DisposableObject => throw new NotSupportedException();
            public UniTask<TView> CreateAsync<TView, TViewModel>(string address, Transform parent = null) where TView : BindableView<TViewModel> where TViewModel : DisposableObject => throw new NotSupportedException();
            public UniTask<TView> CreateAsync<TView, TViewModel>(ViewShowParams showParams) where TView : BindableView<TViewModel> where TViewModel : DisposableObject => throw new NotSupportedException();
            public void Destroy(IView view) => throw new NotSupportedException();
        }

        private sealed class Animator : IViewAnimator
        {
            public UniTask AnimateShowAsync(IView view, ViewTransitionStyle style, float? duration = null) => UniTask.CompletedTask;
            public UniTask AnimateHideAsync(IView view, ViewTransitionStyle style, float? duration = null) => UniTask.CompletedTask;
            public void StopAnimation(IView view) { }
        }
    }
}
