using Core.Abstractions.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI.Controls
{
    public class BaseButton : Button
    {
        [Header("Button")]
        [SerializeField] private float _doubleClickThreshold = 0.3f;
        [SerializeField] private float _longPressDuration = 0.8f;
        [SerializeField] private float _cooldownDuration;

        private readonly Subject<Unit> _onDoubleClick = new();
        private readonly Subject<float> _onLongPress = new();
        private readonly Subject<Unit> _onHoldComplete = new();

        private IControlBehavior _base;
        private float _lastClickTime;
        private float _pointerDownTime;
        private bool _isPointerDown;
        private bool _isHoldComplete;
        private float _lastActionTime;

        public Observable<Unit> OnDoubleClick => _onDoubleClick;
        public Observable<float> OnLongPress => _onLongPress;
        public Observable<Unit> OnHoldComplete => _onHoldComplete;

        protected override void Awake()
        {
            base.Awake();
            _base = GetComponent<IControlBehavior>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _onDoubleClick.Dispose();
            _onLongPress.Dispose();
            _onHoldComplete.Dispose();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            onClick.AddListener(HandleClick);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onClick.RemoveListener(HandleClick);
            _isPointerDown = false;
        }

        private void Update()
        {
            if (!_isPointerDown || _isHoldComplete || _longPressDuration <= 0) return;
            var elapsed = Time.unscaledTime - _pointerDownTime;
            var progress = Mathf.Clamp01(elapsed / _longPressDuration);
            _onLongPress.OnNext(progress);
            if (progress >= 1f)
            {
                _isHoldComplete = true;
                _onHoldComplete.OnNext(Unit.Default);
            }
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
            if (!interactable) return;
            _isPointerDown = true;
            _isHoldComplete = false;
            _pointerDownTime = Time.unscaledTime;
            _base?.TransitionTo(ControlState.Pressed);
            _base?.PlayPressEffect();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            _isPointerDown = false;
            _base?.TransitionTo(ControlState.Normal);
        }

        private void HandleClick()
        {
            if (_cooldownDuration > 0 && Time.unscaledTime - _lastActionTime < _cooldownDuration) return;
            _lastActionTime = Time.unscaledTime;
            _base?.PlaySound(_base.ClickSound);

            var now = Time.unscaledTime;
            if (now - _lastClickTime <= _doubleClickThreshold)
            {
                _onDoubleClick.OnNext(Unit.Default);
                _lastClickTime = 0;
            }
            else
            {
                _lastClickTime = now;
            }
        }
    }
}
