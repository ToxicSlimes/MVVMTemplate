#if UNITY_6000_5_OR_NEWER
using System.Collections;
using Core.Abstractions.MVVM.ViewModels;
using Core.UI.Toolkit;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Core.Tests.Toolkit
{
    public sealed class ToolkitPanelRendererTests
    {
        public sealed class TestModel : DisposableObject { }

        public sealed class TestView : ToolkitBindableView<TestModel>
        {
            public VisualElement BoundRoot;
            public int BindCount;
            public UniTaskCompletionSource ShowGate;
            public bool AfterShow;
            public VisualElement CurrentRoot => Root;
            public void DisableForTest() => base.OnDisable();
            public void EnableForTest() => base.OnEnable();
            protected override UniTask OnBeforeShow() => ShowGate?.Task ?? UniTask.CompletedTask;
            protected override UniTask OnAfterShow() { AfterShow = true; return UniTask.CompletedTask; }
            protected override void OnBind(ref DisposableBag disposables)
            {
                BoundRoot = Root;
                BindCount++;
            }
        }

        [Test]
        public void Dispose_WithPendingBinding_PreventsFurtherBinding()
        {
            var gameObject = new GameObject("Disposed view test");
            try
            {
                var view = gameObject.AddComponent<TestView>();
                view.BindTo(new TestModel());
                view.Dispose();
                Assert.That(view.ViewModel, Is.Null);
                Assert.Throws<System.ObjectDisposedException>(() => view.BindTo(new TestModel()));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void BindingHandle_Disposal_OnlyUnbindsItsOwnGeneration()
        {
            var gameObject = new GameObject("Binding handle test");
            try
            {
                var view = gameObject.AddComponent<TestView>();
                var first = view.BindTo(new TestModel());
                var current = new TestModel();
                var second = view.BindTo(current);
                first.Dispose();
                Assert.That(view.ViewModel, Is.SameAs(current));
                second.Dispose();
                Assert.That(view.ViewModel, Is.Null);
            }
            finally { Object.DestroyImmediate(gameObject); }
        }

        [Test]
        public void BindingHandle_RemainsEffectiveAfterReenable()
        {
            var gameObject = new GameObject("Binding reenable test");
            try
            {
                gameObject.AddComponent<UIDocument>();
                var view = gameObject.AddComponent<TestView>();
                var handle = view.BindTo(new TestModel());
                var initialCount = view.BindCount;
                Assert.That(initialCount, Is.GreaterThan(0));
                view.DisableForTest();
                view.EnableForTest();
                Assert.That(view.BindCount, Is.EqualTo(initialCount + 1));
                handle.Dispose();
                Assert.That(view.ViewModel, Is.Null);
                view.DisableForTest();
                view.EnableForTest();
                Assert.That(view.BindCount, Is.EqualTo(initialCount + 1));
            }
            finally { Object.DestroyImmediate(gameObject); }
        }

        [Test]
        public void OnDisable_WithBothHosts_RetainsDocumentRoot()
        {
            var gameObject = new GameObject("Both hosts test");
            try
            {
                var document = gameObject.AddComponent<UIDocument>();
                var renderer = gameObject.AddComponent<PanelRenderer>();
                var view = gameObject.AddComponent<TestView>();
                typeof(ToolkitBindableView<TestModel>).GetField("_panelRenderer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(view, renderer);
                view.BindTo(new TestModel());
                Assert.That(view.CurrentRoot, Is.SameAs(document.rootVisualElement));
                Assert.That(view.CurrentRoot, Is.Not.Null);
                view.DisableForTest();
                Assert.That(view.CurrentRoot, Is.SameAs(document.rootVisualElement));
            }
            finally { Object.DestroyImmediate(gameObject); }
        }

        [UnityTest]
        public IEnumerator OnDisable_CancelsPendingShow()
        {
            var gameObject = new GameObject("Pending show test");
            try
            {
                var view = gameObject.AddComponent<TestView>();
                view.ShowGate = new UniTaskCompletionSource();
                var task = view.ShowAsync();
                view.DisableForTest();
                view.ShowGate.TrySetResult();
                yield return task.ToCoroutine();
                Assert.That(view.AfterShow, Is.False);
            }
            finally { Object.DestroyImmediate(gameObject); }
        }

        [UnityTest]
        public IEnumerator BindTo_WithPanelRenderer_BindsToRendererRoot()
        {
            var gameObject = new GameObject("PanelRenderer test");
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            try
            {
                gameObject.SetActive(false);
                var renderer = gameObject.AddComponent<PanelRenderer>();
                renderer.panelSettings = settings;
                renderer.visualTreeAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.toxicslimes.mvvm-template/UI/Toolkit/MainMenu/MainMenuScreen.uxml");
                Assert.That(renderer.visualTreeAsset, Is.Not.Null);
                var view = gameObject.AddComponent<TestView>();
                view.BindTo(new TestModel());
                for (var i = 0; i < 10 && view.BoundRoot == null; i++)
                    yield return null;
                Assert.That(view.BoundRoot, Is.Not.Null, "PanelRenderer root must be bound");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(settings);
            }
        }
    }
}
#endif
