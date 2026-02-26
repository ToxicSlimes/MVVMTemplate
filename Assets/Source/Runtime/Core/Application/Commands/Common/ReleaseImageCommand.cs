using Core.Infrastructure.Mediator.Abstractions;
using Core.Data.Domain.Models.ImageGen;

namespace Core.Application.Commands.Common
{
    public readonly struct ReleaseImageCommand : IRequest<Nothing>
    {
        public readonly IImageHandle Image;

        public ReleaseImageCommand(IImageHandle image)
        {
            Image = image;
        }
    }
}
