using System;
using Core.Abstractions.MVVM.Elements;
using Core.Data.UI;
using R3;
using Core.Abstractions.Popup;
using Core.Data.Domain.Models.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Abstractions.MVVM.Elements
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BindableElement<TData> : MonoBehaviour, ISelectableElement<TData>
        where TData : struct, IElementData
    {
        [Header("Interaction")]
        [SerializeField] protected Button _clickButton;

        [Header("Selection")]
        [SerializeField] protected GameObject _selectionIndicator;

        [Header("Popup")]
        [SerializeField] protected PopupTrigger _popupTrigger;

        private TData _currentData;
        private bool _isBound;
        private bool _isSelected;
        private Action<TData> _onClicked;
        private DisposableBag _disposables;

        public string ElementId => IsBound ? _currentData.Id : null;
        public bool IsBound => _isBound;
        public bool IsSelected => _isSelected;
        public bool HasPopup => _popupTrigger != null;

        public Action<TData> OnClicked
        {
            set => _onClicked = value;
        }

        protected TData CurrentData => _currentData;
        protected ref DisposableBag Disposables => ref _disposables;

        protected virtual void Awake()
        {
            _disposables = new DisposableBag();
            SetupClickHandler();
        }

        private void SetupClickHandler()
        {
            if (_clickButton == null) return;

            _clickButton.OnClickAsObservable()
                .Subscribe(this, static (_, self) => self.HandleClick())
                .AddTo(ref _disposables);
        }

        private void HandleClick()
        {
            if (!_isBound) return;
            _onClicked?.Invoke(_currentData);
        }

        public void Bind(TData data)
        {
            if (_isBound)
            {
                Refresh(data);
                return;
            }

            _currentData = data;
            _isBound = true;
            OnBind(data);
            UpdatePopupData(data);
        }

        public void Refresh(TData data)
        {
            if (!_isBound)
            {
                Bind(data);
                return;
            }

            var oldData = _currentData;
            _currentData = data;
            OnRefresh(oldData, data);
            UpdatePopupData(data);
        }

        private void UpdatePopupData(TData data)
        {
            if (_popupTrigger == null) return;

            var popupData = GetPopupData(data);
            if (popupData != null)
            {
                _popupTrigger.SetDataProvider(() => popupData);
            }
        }

        public void SetSelected(bool selected)
        {
            if (_isSelected == selected) return;

            _isSelected = selected;

            if (_selectionIndicator != null)
            {
                _selectionIndicator.SetActive(selected);
            }

            OnSelectionChanged(selected);
        }

        public void Unbind()
        {
            if (!_isBound) return;

            _isBound = false;
            _isSelected = false;
            _currentData = default;

            if (_selectionIndicator != null)
            {
                _selectionIndicator.SetActive(false);
            }

            OnUnbind();
        }

        public void Dispose()
        {
            Unbind();
            _disposables.Dispose();
            OnDispose();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        protected abstract void OnBind(TData data);

        protected virtual void OnRefresh(TData oldData, TData newData) => OnBind(newData);
        protected virtual void OnUnbind() { }
        protected virtual void OnSelectionChanged(bool selected) { }
        protected virtual void OnDispose() { }

        protected virtual IPopupData GetPopupData(TData data) => null;
    }
}
