using System.Threading;
using Core.Abstractions.Config;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Queries.Common;

namespace Core.Application.Handlers.Common
{
    public sealed class GetConfigHandler<TConfig> : IRequestHandler<GetConfigQuery<TConfig>, TConfig>
        where TConfig : ConfigBase
    {
        private readonly IConfigProvider _configProvider;

        public GetConfigHandler(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public UniTask<TConfig> Handle(GetConfigQuery<TConfig> query, CancellationToken ct)
        {
            var config = _configProvider.GetConfig<TConfig>();
            return UniTask.FromResult(config);
        }
    }
}
