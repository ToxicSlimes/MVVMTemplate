using Core.Abstractions.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI.Controls
{
    public class BaseSlider : Slider
    {
        [Header("Slider")]
        [SerializeField] private float _step;
        [SerializeField] private TMP_Text _valueLabel;
        [SerializeField] private string _labelFormat = "{0:P0}";

        private readonly ReactiveProperty<float> _value = new();
        private readonly Subject<Unit> _onDragStarted = new();
        private readonly Subject<Unit> _onDragEnded = new();
        private DisposableBag _disposables;
        private IControlBehavior _base;
        private bool _suppressSync;

        public ReactiveProperty<float> Value => _value;
        public Observable<Unit> OnDragStarted => _onDragStarted;
        public Observable<Unit> OnDragEnded => _onDragEnded;

        protected override void Awake()
        {
            base.Awake();
            _base = GetComponent<IControlBehavior>();
            _value.Value = value;

            onValueChanged.AddListener(OnSliderChanged);
            _value.Subscribe(this, static (val, self) =>
            {
                if (self._suppressSync) return;
                self._suppressSync = true;
                self.value = val;
                self._suppressSync = false;
                self.UpdateLabel(val);
            }).AddTo(ref _disposables);

            UpdateLabel(value);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _onDragStarted.Dispose();
            _onDragEnded.Dispose();
            _value.Dispose();
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

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            _onDragStarted.OnNext(Unit.Default);
            _base?.TransitionTo(ControlState.Pressed);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            _onDragEnded.OnNext(Unit.Default);
            _base?.TransitionTo(ControlState.Normal);
        }

        private void OnSliderChanged(float val)
        {
            if (_suppressSync) return;

            if (_step > 0)
            {
                val = Mathf.Round(val / _step) * _step;
                _suppressSync = true;
                value = val;
                _suppressSync = false;
            }

            _suppressSync = true;
            _value.Value = val;
            _suppressSync = false;

            UpdateLabel(val);
        }

        private void UpdateLabel(float val)
        {
            if (_valueLabel != null)
                _valueLabel.text = string.Format(_labelFormat, val);
        }
    }
}
