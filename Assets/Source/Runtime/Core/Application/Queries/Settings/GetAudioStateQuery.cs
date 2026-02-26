using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Queries.Settings
{
    public readonly struct GetAudioStateQuery : IRequest<AudioState>
    {
    }

    public readonly struct AudioState
    {
        public readonly float MasterVolume;
        public readonly float MusicVolume;
        public readonly float SfxVolume;
        public readonly bool IsMasterMuted;
        public readonly bool IsMusicMuted;
        public readonly bool IsSfxMuted;

        public AudioState(
            float masterVolume,
            float musicVolume,
            float sfxVolume,
            bool isMasterMuted,
            bool isMusicMuted,
            bool isSfxMuted)
        {
            MasterVolume = masterVolume;
            MusicVolume = musicVolume;
            SfxVolume = sfxVolume;
            IsMasterMuted = isMasterMuted;
            IsMusicMuted = isMusicMuted;
            IsSfxMuted = isSfxMuted;
        }
    }
}
