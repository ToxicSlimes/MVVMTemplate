using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.UI.Effects
{
    [RequireComponent(typeof(RawImage))]
    public sealed class BlurBackdrop : MonoBehaviour
    {
        private static readonly int OffsetId = Shader.PropertyToID("_Offset");

        [SerializeField] private Material _blur;
        [SerializeField, Min(1)] private int _downsample = 4;
        [SerializeField, Min(1)] private int _iterations = 4;

        private RawImage _image;
        private Canvas _canvas;
        private RenderTexture _texture;

        private void Awake()
        {
            _image = GetComponent<RawImage>();
            _canvas = GetComponentInParent<Canvas>().rootCanvas;
        }

        private void OnEnable() => StartCoroutine(Capture());

        private void OnDisable()
        {
            StopAllCoroutines();
            _canvas.enabled = true;
            _image.texture = null;
            Release();
        }

        private IEnumerator Capture()
        {
            _canvas.enabled = false;
            yield return new WaitForEndOfFrame();
            var screen = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(screen);
            Release();
            var width = Mathf.Max(1, Screen.width / _downsample);
            var height = Mathf.Max(1, Screen.height / _downsample);
            _texture = RenderTexture.GetTemporary(width, height, 0);
            var swap = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(screen, _texture);
            RenderTexture.ReleaseTemporary(screen);
            for (var i = 0; i < _iterations; i++)
            {
                _blur.SetFloat(OffsetId, i);
                Graphics.Blit(_texture, swap, _blur);
                (_texture, swap) = (swap, _texture);
            }

            RenderTexture.ReleaseTemporary(swap);
            _image.texture = _texture;
            _image.uvRect = SystemInfo.graphicsUVStartsAtTop ? new Rect(0f, 1f, 1f, -1f) : new Rect(0f, 0f, 1f, 1f);
            _canvas.enabled = true;
        }

        private void Release()
        {
            if (_texture == null)
            {
                return;
            }

            RenderTexture.ReleaseTemporary(_texture);
            _texture = null;
        }
    }
}
