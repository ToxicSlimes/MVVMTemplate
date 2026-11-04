using System.Threading;
using Core.Abstractions.Services.Ads;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Services.Ads
{
    public sealed class NullAdsService : IAdsService
    {
        public UniTask<bool> ShowRewardedAsync(string placement, CancellationToken cancellationToken) => UniTask.FromResult(false);

        public UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken) => UniTask.FromResult(false);

        public void SetBannerVisible(bool visible) { }
    }
}
