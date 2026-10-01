using System;

namespace Core.Data.Domain.Models
{
    [Serializable]
    public class AudioSettingsData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 0.8f;
        public bool IsMasterMuted;
        public bool IsMusicMuted;
        public bool IsSfxMuted;

        public static AudioSettingsData Default => new()
        {
            MasterVolume = 1f,
            MusicVolume = 0.7f,
            SfxVolume = 0.8f,
            IsMasterMuted = false,
            IsMusicMuted = false,
            IsSfxMuted = false
        };
    }
}
