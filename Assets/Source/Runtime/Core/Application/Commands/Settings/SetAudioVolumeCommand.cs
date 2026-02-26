using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Settings
{
    public enum AudioChannel { Master, Music, Sfx }

    public readonly struct SetAudioVolumeCommand : IRequest<Nothing>
    {
        public readonly AudioChannel Channel;
        public readonly float Value;

        public SetAudioVolumeCommand(AudioChannel channel, float value)
        {
            Channel = channel;
            Value = value;
        }
    }
}
