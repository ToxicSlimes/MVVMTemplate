using System;
using System.Collections.Generic;
using Core.Domain.Enums;
using UnityEngine;

namespace Core.Domain.Audio
{
    [Serializable]
    public class SoundSetup
    {
        public SoundType Id;
        public AudioMixerGroupType MixerGroup = AudioMixerGroupType.SoundEffects;
        public SoundPlaybackScope PlaybackScope = SoundPlaybackScope.Global;

        [Range(0f, 1f)]
        public float Volume = 1f;

        [Range(0.1f, 20f)]
        public float MinDistance = 1f;

        [Range(1f, 200f)]
        public float MaxDistance = 25f;

        public bool OverrideSpatialSettings;

        [Range(0f, 1f)]
        public float SpatialBlend = 1f;

        [Range(0f, 5f)]
        public float DopplerLevel = 1f;

        [Range(0f, 360f)]
        public float Spread;

        public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;

        [Range(0f, 1f)]
        public float ReverbLevel;

        public AnimationCurve CustomRolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        public List<AudioClipEntry> Clips = new();
    }
}
