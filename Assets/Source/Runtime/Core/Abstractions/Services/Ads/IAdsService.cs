using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services.Ads
{
    public interface IAdsService
    {
        UniTask<bool> ShowRewardedAsync(string placement, CancellationToken cancellationToken);

        UniTask<bool> ShowInterstitialAsync(CancellationToken cancellationToken);

        void SetBannerVisible(bool visible);
    }
}
