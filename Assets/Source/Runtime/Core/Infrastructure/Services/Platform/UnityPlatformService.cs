using Core.Abstractions.Services.Platform;
using UnityEngine;

namespace Core.Infrastructure.Services.Platform
{
    public sealed class UnityPlatformService : IPlatformService
    {
        private const float TabletMinDiagonalInches = 7f;
        private const float FallbackDpi = 160f;

        public GamePlatform Platform => UnityEngine.Application.platform switch
        {
            RuntimePlatform.Android => GamePlatform.Android,
            RuntimePlatform.IPhonePlayer => GamePlatform.IOS,
            RuntimePlatform.WebGLPlayer => GamePlatform.WebGL,
            RuntimePlatform.WindowsEditor or RuntimePlatform.OSXEditor or RuntimePlatform.LinuxEditor => GamePlatform.Editor,
            _ => GamePlatform.Standalone
        };

        public DeviceKind Device => SystemInfo.deviceType switch
        {
            DeviceType.Desktop => DeviceKind.Desktop,
            DeviceType.Console => DeviceKind.Tv,
            DeviceType.Handheld => ScreenDiagonalInches() >= TabletMinDiagonalInches ? DeviceKind.Tablet : DeviceKind.Phone,
            _ => DeviceKind.Desktop
        };

        public string Language => LanguageCode(UnityEngine.Application.systemLanguage);

        public void MarkReady() { }

        public void SetGameplayActive(bool active) { }

        private static float ScreenDiagonalInches()
        {
            var dpi = Screen.dpi > 0f ? Screen.dpi : FallbackDpi;
            var width = Screen.width / dpi;
            var height = Screen.height / dpi;
            return Mathf.Sqrt(width * width + height * height);
        }

        private static string LanguageCode(SystemLanguage language) => language switch
        {
            SystemLanguage.Russian => "ru",
            SystemLanguage.Ukrainian => "uk",
            SystemLanguage.Belarusian => "be",
            SystemLanguage.German => "de",
            SystemLanguage.French => "fr",
            SystemLanguage.Spanish => "es",
            SystemLanguage.Portuguese => "pt",
            SystemLanguage.Italian => "it",
            SystemLanguage.Turkish => "tr",
            SystemLanguage.Polish => "pl",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.Korean => "ko",
            SystemLanguage.Chinese or SystemLanguage.ChineseSimplified or SystemLanguage.ChineseTraditional => "zh",
            _ => "en"
        };
    }
}
