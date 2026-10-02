using System;
using UnityEngine;

namespace Core.Domain.Models.Elements.Samples
{
    public struct ActionListItemData : IElementData, IEquatable<ActionListItemData>
    {
        public string Id { get; set; }
        public string Title;
        public string Subtitle;
        public Sprite Icon;
        public string ActionLabel;
        public bool IsActionEnabled;

        public bool Equals(ActionListItemData other) =>
            Id == other.Id && Title == other.Title &&
            Subtitle == other.Subtitle && ActionLabel == other.ActionLabel &&
            IsActionEnabled == other.IsActionEnabled;

        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;
    }
}
