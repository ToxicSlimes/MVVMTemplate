using System;
using System.Collections.Generic;
using Core.Abstractions.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI.Controls
{
    public class BaseDropdown : TMP_Dropdown
    {
        private readonly ReactiveProperty<int> _selectedIndex = new();
        private readonly Subject<bool> _onOpenClose = new();
        private DisposableBag _disposables;
        private IControlBehavior _base;
        private bool _suppressSync;

        public ReactiveProperty<int> SelectedIndex => _selectedIndex;
        public Observable<bool> OnOpenClose => _onOpenClose;

        protected override void Awake()
        {
            base.Awake();
            _base = GetComponent<IControlBehavior>();
            _selectedIndex.Value = value;

            onValueChanged.AddListener(OnDropdownChanged);
            _selectedIndex.Subscribe(this, static (val, self) =>
            {
                if (self._suppressSync) return;
                self._suppressSync = true;
                self.value = val;
                self._suppressSync = false;
            }).AddTo(ref _disposables);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _onOpenClose.Dispose();
            _selectedIndex.Dispose();
            _disposables.Dispose();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (!interactable) return;
            _base?.TransitionTo(ControlState.Hovered);
            _base?.PlayHoverEffect();
            _base?.PlaySound(_base.HoverSound);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            _base?.TransitionTo(ControlState.Normal);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            _base?.PlaySound(_base.ClickSound);
            _onOpenClose.OnNext(true);
        }

        protected override void DestroyDropdownList(GameObject dropdownList)
        {
            base.DestroyDropdownList(dropdownList);
            _onOpenClose.OnNext(false);
        }

        public void PopulateFromEnum<TEnum>() where TEnum : struct, Enum
        {
            ClearOptions();
            var names = Enum.GetNames(typeof(TEnum));
            var optionList = new List<OptionData>(names.Length);
            foreach (var name in names)
                optionList.Add(new OptionData(name));
            AddOptions(optionList);
        }

        private void OnDropdownChanged(int val)
        {
            if (_suppressSync) return;
            _suppressSync = true;
            _selectedIndex.Value = val;
            _suppressSync = false;
        }
    }
}
