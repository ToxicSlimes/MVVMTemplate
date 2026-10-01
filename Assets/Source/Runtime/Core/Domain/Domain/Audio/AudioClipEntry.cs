using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Data.Configs.Audio
{
    [Serializable]
    public struct AudioClipEntry
    {
        [SerializeField]
        public AssetReferenceT<AudioClip> ClipReference;

        [SerializeField]
        public AudioClip LegacyClip;

        [SerializeField]
        [Range(0, 50)]
        public int Weight;

        public AudioClipEntry(AudioClip clip, int weight = 5)
        {
            ClipReference = null;
            LegacyClip = clip;
            Weight = Mathf.Clamp(weight, 0, 50);
        }

        public bool HasClip => (ClipReference != null && ClipReference.RuntimeKeyIsValid()) || LegacyClip != null;
    }
}
