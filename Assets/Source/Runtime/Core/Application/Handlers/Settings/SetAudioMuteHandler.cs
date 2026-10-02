using System.Threading;
using Core.Abstractions.Services;
using Core.Application.Commands.Settings;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Settings
{
    public sealed class SetAudioMuteHandler : IRequestHandler<SetAudioMuteCommand, Nothing>
    {
        private readonly IAudioService _audioService;

        public SetAudioMuteHandler(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public UniTask<Nothing> Handle(SetAudioMuteCommand command, CancellationToken ct)
        {
            switch (command.Channel)
            {
                case AudioChannel.Master:
                    _audioService.MuteMaster(command.Muted);
                    break;
                case AudioChannel.Music:
                    _audioService.MuteMusic(command.Muted);
                    break;
                case AudioChannel.Sfx:
                    _audioService.MuteSfx(command.Muted);
                    break;
            }

            return UniTask.FromResult(Nothing.Value);
        }
    }
}
