using UnityEngine;

namespace Core.Abstractions.Popup
{
    public class PopupShowParams
    {
        public PopupPosition Position { get; private set; } = PopupPosition.Auto;
        public Vector2 Offset { get; private set; } = Vector2.zero;
        public float ShowDelay { get; private set; }
        public bool ClampToScreen { get; private set; } = true;
        public float ScreenPadding { get; private set; } = 10f;
        public bool AllowFlip { get; private set; } = true;
        public bool FollowTarget { get; private set; }
        public bool Exclusive { get; private set; }

        public static PopupShowParams Default() => new();

        public static PopupShowParams Top(float offsetY = 10f) =>
            new PopupShowParams().SetPosition(PopupPosition.Top).SetOffset(new Vector2(0, offsetY));

        public static PopupShowParams Bottom(float offsetY = 10f) =>
            new PopupShowParams().SetPosition(PopupPosition.Bottom).SetOffset(new Vector2(0, -offsetY));

        public static PopupShowParams Left(float offsetX = 10f) =>
            new PopupShowParams().SetPosition(PopupPosition.Left).SetOffset(new Vector2(-offsetX, 0));

        public static PopupShowParams Right(float offsetX = 10f) =>
            new PopupShowParams().SetPosition(PopupPosition.Right).SetOffset(new Vector2(offsetX, 0));

        public PopupShowParams SetPosition(PopupPosition position)
        {
            Position = position;
            return this;
        }

        public PopupShowParams SetOffset(Vector2 offset)
        {
            Offset = offset;
            return this;
        }

        public PopupShowParams SetDelay(float delay)
        {
            ShowDelay = delay;
            return this;
        }

        public PopupShowParams SetScreenPadding(float padding)
        {
            ScreenPadding = padding;
            return this;
        }

        public PopupShowParams DisableClamping()
        {
            ClampToScreen = false;
            return this;
        }

        public PopupShowParams DisableFlip()
        {
            AllowFlip = false;
            return this;
        }

        public PopupShowParams EnableFollowTarget()
        {
            FollowTarget = true;
            return this;
        }

        public PopupShowParams SetExclusive(bool exclusive = true)
        {
            Exclusive = exclusive;
            return this;
        }
    }
}
