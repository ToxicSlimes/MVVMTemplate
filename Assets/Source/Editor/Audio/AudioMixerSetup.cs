#if UNITY_EDITOR
using Core.Infrastructure.Configs.Services;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.Audio
{
    public static class AudioMixerSetup
    {
        private const string MixerPath = "Assets/Source/00. Configs/Audio/MainAudioMixer.mixer";
        private const string AudioConfigPath = "Assets/Source/00. Configs/Audio/AudioConfig.asset";

        [MenuItem("Tools/MVVM/Setup Audio Assets")]
        public static void SetupAudioAssets()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>(MixerPath);
            if (mixer == null)
            {
                Debug.LogError($"[Audio] Missing mixer at {MixerPath}. Reimport the project or restore MainAudioMixer.mixer.");
                return;
            }

            var audioConfig = AssetDatabase.LoadAssetAtPath<AudioConfig>(AudioConfigPath);
            if (audioConfig == null)
            {
                Debug.LogError($"[Audio] Missing AudioConfig at {AudioConfigPath}");
                return;
            }

            var serializedConfig = new SerializedObject(audioConfig);
            serializedConfig.FindProperty("audioMixer").objectReferenceValue = mixer;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(audioConfig);
            AssetDatabase.SaveAssets();

            Debug.Log("[Audio] AudioConfig audioMixer reference refreshed.");
        }
    }
}
#endif
