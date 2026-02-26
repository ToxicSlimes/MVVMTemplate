namespace Core.Data.UI
{
    public class HeroTooltipData : IPopupData
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public float HealthPercent { get; set; }
        public bool IsDead { get; set; }
        public string Class { get; set; }
    }
}
