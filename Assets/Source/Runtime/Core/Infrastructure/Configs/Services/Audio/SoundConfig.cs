using System.Collections.Generic;
using Core.Abstractions.Config;
using Core.Domain.Audio;
using Core.Domain.Enums;
using UnityEngine;

namespace Core.Infrastructure.Configs.Audio
{
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "Configs/Audio/Sound Config")]
    public class SoundConfig : ConfigBase
    {
        [SerializeField]
        private List<SoundSetup> soundSetups = new();

        public IReadOnlyList<SoundSetup> SoundSetups => soundSetups;

        public SoundSetup GetSetup(SoundType soundType)
        {
            foreach (var setup in soundSetups)
            {
                if (setup.Id == soundType)
                    return setup;
            }

            Debug.LogWarning($"[SoundConfig] Setup not found for {soundType}");
            return null;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (var setup in soundSetups)
            {
                if (setup == null)
                    continue;

                if (setup.MaxDistance < setup.MinDistance)
                    setup.MaxDistance = setup.MinDistance;

                setup.MinDistance = Mathf.Max(0.01f, setup.MinDistance);
                setup.MaxDistance = Mathf.Max(setup.MinDistance, setup.MaxDistance);
                setup.SpatialBlend = Mathf.Clamp01(setup.SpatialBlend);
                setup.DopplerLevel = Mathf.Clamp(setup.DopplerLevel, 0f, 5f);
                setup.Spread = Mathf.Clamp(setup.Spread, 0f, 360f);
                setup.ReverbLevel = Mathf.Clamp01(setup.ReverbLevel);

                if (setup.CustomRolloffCurve == null || setup.CustomRolloffCurve.length == 0)
                    setup.CustomRolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            }
        }
#endif
    }
}
