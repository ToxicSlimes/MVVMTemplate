using System.Threading;
using Core.Abstractions.Services;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Commands.Common;

namespace Core.Application.Handlers.Common
{
    public sealed class SavePrefsHandler<T> : IRequestHandler<SavePrefsCommand<T>, Nothing>
        where T : class
    {
        private readonly IPrefsService _prefsService;

        public SavePrefsHandler(IPrefsService prefsService)
        {
            _prefsService = prefsService;
        }

        public UniTask<Nothing> Handle(SavePrefsCommand<T> command, CancellationToken ct)
        {
            _prefsService.Save(command.Key, command.Value);
            return NothingTask.Completed;
        }
    }
}
