using UnityEngine;

namespace Core.Abstractions.Popup
{
    public interface IPopupPool
    {
        IPopup Get();
        void Return(IPopup popup);
        void Clear();
        int CountActive { get; }
        int CountInactive { get; }
    }

    public interface IPopupPoolFactory
    {
        IPopupPool CreatePool(GameObject prefab, Transform poolContainer);
    }
}