using System;
using System.Collections.Generic;
using Core.Abstractions.Services;
using Core.Domain.Enums;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Toolkit.Binding
{
    public static class ToolkitBindingExtensions
    {
        public static void BindTo(this Button button, ReactiveCommand<Unit> command, ref DisposableBag bag)
        {
            if (button == null || command == null) return;

            button.clicked += OnClick;
            bag.Add(Disposable.Create(() => button.clicked -= OnClick));
            return;

            void OnClick() => command.Execute(Unit.Default);
        }

        public static void BindTo(this Label label, ReadOnlyReactiveProperty<string> property, ref DisposableBag bag)
        {
            if (label == null || property == null) return;

            property.Subscribe(label, static (value, target) => target.text = value ?? string.Empty)
                .AddTo(ref bag);
        }

        public static void BindTo(this Label label, ReadOnlyReactiveProperty<int> property, ref DisposableBag bag)
        {
            if (label == null || property == null) return;

            property.Subscribe(label, static (value, target) => target.text = value.ToString())
                .AddTo(ref bag);
        }

        public static void BindTo<T>(this Label label, ReadOnlyReactiveProperty<T> property, string format, ref DisposableBag bag)
        {
            if (label == null || property == null) return;

            property.Subscribe((label, format), static (value, state) =>
                    state.label.text = string.Format(state.format, value))
                .AddTo(ref bag);
        }

        public static void BindEnabledFrom(this Button button, ReadOnlyReactiveProperty<bool> property, ref DisposableBag bag)
        {
            if (button == null || property == null) return;

            property.Subscribe(button, static (enabled, target) => target.SetEnabled(enabled))
                .AddTo(ref bag);
        }

        public static void BindDisplay(this VisualElement element, ReadOnlyReactiveProperty<bool> property, ref DisposableBag bag)
        {
            if (element == null || property == null) return;

            property.Subscribe(element, static (visible, target) =>
                    target.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None)
                .AddTo(ref bag);
        }

        public static void BindTo(this TextField field, ReactiveProperty<string> property, ref DisposableBag bag)
        {
            if (field == null || property == null) return;

            field.SetValueWithoutNotify(property.Value ?? string.Empty);

            void OnChanged(ChangeEvent<string> evt) => property.Value = evt.newValue;
            field.RegisterValueChangedCallback(OnChanged);
            bag.Add(Disposable.Create(() => field.UnregisterValueChangedCallback(OnChanged)));

            property.Subscribe(field, static (value, target) => target.SetValueWithoutNotify(value ?? string.Empty))
                .AddTo(ref bag);
        }

        public static void BindTo(this Toggle toggle, ReactiveProperty<bool> property, ref DisposableBag bag)
        {
            if (toggle == null || property == null) return;

            toggle.SetValueWithoutNotify(property.Value);

            void OnChanged(ChangeEvent<bool> evt) => property.Value = evt.newValue;
            toggle.RegisterValueChangedCallback(OnChanged);
            bag.Add(Disposable.Create(() => toggle.UnregisterValueChangedCallback(OnChanged)));

            property.Subscribe(toggle, static (value, target) => target.SetValueWithoutNotify(value))
                .AddTo(ref bag);
        }

        public static void BindTo(this Slider slider, ReactiveProperty<float> property, ref DisposableBag bag)
        {
            if (slider == null || property == null) return;

            slider.SetValueWithoutNotify(property.Value);

            void OnChanged(ChangeEvent<float> evt) => property.Value = evt.newValue;
            slider.RegisterValueChangedCallback(OnChanged);
            bag.Add(Disposable.Create(() => slider.UnregisterValueChangedCallback(OnChanged)));

            property.Subscribe(slider, static (value, target) => target.SetValueWithoutNotify(value))
                .AddTo(ref bag);
        }

        public static void BindTo(
            this Slider slider,
            ReactiveProperty<float> property,
            Label valueLabel,
            string format,
            ref DisposableBag bag)
        {
            slider.BindTo(property, ref bag);
            if (valueLabel == null) return;

            property.Subscribe((valueLabel, format), static (value, state) =>
                    state.valueLabel.text = string.Format(state.format, value))
                .AddTo(ref bag);
        }

        public static void BindTo(this DropdownField dropdown, ReactiveProperty<int> property, ref DisposableBag bag)
        {
            if (dropdown == null || property == null) return;

            dropdown.index = property.Value;

            void OnChanged(ChangeEvent<string> evt)
            {
                var index = dropdown.choices.IndexOf(evt.newValue);
                if (index >= 0)
                    property.Value = index;
            }

            dropdown.RegisterValueChangedCallback(OnChanged);
            bag.Add(Disposable.Create(() => dropdown.UnregisterValueChangedCallback(OnChanged)));

            property.Subscribe(dropdown, static (value, target) =>
            {
                if (value >= 0 && value < target.choices.Count)
                    target.index = value;
            }).AddTo(ref bag);
        }

        public static void BindToEnum<TEnum>(this DropdownField dropdown, ReactiveProperty<TEnum> property, ref DisposableBag bag)
            where TEnum : struct, Enum
        {
            if (dropdown == null || property == null) return;

            dropdown.choices = new List<string>(Enum.GetNames(typeof(TEnum)));
            dropdown.index = Convert.ToInt32(property.Value);

            void OnChanged(ChangeEvent<string> evt)
            {
                var index = dropdown.choices.IndexOf(evt.newValue);
                if (index >= 0 && Enum.IsDefined(typeof(TEnum), index))
                    property.Value = (TEnum)Enum.ToObject(typeof(TEnum), index);
            }

            dropdown.RegisterValueChangedCallback(OnChanged);
            bag.Add(Disposable.Create(() => dropdown.UnregisterValueChangedCallback(OnChanged)));

            property.Subscribe(dropdown, static (value, target) => target.index = Convert.ToInt32(value))
                .AddTo(ref bag);
        }

        public static void BindFill(this VisualElement fill, ReadOnlyReactiveProperty<float> property, ref DisposableBag bag)
        {
            if (fill == null || property == null) return;

            property.Subscribe(fill, static (value, target) =>
                    target.style.width = new Length(Mathf.Clamp01(value) * 100f, LengthUnit.Percent))
                .AddTo(ref bag);
        }

        public static void BindLocalized(
            this Label label,
            ILocalizationService localization,
            string table,
            string key,
            ref DisposableBag bag)
        {
            if (label == null || localization == null) return;

            void Refresh() => label.text = localization.Localize(table, key);

            Refresh();
            Action<GameLanguage> onLanguageChanged = _ => Refresh();
            localization.LanguageChanged += onLanguageChanged;
            bag.Add(Disposable.Create(() => localization.LanguageChanged -= onLanguageChanged));
        }

        public static void BindLocalized(
            this Label label,
            ILocalizationService localization,
            string table,
            string key,
            Func<object[]> argumentFactory,
            ref DisposableBag bag)
        {
            if (label == null || localization == null || argumentFactory == null) return;

            void Refresh()
            {
                var args = argumentFactory();
                label.text = args == null || args.Length == 0
                    ? localization.Localize(table, key)
                    : localization.Localize(table, key, args);
            }

            Refresh();
            Action<GameLanguage> onLanguageChanged = _ => Refresh();
            localization.LanguageChanged += onLanguageChanged;
            bag.Add(Disposable.Create(() => localization.LanguageChanged -= onLanguageChanged));
        }

        public static void BindLocalized(
            this Button button,
            ILocalizationService localization,
            string table,
            string key,
            ref DisposableBag bag)
        {
            if (button == null || localization == null) return;

            void Refresh() => button.text = localization.Localize(table, key);

            Refresh();
            Action<GameLanguage> onLanguageChanged = _ => Refresh();
            localization.LanguageChanged += onLanguageChanged;
            bag.Add(Disposable.Create(() => localization.LanguageChanged -= onLanguageChanged));
        }

        public static void BindLocalizedLabel(
            this Slider slider,
            ILocalizationService localization,
            string table,
            string key,
            ref DisposableBag bag)
        {
            if (slider == null || localization == null) return;

            void Refresh() => slider.label = localization.Localize(table, key);

            Refresh();
            Action<GameLanguage> onLanguageChanged = _ => Refresh();
            localization.LanguageChanged += onLanguageChanged;
            bag.Add(Disposable.Create(() => localization.LanguageChanged -= onLanguageChanged));
        }

        public static void BindLocalizedLabel(
            this DropdownField dropdown,
            ILocalizationService localization,
            string table,
            string key,
            ref DisposableBag bag)
        {
            if (dropdown == null || localization == null) return;

            void Refresh() => dropdown.label = localization.Localize(table, key);

            Refresh();
            Action<GameLanguage> onLanguageChanged = _ => Refresh();
            localization.LanguageChanged += onLanguageChanged;
            bag.Add(Disposable.Create(() => localization.LanguageChanged -= onLanguageChanged));
        }
    }
}
