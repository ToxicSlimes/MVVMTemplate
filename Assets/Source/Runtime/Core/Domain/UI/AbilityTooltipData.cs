namespace Core.Domain.UI
{
    /// <summary>
    /// TEMPLATE EXAMPLE: Custom popup data class.
    /// Implement IPopupData to create your own typed popup data.
    /// </summary>
    public class AbilityTooltipData : IPopupData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CooldownTurns { get; set; }
        public int ManaCost { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
