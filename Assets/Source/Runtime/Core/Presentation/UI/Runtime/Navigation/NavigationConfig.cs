using UnityEngine;

namespace Core.Presentation.UI.Navigation
{
    [CreateAssetMenu(menuName = "MVVM/UI/Navigation Config")]
    public sealed class NavigationConfig : ScriptableObject
    {
        [SerializeField] private float _focusTransitionDuration = 0.15f;
        [SerializeField] private bool _wrapAround = true;
        [SerializeField] private bool _autoSelectOnViewShow = true;

        public float FocusTransitionDuration => _focusTransitionDuration;
        public bool WrapAround => _wrapAround;
        public bool AutoSelectOnViewShow => _autoSelectOnViewShow;
    }
}
