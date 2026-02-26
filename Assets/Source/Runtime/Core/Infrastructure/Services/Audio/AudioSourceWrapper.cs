using UnityEngine;

namespace Core.Infrastructure.Services.Audio
{
    public class AudioSourceWrapper
    {
        public AudioSource Source { get; }
        public bool IsActive { get; set; }

        public AudioSourceWrapper(AudioSource source)
        {
            Source = source;
        }

        public void Tick()
        {
            if (IsActive && !Source.isPlaying)
                IsActive = false;
        }

        public void Stop()
        {
            IsActive = false;
            Source.Stop();
            Source.clip = null;
            Source.loop = false;
        }
    }
}
