using System;
using Core.Abstractions.Services;
using Core.Network.Abstractions.Gateway.Transport;

namespace Core.Network.FusionAdapter.Gateway
{
    public sealed class GatewayErrorReporter : IGatewayErrorReporter
    {
        private readonly ILogger _logger;

        public GatewayErrorReporter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Report(Exception exception, string route)
        {
            _logger.Error(exception, "[Gateway] Server request failed for route {0}", route);
        }
    }
}
