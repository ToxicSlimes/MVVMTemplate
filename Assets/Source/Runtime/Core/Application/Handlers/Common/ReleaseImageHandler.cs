using System.Threading;
using Core.Abstractions.Services;
using Core.Abstractions.Services.Platform;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Commands.Common;

namespace Core.Application.Handlers.Common
{
    public sealed class ReleaseImageHandler : IRequestHandler<ReleaseImageCommand, Nothing>
    {
        private readonly IResourceService _resourceService;
        private readonly ILogger _logger;

        public ReleaseImageHandler(IResourceService resourceService, ILogger logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        public UniTask<Nothing> Handle(ReleaseImageCommand command, CancellationToken ct)
        {
            if (command.Image != null)
            {
                _resourceService.ReleaseImage(command.Image);
                _logger.Info("[ReleaseImageHandler] Image released");
            }

            return Nothing.Task;
        }
    }
}
