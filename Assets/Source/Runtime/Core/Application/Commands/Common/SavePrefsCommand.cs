using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Commands.Common
{
    public readonly struct SavePrefsCommand<T> : IRequest<Nothing>
        where T : class
    {
        public readonly string Key;
        public readonly T Value;

        public SavePrefsCommand(string key, T value)
        {
            Key = key;
            Value = value;
        }
    }
}
