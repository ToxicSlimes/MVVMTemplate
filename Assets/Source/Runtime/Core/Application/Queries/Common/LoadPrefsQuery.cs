using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Queries.Common
{
    public readonly struct LoadPrefsQuery<T> : IRequest<T>
        where T : class
    {
        public readonly string Key;
        public readonly T DefaultValue;

        public LoadPrefsQuery(string key, T defaultValue = null)
        {
            Key = key;
            DefaultValue = defaultValue;
        }
    }
}
