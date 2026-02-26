using System;
using System.Collections.Generic;
using Core.Data.Domain.Models.ServicePanel;

namespace Core.Abstractions.MVVM.Models.TrainingPanel
{
    public interface ITrainingPanelModel : IDisposable
    {
        string Title { get; }
        IReadOnlyList<ServiceRowData> Rows { get; }
        string SelectedSlotId { get; }

        event Action<IReadOnlyList<ServiceRowData>> RowsChanged;
        event Action<string> SelectedSlotIdChanged;

        void LoadData();
        void SelectSlot(string slotId);
        bool AssignHeroToSlot(string slotId, string heroId);
        bool RemoveHeroFromSlot(string slotId);
        bool UnlockSlot(string slotId);
        string AssignHeroToFirstAvailable(string heroId);
    }
}
