using Core.Abstractions.MVVM.ViewModels;
using Core.Presentation.MVVM.Animation;
using Core.UI.Binding;
using Core.UI.Controls;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.MVVM.Views
{
    public abstract class PanelView<TViewModel> : AnimatedView<TViewModel> where TViewModel : DisposableObject
    {
        protected sealed override void OnBind(ref DisposableBag disposables)
        {
            BindPanel(ref disposables);
        }

        protected abstract void BindPanel(ref DisposableBag disposables);

        public override async UniTask ShowAsync()
        {
            SetViewModelVisible(true);
            await base.ShowAsync();
        }

        public override async UniTask HideAsync()
        {
            SetViewModelVisible(false);
            await base.HideAsync();
        }

        protected static void Bind(BaseText target, ReadOnlyReactiveProperty<string> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind(BaseText target, ReadOnlyReactiveProperty<int> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind<T>(BaseText target, ReadOnlyReactiveProperty<T> source, string format, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, format, ref bag);
        }

        protected static void Bind(BaseButton target, ReactiveCommand<Unit> command, ref DisposableBag bag)
        {
            if (target == null || command == null) return;
            target.BindTo(command, ref bag);
        }

        protected static void Bind(BaseInputField target, ReactiveProperty<string> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind(BaseToggle target, ReactiveProperty<bool> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void BindText(BaseText target, string value)
        {
            if (target == null) return;
            target.SetText(value);
        }

        protected static void BindActive(GameObject target, ReadOnlyReactiveProperty<bool> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;

            source.Subscribe(target, static (visible, go) => go.SetActive(visible))
                .AddTo(ref bag);
        }

        protected static void BindActive(Component target, ReadOnlyReactiveProperty<bool> source, ref DisposableBag bag)
        {
            if (target == null) return;
            BindActive(target.gameObject, source, ref bag);
        }

        protected static void BindInteractable(BaseButton target, ReadOnlyReactiveProperty<bool> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;

            source.Subscribe(target, static (value, button) => button.interactable = value)
                .AddTo(ref bag);
        }

        protected static void BindFill(Image target, ReadOnlyReactiveProperty<float> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;

            source.Subscribe(target, static (value, image) => image.fillAmount = Mathf.Clamp01(value))
                .AddTo(ref bag);
        }

        protected static void BindColor(Graphic target, ReadOnlyReactiveProperty<Color> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;

            source.Subscribe(target, static (value, graphic) => graphic.color = value)
                .AddTo(ref bag);
        }

        private void SetViewModelVisible(bool value)
        {
            if (ViewModel is IVisibilityAware visibilityAware)
            {
                visibilityAware.IsVisible.Value = value;
            }
        }
    }
}
