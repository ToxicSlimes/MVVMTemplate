using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Config;
using Core.Domain.Audio;
using Core.Domain.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Infrastructure.Configs.Audio
{
    [CreateAssetMenu(fileName = "AudioMixerGroupConfig", menuName = "Configs/Audio/Mixer Group Config")]
    public class AudioMixerGroupConfig : ConfigBase
    {
        [SerializeField]
        private List<AudioMixerGroupSetup> _mixerGroupSetups = new();

        public IReadOnlyList<AudioMixerGroupSetup> MixerGroupSetups => _mixerGroupSetups;

        public AudioMixerGroupSetup GetSetup(AudioMixerGroupType groupType)
        {
            foreach (var setup in _mixerGroupSetups.Where(setup => setup.Id == groupType))
            {
                return setup;
            }

            Debug.LogWarning($"[AudioMixerGroupConfig] Setup not found for {groupType}");
            return null;
        }
    }
}
