using Core.Abstractions.Services.Input;
using Core.Application.Notifications.Player;
using Core.Infrastructure.Mediator.Abstractions;
using Cysharp.Threading.Tasks;
using Fusion;
using Reflex.Attributes;

namespace Core.Network.FusionAdapter.Player
{
    public sealed class PlayerPauseInput : NetworkBehaviour
    {
        [Inject] private IInputService _input;
        [Inject] private IMediator _mediator;

        public override void Render()
        {
            if (!HasInputAuthority || !_input.IsMenuPressed)
            {
                return;
            }

            _mediator.Publish(new PauseToggleNotification()).Forget();
        }
    }
}
