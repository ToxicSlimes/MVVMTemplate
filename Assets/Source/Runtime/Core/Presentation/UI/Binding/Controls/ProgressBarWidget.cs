using UnityEngine;

namespace Core.UI.Controls
{
    public sealed class ProgressBarWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform _fill;

        public void SetValue(float value)
        {
            var anchor = _fill.anchorMax;
            anchor.x = Mathf.Clamp01(value);
            _fill.anchorMax = anchor;
        }
    }
}
