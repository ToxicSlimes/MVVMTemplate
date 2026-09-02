using Core.Abstractions.MVVM.ViewModels;
using Core.UI.Toolkit.Binding;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit
{
    public abstract class ToolkitPanelView<TViewModel> : ToolkitBindableView<TViewModel>
        where TViewModel : DisposableObject
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

        protected static void Bind(Label target, ReadOnlyReactiveProperty<string> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind(Label target, ReadOnlyReactiveProperty<int> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind<T>(Label target, ReadOnlyReactiveProperty<T> source, string format, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, format, ref bag);
        }

        protected static void Bind(Button target, ReactiveCommand<Unit> command, ref DisposableBag bag)
        {
            if (target == null || command == null) return;
            target.BindTo(command, ref bag);
        }

        protected static void Bind(TextField target, ReactiveProperty<string> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind(Toggle target, ReactiveProperty<bool> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind(Slider target, ReactiveProperty<float> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void Bind(
            Slider target,
            ReactiveProperty<float> source,
            Label valueLabel,
            string format,
            ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, valueLabel, format, ref bag);
        }

        protected static void Bind(DropdownField target, ReactiveProperty<int> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindTo(source, ref bag);
        }

        protected static void BindEnum<TEnum>(DropdownField target, ReactiveProperty<TEnum> source, ref DisposableBag bag)
            where TEnum : struct, System.Enum
        {
            if (target == null || source == null) return;
            target.BindToEnum(source, ref bag);
        }

        protected static void BindDisplay(VisualElement target, ReadOnlyReactiveProperty<bool> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindDisplay(source, ref bag);
        }

        protected static void BindEnabled(Button target, ReadOnlyReactiveProperty<bool> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindEnabledFrom(source, ref bag);
        }

        protected static void BindFill(VisualElement target, ReadOnlyReactiveProperty<float> source, ref DisposableBag bag)
        {
            if (target == null || source == null) return;
            target.BindFill(source, ref bag);
        }

        private void SetViewModelVisible(bool value)
        {
            if (ViewModel is IVisibilityAware visibilityAware)
                visibilityAware.IsVisible.Value = value;
        }
    }
}
