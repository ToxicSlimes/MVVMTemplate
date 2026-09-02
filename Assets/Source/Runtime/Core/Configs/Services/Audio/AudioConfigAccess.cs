using Core.Data.Configs.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Configs.Services
{
    public static class AudioConfigAccess
    {
        public static bool TryGetRuntimeAssets(
            AudioConfig config,
            out AudioMixer audioMixer,
            out SoundConfig soundConfig,
            out AudioMixerGroupConfig mixerGroupConfig,
            out GameObject audioSourcePrefab)
        {
            audioMixer = null;
            soundConfig = null;
            mixerGroupConfig = null;
            audioSourcePrefab = null;

            if (config == null)
                return false;

            try
            {
                audioMixer = config.AudioMixer;
                soundConfig = config.SoundConfig;
                mixerGroupConfig = config.MixerGroupConfig;
                audioSourcePrefab = config.AudioSourcePrefab;
            }
            catch
            {
                return false;
            }

            return audioMixer != null && audioSourcePrefab != null;
        }
    }
}
