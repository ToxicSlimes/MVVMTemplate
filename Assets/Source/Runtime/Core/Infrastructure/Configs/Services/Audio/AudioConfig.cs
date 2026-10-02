using Core.Abstractions.Config;
using Core.Infrastructure.Configs.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Configs.Services
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Services/Audio Config")]
    public class AudioConfig : ConfigBase
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Configs")]
        [SerializeField] private SoundConfig soundConfig;
        [SerializeField] private AudioMixerGroupConfig mixerGroupConfig;

        [Header("Prefabs")]
        [SerializeField] private GameObject audioSourcePrefab;

        public AudioMixer AudioMixer => audioMixer;
        public SoundConfig SoundConfig => soundConfig;
        public AudioMixerGroupConfig MixerGroupConfig => mixerGroupConfig;
        public GameObject AudioSourcePrefab => audioSourcePrefab;
    }
}
