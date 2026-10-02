namespace Core.Domain.Enums
{
    public enum SettingsTab
    {
        Audio,
        Video,
        Display,
        Controls,
        Game
    }

    public enum QualityPreset
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public enum ColorblindType
    {
        None,
        Deuteranopia,
        Protanopia,
        Tritanopia
    }

    public enum GameLanguage
    {
        English,
        Russian,
        German,
        French,
        Spanish,
        Chinese,
        Japanese,
        Korean
    }

    public enum TargetFpsOption
    {
        Fps30 = 30,
        Fps60 = 60,
        Fps120 = 120,
        Unlimited = -1
    }
}
