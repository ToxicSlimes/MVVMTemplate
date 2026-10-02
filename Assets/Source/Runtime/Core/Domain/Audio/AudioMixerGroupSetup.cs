using System;
using Core.Domain.Enums;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Domain.Audio
{
    [Serializable]
    public class AudioMixerGroupSetup
    {
        public AudioMixerGroupType Id;
        public AudioMixerGroup MixerGroup;

        [Range(0, 10f)]
        [Tooltip("Minimum time between sounds in this group")]
        public float Cooldown;

        [Range(1, 50)]
        [Tooltip("Number of audio sources in pool")]
        public int PoolSize = 5;

        public bool Loop;
    }
}
