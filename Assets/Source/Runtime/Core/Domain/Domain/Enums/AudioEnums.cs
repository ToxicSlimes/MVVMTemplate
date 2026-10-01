namespace Core.Data.Domain.Enums
{
    public enum AudioMixerGroupType
    {
        Unknown = 0,
        Master = 1,
        Music = 2,
        SoundEffects = 3,
        UserInterface = 4,
        Environment = 5,
        Voice = 6
    }

    public enum SoundPlaybackScope
    {
        Global = 0,
        Local = 1
    }
}
