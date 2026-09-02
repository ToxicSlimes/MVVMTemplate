using System;
using Core.Abstractions.Services;
using Core.Data.Domain.Enums;
using UnityEngine;

namespace Core.Infrastructure.Services.Audio
{
    public sealed class NullAudioService : IAudioService
    {
        public float MasterVolume => 1f;
        public float MusicVolume => 0.7f;
        public float SfxVolume => 0.8f;
        public bool IsMasterMuted => false;
        public bool IsMusicMuted => false;
        public bool IsSfxMuted => false;
        public bool IsMusicPlaying => false;

        public event Action<float> MasterVolumeChanged;
        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action<bool> MasterMutedChanged;
        public event Action<bool> MusicMutedChanged;
        public event Action<bool> SfxMutedChanged;

        public void Play(SoundType sound, bool randomPitch = true, Vector3 position = default, Transform followTarget = null) { }
        public void PlayMusic(SoundType music) { }
        public void StopMusic() { }
        public void StopAll() { }
        public void SetMasterVolume(float value) { }
        public void SetMusicVolume(float value, bool save = true) { }
        public void SetSfxVolume(float value) { }
        public void MuteMaster(bool muted) { }
        public void MuteMusic(bool muted) { }
        public void MuteSfx(bool muted) { }
        public void SaveSettings() { }
        public void LoadSettings() { }
        public void ResetToDefaults() { }
    }
}
