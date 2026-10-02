using Core.Mediator.Abstractions;

namespace Core.Application.Commands.Settings
{
    public readonly struct SetAudioMuteCommand : IRequest<Nothing>
    {
        public readonly AudioChannel Channel;
        public readonly bool Muted;

        public SetAudioMuteCommand(AudioChannel channel, bool muted)
        {
            Channel = channel;
            Muted = muted;
        }
    }
}
