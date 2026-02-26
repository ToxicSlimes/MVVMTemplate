using UnityEngine;

namespace Core.Abstractions.MVVM.Canvas
{
    public interface ICanvasProvider
    {
        UnityEngine.Canvas GetCanvas(CanvasLayer layer);

        Transform GetCanvasTransform(CanvasLayer layer);

        void RegisterCanvas(CanvasLayer layer, UnityEngine.Canvas canvas);

        void UnregisterCanvas(CanvasLayer layer);

        bool HasCanvas(CanvasLayer layer);
    }
}
