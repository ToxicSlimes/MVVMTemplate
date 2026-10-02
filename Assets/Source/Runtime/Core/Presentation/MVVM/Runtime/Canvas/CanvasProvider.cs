using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Canvas;
using UnityEngine;

namespace Core.Presentation.MVVM.Canvas
{
    public class CanvasProvider : ICanvasProvider
    {
        private readonly Dictionary<CanvasLayer, UnityEngine.Canvas> _canvases = new();
        private readonly object _lock = new();

        public UnityEngine.Canvas GetCanvas(CanvasLayer layer)
        {
            lock (_lock)
            {
                return _canvases.TryGetValue(layer, out var canvas) ? canvas : null;
            }
        }

        public Transform GetCanvasTransform(CanvasLayer layer)
        {
            var canvas = GetCanvas(layer);
            return canvas != null ? canvas.transform : null;
        }

        public void RegisterCanvas(CanvasLayer layer, UnityEngine.Canvas canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas), $"Cannot register null canvas for layer {layer}");
            }

            lock (_lock)
            {
                if (_canvases.ContainsKey(layer))
                {
                    Debug.LogWarning($"Canvas for layer {layer} is already registered. Overwriting existing canvas.");
                }

                _canvases[layer] = canvas;
                Debug.Log($"Canvas registered for layer {layer}: {canvas.name}");
            }
        }

        public void UnregisterCanvas(CanvasLayer layer)
        {
            lock (_lock)
            {
                if (_canvases.Remove(layer))
                {
                    Debug.Log($"Canvas unregistered for layer {layer}");
                }
                else
                {
                    Debug.LogWarning($"No canvas registered for layer {layer}");
                }
            }
        }

        public bool HasCanvas(CanvasLayer layer)
        {
            lock (_lock)
            {
                return _canvases.ContainsKey(layer);
            }
        }
    }
}
