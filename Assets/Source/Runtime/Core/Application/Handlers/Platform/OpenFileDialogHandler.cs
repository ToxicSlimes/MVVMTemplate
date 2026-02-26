using System.Threading;
using Core.Abstractions.Services.Platform;
using Core.Infrastructure.Mediator.Handlers;
using Cysharp.Threading.Tasks;
using Core.Application.Queries.Platform;

namespace Core.Application.Handlers.Platform
{
    public sealed class OpenImageFileHandler : IRequestHandler<OpenImageFileQuery, FileDialogResult>
    {
        private readonly IFileDialogService _fileDialogService;

        public OpenImageFileHandler(IFileDialogService fileDialogService)
        {
            _fileDialogService = fileDialogService;
        }

        public async UniTask<FileDialogResult> Handle(OpenImageFileQuery query, CancellationToken ct)
        {
            return await _fileDialogService.OpenImageFileAsync(query.Title, query.Filter);
        }
    }
}
