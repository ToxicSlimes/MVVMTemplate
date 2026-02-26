namespace Core.Data.UI
{
    public class AbilityTooltipData : IPopupData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CooldownTurns { get; set; }
        public int ManaCost { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
