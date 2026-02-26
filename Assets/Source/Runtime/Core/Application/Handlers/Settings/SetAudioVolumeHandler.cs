using System.Threading;
using Core.Abstractions.Services;
using Core.Application.Commands.Settings;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Settings
{
    public sealed class SetAudioVolumeHandler : IRequestHandler<SetAudioVolumeCommand, Nothing>
    {
        private readonly IAudioService _audioService;

        public SetAudioVolumeHandler(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public UniTask<Nothing> Handle(SetAudioVolumeCommand command, CancellationToken ct)
        {
            switch (command.Channel)
            {
                case AudioChannel.Master:
                    _audioService.SetMasterVolume(command.Value);
                    break;
                case AudioChannel.Music:
                    _audioService.SetMusicVolume(command.Value);
                    break;
                case AudioChannel.Sfx:
                    _audioService.SetSfxVolume(command.Value);
                    break;
            }

            return UniTask.FromResult(Nothing.Value);
        }
    }
}
