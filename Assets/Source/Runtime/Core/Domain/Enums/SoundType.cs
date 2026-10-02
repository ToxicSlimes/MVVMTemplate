namespace Core.Domain.Enums
{
    public enum SoundType
    {
        Unknown = 0,

        Music_MainMenu = 1,
        Music_Gameplay = 2,
        Music_Battle = 3,
        Music_Victory = 4,
        Music_Defeat = 5,

        UI_ButtonClick = 100,
        UI_ButtonHover = 101,
        UI_MenuOpen = 102,
        UI_MenuClose = 103,
        UI_Notification = 104,
        UI_Purchase = 105,
        UI_Error = 106,
        UI_Success = 107,

        SFX_Hit = 200,
        SFX_Miss = 201,
        SFX_Critical = 202,
        SFX_Death = 203,
        SFX_LevelUp = 204,
        SFX_ItemPickup = 205,
        SFX_ItemDrop = 206,
        SFX_Equip = 207,
        SFX_Unequip = 208,

        Environment_Ambient = 300,
        Environment_Weather = 301,
        Environment_Footstep = 302
    }
}
