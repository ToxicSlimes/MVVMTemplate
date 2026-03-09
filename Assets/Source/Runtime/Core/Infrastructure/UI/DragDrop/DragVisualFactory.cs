using UnityEngine;
using UnityEngine.UI;

namespace Core.Infrastructure.UI.DragDrop
{
    public static class DragVisualFactory
    {
        public static GameObject CreateGhost(RectTransform source, Canvas targetCanvas, float alpha)
        {
            var ghost = Object.Instantiate(source.gameObject, targetCanvas.transform);
            ghost.name = "DragGhost";

            StripInteractiveComponents(ghost);

            var canvasGroup = ghost.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = ghost.AddComponent<CanvasGroup>();

            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var ghostRect = ghost.GetComponent<RectTransform>();
            ghostRect.sizeDelta = source.sizeDelta;

            return ghost;
        }

        private static void StripInteractiveComponents(GameObject ghost)
        {
            var buttons = ghost.GetComponentsInChildren<Button>(true);
            
            for (var i = 0; i < buttons.Length; i++)
                Object.Destroy(buttons[i]);

            var toggles = ghost.GetComponentsInChildren<Toggle>(true);
            
            for (var i = 0; i < toggles.Length; i++)
                Object.Destroy(toggles[i]);

            var scrollRects = ghost.GetComponentsInChildren<ScrollRect>(true);
            
            for (var i = 0; i < scrollRects.Length; i++)
                Object.Destroy(scrollRects[i]);

            var inputFields = ghost.GetComponentsInChildren<InputField>(true);
            
            for (var i = 0; i < inputFields.Length; i++)
                Object.Destroy(inputFields[i]);

            var selectables = ghost.GetComponentsInChildren<Selectable>(true);
            
            for (var i = 0; i < selectables.Length; i++)
            {
                // if (selectables[i] is not Image && selectables[i] is not RawImage)
                //     Object.Destroy(selectables[i]);
            }
        }
    }
}
