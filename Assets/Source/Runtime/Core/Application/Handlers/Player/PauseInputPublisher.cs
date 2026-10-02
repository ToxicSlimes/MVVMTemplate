using System;
using System.Threading;
using Core.Abstractions.Services.Input;
using Core.Application.Notifications.Player;
using Core.Mediator.Abstractions;
using Core.Network.Abstractions.Services;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Player
{
    public sealed class PauseInputPublisher : IDisposable
    {
        private readonly CancellationTokenSource _lifetime = new();

        public PauseInputPublisher(
            IInputService input,
            IMediator mediator,
            INetworkRunnerController runner)
        {
            Run(input, mediator, runner, _lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private static async UniTaskVoid Run(
            IInputService input,
            IMediator mediator,
            INetworkRunnerController runner,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow())
                {
                    return;
                }

                if (runner.IsRunning && input.IsMenuPressed)
                {
                    await mediator.Publish(new PauseToggleNotification());
                }
            }
        }
    }
}
