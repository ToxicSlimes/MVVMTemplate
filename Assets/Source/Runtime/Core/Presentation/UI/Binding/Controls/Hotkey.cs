using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Core.UI.Controls
{
    public sealed class Hotkey : MonoBehaviour
    {
        private static readonly List<RaycastResult> Hits = new();
        private static int _tappedFrame = -1;

        [SerializeField] private Key _key;
        [SerializeField] private GameObject _hint;

        private void Awake()
        {
            _hint.SetActive(UnityEngine.Application.isEditor);
            _hint.GetComponentInChildren<BaseText>(true).SetText(Label(_key));
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[_key].wasPressedThisFrame || _tappedFrame == Time.frameCount)
            {
                return;
            }

            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var rect = (RectTransform)transform;
            var tap = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center)),
            };
            EventSystem.current.RaycastAll(tap, Hits);
            if (Hits.Count == 0 || !Hits[0].gameObject.transform.IsChildOf(canvas.transform))
            {
                return;
            }

            _tappedFrame = Time.frameCount;
            tap.pointerCurrentRaycast = Hits[0];
            tap.pointerPressRaycast = Hits[0];
            ExecuteEvents.ExecuteHierarchy(Hits[0].gameObject, tap, ExecuteEvents.pointerClickHandler);
        }

        private static string Label(Key key) => key switch
        {
            Key.Escape => "Esc",
            Key.LeftCtrl => "Ctrl",
            _ => key.ToString().Replace("Digit", string.Empty),
        };
    }
}
