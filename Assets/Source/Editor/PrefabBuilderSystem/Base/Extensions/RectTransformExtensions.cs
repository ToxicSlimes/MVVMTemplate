#if UNITY_EDITOR
using UnityEngine;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Extensions
{
    public enum AnchorPreset
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        TopStretch,
        MiddleStretch,
        BottomStretch,
        LeftStretch,
        CenterStretch,
        RightStretch,
        StretchAll
    }

    public static class RectTransformExtensions
    {
        public static void ApplyAnchorPreset(this RectTransform rt, AnchorPreset preset, bool setPivot = true)
        {
            var (anchorMin, anchorMax, pivot) = GetAnchorValues(preset);
            
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            
            if (setPivot)
            {
                rt.pivot = pivot;
            }
        }

        public static void SetAnchoredPositionX(this RectTransform rt, float x)
        {
            rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
        }

        public static void SetAnchoredPositionY(this RectTransform rt, float y)
        {
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        }

        public static void SetSizeX(this RectTransform rt, float width)
        {
            rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
        }

        public static void SetSizeY(this RectTransform rt, float height)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        public static void Stretch(this RectTransform rt, float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static void StretchHorizontal(this RectTransform rt, float left = 0, float right = 0)
        {
            rt.anchorMin = new Vector2(0, rt.anchorMin.y);
            rt.anchorMax = new Vector2(1, rt.anchorMax.y);
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void StretchVertical(this RectTransform rt, float top = 0, float bottom = 0)
        {
            rt.anchorMin = new Vector2(rt.anchorMin.x, 0);
            rt.anchorMax = new Vector2(rt.anchorMax.x, 1);
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void AnchorToLeft(this RectTransform rt, float offset = 0)
        {
            rt.anchorMin = new Vector2(0, rt.anchorMin.y);
            rt.anchorMax = new Vector2(0, rt.anchorMax.y);
            rt.pivot = new Vector2(0, rt.pivot.y);
            rt.anchoredPosition = new Vector2(offset, rt.anchoredPosition.y);
        }

        public static void AnchorToRight(this RectTransform rt, float offset = 0)
        {
            rt.anchorMin = new Vector2(1, rt.anchorMin.y);
            rt.anchorMax = new Vector2(1, rt.anchorMax.y);
            rt.pivot = new Vector2(1, rt.pivot.y);
            rt.anchoredPosition = new Vector2(-offset, rt.anchoredPosition.y);
        }

        public static void AnchorToCenter(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        private static (Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot) GetAnchorValues(AnchorPreset preset)
        {
            return preset switch
            {
                AnchorPreset.TopLeft => (new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1)),
                AnchorPreset.TopCenter => (new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1)),
                AnchorPreset.TopRight => (new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1)),
                AnchorPreset.MiddleLeft => (new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f)),
                AnchorPreset.MiddleCenter => (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)),
                AnchorPreset.MiddleRight => (new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f)),
                AnchorPreset.BottomLeft => (new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0)),
                AnchorPreset.BottomCenter => (new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0)),
                AnchorPreset.BottomRight => (new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0)),
                AnchorPreset.TopStretch => (new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1)),
                AnchorPreset.MiddleStretch => (new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f)),
                AnchorPreset.BottomStretch => (new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0)),
                AnchorPreset.LeftStretch => (new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f)),
                AnchorPreset.CenterStretch => (new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f)),
                AnchorPreset.RightStretch => (new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f)),
                AnchorPreset.StretchAll => (Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f)),
                _ => (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f))
            };
        }
    }
}
#endif
