using Core.Abstractions.Config;
using Core.Network.Abstractions.Services;

namespace Core.Infrastructure.Configs.Services
{
    public sealed class NetworkSessionOptionsProvider : INetworkSessionOptionsProvider
    {
        private readonly IConfigProvider _configProvider;

        public NetworkSessionOptionsProvider(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public bool TryGet(out NetworkSessionOptions options)
        {
            var config = _configProvider.GetConfig<NetworkConfig>();
            if (config == null)
            {
                options = default;
                return false;
            }

            options = new NetworkSessionOptions(
                config.DefaultPlayerCount,
                config.SessionVisible,
                config.SessionOpen,
                config.DefaultScenePath);
            return true;
        }
    }
}
