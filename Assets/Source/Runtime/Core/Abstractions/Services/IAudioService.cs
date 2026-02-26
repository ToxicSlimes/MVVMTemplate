using System;
using Core.Data.Domain.Enums;
using UnityEngine;

namespace Core.Abstractions.Services
{
    public interface IAudioService
    {
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }
        bool IsMasterMuted { get; }
        bool IsMusicMuted { get; }
        bool IsSfxMuted { get; }
        bool IsMusicPlaying { get; }

        event Action<float> MasterVolumeChanged;
        event Action<float> MusicVolumeChanged;
        event Action<float> SfxVolumeChanged;
        event Action<bool> MasterMutedChanged;
        event Action<bool> MusicMutedChanged;
        event Action<bool> SfxMutedChanged;

        void Play(SoundType sound, bool randomPitch = true, Vector3 position = default, Transform followTarget = null);
        void PlayMusic(SoundType music);
        void StopMusic();
        void StopAll();

        void SetMasterVolume(float value);
        void SetMusicVolume(float value, bool save = true);
        void SetSfxVolume(float value);

        void MuteMaster(bool muted);
        void MuteMusic(bool muted);
        void MuteSfx(bool muted);

        void SaveSettings();
        void LoadSettings();
        void ResetToDefaults();
    }
}
