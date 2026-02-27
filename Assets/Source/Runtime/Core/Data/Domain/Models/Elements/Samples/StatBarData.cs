namespace Core.Data.Domain.Models.Elements.Samples
{
    public struct StatBarData : IElementData
    {
        public string Id { get; set; }
        public string Label;
        public float CurrentValue;
        public float MaxValue;
        public string FormattedValue;
    }
}
