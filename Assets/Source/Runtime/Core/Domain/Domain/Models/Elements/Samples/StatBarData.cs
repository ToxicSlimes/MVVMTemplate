using System;

namespace Core.Data.Domain.Models.Elements.Samples
{
    public struct StatBarData : IElementData, IEquatable<StatBarData>
    {
        public string Id { get; set; }
        public string Label;
        public float CurrentValue;
        public float MaxValue;
        public string FormattedValue;

        public bool Equals(StatBarData other) =>
            Id == other.Id && Label == other.Label &&
            Math.Abs(CurrentValue - other.CurrentValue) < 0.001f &&
            Math.Abs(MaxValue - other.MaxValue) < 0.001f;

        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;
    }
}
