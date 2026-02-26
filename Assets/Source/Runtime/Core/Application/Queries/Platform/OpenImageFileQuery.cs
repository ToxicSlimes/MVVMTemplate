using Core.Abstractions.Services.Platform;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Queries.Platform
{
    public readonly struct OpenImageFileQuery : IRequest<FileDialogResult>
    {
        public readonly string Title;
        public readonly string Filter;

        public OpenImageFileQuery(string title = "Select Image", string filter = "png,jpg,jpeg,webp")
        {
            Title = title;
            Filter = filter;
        }
    }
}
