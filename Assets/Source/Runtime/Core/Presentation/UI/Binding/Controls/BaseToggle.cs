using Core.Abstractions.UI;
using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI.Controls
{
    public class BaseToggle : Toggle
    {
        [Header("Toggle")]
        [SerializeField] private RectTransform _checkmark;

        private readonly ReactiveProperty<bool> _value = new();
        private DisposableBag _disposables;
        private IControlBehavior _base;
        private bool _suppressSync;
        private MotionHandle _checkmarkHandle;

        public ReactiveProperty<bool> Value => _value;

        protected override void Awake()
        {
            base.Awake();
            _base = GetComponent<IControlBehavior>();
            _value.Value = isOn;

            onValueChanged.AddListener(OnToggleChanged);
            _value.Subscribe(this, static (val, self) =>
            {
                if (self._suppressSync) return;
                self._suppressSync = true;
                self.isOn = val;
                self._suppressSync = false;
            }).AddTo(ref _disposables);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _value.Dispose();
            _disposables.Dispose();
            if (_checkmarkHandle.IsActive()) _checkmarkHandle.Cancel();
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

        private void OnToggleChanged(bool val)
        {
            if (_suppressSync) return;
            _suppressSync = true;
            _value.Value = val;
            _suppressSync = false;

            _base?.PlaySound(_base.ClickSound);
            AnimateCheckmark(val);
        }

        private void AnimateCheckmark(bool on)
        {
            if (_checkmark == null) return;
            if (_checkmarkHandle.IsActive()) _checkmarkHandle.Cancel();

            var from = _checkmark.localScale.x;
            var to = on ? 1f : 0f;
            _checkmarkHandle = LMotion.Create(from, to, 0.15f)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(on ? Ease.OutBack : Ease.InQuad)
                .Bind(_checkmark, static (s, target) => target.localScale = new Vector3(s, s, 1f));
        }
    }
}
