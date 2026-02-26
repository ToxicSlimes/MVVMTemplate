using Core.Abstractions.Services.Platform;
using Core.Data.Domain.Models.ImageGen;

namespace Core.Infrastructure.Services.Platform
{
    public class ResourceService : IResourceService
    {
        public void ReleaseImage(IImageHandle image)
        {
            image?.Dispose();
        }

        public void ReleaseImageImmediate(IImageHandle image)
        {
            image?.Dispose();
        }
    }
}
