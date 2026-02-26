using System.Threading;
using Core.Abstractions.Services;
using Core.Application.Commands.Settings;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Settings
{
    public sealed class ResetAudioHandler : IRequestHandler<ResetAudioCommand, Nothing>
    {
        private readonly IAudioService _audioService;

        public ResetAudioHandler(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public UniTask<Nothing> Handle(ResetAudioCommand command, CancellationToken ct)
        {
            _audioService.ResetToDefaults();
            return UniTask.FromResult(Nothing.Value);
        }
    }
}
