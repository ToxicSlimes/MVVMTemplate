using Core.Abstractions.MVVM.Views;

namespace Core.Application.Requests.Context
{
    public readonly struct HeroDetailContext : IViewContext
    {
        public readonly string HeroId;

        public HeroDetailContext(string heroId)
        {
            HeroId = heroId;
        }
    }
}
