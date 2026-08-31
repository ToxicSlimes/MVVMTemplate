using System;
using UnityEngine;

namespace Core.AI.Utility
{
    [Serializable]
    public sealed class UtilityResponse
    {
        [SerializeField] private float _weight = 1f;
        [SerializeField] private AnimationCurve _response = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float Evaluate(float input)
        {
            if (_response == null || _response.length == 0)
            {
                return 0f;
            }

            return _weight * Mathf.Clamp01(_response.Evaluate(Mathf.Clamp01(input)));
        }
    }
}
