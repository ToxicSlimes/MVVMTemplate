using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Abstractions.Services;
using Core.Data.Configs.Audio;
using Core.Data.Domain.Enums;
using Core.Data.Domain.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core.Infrastructure.Services.Audio
{
    public class AudioService : MonoBehaviour, IAudioService
    {
        private const string PrefsKey = "AudioSettings";
        private const float SliderMutedThreshold = -40f;
        private const float MutedDb = -80f;

        private const string MasterParam = "Master";
        private const string MusicParam = "Music";
        private const string SfxParam = "SoundEffects";

        private AudioMixer _audioMixer;
        private SoundConfig _soundConfig;
        private AudioMixerGroupConfig _mixerGroupConfig;
        private GameObject _audioSourcePrefab;

        private readonly Dictionary<AudioMixerGroupType, AudioMixerGroupWrapper> _mixerGroups = new();
        private readonly List<AudioSource> _localSources = new();
        private Transform _container;

        private IPrefsService _prefsService;
        private AudioSettingsData _settings;

        public float MasterVolume => _settings?.MasterVolume ?? 1f;
        public float MusicVolume => _settings?.MusicVolume ?? 0.7f;
        public float SfxVolume => _settings?.SfxVolume ?? 0.8f;
        public bool IsMasterMuted => _settings?.IsMasterMuted ?? false;
        public bool IsMusicMuted => _settings?.IsMusicMuted ?? false;
        public bool IsSfxMuted => _settings?.IsSfxMuted ?? false;
        public bool IsMusicPlaying { get; private set; }

        public event Action<float> MasterVolumeChanged;
        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action<bool> MasterMutedChanged;
        public event Action<bool> MusicMutedChanged;
        public event Action<bool> SfxMutedChanged;

        public void Initialize(IPrefsService prefsService, AudioMixer audioMixer, SoundConfig soundConfig, AudioMixerGroupConfig mixerGroupConfig, GameObject audioSourcePrefab)
        {
            _prefsService = prefsService;
            _audioMixer = audioMixer;
            _soundConfig = soundConfig;
            _mixerGroupConfig = mixerGroupConfig;
            _audioSourcePrefab = audioSourcePrefab;

            var containerGo = new GameObject("AudioSourcesContainer");
            containerGo.transform.SetParent(transform);
            _container = containerGo.transform;

            CreateMixerGroupWrappers();
            LoadSettings();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            foreach (var wrapper in _mixerGroups.Values)
                wrapper.Tick(deltaTime);
        }

        public void Play(SoundType sound, bool randomPitch = true, Vector3 position = default, Transform followTarget = null)
        {
            PlayInternal(sound, randomPitch, position, followTarget, isMusic: false, destroyCancellationToken).Forget();
        }

        public void PlayMusic(SoundType music)
        {
            IsMusicPlaying = true;
            PlayInternal(music, randomPitch: false, Vector3.zero, null, isMusic: true, destroyCancellationToken).Forget();
        }

        public void StopMusic()
        {
            IsMusicPlaying = false;
            if (_mixerGroups.TryGetValue(AudioMixerGroupType.Music, out var wrapper))
                wrapper.StopAll();
        }

        public void StopAll()
        {
            IsMusicPlaying = false;

            foreach (var wrapper in _mixerGroups.Values)
                wrapper.StopAll();

            foreach (var source in _localSources)
            {
                if (source == null) continue;
                source.Stop();
                Destroy(source.gameObject);
            }

            _localSources.Clear();
        }

        public void SetMasterVolume(float value)
        {
            _settings.MasterVolume = Mathf.Clamp01(value);
            ApplyMasterVolume();
            SaveSettings();
            MasterVolumeChanged?.Invoke(_settings.MasterVolume);
        }

        public void SetMusicVolume(float value, bool save = true)
        {
            _settings.MusicVolume = Mathf.Clamp01(value);
            ApplyMusicVolume();
            if (save) SaveSettings();
            MusicVolumeChanged?.Invoke(_settings.MusicVolume);
        }

        public void SetSfxVolume(float value)
        {
            _settings.SfxVolume = Mathf.Clamp01(value);
            ApplySfxVolume();
            SaveSettings();
            SfxVolumeChanged?.Invoke(_settings.SfxVolume);
        }

        public void MuteMaster(bool muted)
        {
            _settings.IsMasterMuted = muted;
            ApplyMasterVolume();
            SaveSettings();
            MasterMutedChanged?.Invoke(muted);
        }

        public void MuteMusic(bool muted)
        {
            _settings.IsMusicMuted = muted;
            ApplyMusicVolume();
            SaveSettings();
            MusicMutedChanged?.Invoke(muted);
        }

        public void MuteSfx(bool muted)
        {
            _settings.IsSfxMuted = muted;
            ApplySfxVolume();
            SaveSettings();
            SfxMutedChanged?.Invoke(muted);
        }

        public void SaveSettings()
        {
            _prefsService?.Save(PrefsKey, _settings);
        }

        public void LoadSettings()
        {
            _settings = _prefsService?.Load<AudioSettingsData>(PrefsKey) ?? AudioSettingsData.Default;
            ApplyAllVolumes();
        }

        public void ResetToDefaults()
        {
            _settings = AudioSettingsData.Default;
            ApplyAllVolumes();
            SaveSettings();
        }

        private async UniTaskVoid PlayInternal(SoundType soundType, bool randomPitch, Vector3 position, Transform followTarget, bool isMusic, CancellationToken ct)
        {
            if (soundType == SoundType.Unknown)
            {
                Debug.LogWarning("[AudioService] Unknown sound type");
                return;
            }

            var soundSetup = _soundConfig?.GetSetup(soundType);
            if (soundSetup == null)
            {
                Debug.LogWarning($"[AudioService] Sound setup not found for {soundType}");
                return;
            }

            if (soundSetup.Clips == null || soundSetup.Clips.Count == 0)
            {
                Debug.LogWarning($"[AudioService] Sound {soundType} has no clips");
                return;
            }

            var groupSetup = _mixerGroupConfig?.GetSetup(soundSetup.MixerGroup);
            if (groupSetup == null)
            {
                Debug.LogWarning($"[AudioService] Mixer group not found for {soundSetup.MixerGroup}");
                return;
            }

            AudioSourceWrapper wrapper = null;
            AudioSource source;
            bool useGlobalSource = soundSetup.PlaybackScope == SoundPlaybackScope.Global || isMusic;

            if (useGlobalSource)
            {
                if (!_mixerGroups.TryGetValue(soundSetup.MixerGroup, out var groupWrapper))
                    return;

                wrapper = isMusic ? groupWrapper.GetForMusic() : groupWrapper.GetAvailable();
                if (wrapper == null) return;

                wrapper.IsActive = true;
                source = wrapper.Source;
            }
            else
            {
                source = CreateLocalSource(groupSetup, position, followTarget);
                if (source == null) return;
            }

            source.pitch = randomPitch ? UnityEngine.Random.Range(0.95f, 1.1f) : 1f;

            if (!TryGetWeightedClip(soundSetup.Clips, out var clipEntry))
            {
                if (wrapper != null) wrapper.IsActive = false;
                else CleanupLocalSource(source);
                return;
            }

            ConfigureSource(source, soundSetup);

            ct.ThrowIfCancellationRequested();

            var clip = await LoadClipAsync(clipEntry, ct);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioService] Clip missing for {soundType}");
                if (wrapper != null) wrapper.Stop();
                else CleanupLocalSource(source);
                return;
            }

            var playPosition = followTarget != null ? followTarget.position : position;
            PlayClip(clip, source, soundSetup.Volume, playPosition, groupSetup.Loop);

            if (!useGlobalSource && !groupSetup.Loop)
                StartCoroutine(ReleaseWhenDone(source));
        }

        private void ConfigureSource(AudioSource source, SoundSetup setup)
        {
            source.minDistance = Mathf.Max(0.01f, setup.MinDistance);
            source.maxDistance = Mathf.Max(source.minDistance, setup.MaxDistance);

            if (setup.OverrideSpatialSettings)
            {
                source.spatialBlend = Mathf.Clamp01(setup.SpatialBlend);
                source.dopplerLevel = Mathf.Clamp(setup.DopplerLevel, 0f, 5f);
                source.spread = Mathf.Clamp(setup.Spread, 0f, 360f);
                source.rolloffMode = setup.RolloffMode;
            }
            else
            {
                source.spatialBlend = setup.PlaybackScope == SoundPlaybackScope.Local ? 1f : 0f;
                source.dopplerLevel = 1f;
                source.spread = 0f;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
            }

            var reverbFilter = source.GetComponent<AudioReverbFilter>();
            if (setup.ReverbLevel > 0f)
            {
                if (reverbFilter == null)
                    reverbFilter = source.gameObject.AddComponent<AudioReverbFilter>();

                reverbFilter.enabled = true;
                reverbFilter.reverbLevel = Mathf.Lerp(-10000f, 0f, setup.ReverbLevel);
            }
            else if (reverbFilter != null)
            {
                reverbFilter.enabled = false;
            }
        }

        private void PlayClip(AudioClip clip, AudioSource source, float volume, Vector3 position, bool loop)
        {
            if (loop)
            {
                source.loop = true;
                source.clip = clip;
                source.volume = volume;
                source.Play();
            }
            else
            {
                source.transform.position = position;
                source.PlayOneShot(clip, volume);
            }
        }

        private void CreateMixerGroupWrappers()
        {
            if (_mixerGroupConfig == null) return;

            foreach (var setup in _mixerGroupConfig.MixerGroupSetups)
            {
                var groupGo = new GameObject($"{setup.Id}Group");
                groupGo.transform.SetParent(_container);

                var wrapper = new AudioMixerGroupWrapper(setup.MixerGroup, setup.Cooldown);
                _mixerGroups[setup.Id] = wrapper;

                for (var i = 0; i < setup.PoolSize; i++)
                {
                    var sourceGo = Instantiate(_audioSourcePrefab, groupGo.transform);
                    sourceGo.name = $"{setup.Id}Source_{i}";

                    var source = sourceGo.GetComponent<AudioSource>();
                    source.outputAudioMixerGroup = setup.MixerGroup;

                    wrapper.Add(new AudioSourceWrapper(source));
                }
            }
        }

        private AudioSource CreateLocalSource(AudioMixerGroupSetup groupSetup, Vector3 position, Transform followTarget)
        {
            if (_audioSourcePrefab == null || groupSetup == null)
                return null;

            var parent = followTarget != null ? followTarget : _container;
            var instance = Instantiate(_audioSourcePrefab, parent);

            if (followTarget != null)
                instance.transform.localPosition = Vector3.zero;
            else
                instance.transform.position = position;

            var source = instance.GetComponent<AudioSource>();
            source.outputAudioMixerGroup = groupSetup.MixerGroup;
            source.spatialBlend = 1f;
            source.loop = false;

            _localSources.Add(source);
            return source;
        }

        private void CleanupLocalSource(AudioSource source)
        {
            if (source == null) return;
            _localSources.Remove(source);
            Destroy(source.gameObject);
        }

        private IEnumerator ReleaseWhenDone(AudioSource source)
        {
            while (source != null && source.isPlaying)
                yield return null;

            if (source != null)
                CleanupLocalSource(source);
        }

        private static bool TryGetWeightedClip(List<AudioClipEntry> clips, out AudioClipEntry result)
        {
            result = default;

            if (clips == null || clips.Count == 0)
                return false;

            if (clips.Count == 1)
            {
                result = clips[0];
                return result.HasClip;
            }

            var totalWeight = clips.Sum(c => c.Weight);
            if (totalWeight == 0)
            {
                result = clips[UnityEngine.Random.Range(0, clips.Count)];
                return result.HasClip;
            }

            var random = UnityEngine.Random.Range(0, totalWeight);
            var current = 0;

            foreach (var clip in clips)
            {
                current += clip.Weight;
                if (random < current)
                {
                    result = clip;
                    return result.HasClip;
                }
            }

            result = clips[^1];
            return result.HasClip;
        }

        private async UniTask<AudioClip> LoadClipAsync(AudioClipEntry entry, CancellationToken ct)
        {
            if (entry.LegacyClip != null)
                return entry.LegacyClip;

            if (entry.ClipReference == null || !entry.ClipReference.RuntimeKeyIsValid())
                return null;

            AsyncOperationHandle<AudioClip> handle;

            if (entry.ClipReference.OperationHandle.IsValid())
            {
                handle = entry.ClipReference.OperationHandle.Convert<AudioClip>();
                if (!handle.IsDone)
                    await handle.Task.AsUniTask().AttachExternalCancellation(ct);
            }
            else
            {
                handle = entry.ClipReference.LoadAssetAsync();
                await handle.Task.AsUniTask().AttachExternalCancellation(ct);
            }

            return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null;
        }

        private void ApplyAllVolumes()
        {
            ApplyMasterVolume();
            ApplyMusicVolume();
            ApplySfxVolume();
        }

        private void ApplyMasterVolume()
        {
            var db = _settings.IsMasterMuted ? MutedDb : SliderToDb(_settings.MasterVolume);
            _audioMixer?.SetFloat(MasterParam, db);
        }

        private void ApplyMusicVolume()
        {
            var db = _settings.IsMusicMuted ? MutedDb : SliderToDb(_settings.MusicVolume);
            _audioMixer?.SetFloat(MusicParam, db);
        }

        private void ApplySfxVolume()
        {
            var db = _settings.IsSfxMuted ? MutedDb : SliderToDb(_settings.SfxVolume);
            _audioMixer?.SetFloat(SfxParam, db);
        }

        private static float SliderToDb(float sliderValue)
        {
            return sliderValue <= 0 ? MutedDb : Mathf.Log10(sliderValue) * 20f;
        }

        private void OnDestroy()
        {
            foreach (var source in _localSources)
            {
                if (source == null) continue;
                source.Stop();
                Destroy(source.gameObject);
            }

            _localSources.Clear();
        }
    }
}
