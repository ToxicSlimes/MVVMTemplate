using System;
using Core.Network.Abstractions.Gateway.Transport;
using Core.Network.Abstractions.Services;

namespace Core.Network.FusionAdapter.Gateway
{
    public sealed class FusionGatewayAuthority : IGatewayAuthority
    {
        private readonly INetworkRunnerController _runnerController;

        public bool CanHandleRequests => _runnerController.IsStateAuthority;

        public FusionGatewayAuthority(INetworkRunnerController runnerController)
        {
            _runnerController = runnerController ??
                                throw new ArgumentNullException(nameof(runnerController));
        }
    }
}
