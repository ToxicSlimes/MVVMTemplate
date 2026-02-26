using System;
using Core.Abstractions.MVVM.Canvas;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Abstractions.MVVM.Views
{
    public class ViewShowParams
    {
        public ViewDisplayMode DisplayMode { get; private set; } = ViewDisplayMode.Single;

        public ViewTransitionStyle TransitionStyle { get; private set; } = ViewTransitionStyle.Fade;

        public CanvasLayer? TargetCanvas { get; private set; }

        public Transform CustomParent { get; private set; }

        public Func<UniTask> OnShown { get; private set; }

        public Func<UniTask> OnHidden { get; private set; }

        public float? AnimationDuration { get; private set; }

        public ViewShowParams SetDisplayMode(ViewDisplayMode mode)
        {
            DisplayMode = mode;
            return this;
        }

        public ViewShowParams SetTransition(ViewTransitionStyle style)
        {
            TransitionStyle = style;
            return this;
        }

        public ViewShowParams SetCanvas(CanvasLayer layer)
        {
            TargetCanvas = layer;
            return this;
        }

        public ViewShowParams SetCustomParent(Transform parent)
        {
            CustomParent = parent;
            return this;
        }

        public ViewShowParams SetOnShown(Func<UniTask> callback)
        {
            OnShown = callback;
            return this;
        }

        public ViewShowParams SetOnHidden(Func<UniTask> callback)
        {
            OnHidden = callback;
            return this;
        }

        public ViewShowParams SetAnimationDuration(float duration)
        {
            if (duration < 0)
            {
                throw new ArgumentException("Animation duration cannot be negative", nameof(duration));
            }
            AnimationDuration = duration;
            return this;
        }

        public static ViewShowParams Default() => new ViewShowParams();

        public static ViewShowParams Instant() => new ViewShowParams()
            .SetTransition(ViewTransitionStyle.None)
            .SetAnimationDuration(0f);

        public static ViewShowParams Overlay() => new ViewShowParams()
            .SetDisplayMode(ViewDisplayMode.Additive)
            .SetCanvas(CanvasLayer.Overlay);
    }
}
