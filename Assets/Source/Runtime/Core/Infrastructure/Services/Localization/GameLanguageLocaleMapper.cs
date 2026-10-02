using Core.Domain.Enums;

namespace Core.Infrastructure.Services.Localization
{
    public static class GameLanguageLocaleMapper
    {
        public static string ToLocaleCode(GameLanguage language)
        {
            return language switch
            {
                GameLanguage.English => "en",
                GameLanguage.Russian => "ru",
                GameLanguage.German => "de",
                GameLanguage.French => "fr",
                GameLanguage.Spanish => "es",
                GameLanguage.Chinese => "zh-Hans",
                GameLanguage.Japanese => "ja",
                GameLanguage.Korean => "ko",
                _ => "en"
            };
        }
    }
}
