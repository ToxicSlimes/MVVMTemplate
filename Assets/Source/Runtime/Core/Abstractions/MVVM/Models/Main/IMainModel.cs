using System;
using System.Collections.Generic;
using System.Threading;
using Core.Abstractions.MVVM.Models;
using Core.Data.Domain.Models.Heroes;
using Core.Data.Domain.Models.Main;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.MVVM.Models.Main
{
    public interface IMainModel : IModel
    {
        event Action<IReadOnlyList<HeroData>> HeroesChanged;
        event Action<IReadOnlyList<StatRowData>> StatsChanged;
        event Action<IReadOnlyList<ResourceData>> ResourcesChanged;
        event Action<string> SelectedHeroIdChanged;

        IReadOnlyList<HeroData> Heroes { get; }
        IReadOnlyList<StatRowData> Stats { get; }
        IReadOnlyList<ResourceData> Resources { get; }
        string SelectedHeroId { get; }

        UniTask LoadDataAsync(CancellationToken ct = default);

        [Obsolete("Use LoadDataAsync")]
        void LoadData();

        void SelectHero(string heroId);
    }
}
