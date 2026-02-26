using System;
using System.Collections.Generic;
using System.Threading;
using Core.Data.Domain.Models.Heroes;
using Core.Data.Domain.Models.HeroesGridPanel;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.MVVM.Models.HeroesGridPanel
{
    public interface IHeroesGridPanelModel : IDisposable
    {
        IReadOnlyList<HeroData> MyHeroes { get; }
        IReadOnlyList<HeroData> ShopHeroes { get; }
        IReadOnlyList<HeroData> DeadHeroes { get; }
        HeroesGridTab CurrentTab { get; }
        HeroesGridMode CurrentMode { get; }
        string SelectedHeroId { get; }

        event Action<IReadOnlyList<HeroData>> MyHeroesChanged;
        event Action<IReadOnlyList<HeroData>> ShopHeroesChanged;
        event Action<IReadOnlyList<HeroData>> DeadHeroesChanged;
        event Action<HeroesGridTab> TabChanged;
        event Action<string> SelectedHeroIdChanged;

        void SetMode(HeroesGridMode mode);
        void SelectTab(HeroesGridTab tab);
        void SelectHero(string heroId);

        UniTask LoadDataAsync(CancellationToken ct = default);

        [Obsolete("Use LoadDataAsync")]
        void LoadData();
    }
}
