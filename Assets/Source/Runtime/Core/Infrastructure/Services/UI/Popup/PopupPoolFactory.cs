using Core.Abstractions.Popup;
using UnityEngine;

namespace Core.Abstractions.Popup
{
    public class PopupPoolFactory : IPopupPoolFactory
    {
        public IPopupPool CreatePool(GameObject prefab, Transform poolContainer)
        {
            return new PopupPool(prefab, poolContainer);
        }
    }
}
