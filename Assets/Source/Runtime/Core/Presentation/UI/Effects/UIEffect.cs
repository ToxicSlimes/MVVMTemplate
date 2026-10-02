using LitMotion;
using UnityEngine;

namespace Core.Presentation.UI.Effects
{
    [CreateAssetMenu(fileName = "UIEffect", menuName = "MVVM/UI/Effect")]
    public class UIEffect : ScriptableObject
    {
        public UIEffectType Type;
        public float Duration = 0.15f;
        public float Strength = 1f;
        public Ease Easing = Ease.OutQuad;
        public int Frequency = 10;
        public float DampingRatio = 1f;
        public bool UseInspectorSettings;
    }
}
