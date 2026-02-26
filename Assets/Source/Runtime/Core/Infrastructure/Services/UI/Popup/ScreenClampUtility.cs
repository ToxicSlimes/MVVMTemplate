using Core.Abstractions.Popup;
using UnityEngine;

namespace Core.Infrastructure.Popup
{
    public static class ScreenClampUtility
    {
        public static Vector2 ClampToCanvas(Vector2 position, Vector2 popupSize, Vector2 canvasSize, float padding)
        {
            var halfPopup = popupSize * 0.5f;
            var halfCanvas = canvasSize * 0.5f;

            var minX = -halfCanvas.x + halfPopup.x + padding;
            var maxX = halfCanvas.x - halfPopup.x - padding;
            var minY = -halfCanvas.y + halfPopup.y + padding;
            var maxY = halfCanvas.y - halfPopup.y - padding;

            return new Vector2(
                Mathf.Clamp(position.x, minX, maxX),
                Mathf.Clamp(position.y, minY, maxY));
        }

        public static bool IsOutOfBounds(Vector2 position, Vector2 popupSize, Vector2 canvasSize, float padding)
        {
            var halfPopup = popupSize * 0.5f;
            var halfCanvas = canvasSize * 0.5f;

            return position.x - halfPopup.x < -halfCanvas.x + padding ||
                   position.x + halfPopup.x > halfCanvas.x - padding ||
                   position.y - halfPopup.y < -halfCanvas.y + padding ||
                   position.y + halfPopup.y > halfCanvas.y - padding;
        }

        public static PopupPosition GetBestPosition(
            Vector2 targetPositionInCanvas,
            Vector2 popupSize,
            Vector2 canvasSize,
            float padding)
        {
            var halfCanvas = canvasSize * 0.5f;

            var spaceTop = halfCanvas.y - targetPositionInCanvas.y - padding;
            var spaceBottom = targetPositionInCanvas.y + halfCanvas.y - padding;
            var spaceRight = halfCanvas.x - targetPositionInCanvas.x - padding;
            var spaceLeft = targetPositionInCanvas.x + halfCanvas.x - padding;

            if (spaceBottom >= popupSize.y) return PopupPosition.Bottom;
            if (spaceTop >= popupSize.y) return PopupPosition.Top;
            if (spaceRight >= popupSize.x) return PopupPosition.Right;
            if (spaceLeft >= popupSize.x) return PopupPosition.Left;

            return spaceTop >= spaceBottom ? PopupPosition.Top : PopupPosition.Bottom;
        }

        public static PopupPosition FlipPosition(PopupPosition position)
        {
            return position switch
            {
                PopupPosition.Top => PopupPosition.Bottom,
                PopupPosition.Bottom => PopupPosition.Top,
                PopupPosition.Left => PopupPosition.Right,
                PopupPosition.Right => PopupPosition.Left,
                PopupPosition.TopLeft => PopupPosition.BottomRight,
                PopupPosition.TopRight => PopupPosition.BottomLeft,
                PopupPosition.BottomLeft => PopupPosition.TopRight,
                PopupPosition.BottomRight => PopupPosition.TopLeft,
                _ => position
            };
        }

        public static Vector2 CalculateOffset(
            PopupPosition position,
            Vector2 halfTarget,
            Vector2 halfPopup,
            Vector2 userOffset)
        {
            return position switch
            {
                PopupPosition.Top => new Vector2(
                    userOffset.x,
                    halfTarget.y + halfPopup.y + userOffset.y),

                PopupPosition.Bottom => new Vector2(
                    userOffset.x,
                    -halfTarget.y - halfPopup.y + userOffset.y),

                PopupPosition.Left => new Vector2(
                    -halfTarget.x - halfPopup.x + userOffset.x,
                    userOffset.y),

                PopupPosition.Right => new Vector2(
                    halfTarget.x + halfPopup.x + userOffset.x,
                    userOffset.y),

                PopupPosition.TopLeft => new Vector2(
                    -halfTarget.x - halfPopup.x + userOffset.x,
                    halfTarget.y + halfPopup.y + userOffset.y),

                PopupPosition.TopRight => new Vector2(
                    halfTarget.x + halfPopup.x + userOffset.x,
                    halfTarget.y + halfPopup.y + userOffset.y),

                PopupPosition.BottomLeft => new Vector2(
                    -halfTarget.x - halfPopup.x + userOffset.x,
                    -halfTarget.y - halfPopup.y + userOffset.y),

                PopupPosition.BottomRight => new Vector2(
                    halfTarget.x + halfPopup.x + userOffset.x,
                    -halfTarget.y - halfPopup.y + userOffset.y),

                PopupPosition.Center => userOffset,
                _ => userOffset
            };
        }

        public static Vector2 GetMousePosition(RectTransform canvasRect, Vector2 screenPosition, Camera camera = null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                camera,
                out var localPoint);
            return localPoint;
        }
    }
}
