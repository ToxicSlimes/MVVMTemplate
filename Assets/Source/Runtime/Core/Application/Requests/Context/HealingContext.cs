using Core.Abstractions.MVVM.Views;

namespace Core.Application.Requests.Context
{
    public readonly struct HealingContext : IViewContext
    {
        public readonly string PreselectedHeroId;

        public HealingContext(string preselectedHeroId = null)
        {
            PreselectedHeroId = preselectedHeroId;
        }
    }
}
