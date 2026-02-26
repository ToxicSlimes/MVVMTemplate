using Core.Data.Domain.Models.HeroesGridPanel;
using Core.Abstractions.MVVM.Views;

namespace Core.Application.Requests.Context
{
    public readonly struct HeroesGridContext : IViewContext
    {
        public readonly HeroesGridMode Mode;

        public HeroesGridContext(HeroesGridMode mode)
        {
            Mode = mode;
        }
    }
}
