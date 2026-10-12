using System;

namespace Core.Network.Abstractions.Gateway.Transport
{
    public interface IGatewayErrorReporter
    {
        void Report(Exception exception, string route);
    }
}
