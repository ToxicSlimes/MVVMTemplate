using System.Collections.Generic;
using Core.Abstractions.UI;
using Core.Abstractions.UI.Validation;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI.Controls
{
    public class BaseInputField : TMP_InputField
    {
        [Header("Input Field")]
        [SerializeField] private Image _errorBorder;
        [SerializeField] private float _validationDebounceMs = 300f;

        private readonly ReactiveProperty<string> _text = new(string.Empty);
        private readonly ReactiveProperty<bool> _isValid = new(true);
        private readonly Subject<string> _onSubmitObservable = new();
        private readonly List<IValidationRule> _validationRules = new();
        private DisposableBag _disposables;
        private IControlBehavior _base;
        private bool _suppressSync;

        public ReactiveProperty<string> Text => _text;
        public ReactiveProperty<bool> IsValid => _isValid;
        public Observable<string> OnSubmitObservable => _onSubmitObservable;
        public List<IValidationRule> ValidationRules => _validationRules;

        protected override void Awake()
        {
            base.Awake();
            _base = GetComponent<IControlBehavior>();
            _text.Value = text;

            onValueChanged.AddListener(OnInputChanged);
            onSubmit.AddListener(OnInputSubmit);

            _text.Subscribe(this, static (val, self) =>
            {
                if (self._suppressSync) return;
                self._suppressSync = true;
                self.text = val;
                self._suppressSync = false;
            }).AddTo(ref _disposables);

            _text
                .Debounce(System.TimeSpan.FromMilliseconds(_validationDebounceMs))
                .Subscribe(this, static (val, self) => self.RunValidation(val))
                .AddTo(ref _disposables);

            if (_errorBorder != null)
            {
                _isValid.Subscribe(_errorBorder, static (valid, border) => border.enabled = !valid)
                    .AddTo(ref _disposables);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _text.Dispose();
            _isValid.Dispose();
            _onSubmitObservable.Dispose();
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

        public void AddRule(IValidationRule rule)
        {
            _validationRules.Add(rule);
        }

        private void OnInputChanged(string val)
        {
            if (_suppressSync) return;
            _suppressSync = true;
            _text.Value = val;
            _suppressSync = false;
        }

        private void OnInputSubmit(string val)
        {
            _onSubmitObservable.OnNext(val);
        }

        private void RunValidation(string val)
        {
            if (_validationRules.Count == 0)
            {
                _isValid.Value = true;
                _base?.TransitionTo(ControlState.Normal);
                return;
            }

            foreach (var rule in _validationRules)
            {
                var result = rule.Validate(val);
                if (!result.IsValid)
                {
                    _isValid.Value = false;
                    _base?.TransitionTo(ControlState.Error);
                    return;
                }
            }

            _isValid.Value = true;
            _base?.TransitionTo(ControlState.Normal);
        }
    }
}
