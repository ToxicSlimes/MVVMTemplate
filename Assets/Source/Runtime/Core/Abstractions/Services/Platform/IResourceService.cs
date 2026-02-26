using Core.Data.Domain.Models.ImageGen;

namespace Core.Abstractions.Services.Platform
{
    public interface IResourceService
    {
        void ReleaseImage(IImageHandle image);
        void ReleaseImageImmediate(IImageHandle image);
    }
}
