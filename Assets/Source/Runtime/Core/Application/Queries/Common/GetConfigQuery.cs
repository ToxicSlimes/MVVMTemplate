using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Queries.Common
{
    public readonly struct GetConfigQuery<TConfig> : IRequest<TConfig>
        where TConfig : class
    {
    }
}
