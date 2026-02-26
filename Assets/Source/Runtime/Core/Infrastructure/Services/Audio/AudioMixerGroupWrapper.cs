using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Services.Audio
{
    public class AudioMixerGroupWrapper
    {
        private readonly List<AudioSourceWrapper> _sources = new();
        private readonly float _cooldown;
        private float _cooldownTimer;

        public AudioMixerGroup MixerGroup { get; }

        public AudioMixerGroupWrapper(AudioMixerGroup mixerGroup, float cooldown)
        {
            MixerGroup = mixerGroup;
            _cooldown = cooldown;
        }

        public void Add(AudioSourceWrapper wrapper)
        {
            _sources.Add(wrapper);
        }

        public AudioSourceWrapper GetAvailable()
        {
            if (_cooldownTimer > 0)
                return null;

            _cooldownTimer = _cooldown;

            foreach (var wrapper in _sources)
            {
                if (!wrapper.IsActive)
                    return wrapper;
            }

            return null;
        }

        public AudioSourceWrapper GetForMusic()
        {
            if (_sources.Count == 0)
                return null;

            var wrapper = _sources[0];

            if (wrapper.Source.isPlaying)
            {
                wrapper.Source.Stop();
                wrapper.Source.clip = null;
            }

            wrapper.IsActive = false;
            return wrapper;
        }

        public void Tick(float deltaTime)
        {
            _cooldownTimer = Mathf.Max(0, _cooldownTimer - deltaTime);

            foreach (var wrapper in _sources)
            {
                if (wrapper.IsActive)
                    wrapper.Tick();
            }
        }

        public void StopAll()
        {
            foreach (var wrapper in _sources)
                wrapper.Stop();
        }

        public void Mute(bool muted)
        {
            foreach (var wrapper in _sources)
                wrapper.Source.mute = muted;
        }
    }
}
