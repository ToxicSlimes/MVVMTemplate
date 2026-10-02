using System;
using UnityEngine;

namespace Core.Domain.Models.Elements.Samples
{
    public struct TextCardData : IElementData, IEquatable<TextCardData>
    {
        public string Id { get; set; }
        public string Title;
        public string Description;
        public Sprite Icon;

        public bool Equals(TextCardData other) =>
            Id == other.Id && Title == other.Title && Description == other.Description;

        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;
    }
}
