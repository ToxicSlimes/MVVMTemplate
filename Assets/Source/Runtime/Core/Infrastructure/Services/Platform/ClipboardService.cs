using Core.Abstractions.Services.Platform;
using UnityEngine;

namespace Core.Infrastructure.Services.Platform
{
    public class ClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        public string GetText()
        {
            return GUIUtility.systemCopyBuffer;
        }
    }
}
