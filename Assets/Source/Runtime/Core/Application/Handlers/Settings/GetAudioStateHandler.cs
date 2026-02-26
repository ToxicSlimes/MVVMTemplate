using System.Threading;
using Core.Abstractions.Services;
using Core.Application.Queries.Settings;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Settings
{
    public sealed class GetAudioStateHandler : IRequestHandler<GetAudioStateQuery, AudioState>
    {
        private readonly IAudioService _audioService;

        public GetAudioStateHandler(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public UniTask<AudioState> Handle(GetAudioStateQuery query, CancellationToken ct)
        {
            var state = new AudioState(
                masterVolume: _audioService.MasterVolume,
                musicVolume: _audioService.MusicVolume,
                sfxVolume: _audioService.SfxVolume,
                isMasterMuted: _audioService.IsMasterMuted,
                isMusicMuted: _audioService.IsMusicMuted,
                isSfxMuted: _audioService.IsSfxMuted
            );
            return UniTask.FromResult(state);
        }
    }
}
