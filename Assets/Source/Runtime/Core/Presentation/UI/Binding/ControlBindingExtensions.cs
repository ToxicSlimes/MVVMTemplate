using System;
using System.Collections.Generic;
using Core.UI.Controls;
using R3;
using TMPro;

namespace Core.UI.Binding
{
    public static class ControlBindingExtensions
    {
        public static void BindTo(this BaseButton button, ReactiveCommand<Unit> command, ref DisposableBag bag)
        {
            if (button == null) return;
            button.onClick.AsObservable()
                .Subscribe(command, static (_, cmd) => cmd.Execute(Unit.Default))
                .AddTo(ref bag);
        }

        public static void BindTo(this BaseToggle toggle, ReactiveProperty<bool> property, ref DisposableBag bag)
        {
            if (toggle == null) return;
            property.Subscribe(toggle.Value, static (v, cv) =>
            {
                if (cv.Value != v) cv.Value = v;
            }).AddTo(ref bag);
            toggle.Value.Subscribe(property, static (v, prop) =>
            {
                if (prop.Value != v) prop.Value = v;
            }).AddTo(ref bag);
        }

        public static void BindTo(this BaseSlider slider, ReactiveProperty<float> property, ref DisposableBag bag)
        {
            if (slider == null) return;
            property.Subscribe(slider.Value, static (v, cv) =>
            {
                if (Math.Abs(cv.Value - v) > 1e-6f) cv.Value = v;
            }).AddTo(ref bag);
            slider.Value.Subscribe(property, static (v, prop) =>
            {
                if (Math.Abs(prop.Value - v) > 1e-6f) prop.Value = v;
            }).AddTo(ref bag);
        }

        public static void BindTo(this BaseSlider slider, ReactiveProperty<float> property, TMP_Text label, string format, ref DisposableBag bag)
        {
            if (slider == null) return;
            var sliderValue = slider.Value;
            property.Subscribe((sliderValue, label, format), static (v, state) =>
            {
                if (Math.Abs(state.sliderValue.Value - v) > 1e-6f) state.sliderValue.Value = v;
                if (state.label != null) state.label.SetText(string.Format(state.format, v));
            }).AddTo(ref bag);
            slider.Value.Subscribe(property, static (v, prop) =>
            {
                if (Math.Abs(prop.Value - v) > 1e-6f) prop.Value = v;
            }).AddTo(ref bag);
        }

        public static void BindTo(this BaseDropdown dropdown, ReactiveProperty<int> property, ref DisposableBag bag)
        {
            if (dropdown == null) return;
            property.Subscribe(dropdown.SelectedIndex, static (v, cv) =>
            {
                if (cv.Value != v) cv.Value = v;
            }).AddTo(ref bag);
            dropdown.SelectedIndex.Subscribe(property, static (v, prop) =>
            {
                if (prop.Value != v) prop.Value = v;
            }).AddTo(ref bag);
        }

        public static void BindToEnum<TEnum>(this BaseDropdown dropdown, ReactiveProperty<TEnum> property, ref DisposableBag bag) where TEnum : struct, Enum
        {
            if (dropdown == null) return;
            dropdown.PopulateFromEnum<TEnum>();
            property.Subscribe(dropdown.SelectedIndex, static (v, cv) =>
            {
                var index = Convert.ToInt32(v);
                if (cv.Value != index) cv.Value = index;
            }).AddTo(ref bag);
            dropdown.SelectedIndex.Subscribe(property, static (v, prop) =>
            {
                var enumVal = (TEnum)Enum.ToObject(typeof(TEnum), v);
                if (!EqualityComparer<TEnum>.Default.Equals(prop.Value, enumVal)) prop.Value = enumVal;
            }).AddTo(ref bag);
        }

        public static void BindTo(this BaseInputField input, ReactiveProperty<string> property, ref DisposableBag bag)
        {
            if (input == null) return;
            property.Subscribe(input.Text, static (v, cv) =>
            {
                if (cv.Value != v) cv.Value = v;
            }).AddTo(ref bag);
            input.Text.Subscribe(property, static (v, prop) =>
            {
                if (prop.Value != v) prop.Value = v;
            }).AddTo(ref bag);
        }

        public static void BindTo(this BaseText text, ReadOnlyReactiveProperty<int> property, ref DisposableBag bag)
        {
            if (text == null) return;
            property.Subscribe(text, static (v, t) => t.SetText("{0}", v)).AddTo(ref bag);
        }

        public static void BindTo(this BaseText text, ReadOnlyReactiveProperty<float> property, ref DisposableBag bag)
        {
            if (text == null) return;
            property.Subscribe(text, static (v, t) => t.SetText("{0}", v)).AddTo(ref bag);
        }

        public static void BindTo(this BaseText text, ReadOnlyReactiveProperty<string> property, ref DisposableBag bag)
        {
            if (text == null) return;
            property.Subscribe(text, static (v, t) => t.SetText(v)).AddTo(ref bag);
        }

        public static void BindTo<T>(this BaseText text, ReadOnlyReactiveProperty<T> property, ref DisposableBag bag)
        {
            if (text == null) return;
            property.Subscribe(text, static (v, t) => t.SetText(v?.ToString() ?? string.Empty)).AddTo(ref bag);
        }

        public static void BindTo<T>(this BaseText text, ReadOnlyReactiveProperty<T> property, string format, ref DisposableBag bag)
        {
            if (text == null) return;
            property.Subscribe((text, format), static (v, state) => state.text.SetText(string.Format(state.format, v))).AddTo(ref bag);
        }
    }
}
