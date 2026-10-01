using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

namespace Core.UI.Controls
{
    public class BaseText : TextMeshProUGUI
    {
        private MotionHandle _activeHandle;

        public void SetFormatted<T>(string format, T value)
        {
            text = string.Format(format, value);
        }

        public async UniTask TypewriterAsync(string fullText, float charsPerSecond, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(fullText) || charsPerSecond <= 0)
            {
                text = fullText;
                return;
            }

            CancelActive();
            var length = fullText.Length;
            var duration = length / charsPerSecond;
            text = fullText;
            maxVisibleCharacters = 0;

            var txt = this;
            _activeHandle = LMotion.Create(0, length, duration)
                .Bind(txt, static (charCount, target) => target.maxVisibleCharacters = charCount);

            await _activeHandle.ToUniTask(ct);
        }

        public async UniTask CountUpAsync(int from, int to, float duration, CancellationToken ct = default)
        {
            if (duration <= 0)
            {
                SetText("{0}", to);
                return;
            }

            CancelActive();

            var txt = this;
            _activeHandle = LMotion.Create(from, to, duration)
                .WithEase(Ease.OutQuad)
                .Bind(txt, static (val, target) => target.SetText("{0}", val));

            await _activeHandle.ToUniTask(ct);
        }

        private void CancelActive()
        {
            if (_activeHandle.IsActive()) _activeHandle.Cancel();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CancelActive();
        }
    }
}
