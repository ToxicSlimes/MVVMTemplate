using Core.Abstractions.MVVM.Views;

namespace Core.Application.Requests.Context
{
    public readonly struct TrainingContext : IViewContext
    {
        public readonly string PreselectedHeroId;

        public TrainingContext(string preselectedHeroId = null)
        {
            PreselectedHeroId = preselectedHeroId;
        }
    }
}
