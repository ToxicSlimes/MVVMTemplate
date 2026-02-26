using System.Threading;
using Core.Abstractions.Services;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Queries.Common;

namespace Core.Application.Handlers.Common
{
    public sealed class LoadPrefsHandler<T> : IRequestHandler<LoadPrefsQuery<T>, T>
        where T : class
    {
        private readonly IPrefsService _prefsService;

        public LoadPrefsHandler(IPrefsService prefsService)
        {
            _prefsService = prefsService;
        }

        public UniTask<T> Handle(LoadPrefsQuery<T> query, CancellationToken ct)
        {
            var result = _prefsService.Load<T>(query.Key);
            return UniTask.FromResult(result ?? query.DefaultValue);
        }
    }
}
