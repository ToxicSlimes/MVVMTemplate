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
            if (!_configProvider.IsConfigLoaded<NetworkConfig>())
            {
                options = default;
                return false;
            }

            var config = _configProvider.GetConfig<NetworkConfig>();
            options = new NetworkSessionOptions(
                config.DefaultPlayerCount,
                config.SessionVisible,
                config.SessionOpen,
                config.DefaultScenePath);
            return true;
        }
    }
}
