using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Views;
using LitMotion;
using TriInspector;
using UnityEngine;

namespace Core.Presentation.MVVM.Animation
{
    [CreateAssetMenu(fileName = "PanelTransitionPreset", menuName = "MVVM/Animation/Panel Transition Preset")]
    public class PanelTransitionPreset : ViewConfigBase
    {
        [Group("animations")][LabelText("Show Transition")]
        [SerializeField] private ViewTransitionStyle _showTransition = ViewTransitionStyle.Fade;
        [Group("animations")][LabelText("Show Duration")]
        [SerializeField] private float _showDuration = 0.25f;
        [Group("animations")][LabelText("Show Delay")]
        [SerializeField] private float _showDelay;
        [Group("animations")][LabelText("Show Ease")]
        [SerializeField] private Ease _showEase = Ease.OutQuad;

        [Group("animations")][LabelText("Hide Transition")]
        [SerializeField] private ViewTransitionStyle _hideTransition = ViewTransitionStyle.Fade;
        [Group("animations")][LabelText("Hide Duration")]
        [SerializeField] private float _hideDuration = 0.2f;
        [Group("animations")][LabelText("Hide Delay")]
        [SerializeField] private float _hideDelay;
        [Group("animations")][LabelText("Hide Ease")]
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        [Group("animations")][LabelText("Slide Offset (screen fraction)")]
        [Range(0f, 2f)]
        [SerializeField] private float _slideOffset = 1f;

        public ViewTransitionStyle ShowTransition => _showTransition;
        public float ShowDuration => _showDuration;
        public float ShowDelay => _showDelay;
        public Ease ShowEase => _showEase;

        public ViewTransitionStyle HideTransition => _hideTransition;
        public float HideDuration => _hideDuration;
        public float HideDelay => _hideDelay;
        public Ease HideEase => _hideEase;

        public float SlideOffset => _slideOffset;

        protected override void ValidateConfig()
        {
            _showDuration = Mathf.Max(0f, _showDuration);
            _hideDuration = Mathf.Max(0f, _hideDuration);
            _showDelay = Mathf.Max(0f, _showDelay);
            _hideDelay = Mathf.Max(0f, _hideDelay);
        }
    }
}
