using UnityEngine;

namespace Core.AI.Utility
{
    public abstract class UtilityAgentProfile : ScriptableObject
    {
        [SerializeField, Range(0.05f, 2f)] private float _decisionPeriod = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _inertiaBonus = 0.2f;

        public float DecisionPeriod => _decisionPeriod;
        public float InertiaBonus => _inertiaBonus;
    }
}
